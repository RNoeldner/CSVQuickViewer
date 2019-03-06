using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CsvTools
{
  public static class FileDialogWrapper
  {

    string Open(string InitialDirectory)
    {
      if (CommonOpenFileDialog.IsPlatformSupported)
      {
        using (CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog("Please select setting file"))
        {
          commonOpenFileDialog.Filters.Add(new CommonFileDialogFilter("Setting", "*.xml"));
          commonOpenFileDialog.Multiselect = false;
          commonOpenFileDialog.EnsureFileExists = true;
          commonOpenFileDialog.IsFolderPicker = false;
          commonOpenFileDialog.InitialDirectory = InitialDirectory;
          if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            return commonOpenFileDialog.FileName;
        }
      }
      else
      {
        using (OpenFileDialog openFileDialogReference = new System.Windows.Forms.OpenFileDialog())
        {
          openFileDialogReference.AddExtension = false;
          openFileDialogReference.FileName = "*.xml";
          openFileDialogReference.Filter = "XML Settings (*.xml)|*.xml|All files (*.*)|*.*";
          openFileDialogReference.InitialDirectory = InitialDirectory;
          if (openFileDialogReference.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            return openFileDialogReference.FileName;
        }
      }
      return null;
    }
   
  }
}
