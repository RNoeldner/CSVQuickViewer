using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace CsvTools
{
  public static class WindowsAPICodePackWrapper
  {
    private static bool s_CommonFileDialogSupported = CommonFileDialog.IsPlatformSupported;
    private static bool s_TaskbarManagerSupported = TaskbarManager.IsPlatformSupported;

    public static string Open(string InitialDirectory, string title, string filter)
    {
      if (s_CommonFileDialogSupported)
      {
        using (var commonOpenFileDialog = new CommonOpenFileDialog(title))
        {
          var parts = filter.Split('|');
          int part = 0;
          while (parts.Length > part + 2)
            commonOpenFileDialog.Filters.Add(new CommonFileDialogFilter(parts[part++], parts[part++]));
          commonOpenFileDialog.Multiselect = false;
          commonOpenFileDialog.EnsureFileExists = true;
          commonOpenFileDialog.IsFolderPicker = false;
          commonOpenFileDialog.InitialDirectory = InitialDirectory.RemovePrefix();
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
          openFileDialogReference.InitialDirectory = InitialDirectory.RemovePrefix();
          if (openFileDialogReference.ShowDialog() == DialogResult.OK)
            return openFileDialogReference.FileName.LongFileName();
        }
      }
      return null;
    }

    public static string Save(string InitialDirectory, string title, string filter, string defaultExt)
    {
      if (s_CommonFileDialogSupported)
      {
        using (var commonOpenFileDialog = new CommonSaveFileDialog(title))
        {
          var parts = filter.Split('|');
          int part = 0;
          while (parts.Length > part + 2)
            commonOpenFileDialog.Filters.Add(new CommonFileDialogFilter(parts[part++], parts[part++]));
          commonOpenFileDialog.DefaultExtension = defaultExt;
          commonOpenFileDialog.InitialDirectory = InitialDirectory.RemovePrefix();
          commonOpenFileDialog.OverwritePrompt = true;
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
          saveFileDialog.Title = title;
          saveFileDialog.InitialDirectory = InitialDirectory.RemovePrefix();

          if (saveFileDialog.ShowDialog() != DialogResult.OK)
            return saveFileDialog.FileName.LongFileName();
        }
      }
      return null;
    }

    public static string Folder(string initialDiretory, string title)
    {

      if (s_CommonFileDialogSupported)
      {
        using (var commonOpenFileDialog = new CommonOpenFileDialog(title))
        {
          commonOpenFileDialog.Multiselect = false;
          commonOpenFileDialog.EnsurePathExists = true;
          commonOpenFileDialog.AllowNonFileSystemItems = false;
          commonOpenFileDialog.IsFolderPicker = true;
          commonOpenFileDialog.InitialDirectory = initialDiretory;
          if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            return commonOpenFileDialog.FileName;
        }
      }
      else
        using (var folderDialog = new FolderTree())
        {
          folderDialog.SetCurrentPath(initialDiretory);
          if (folderDialog.ShowDialog() == DialogResult.OK)
            return folderDialog.SelectedPath;
        }
      return null;
    }

    public static void SetProgressState(bool noProgress)
    {
      if (!s_TaskbarManagerSupported) return;
      try
      {
        if (!noProgress)
          TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
        else
          TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
      }
      catch (System.Exception)
      {
        //ignore
        s_TaskbarManagerSupported = false;
      }
    }

    public static void SetProgressValue(int currentValue, int maximumValue)
    {
      if (!s_TaskbarManagerSupported) return;
      try
      {
        TaskbarManager.Instance.SetProgressValue(currentValue, maximumValue);
      }
      catch (System.Exception)
      {
        //ignore
        s_TaskbarManagerSupported = false;
      }

    }

    public static void AttachTaskbarProgress(this IProcessDisplayTime MainProcess)
    {
      // Handle the TaskBarProcess as well
      MainProcess.Progress += delegate (object sender, ProgressEventArgs e)
      {
        if (MainProcess.Maximum != -1 && MainProcess.TimeToCompletion.Value > -1 && MainProcess.TimeToCompletion.Value != MainProcess.TimeToCompletion.TargetValue)
        {
          SetProgressState(false);
          SetProgressValue(MainProcess.TimeToCompletion.Value.ToInt(), MainProcess.TimeToCompletion.TargetValue.ToInt());
        }
        if (MainProcess.TimeToCompletion.Value == MainProcess.TimeToCompletion.TargetValue)
          // done
          SetProgressState(true);
      };

      MainProcess.SetMaximum += delegate (object sender, long max)
      {
        if (max < 1) SetProgressState(true);
      };
    }
  }
}