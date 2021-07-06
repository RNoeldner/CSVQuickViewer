using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Windows.Forms;

namespace CsvTools
{
  public static class WindowsAPICodePackWrapper
  {
    private static bool m_CommonFileDialogSupported = CommonFileDialog.IsPlatformSupported;

    private static bool m_TaskbarManagerSupported = TaskbarManager.IsPlatformSupported;

    public static void AttachTaskbarProgress(this IProcessDisplayTime mainProcess)
    {
      if (!m_TaskbarManagerSupported) return;

      // Handle the TaskBarProcess
      mainProcess.ProgressTime += (sender, args) =>
      {
        if (m_TaskbarManagerSupported)
          try
          {
            if ((string.IsNullOrEmpty(args.Text) && args.Value < 0) || args.Percent >= 1d)
              TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            else if (args.Value > -1)
            {
              TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
              TaskbarManager.Instance.SetProgressValue((args.Percent * 1000d).ToInt(), 1000);
            }
          }
          catch (Exception)
          {
            // ignore
            m_TaskbarManagerSupported = false;
          }
      };

      mainProcess.SetMaximum += delegate (object sender, long max)
      {
        try
        {
          if (m_TaskbarManagerSupported && max < 1)
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
        }
        catch (Exception)
        {
          // Ignore
        }
      };
    }

    public static string? Folder(string initialDirectory, string title)
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

      return null;
    }

    private static void SetFilter(string filter, CommonFileDialogFilterCollection col)
    {
      var parts = filter.Split('|');
      if (parts.Length>1)
      {
        for (var part = 0; parts.Length >= part + 2; part+=2)
          col.Add(new CommonFileDialogFilter(parts[part], parts[part+1]));
      }
      else
        col.Add(new CommonFileDialogFilter(filter, filter));
    }

    public static string? Open(string initialDirectory, string title, string filter,
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
        if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok && commonOpenFileDialog.FileAsShellObject.IsFileSystemObject)
        {
          return ((Microsoft.WindowsAPICodePack.Shell.ShellFile) commonOpenFileDialog.FileAsShellObject).Path.LongFileName();
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
        if (openFileDialogReference.ShowDialog() == DialogResult.OK)
          return openFileDialogReference.FileName?.LongFileName();
      }

      return null;
    }

    public static string? Save(
      string initialDirectory,
      string title,
      string filter,
      string defaultExt,
      bool overwritePrompt = true,
      string? preselectFileName = null)
    {
      Retry:
      if (m_CommonFileDialogSupported)
      {
        using var commonSaveFileDialog = new CommonSaveFileDialog(title);
        SetFilter(filter, commonSaveFileDialog.Filters);
        commonSaveFileDialog.InitialDirectory = initialDirectory.RemovePrefix();
        commonSaveFileDialog.EnsurePathExists = true;
        commonSaveFileDialog.EnsureValidNames = true;
        commonSaveFileDialog.OverwritePrompt = overwritePrompt;
        commonSaveFileDialog.RestoreDirectory = true;
        if (!string.IsNullOrEmpty(preselectFileName))
        {
          commonSaveFileDialog.DefaultFileName = preselectFileName;
          commonSaveFileDialog.DefaultExtension = defaultExt;
        }
        try
        {
          if (commonSaveFileDialog.ShowDialog() == CommonFileDialogResult.Ok && commonSaveFileDialog.FileAsShellObject.IsFileSystemObject)
            // can not use commonSaveFileDialog.FileName the extension is wrong / first extension
            // of filter is added no matter what is entered in dialog
            return ((Microsoft.WindowsAPICodePack.Shell.ShellFile) commonSaveFileDialog.FileAsShellObject).Path.LongFileName();
        }
        catch (Exception exception)
        {
          Logger.Warning(exception, "Using CommonSaveFileDialog");
          m_CommonFileDialogSupported = false;
          goto Retry;
        }
      }
      else
      {
        using var saveFileDialog = new SaveFileDialog();
        //saveFileDialog.DefaultExt = defaultExt;
        saveFileDialog.Filter = filter;
        saveFileDialog.OverwritePrompt = overwritePrompt;
        saveFileDialog.CheckFileExists = true;
        saveFileDialog.CheckPathExists = true;
        saveFileDialog.RestoreDirectory = true;
        saveFileDialog.Title = title;
        saveFileDialog.InitialDirectory = initialDirectory.RemovePrefix();
        if (!string.IsNullOrEmpty(preselectFileName))
          saveFileDialog.FileName = preselectFileName;

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
          return saveFileDialog.FileName?.LongFileName();
      }

      return null;
    }
  }
}