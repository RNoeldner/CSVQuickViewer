/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using System;
using System.Collections.Generic;
using Pri.LongPath;
using WinSCP;

namespace CsvTools
{
  /// <summary>
  /// Class to help accessing sFTP and FTP resources, the session will be reused, opened when not already open or reconnection is needed
  /// </summary>
  public static class sFTP
  {
    private static Session m_OpenSession = null;
    private static DateTime m_OpenedAt = DateTime.MinValue.ToUniversalTime();
    private static Action<string, int> SetProcess = null;

    private static void CheckSettings()
    {
      if (ApplicationSetting.ToolSetting == null)
        throw new InvalidOperationException("ToolSetting is not set");

      if (ApplicationSetting.ToolSetting.RemoteAccess == null || !ApplicationSetting.ToolSetting.RemoteAccess.CanConnect)
        throw new InvalidOperationException("Remote Access is not set");

      if (ApplicationSetting.ToolSetting.RemoteAccess.Protocol != AccessProtocol.Local && !ApplicationSetting.ToolSetting.RemoteAccess.CanConnect)
        throw new InvalidOperationException("Host can not be connected");
    }

    /// <summary>
    /// Gets an sFTP session
    /// </summary>
    /// <returns></returns>
    private static Session GetOpenSession()
    {
      if (m_OpenSession == null ||
          !m_OpenSession.Opened ||
            m_OpenedAt.Add(m_OpenSession.ReconnectTime) < DateTime.UtcNow)
      {
        m_OpenSession = new Session
        {
          ReconnectTimeInMilliseconds = 120000
        };
        m_OpenSession.FileTransferProgress += delegate (object sender, FileTransferProgressEventArgs e)
        {
          SetProcess?.Invoke("Transfer", (int)(e.OverallProgress * 100.0));
        };
        m_OpenSession.Open(new SessionOptions
        {
          Protocol = (WinSCP.Protocol)ApplicationSetting.ToolSetting.RemoteAccess.Protocol,
          HostName = ApplicationSetting.ToolSetting.RemoteAccess.HostName,
          UserName = ApplicationSetting.ToolSetting.RemoteAccess.User,
          Password = ApplicationSetting.ToolSetting.RemoteAccess.Password.Decrypt(),
          GiveUpSecurityAndAcceptAnySshHostKey = (ApplicationSetting.ToolSetting.RemoteAccess.Protocol == AccessProtocol.Scp
                                               || ApplicationSetting.ToolSetting.RemoteAccess.Protocol == AccessProtocol.Sftp)
        });
        if (ApplicationSetting.ToolSetting.RemoteAccess.Protocol == AccessProtocol.Scp ||
          ApplicationSetting.ToolSetting.RemoteAccess.Protocol == AccessProtocol.Sftp)
        {
        }
        m_OpenedAt = DateTime.UtcNow;
      }
      return m_OpenSession;
    }

    public static void CloseSession()
    {
      m_OpenedAt = DateTime.MinValue.ToUniversalTime();
      if (m_OpenSession == null)
        return;
      if (m_OpenSession.Opened)
      {
        m_OpenSession.Close();
      }
      m_OpenSession.Dispose();
      m_OpenSession = null;
    }

    /// <summary>
    /// Tests the connection, this does not reuse possibly open connections
    /// </summary>
    public static void TestConnection()
    {
      CheckSettings();
      if (ApplicationSetting.ToolSetting.RemoteAccess.Protocol == AccessProtocol.Local) return;
      GetOpenSession();
    }

    public static string GetRemotePath(string basepath, string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentException("File name can not be empty", nameof(fileName));
      }

      var comp = fileName.RemovePrefix().HandleDirectorySeparatorRoot();
      if (string.IsNullOrEmpty(basepath))
        return comp;

      CheckSettings();
      var session = GetOpenSession();

      var remotepath = basepath.HandleDirectorySeparatorRoot();
      if (session.GetFileInfo(remotepath).IsDirectory)
      {
        if (comp.StartsWith(remotepath))
        {
          remotepath += '/' + comp.Substring(remotepath.Length + 1);
        }
        else
        {
          remotepath += comp;
        }
      }
      return remotepath;
    }

    public static bool RemoteFileExists(string path, string fileName)
    {
      if (string.IsNullOrEmpty(path))
        return false;

      if (ApplicationSetting.ToolSetting.RemoteAccess.Protocol == AccessProtocol.Local)
      {
        return File.Exists(fileName);
      }
      else
      {
        return FileExists(GetRemotePath(path, fileName));
      }
    }

