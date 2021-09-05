using CSVQuickViewer.Xamarin.Views;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace CSVQuickViewer.Xamarin.ViewModels
{
  public class SelectFileViewModel : BaseViewModel, IProcessDisplay
  {
    public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

    private static readonly FilePickerFileType filePickerFileType =
      new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                             {
                               { DevicePlatform.iOS, new[] { "UTType.Text", "UTType.JSONx", "UTType.DelimitedText", "UTType.CommaSeparatedText" } },
                               { DevicePlatform.Android, new[] { "text/plain" } },
                               { DevicePlatform.UWP, new[] { ".txt", ".tab", ".csv" } },
                             });

    private static readonly PickOptions pickOptions =
        new PickOptions
        {
          PickerTitle = "Please select a delimiter text file",
          FileTypes = filePickerFileType,
        };

    private string fileName = string.Empty;
    private bool guessCodePage;
    private bool guessComment;
    private bool guessDelimiter;
    private bool guessHasHeader;
    private bool guessNewLine;
    private bool guessQualifier;
    private bool guessStartRow;
    private bool isRunning = false;
    public string Status { get; private set; } = string.Empty;

    public SelectFileViewModel()
    {
      OpenFileCommand = new Command(OpenFileAsync);
      SelectFileCommand = new Command(PickAndShow);
      AnalyseFileCommand =  new Command(AnalyseFileAsync);
      
      GuessCodePage = Settings.GuessCodePage;
      GuessComment = Settings.GuessComment;
      GuessDelimiter = Settings.GuessDelimiter;
      GuessHasHeader = Settings.GuessHasHeader;
      GuessQualifier = Settings.GuessQualifier;
      GuessStartRow = Settings.GuessStartRow;
      GuessNewLine = Settings.GuessNewLine;
      FileName = Settings.CurrentFile;
    }

    public Command AnalyseFileCommand { get; }
    public string FileName
    {
      get { return fileName; }
      set
      {
        SetProperty(ref fileName, value);
        OnPropertyChanged(nameof(IsEnabled));
      }
    }

    public bool GuessCodePage
    {
      get { return guessCodePage; }
      set { SetProperty(ref guessCodePage, value); }
    }

    public bool GuessComment
    {
      get { return guessComment; }
      set { SetProperty(ref guessComment, value); }
    }

    public bool GuessDelimiter
    {
      get { return guessDelimiter; }
      set { SetProperty(ref guessDelimiter, value); }
    }

    public bool GuessHasHeader
    {
      get { return guessHasHeader; }
      set { SetProperty(ref guessHasHeader, value); }
    }

    public bool GuessNewLine
    {
      get { return guessNewLine; }
      set { SetProperty(ref guessNewLine, value); }
    }

    public bool GuessQualifier
    {
      get { return guessQualifier; }
      set { SetProperty(ref guessQualifier, value); }
    }

    public bool GuessStartRow
    {
      get { return guessStartRow; }
      set { SetProperty(ref guessStartRow, value); }
    }

    public bool IsEnabled
    {
      get { return !string.IsNullOrEmpty(fileName); }
    }

    public bool IsRunning
    {
      get { return isRunning; }
      set { SetProperty(ref isRunning, value); }
    }

    public Command OpenFileCommand { get; }

    public Command SelectFileCommand { get; }

    private async void AnalyseFileAsync(object obj)
    {
      try
      {
        StoreSettings();
        IsRunning = true;
        //var fr = new FileResult(new FileBase(FileName, "text/plain"));
        using var improvedStream = new ImprovedStream(new SourceAccess(FileName, true,FileName));
        var det = await improvedStream.GetDetectionResult(FileName,
                    this,
                    true,
                    GuessCodePage,
                    GuessDelimiter,
                    GuessQualifier,
                    GuessStartRow,
                    GuessHasHeader,
                    GuessNewLine,
                    GuessComment);
      }
      catch (Exception e)
      {
        Status = e.Message;
        OnPropertyChanged(nameof(Status));
      }
      finally
      {
        IsRunning = false;
      }
    }

    private async void OpenFileAsync(object obj)
    {
      StoreSettings();
      await Shell.Current.GoToAsync($"//{nameof(ItemsPage)}");
    }


    private async void PickAndShow(object obj)
    {
      var result = await FilePicker.PickAsync(pickOptions);

      if (result != null)
        FileName= result.FullPath;
    }

    private void StoreSettings()
    {
      Settings.CurrentFile = FileName;
      Settings.GuessCodePage = GuessCodePage;
      Settings.GuessComment = GuessComment;
      Settings.GuessDelimiter = GuessDelimiter;
      Settings.GuessQualifier = GuessQualifier;
      Settings.GuessHasHeader = GuessHasHeader;
      Settings.GuessStartRow = GuessStartRow;
      Settings.GuessNewLine = GuessNewLine;
    }

    public void Dispose() => CancellationTokenSource.Dispose();

    public event EventHandler<ProgressEventArgs>? Progress;

    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    public string Title { get; set; } = string.Empty;

    public void SetProcess(object? sender, ProgressEventArgs? e) => Status = e?.Text ??  string.Empty;

    public void SetProcess(string text, long value, bool log) => Status = text;
  }
}