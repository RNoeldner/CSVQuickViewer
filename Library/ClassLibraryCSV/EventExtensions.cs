using System;
namespace CsvTools
{
  /// <summary>
  /// Making event invocation safer and logging exceptions from handlers
  /// </summary>
  public static class EventExtensions
  {
    /// <summary>
    /// Safely invokes an event handler, making a thread-safe copy of the delegate.
    /// Catches exceptions from handlers swallows but logs them
    /// </summary>
    public static void SafeInvoke<TEventArgs>(
        this EventHandler<TEventArgs> handler,
        object sender,
        TEventArgs e) // where TEventArgs : EventArgs
    {
      if (handler == null)
        return;
      var array = handler.GetInvocationList();
      for (var i = 0; i<array.Length; i++)
      {
        var singleHandler = (EventHandler<TEventArgs>) array[i];
        try
        {
          singleHandler(sender, e);
        }
        catch (Exception ex)
        {
          Logger.Warning(ex, "Event handler threw an exception: {Error}", ex.Message);
        }
      }
    }
    /// <summary>
    /// Safely invokes an event handler, making a thread-safe copy of the delegate.
    /// Catches exceptions from handlers swallows but logs them
    /// </summary>
    public static void SafeInvoke(
        this EventHandler handler,
        object sender) 
    {
      var array = handler.GetInvocationList();
      for (var i = 0; i<array.Length; i++)
      {
        var singleHandler = (EventHandler) array[i];
        try
        {
          singleHandler(sender, EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Logger.Warning(ex, "Event handler threw an exception: {Error}", ex.Message);
        }
      }
    }
  }
}