    public static void ProcessRemoteFileRead(string path, string localName, IProcessDisplay processDisplay, bool throwNotFileExists = true)
    {
      if (string.IsNullOrEmpty(localName) || string.IsNullOrEmpty(path))
        return;

      if (ApplicationSetting.ToolSetting.RemoteAccess.Protocol == AccessProtocol.Local)
      {
        File.Copy(path, localName);
      }
      else
      {
        var remotepath = path.HandleDirectorySeparatorRoot(); // GetRemotePath(, localName, false);

        CheckSettings();
        var session = GetOpenSession();

        remotepath = GetRemotePath(path, localName);
        if (!session.FileExists(remotepath))
        {
          if (throwNotFileExists)
          {
            throw new System.IO.FileNotFoundException($"The file {remotepath} has not been found on server");
          }
        }
        else
        {
          var download = false;
          var fiLocal = new Pri.LongPath.FileInfo(localName);
          if (!fiLocal.Exists || fiLocal.Length == 0)
            download = true;
          else
          {
            var fiRemote = session.GetFileInfo(remotepath);
            download = (fiRemote.LastWriteTime > fiLocal.LastWriteTime || fiRemote.Length != fiLocal.Length);
          }

          // Only download if the file is not current
          if (download)
          {
            if (processDisplay != null)
            {
              processDisplay.Maximum = 100;
              SetProcess = processDisplay.SetProcess;
            }

            session.GetFiles(remotepath, localName, false, new TransferOptions
            {
              OverwriteMode = OverwriteMode.Overwrite,
              TransferMode = TransferMode.Binary,
              PreserveTimestamp = true
            });
            if (processDisplay != null)
              SetProcess = null;
          }
          else
            processDisplay?.SetProcess("File is current and will not be downloaded.");
        }

        if (processDisplay != null)
          processDisplay.Maximum = 0;
      }
    }

    /// <summary>
    /// Handles the directory separator and make it a path from root
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns></returns>
    private static string HandleDirectorySeparatorRoot(this string input)
    {
      return '/' + input.Replace('\\', '/').TrimStart(new char[] { '.', '/' }).TrimEnd();
    }

    public static void ProcessRemoteFileWrite(string localPath, string destPath, IProcessDisplay processDisplay)
    {
      if (string.IsNullOrEmpty(localPath) || string.IsNullOrEmpty(destPath))
        return;
      CheckSettings();
      if (ApplicationSetting.ToolSetting.RemoteAccess.Protocol == AccessProtocol.Local)
      {
        File.Copy(localPath, destPath);
      }
      else
      {
        var session = GetOpenSession();
        if (processDisplay != null)
        {
          processDisplay.Maximum = 100;
          SetProcess = processDisplay.SetProcess;
        }

        session.PutFiles(localPath, destPath.HandleDirectorySeparatorRoot(), false, new TransferOptions
        {
          OverwriteMode = OverwriteMode.Overwrite,
          TransferMode = TransferMode.Binary,
          PreserveTimestamp = true
        });
        if (processDisplay != null)
          SetProcess = null;
      }

      if (processDisplay != null)
        processDisplay.Maximum = 0;
    }

    public static IEnumerable<RemoteFileInfo> EnumerateRemoteFiles(string path, string mask)
    {
      CheckSettings();
      return GetOpenSession().EnumerateRemoteFiles(path, mask, EnumerationOptions.None);
    }

    public static bool FileExists(string path)
    {
      CheckSettings();
      try
      {
        return GetOpenSession().FileExists(path.HandleDirectorySeparatorRoot());
      }
      catch 
      {
        return false;        
      }
      
    }

    public static void CreateDirectory(string path)
    {
      CheckSettings();
      GetOpenSession().CreateDirectory(path.HandleDirectorySeparatorRoot());
    }

    public static bool DirectoryExists(string path)
    {
      CheckSettings();
      var basepath = path.HandleDirectorySeparatorRoot();
      var mask = string.Empty;
      var pos = basepath.LastIndexOf('/');
      if (pos != -1)
      {
        mask = basepath.Substring(pos + 1);
        basepath = basepath.Substring(0, pos + 1);
      }
      try
      {
        foreach (var res in GetOpenSession().EnumerateRemoteFiles(basepath, mask,
                EnumerationOptions.MatchDirectories))
          return res.IsDirectory;
      }
      catch
      {
      }
      return false;
    }

    public static bool FileDelete(string path)
    {
      CheckSettings();
      var res = GetOpenSession().RemoveFiles(path.HandleDirectorySeparatorRoot());
      return res.IsSuccess;
    }

    public static RemoteFileInfo GetFileInfo(string path)
    {
      CheckSettings();
      return GetOpenSession().GetFileInfo(path.HandleDirectorySeparatorRoot());
    }

    public static RemoteDirectoryInfo ListDirectory(string path)
    {
      CheckSettings();
      return GetOpenSession().ListDirectory(path.HandleDirectorySeparatorRoot());
    }
  }
}