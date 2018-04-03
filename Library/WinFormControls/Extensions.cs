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

using Pri.LongPath;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class Extensions
  {
    public static void WriteBinding(this Control ctrl)
    {
      var bind = ctrl.GetTextBindng();
      bind?.WriteValue();
    }

    public static bool DeleteFileQuestion(this string fileName, bool ask)
    {
      if (!FileSystemUtils.FileExists(fileName)) return true;
      if (!ask)
      {
        try
        {
          File.Delete(fileName);
          return true;
        }
        catch (Exception)
        {
          // ignored
        }
        return false;
      }

      var disp = fileName.Length > fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1
        ? fileName.Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)
        : fileName;

      if (_MessageBox.Show(null,
            $"The file {disp} does exist, do you want to remove it?", "File", MessageBoxButtons.YesNo) == DialogResult.Yes)
      {
        retry:
        try
        {
          File.Delete(fileName);
          return true;
        }
        catch (Exception ex)
        {
          if (MessageBox.Show(
                $"The file {disp} could not be deleted.\n{ex.ExceptionMessages()}", "File",
                MessageBoxButtons.RetryCancel, MessageBoxIcon.Question) == DialogResult.Retry)
            goto retry;
        }
      }
      return false;
    }

    public static string GetEncryptedPassphrase(this IFileSetting setting)
    {
      Contract.Ensures(Contract.Result<string>() != null);

      if (!setting.FileName.AssumePgp())
        return string.Empty;

      if ( // setting.ToolSetting == null ||
        ApplicationSetting.ToolSetting.PGPInformation.PrivateKeys.IsEmpty())
        throw new ApplicationException("The private key for decryption has not been setup");

      if (!string.IsNullOrEmpty(setting.Passphrase))
        return setting.Passphrase;

      if (!string.IsNullOrEmpty(ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase))
        return ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase;

      // Need to enter Passphrase
      using (var frm = new FormPassphrase())
      {
        if (frm.ShowDialog() == DialogResult.OK)

          return frm.EncryptedPassphrase;
      }

      throw new ApplicationException("A passphrase is needed for decryption.");
    }

    public static string GetProcessDisplayTitle(this IFileSetting fileSetting)
    {
      return string.IsNullOrEmpty(fileSetting.ID)
        ? FileSystemUtils.GetShortDisplayFileName(fileSetting.FileName, 80)
        : fileSetting.ID;
    }

    /// <summary>
    ///   Gets the process display.
    /// </summary>
    /// <param name="fileSetting">The setting.</param>
    /// <param name="cancellationToken">A Cancellation token</param>
    /// <param name="owner">
    ///   The owner form, in case the owner is minimized or closed this progress will do the same
    /// </param>
    /// <returns>A process display, if the stetting want a process</returns>
    public static IProcessDisplay GetProcessDisplay(this IFileSetting fileSetting, Form owner,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      if (!fileSetting.ShowProgress) return new DummyProcessDisplay(cancellationToken);
      var processDisplay = new FormProcessDisplay(fileSetting.GetProcessDisplayTitle(), cancellationToken);
      processDisplay.Show(owner);
      return processDisplay;
    }

    public static Binding GetTextBindng(this Control ctrl)
    {
      foreach (Binding bind in ctrl.DataBindings)
        if (bind.PropertyName == "Text" || bind.PropertyName == "Value")
          return bind;
      return null;
    }

    public static void LoadWindowState(this Form form, Rectangle windowPosition, FormWindowState windowState)
    {
      if (windowPosition != Rectangle.Empty)
      {
        form.StartPosition = FormStartPosition.Manual;

        var screen = Screen.FromRectangle(windowPosition);
        var width = Math.Min(windowPosition.Width, screen.WorkingArea.Width);
        var height = Math.Min(windowPosition.Height, screen.WorkingArea.Height);
        var left = Math.Min(screen.WorkingArea.Right - width, Math.Max(windowPosition.Left, screen.WorkingArea.Left));
        var top = Math.Min(screen.WorkingArea.Bottom - height, Math.Max(windowPosition.Top, screen.WorkingArea.Top));

        form.DesktopBounds = new Rectangle(left, top, width, height);
        form.WindowState = windowState;
      }
      else
      {
        form.WindowState = FormWindowState.Normal;
        form.StartPosition = FormStartPosition.WindowsDefaultBounds;
      }
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    public static void SafeBeginInvoke(this Control uiElement, Action action)
    {
      if (uiElement == null || uiElement.IsDisposed || action == null)
        return;
      if (uiElement.InvokeRequired)
        uiElement.BeginInvoke(action);
      else
        action();
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    public static void SafeInvoke(this Control uiElement, Action action)
    {
      if (uiElement == null || uiElement.IsDisposed || action == null || !uiElement.IsHandleCreated)
        return;
      if (uiElement.InvokeRequired)
        uiElement.Invoke(action);
      else
        action();
      Application.DoEvents();
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    public static void SafeInvokeNoHandleNeeded(this Control uiElement, Action action)
    {
      if (uiElement == null || uiElement.IsDisposed || action == null)
        return;
      if (uiElement.InvokeRequired)
        uiElement.Invoke(action);
      else
        action();
    }

    public static Tuple<Rectangle, FormWindowState> StoreWindowState(this Form form)
    {
      try
      {
        var windowPosition = form.DesktopBounds;
        var windowState = form.WindowState;
        // Get the original WindowPosition in case of maximize or minimize
        if (windowState == FormWindowState.Normal)
          return new Tuple<Rectangle, FormWindowState>(windowPosition, windowState);
        var oldVis = form.Visible;
        form.Visible = false;
        form.WindowState = FormWindowState.Normal;
        windowPosition = form.DesktopBounds;
        form.WindowState = windowState;
        form.Visible = oldVis;

        return new Tuple<Rectangle, FormWindowState>(windowPosition, windowState);
      }
      catch (Exception)
      {
        return null;
      }
    }

    /// <summary>
    ///   Updates the list view column format.
    /// </summary>
    /// <param name="columnFormat">The column format.</param>
    /// <param name="listView">The list view.</param>
    public static void UpdateListViewColumnFormat(this ListView listView, ICollection<Column> columnFormat)
    {
      if (listView == null || listView.IsDisposed)
        return;
      if (listView.InvokeRequired)
      {
        listView.Invoke((MethodInvoker)delegate { listView.UpdateListViewColumnFormat(columnFormat); });
      }
      else
      {
        var oldSelItem = listView.SelectedItems;
        listView.BeginUpdate();
        listView.Items.Clear();

        foreach (var format in columnFormat)
        {
          var ni = listView.Items.Add(format.Name);
          if (oldSelItem.Count > 0) ni.Selected |= format.Name == oldSelItem[0].Text;
          ni.SubItems.Add(format.GetTypeAndFormatDescription());
        }

        listView.EndUpdate();
      }
    }

    /// <summary>
    ///   Writes the file ans displays performance information
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="showSummary">
    ///   Flag indicating that a message Box should be displayed. If <c>true</c> a message box will be
    ///   shown
    /// </param>
    /// <param name="cancellationToken">A CancellationToken</param>
    /// <param name="fileSettingSourcesCurrent">The file setting sources current.</param>
    /// <param name="settingLaterThanSources">The setting later than sources.</param>
    /// <param name="ask">if set to <c>true</c> [ask].</param>
    /// <returns>
    ///   Number of record written to file
    /// </returns>
    public static long WriteFileWithInfo(this IFileSetting fileSetting, bool showSummary,
      CancellationToken cancellationToken,
      FileSettingChecker fileSettingSourcesCurrent, bool ask, bool onlyOlder)
    {
      if (fileSetting == null)
        return 0;

      long written = 0;
      try
      {
        var fi = FileSystemUtils.FileInfo(fileSetting.FullPath);
        if (!FileSystemUtils.DirectoryExists(fi.DirectoryName))
          if (MessageBox.Show(
                $"The directory {fi.DirectoryName.RemoveLongPathPrefix()} does not exist, should it be created?",
                "Directory", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question) == DialogResult.OK)
            Directory.CreateDirectory(fi.DirectoryName);
          else
            return 0;

        var fileInfo = new FileInfo(fileSetting.FullPath);
        if (fileInfo.Exists)
        {
          fileSetting.FileLastWriteTimeUtc = fi.LastWriteTimeUtc;
        }

        var stringBuilder = new System.Text.StringBuilder();
        using (var processDisplay = fileSetting.GetProcessDisplay(null, cancellationToken))
        {
          fileSettingSourcesCurrent?.Invoke(fileSetting, processDisplay);

          if (onlyOlder && fileSetting.SettingLaterThanSources(processDisplay.CancellationToken))
            return 0;

          fileSetting.FullPath.DeleteFileQuestion(ask);

          var errors = new RowErrorCollection(50);
          var writer = fileSetting.GetFileWriter(processDisplay.CancellationToken);
          writer.ProcessDisplay = processDisplay;
          writer.Warning += errors.Add;
          written = writer.Write();

          var hasIssues = !string.IsNullOrEmpty(writer.ErrorMessage) || (errors.CountRows > 0);

          if (showSummary || hasIssues)
          {
            fi = FileSystemUtils.FileInfo(fileSetting.FullPath);

            // if all source settings are file settings, get the latest file time and set this fileTime
            var latest = DateTime.MinValue;
            var dummy = fileSetting.GetSourceFileSettings(cancellationToken, delegate (IFileSetting setting)
            {
              if (!(setting is IFileSettingRemoteDownload))
              {
                if (latest < setting.FileLastWriteTimeUtc)
                  latest = setting.FileLastWriteTimeUtc;
              }
              else
              {
                var fiSrc = FileSystemUtils.FileInfo(setting.FullPath);
                if (fiSrc.Exists && latest < fiSrc.LastWriteTimeUtc)
                  latest = fiSrc.LastWriteTimeUtc;
              }
              return false;
            });
            stringBuilder.Append($"Finished writing file\r\rRecords: {written:N0}\rFile size: {fi.Length / 1048576.0:N} MB");
            if (latest < DateTime.MaxValue && latest > DateTime.MinValue)
            {
              stringBuilder.Append($"\rTime adjusted to latest source file: {latest.ToLocalTime():D}");
              fi.LastWriteTimeUtc = latest;
              fileSetting.FileLastWriteTimeUtc = latest;
            }

            if (hasIssues)
              stringBuilder.Append("\rIssues:\r");

            if (!string.IsNullOrEmpty(writer.ErrorMessage))
              stringBuilder.Append(writer.ErrorMessage);

            if (errors.CountRows > 0)
              stringBuilder.Append(errors.DisplayByRecordNumber);
          }
        }
        if (stringBuilder.Length > 0)
          _MessageBox.Show(null, stringBuilder.ToString(), FileSystemUtils.GetShortDisplayFileName(fileSetting.FileName, 80), MessageBoxButtons.OK);
      }
      catch (Exception exc)
      {
        MessageBox.Show(
          $"Error processing files : {exc.ExceptionMessages()}", FileSystemUtils.GetShortDisplayFileName(fileSetting.FileName, 80), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return written;
    }
  }
}