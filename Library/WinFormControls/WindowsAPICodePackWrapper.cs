namespace CsvTools
{
  using Microsoft.WindowsAPICodePack.Dialogs;
  using Microsoft.WindowsAPICodePack.Taskbar;
  using System;
  using System.Windows.Forms;

  public static class WindowsAPICodePackWrapper
  {
    private static readonly bool m_CommonFileDialogSupported = CommonFileDialog.IsPlatformSupported;

    private static bool m_TaskbarManagerSupported = TaskbarManager.IsPlatformSupported;

    public static void AttachTaskbarProgress(this IProcessDisplayTime mainProcess)
    {
      // Handle the TaskBarProcess as well
      mainProcess.Progress += delegate
      {
          if (mainProcess.Maximum != -1 && mainProcess.TimeToCompletion.Value > -1
                                        && mainProcess.TimeToCompletion.Value
                                        != mainProcess.TimeToCompletion.TargetValue)
          {
            SetProgressState(false);
            SetProgressValue(
              mainProcess.TimeToCompletion.Value.ToInt(),
              mainProcess.TimeToCompletion.TargetValue.ToInt());
          }

          if (mainProcess.TimeToCompletion.Value == mainProcess.TimeToCompletion.TargetValue)

            // done
            SetProgressState(true);
          Extensions.ProcessUIElements();
        };

      mainProcess.SetMaximum += delegate (object sender, long max)
        {
          if (max < 1)
            SetProgressState(true);
          Extensions.ProcessUIElements();
        };
    }

    public static string Folder(string initialDirectory, string title)
    {
      if (m_CommonFileDialogSupported)
      {
        using (var commonOpenFileDialog = new CommonOpenFileDialog(title))
        {
          commonOpenFileDialog.Multiselect = false;
          commonOpenFileDialog.EnsurePathExists = true;
          commonOpenFileDialog.AllowNonFileSystemItems = false;
          commonOpenFileDialog.IsFolderPicker = true;
          commonOpenFileDialog.InitialDirectory = initialDirectory;
          if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            return commonOpenFileDialog.FileName;
        }
      }
      else
        using (var folderDialog = new FolderTree())
        {
          folderDialog.SetCurrentPath(initialDirectory);
          if (folderDialog.ShowDialog() == DialogResult.OK)
            return folderDialog.SelectedPath;
        }

      return null;
    }

    public static string Open(string initialDirectory, string title, string filter, string preselectFileName)
    {
      if (m_CommonFileDialogSupported)
      {
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
      }
      else
      {
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
      }

      return null;
    }

    public static string Save(
      string initialDirectory,
      string title,
      string filter,
      string defaultExt,
      string preselectFileName)
    {
      if (m_CommonFileDialogSupported)
      {
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
          commonOpenFileDialog.OverwritePrompt = true;
          commonOpenFileDialog.RestoreDirectory = true;
          if (!string.IsNullOrEmpty(preselectFileName))
            commonOpenFileDialog.DefaultFileName = preselectFileName;
          if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            return commonOpenFileDialog.FileName.LongFileName();
        }
      }
      else
      {
        using (var saveFileDialog = new SaveFileDialog())
        {
          saveFileDialog.DefaultExt = defaultExt;
          saveFileDialog.Filter = filter;
          saveFileDialog.OverwritePrompt = true;
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
      }

      return null;
    }

    private static void SetProgressState(bool noProgress)
    {
      if (!m_TaskbarManagerSupported)
        return;
      try
      {
        TaskbarManager.Instance.SetProgressState(!noProgress
          ? TaskbarProgressBarState.Normal
          : TaskbarProgressBarState.NoProgress);
      }
      catch (Exception)
      {
        // ignore
        m_TaskbarManagerSupported = false;
      }
    }

    private static void SetProgressValue(int currentValue, int maximumValue)
    {
      if (!m_TaskbarManagerSupported)
        return;
      try
      {
        TaskbarManager.Instance.SetProgressValue(currentValue, maximumValue);
      }
      catch (Exception)
      {
        // ignore
        m_TaskbarManagerSupported = false;
      }
    }
  }
}