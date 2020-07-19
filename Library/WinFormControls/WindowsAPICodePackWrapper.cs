using System;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace CsvTools
{
  public static class WindowsAPICodePackWrapper
  {
    private static readonly bool m_CommonFileDialogSupported = CommonFileDialog.IsPlatformSupported;

    private static bool m_TaskbarManagerSupported = TaskbarManager.IsPlatformSupported;

    public static void AttachTaskbarProgress([NotNull] this IProcessDisplayTime mainProcess)
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
              TaskbarManager.Instance.SetProgressValue(Convert.ToInt32(args.Percent * 1000d), 1000);
            }
          }
          catch (Exception)
          {
            // ignore
            m_TaskbarManagerSupported = false;
          }
      };

      mainProcess.SetMaximum += delegate(object sender, long max)
      {
        if (m_TaskbarManagerSupported && max < 1)
          TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
      };
    }

    public static string Folder([NotNull] string initialDirectory, [NotNull] string title)
    {
      if (m_CommonFileDialogSupported)
        using (var commonOpenFileDialog = new CommonOpenFileDialog(title))
        {
          commonOpenFileDialog.Multiselect = false;
          commonOpenFileDialog.EnsurePathExists = true;
          commonOpenFileDialog.AllowNonFileSystemItems = false;
          commonOpenFileDialog.IsFolderPicker = true;
          commonOpenFileDialog.InitialDirectory = initialDirectory.RemovePrefix();
          if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            return commonOpenFileDialog.FileName;
        }
      else
        using (var openFolderBrowserDialog = new FolderBrowserDialog())
        {
          openFolderBrowserDialog.SelectedPath = initialDirectory.RemovePrefix();
          if (openFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            return openFolderBrowserDialog.SelectedPath;
        }

      return null;
    }

    public static string Open([NotNull] string initialDirectory, [NotNull] string title, [NotNull] string filter,
      [CanBeNull] string preselectFileName)
    {
      if (m_CommonFileDialogSupported)
        using (var commonOpenFileDialog = new CommonOpenFileDialog(title))
        {
          var parts = filter.Split('|');
          var part = 0;
          while (parts.Length >= part + 2)
            commonOpenFileDialog.Filters.Add(new CommonFileDialogFilter(parts[part++], parts[part++]));
          commonOpenFileDialog.Multiselect = false;
          commonOpenFileDialog.EnsureFileExists = true;
          commonOpenFileDialog.EnsurePathExists = true;
          commonOpenFileDialog.IsFolderPicker = false;
          if (!string.IsNullOrEmpty(preselectFileName))
            commonOpenFileDialog.DefaultFileName = preselectFileName;
          commonOpenFileDialog.InitialDirectory = initialDirectory.RemovePrefix();
          if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            return commonOpenFileDialog.FileName.LongFileName();
        }
      else
        using (var openFileDialogReference = new OpenFileDialog())
        {
          openFileDialogReference.AddExtension = false;
          openFileDialogReference.Filter = filter;
          openFileDialogReference.InitialDirectory = initialDirectory.RemovePrefix();
          if (!string.IsNullOrEmpty(preselectFileName))
            openFileDialogReference.FileName = preselectFileName;
          if (openFileDialogReference.ShowDialog() == DialogResult.OK)
            return openFileDialogReference.FileName.LongFileName();
        }

      return null;
    }

    public static string Save(
      [NotNull] string initialDirectory,
      [NotNull] string title,
      [NotNull] string filter,
      string defaultExt,
      bool overwritePrompt = true,
      [CanBeNull] string preselectFileName = null)
    {
      if (m_CommonFileDialogSupported)
        using (var commonOpenFileDialog = new CommonSaveFileDialog(title))
        {
          var parts = filter.Split('|');
          var part = 0;
          while (parts.Length > part + 2)
            commonOpenFileDialog.Filters.Add(new CommonFileDialogFilter(parts[part++], parts[part++]));
          commonOpenFileDialog.DefaultExtension = defaultExt;
          commonOpenFileDialog.InitialDirectory = initialDirectory.RemovePrefix();
          commonOpenFileDialog.EnsurePathExists = true;
          commonOpenFileDialog.EnsureValidNames = true;
          commonOpenFileDialog.OverwritePrompt = overwritePrompt;
          commonOpenFileDialog.RestoreDirectory = true;
          if (!string.IsNullOrEmpty(preselectFileName))
            commonOpenFileDialog.DefaultFileName = preselectFileName;
          if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            return commonOpenFileDialog.FileName.LongFileName();
        }
      else
        using (var saveFileDialog = new SaveFileDialog())
        {
          saveFileDialog.DefaultExt = defaultExt;
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
            return saveFileDialog.FileName.LongFileName();
        }

      return null;
    }
  }
}