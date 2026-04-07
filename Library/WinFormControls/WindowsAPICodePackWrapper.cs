/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
#nullable enable

using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CsvTools;

public static class WindowsAPICodePackWrapper
{
  private static bool m_CommonFileDialogSupported = CommonFileDialog.IsPlatformSupported;

  private static bool m_TaskbarManagerSupported = TaskbarManager.IsPlatformSupported;

  public static void SetProgressValue(double percent)
  {
    if (!m_TaskbarManagerSupported)
    {
      return;
    }

    try
    {
      if (percent > 0 && percent <= 1)
      {
        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
        TaskbarManager.Instance.SetProgressValue((percent * 1000d).ToInt(), 1000);
      }
      else
      {
        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
      }
    }
    catch (Exception)
    {
      // ignore
      m_TaskbarManagerSupported = false;
    }
  }

  public static string Folder(string initialDirectory, string title)
  {
    if (m_CommonFileDialogSupported)
    {
      using var commonOpenFileDialog = new CommonOpenFileDialog(title);
      commonOpenFileDialog.Multiselect = false;
      commonOpenFileDialog.EnsurePathExists = true;
      commonOpenFileDialog.AllowNonFileSystemItems = false;
      commonOpenFileDialog.IsFolderPicker = true;
      commonOpenFileDialog.InitialDirectory = initialDirectory.RemovePrefix();
      if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
        return commonOpenFileDialog.FileName;
    }
    else
    {
      using var openFolderBrowserDialog = new FolderBrowserDialog();
      openFolderBrowserDialog.SelectedPath = initialDirectory.RemovePrefix();
      if (openFolderBrowserDialog.ShowDialog() == DialogResult.OK)
        return openFolderBrowserDialog.SelectedPath;
    }

    return string.Empty;
  }

  private static void SetFilter(string filter, CommonFileDialogFilterCollection col)
  {
    var parts = filter.Split('|');
    if (parts.Length > 1)
    {
      for (var part = 0; parts.Length >= part + 2; part += 2)
        col.Add(new CommonFileDialogFilter(parts[part], parts[part + 1]));
    }
    else
      col.Add(new CommonFileDialogFilter(filter, filter));
  }

  public static bool IsDialogOpen { get; private set; }

  public static string Open(string initialDirectory, string title, string filter,
    string? preselectFileName)
  {
    if (m_CommonFileDialogSupported)
    {
      using var commonOpenFileDialog = new CommonOpenFileDialog(title);
      SetFilter(filter, commonOpenFileDialog.Filters);
      commonOpenFileDialog.Multiselect = false;
      commonOpenFileDialog.EnsureFileExists = true;
      commonOpenFileDialog.EnsurePathExists = true;
      commonOpenFileDialog.IsFolderPicker = false;
      if (!string.IsNullOrEmpty(preselectFileName))
        commonOpenFileDialog.DefaultFileName = preselectFileName;
      commonOpenFileDialog.InitialDirectory = initialDirectory.RemovePrefix();
      IsDialogOpen = true;
      try
      {
        if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok &&
            commonOpenFileDialog.FileAsShellObject.IsFileSystemObject)
        {
          try
          {
            return ((Microsoft.WindowsAPICodePack.Shell.ShellFile) commonOpenFileDialog.FileAsShellObject).Path
              .LongFileName();
          }
          catch (InvalidCastException)
          {
            return commonOpenFileDialog.FileName;
          }
        }
      }
      finally
      {
        IsDialogOpen = false;
      }
    }
    else
    {
      using var openFileDialogReference = new OpenFileDialog();
      openFileDialogReference.AddExtension = false;
      openFileDialogReference.Filter = filter;
      openFileDialogReference.InitialDirectory = initialDirectory.RemovePrefix();
      if (!string.IsNullOrEmpty(preselectFileName))
        openFileDialogReference.FileName = preselectFileName;
      IsDialogOpen = true;
      try
      {
        if (openFileDialogReference.ShowDialog() == DialogResult.OK)
          return openFileDialogReference.FileName?.LongFileName() ?? string.Empty;
      }
      finally
      {
        IsDialogOpen = false;
      }
    }

    return string.Empty;
  }
  public static string Save(
    string initialDirectory,
    string title,
    string filter,
    string defaultExt,
    bool overwritePrompt = true,
    string? preselectFileName = null)
  {
    // Try CommonFileDialog first, fall back to standard Dialog if it fails or is unsupported
    while (true)
    {
      if (m_CommonFileDialogSupported)
      {
        using var commonSaveFileDialog = new CommonSaveFileDialog(title);
        SetFilter(filter, commonSaveFileDialog.Filters);
        commonSaveFileDialog.InitialDirectory = initialDirectory.RemovePrefix();
        commonSaveFileDialog.EnsurePathExists = true;
        commonSaveFileDialog.EnsureValidNames = true;
        commonSaveFileDialog.OverwritePrompt = overwritePrompt;
        commonSaveFileDialog.RestoreDirectory = true;
        commonSaveFileDialog.DefaultExtension = defaultExt;

        if (!string.IsNullOrEmpty(preselectFileName))
          commonSaveFileDialog.DefaultFileName = preselectFileName;

        try
        {
          IsDialogOpen = true;
          if (commonSaveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
          {
            // Use the ShellObject Path to ensure long file name support and correct extension handling
            return commonSaveFileDialog.FileAsShellObject.ParsingName.LongFileName();
          }

          return string.Empty;
        }
        catch (Exception exception)
        {
          Debug.WriteLine($"CommonSaveFileDialog failed: {exception.Message}. Falling back.");
          m_CommonFileDialogSupported = false;
          // Loop continues and hits the 'else' block
        }
        finally
        {
          IsDialogOpen = false;
        }
      }
      else
      {
        using var saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = filter;
        saveFileDialog.DefaultExt = defaultExt;
        saveFileDialog.OverwritePrompt = overwritePrompt;
        saveFileDialog.CheckPathExists = true;
        saveFileDialog.RestoreDirectory = true;
        saveFileDialog.Title = title;
        saveFileDialog.InitialDirectory = initialDirectory.RemovePrefix();

        if (!string.IsNullOrEmpty(preselectFileName))
          saveFileDialog.FileName = preselectFileName;

        IsDialogOpen = true;
        try
        {
          // FIXED: Return the filename only if the result is OK
          if (saveFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(saveFileDialog.FileName))
          {
            return saveFileDialog.FileName!.LongFileName();
          }
        }
        finally
        {
          IsDialogOpen = false;
        }

        return string.Empty;
      }
    }
  }
}