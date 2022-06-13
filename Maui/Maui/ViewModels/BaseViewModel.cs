using Maui.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui
{
  public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
  {
    protected readonly CancellationTokenSource CancellationTokenSource = new();

    #region Commands
    public ICommand CancelCommand { get; private set; }
    #endregion

    public BaseViewModel() => CancelCommand = new Command(() => CancellationTokenSource.Cancel(), () => IsBusy && !CancellationTokenSource.IsCancellationRequested);

    #region Properties
    bool m_IsViewShown = false;
    public bool IsViewShown
    {
      get { return m_IsViewShown; }
      set { SetProperty(ref m_IsViewShown, value); }
    }
    bool m_IsBusy = false;
    public bool IsBusy
    {
      get { return m_IsBusy; }
      set { SetProperty(ref m_IsBusy, value); }
    }

    string m_ProgressInfo = string.Empty;
    public string ProgressInfo
    {
      get { return m_ProgressInfo; }
      set { SetProperty(ref m_ProgressInfo, value); }
    }
    #endregion

    #region LiveCycle
    public void OnDisappearing()
    {

    }
    #endregion


    #region INotifyPropertyChanged

    public bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        [CanBeNull] Action onChanged = null)
    {
      if (EqualityComparer<T>.Default.Equals(backingStore, value))
        return false;

      backingStore = value;
      onChanged?.Invoke();
      OnPropertyChanged(propertyName);
      return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      // Ordinarily, we release unmanaged resources here;
      // but all are wrapped by safe handles.

      // Release disposable objects.
      if (disposing)
      {

      }
    }
    #endregion

  }
}
