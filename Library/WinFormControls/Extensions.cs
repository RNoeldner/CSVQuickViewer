/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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

using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class Extensions
  {
    public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    ///   Handles a CTRL-A select all in the form.
    /// </summary>
    /// <param name="frm">The calling form</param>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
    public static void CtrlA(this Form frm, object? sender, KeyEventArgs? e)
    {
      if (e is null || !e.Control || e.KeyCode.ToString() != "A")
        return;
      var tb = sender as TextBox;
      if (sender == frm)
        tb = frm.ActiveControl as TextBox;

      if (tb != null)
      {
        tb.SelectAll();
        return;
      }

      var lv = sender as ListView;
      if (sender == frm)
        lv = frm.ActiveControl as ListView;

      if (lv is null || !lv.MultiSelect)
        return;
      foreach (ListViewItem item in lv.Items)
        item.Selected = true;
    }

    /// <summary>
    ///   Gets the process display.
    /// </summary>
    /// <param name="fileSetting">The setting.</param>
    /// <param name="owner">
    ///   The owner form, in case the owner is minimized or closed this progress will do the same
    /// </param>
    /// <param name="withLogger">if set to <c>true</c> [with logger].</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A process display, if the stetting want a process</returns>
    public static FormProgress? GetProgress(
      this IFileSetting fileSetting,
      Form? owner,
      bool withLogger,
      CancellationToken cancellationToken)
    {
      if (!fileSetting.ShowProgress)
        return null;
      var formProgress = new FormProgress(fileSetting.ToString(), withLogger, cancellationToken);
      formProgress.Show(owner);
      return formProgress;
    }

    public static Binding? GetTextBinding(this Control ctrl) => ctrl.DataBindings.Cast<Binding>()
                                                                    .FirstOrDefault(bind => bind.PropertyName == "Text" || bind.PropertyName == "Value");

    public static void InvokeWithHourglass(this Action action)
    {
      if (action is null)
        throw new ArgumentNullException(nameof(action));
      var oldCursor = Cursors.WaitCursor.Equals(Cursor.Current) ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        action.Invoke();
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    public static async Task InvokeWithHourglassAsync(this Func<Task> action)
    {
      if (action is null)
        throw new ArgumentNullException(nameof(action));
      var oldCursor = Cursors.WaitCursor.Equals(Cursor.Current) ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        await action.Invoke();
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    public static void LoadWindowState(
      this Form form,
      WindowState? windowPosition,
      Action<int>? setCustomValue1 = null,
      Action<string>? setCustomValue2 = null)
    {
      if (windowPosition is null || windowPosition.Width == 0 || windowPosition.Height == 0)
        return;
      if (form.IsDisposed)
        return;

      if (!form.Visible)
        form.Show();

      if (!form.Focused)
        form.Focus();

      form.StartPosition = FormStartPosition.Manual;

      var screen = Screen.FromRectangle(
        new Rectangle(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height));
      var width = Math.Min(windowPosition.Width, screen.WorkingArea.Width);
      var height = Math.Min(windowPosition.Height, screen.WorkingArea.Height);
      var left = Math.Min(screen.WorkingArea.Right - width, Math.Max(windowPosition.Left, screen.WorkingArea.Left));
      var top = Math.Min(screen.WorkingArea.Bottom - height, Math.Max(windowPosition.Top, screen.WorkingArea.Top));

      form.DesktopBounds = new Rectangle(left, top, width, height);
      form.WindowState = (FormWindowState) windowPosition.State;
      if (windowPosition.CustomInt != int.MinValue)
        setCustomValue1?.Invoke(windowPosition.CustomInt);
      if (!string.IsNullOrEmpty(windowPosition.CustomText))
        setCustomValue2?.Invoke(windowPosition.CustomText);

      ProcessUIElements();
    }

    /// <summary>
    ///   Handles UI elements while waiting on something
    /// </summary>
    /// <param name="milliseconds">number of milliseconds to not process the calling thread</param>
    public static void ProcessUIElements(int milliseconds = 0)
    {
      try
      {
#if wpf
      Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
#else
        Application.DoEvents();
        if (milliseconds > 10)
          Thread.Sleep(milliseconds);
#endif
      }
      catch (Exception)
      {
        // ignore
      }
    }

    public static void RunStaThread(this Action action, int timeoutMilliseconds = 20000)
    {
      if (action is null)
        throw new ArgumentNullException(nameof(action));
      if (!IsWindows)
      {
        action.Invoke();
        return;
      }
      try
      {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
          action.Invoke();
        else
        {
          var runThread = new Thread(action.Invoke);

          runThread.SetApartmentState(ApartmentState.STA);

          runThread.Start();
          if (timeoutMilliseconds > 0)
            runThread.Join(timeoutMilliseconds);
          else
            runThread.Join();
        }
      }
      catch (Exception e)
      {
        Logger.Error(e);
      }
    }

    public static void RunWithHourglass(this ToolStripItem item, Action action, Form? frm)
    {
      if (item is null)
        throw new ArgumentNullException(nameof(item));
      if (action is null)
        throw new ArgumentNullException(nameof(action));
      try
      {
        item.Enabled = false;
        action.InvokeWithHourglass();
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        frm?.ShowError(ex);
      }
      finally
      {
        item.Enabled = true;
      }
    }

    public static void RunWithHourglass(this Control control, Action action)
    {
      if (control is null)
        throw new ArgumentNullException(nameof(control));
      if (action is null)
        throw new ArgumentNullException(nameof(action));
      try
      {
        control.Enabled = false;
        action.InvokeWithHourglass();
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        var frm = control.FindForm();
        frm?.ShowError(ex);
      }
      finally
      {
        control.Enabled = true;
      }
    }

    public static async Task RunWithHourglassAsync(this ToolStripItem item, Func<Task> action, Form? frm)
    {
      if (item is null)
        throw new ArgumentNullException(nameof(item));

      try
      {
        item.Enabled = false;
        await action.InvokeWithHourglassAsync();
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        frm?.ShowError(ex);
      }
      finally
      {
        item.Enabled = true;
      }
    }

    public static async Task RunWithHourglassAsync(this Control control, Func<Task> action)
    {
      if (control is null)
        throw new ArgumentNullException(nameof(control));
      if (action is null)
        throw new ArgumentNullException(nameof(action));
      try
      {
        control.SafeInvoke(() => control.Enabled = false);
        await action.InvokeWithHourglassAsync();
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        var frm = control.FindForm();
        frm?.ShowError(ex);
      }
      finally
      {
        control.SafeInvoke(() => control.Enabled = true);
      }
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    public static void SafeBeginInvoke(this Control uiElement, Action action)
    {
      if (uiElement.IsDisposed)
        return;

      try
      {
        if (uiElement.InvokeRequired)
        {
          uiElement.BeginInvoke(action);
          return;
        }
      }
      catch
      {
        // sometimes there is an error acessing InvokeRequired
        // if so ignore it and try to preforma the action anyway.
      }
      action();
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    public static void SafeInvoke(this Control uiElement, Action action)
    {
      if (!uiElement.IsHandleCreated)
        return;
      SafeInvokeNoHandleNeeded(uiElement, action);

      ProcessUIElements();
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    /// <param name="timeoutTicks">Timeout to finish action, default is 1/10 of a second</param>
    public static void SafeInvokeNoHandleNeeded(this Control uiElement, Action action,
                                                long timeoutTicks = TimeSpan.TicksPerSecond / 10)
    {
      if (uiElement.IsDisposed)
        return;
      if (uiElement.InvokeRequired)
      {
        var result = uiElement.BeginInvoke(action);
        result.AsyncWaitHandle.WaitOne(new TimeSpan(timeoutTicks));
        result.AsyncWaitHandle.Close();
        if (result.IsCompleted)
          uiElement.EndInvoke(result);
      }
      else
        action();
    }

    public static void SetClipboard(this DataObject dataObject, int timeoutMilliseconds = 120000)
                  => RunStaThread(() =>
      {
        Clipboard.Clear();
        Clipboard.SetDataObject(dataObject, false, 5, 200);
      }, timeoutMilliseconds);

    public static void SetClipboard(this string text) => RunStaThread(() => Clipboard.SetText(text));

    /// <summary>
    ///   Show error information to a user, and logs the message
    /// </summary>
    /// <param name="from">The current Form</param>
    /// <param name="ex">the Exception</param>
    /// <param name="additionalTitle">Title Bar information</param>
    /// <param name="timeout">Timeout in Seconds</param>
    public static void ShowError(this Form? from, Exception ex, string? additionalTitle = "", double timeout = 60.0)
    {
      if (from != null)
        Logger.Warning(ex, "Error in {form} : {message}", from.GetType().Name, ex.SourceExceptionMessage());
      else
        Logger.Warning(ex, ex.SourceExceptionMessage());
      Cursor.Current = Cursors.Default;

      MessageBox.Show(
        ex.ExceptionMessages(),
        string.IsNullOrEmpty(additionalTitle) ? "Error" : $"Error {additionalTitle}",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning,
        timeout: timeout);
    }

    public static WindowState StoreWindowState(this Form form, int customInt = int.MinValue,
                                               string customText = "")
    {
      var windowPosition = form.DesktopBounds;
      var windowState = form.WindowState;

      // Get the original WindowPosition in case of maximize or minimize
      if (windowState != FormWindowState.Normal)
      {
        var oldVis = form.Visible;
        form.Visible = false;
        form.WindowState = FormWindowState.Normal;
        windowPosition = form.DesktopBounds;
        form.WindowState = windowState;
        form.Visible = oldVis;
      }

      return new WindowState(windowPosition.Left, windowPosition.Top, windowPosition.Height, windowPosition.Width,
        windowState, customInt, customText);
    }

    /// <summary>
    ///   Store a bound value
    /// </summary>
    /// <param name="ctrl">The control</param>
    public static void WriteBinding(this Control ctrl) => ctrl.GetTextBinding()?.WriteValue();
  }
}