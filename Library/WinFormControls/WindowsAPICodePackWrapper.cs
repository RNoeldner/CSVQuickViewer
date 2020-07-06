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
      // Handle the TaskBarProcess 
      mainProcess.ProgressTime += (sender, args) =>
      {
        if (string.IsNullOrEmpty(args.Text) && args.Value < 0)
        {
          SetProgressState(true);
        }
        else
        {
          if (args.Percent >= 1d)
          {
            SetProgressState(true);
          }
          else
          {
            if (args.Value > -1 && args.Percent < 1d)
            {
              SetProgressState(false);
              SetProgressValue(Convert.ToInt32(args.Percent * 1000d), 1000);
            }
          }
        }

        FunctionalDI.SignalBackground?.Invoke();
      };

      mainProcess.SetMaximum += delegate(object sender, long max)
      {
        if (max < 1)
          SetProgressState(true);
        FunctionalDI.SignalBackground?.Invoke();
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
        using (var openFileDialogReference = new OpenFileDialog())
        {
          openFileDialogReference.AddExtension = false;
          openFileDialogReference.InitialDirectory = initialDirectory.RemovePrefix();
          if (openFileDialogReference.ShowDialog() == DialogResult.OK)
            return openFileDialogReference.FileName.GetDirectoryName();
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