/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools;

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
  public static void CtrlA(this Form frm, object? sender, KeyEventArgs e)
  {
    if (!e.Control || e.KeyCode != Keys.A)
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
  public static FormProgress GetProgress(
    this IFileSetting fileSetting,
    Form? owner,
    bool withLogger,
    in CancellationToken cancellationToken)
  {
    var formProgress = new FormProgress(fileSetting.GetDisplay(), cancellationToken);
    formProgress.Show(owner);

    return formProgress;
  }

  public static Binding? GetTextBinding(this Control ctrl) => ctrl.DataBindings.Cast<Binding>()
      .FirstOrDefault(bind => string.Equals(bind.PropertyName, "Text", StringComparison.OrdinalIgnoreCase)|| string.Equals(bind.PropertyName, "Value", StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Executes a synchronous action while displaying the wait cursor.
  /// </summary>
  /// <param name="action">The synchronous logic to execute.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
  /// <remarks>
  /// <para>
  ///   <b>UI Blocking:</b> This is a synchronous method and will block the UI message loop. 
  ///   For long-running tasks, consider <see cref="InvokeWithHourglassAsync(Func{Task})"/>.
  /// </para>
  /// <para>
  ///   <b>Thread Safety:</b> Must be called from the UI thread. <see cref="Cursor.Current"/> 
  ///   is thread-specific in Windows Forms.
  /// </para>
  /// </remarks>
  public static void InvokeWithHourglass(this Action action)
  {
    if (action is null)
      throw new ArgumentNullException(nameof(action));

    var oldCursor = Cursor.Current;
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

  /// <summary>
  /// Executes an asynchronous task while displaying the wait cursor.
  /// </summary>
  /// <param name="action">The asynchronous task to execute.</param>
  /// <returns>A task representing the ongoing operation.</returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
  /// <remarks>
  /// <para>
  ///   <b>Threading:</b> This method does not use <c>ConfigureAwait(false)</c>. It captures 
  ///   the UI <see cref="SynchronizationContext"/> to safely restore the cursor on the UI thread.
  /// </para>
  /// </remarks>
  public static async Task InvokeWithHourglassAsync(this Func<Task> action)
  {
    if (action is null)
      throw new ArgumentNullException(nameof(action));

    var oldCursor = Cursor.Current;
    Cursor.Current = Cursors.WaitCursor;
    try
    {
      // Maintain UI context to ensure restoration happens on the correct thread
      await action.Invoke().ConfigureAwait(true);
    }
    finally
    {
      Cursor.Current = oldCursor;
    }
  }

  /// <summary>
  /// Restores a form's size, position, and state from a <see cref="WindowState"/> object.
  /// Validates bounds against available screen real estate to prevent "off-screen" windows.
  /// </summary>
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
    try
    {
      if (!form.Visible)
        form.Show();

      form.StartPosition = FormStartPosition.Manual;

      var screen = Screen.FromRectangle(
        new Rectangle(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height));
      var width = Math.Min(windowPosition.Width, screen.WorkingArea.Width);
      var height = Math.Min(windowPosition.Height, screen.WorkingArea.Height);
      var left = Math.Min(screen.WorkingArea.Right - width, Math.Max(windowPosition.Left, screen.WorkingArea.Left));
      var top = Math.Min(screen.WorkingArea.Bottom - height, Math.Max(windowPosition.Top, screen.WorkingArea.Top));

      form.DesktopBounds = new Rectangle(left, top, width, height);
      form.WindowState = windowPosition.State;
      if (windowPosition.CustomInt != int.MinValue)
        setCustomValue1?.Invoke(windowPosition.CustomInt);
      if (!string.IsNullOrEmpty(windowPosition.CustomText))
        setCustomValue2?.Invoke(windowPosition.CustomText);

      ProcessUIElements();
    }
    catch
    {
      //Ignore
    }
  }

  /// <summary>
  /// Launches the default OS application for a given path or URL. 
  /// Handles known .NET Core issues with shell execution on different platforms.
  /// </summary>
  public static void OpenDefaultApplication(string pathOrUrl)
  {
    if (!string.IsNullOrEmpty(pathOrUrl))
    {
      try
      {
        Process.Start(pathOrUrl);
      }
      catch
      {
        // hack because of this: https://github.com/dotnet/corefx/issues/10361
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
          pathOrUrl = pathOrUrl.Replace("&", "^&");
          Process.Start(new ProcessStartInfo(pathOrUrl) { UseShellExecute = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
          Process.Start("open", pathOrUrl);
        }
        else
          throw;
      }
    }
  }

  /// <summary>
  /// Flushes the UI message queue to keep the interface responsive during synchronous loops.
  /// </summary>
  /// <param name="milliseconds">Optional sleep duration after processing events.</param>
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

  /// <summary>
  /// Resizes an image using high-quality bicubic interpolation.
  /// </summary>
  public static Image ResizeImage(this Image source, int width, int height)
  {
    var bmp = new Bitmap(width, height);
    bmp.SetResolution(source.HorizontalResolution, source.VerticalResolution);

    using (var g = Graphics.FromImage(bmp))
    {
      g.InterpolationMode = InterpolationMode.HighQualityBicubic;
      g.SmoothingMode = SmoothingMode.HighQuality;
      g.PixelOffsetMode = PixelOffsetMode.HighQuality;
      g.DrawImage(source, 0, 0, width, height);
    }

    return bmp;
  }

  /// <summary>
  /// Ensures an action is run on an STA (Single Threaded Apartment) thread, 
  /// which is required for certain Windows features like the Clipboard or File Dialogs.
  /// </summary>
  public static void RunStaThread(this Action action, int timeoutMilliseconds = 20000)
  {
    if (!IsWindows || Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
      action.Invoke();
    else
    {
      var thread = new Thread(action.Invoke)
      {
        IsBackground = true
      };
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      if (timeoutMilliseconds > 0)
        thread.Join(timeoutMilliseconds);
      else
        thread.Join();
    }
  }

  /// <summary>
  /// Disables a <see cref="ToolStripItem"/> and displays a wait cursor during a synchronous operation.
  /// Includes automatic error handling via the provided form.
  /// </summary>
  /// <param name="item">The tool strip item to disable.</param>
  /// <param name="action">The synchronous logic to execute.</param>
  /// <param name="frm">The parent form used for error reporting.</param>
  /// <remarks>
  /// <b>Re-entrancy:</b> Disabling the item prevents duplicate triggers if the user clicks 
  /// while the UI thread is processing.
  /// </remarks>
  public static void RunWithHourglass(this ToolStripItem item, Action action, Form frm)
  {
    if (item == null) throw new ArgumentNullException(nameof(item));
    RunWithHourglassInternal(action, (val) => frm.SafeInvoke(() => item.Enabled = val), frm);
  }

  /// <summary>
  /// Disables a <see cref="Control"/> and displays a wait cursor during a synchronous operation.
  /// </summary>
  /// <param name="control">The control to disable.</param>
  /// <param name="action">The synchronous logic to execute.</param>
  public static void RunWithHourglass(this Control control, Action action)
  {
    if (control == null) throw new ArgumentNullException(nameof(control));
    RunWithHourglassInternal(action, (val) => control.SafeInvoke(() => control.Enabled = val), control.FindForm());
  }

  /// <summary>
  /// Disables a <see cref="ToolStripItem"/> and displays a wait cursor during an asynchronous operation.
  /// </summary>
  /// <param name="item">The tool strip item to disable.</param>
  /// <param name="action">The asynchronous task to execute.</param>
  /// <param name="frm">The parent form used for error reporting.</param>
  /// <returns>A task representing the operation.</returns>
  public static Task RunWithHourglassAsync(this ToolStripItem item, Func<Task> action, Form frm)
  {
    if (item == null) throw new ArgumentNullException(nameof(item));
    return RunWithHourglassInternalAsync(action, (val) => frm.SafeInvoke(() => item.Enabled = val), frm);
  }

  /// <summary>
  /// Disables a <see cref="Control"/> and displays a wait cursor during an asynchronous operation.
  /// </summary>
  /// <param name="control">The control to disable.</param>
  /// <param name="action">The asynchronous task to execute.</param>
  /// <returns>A task representing the operation.</returns>
  public static Task RunWithHourglassAsync(this Control control, Func<Task> action)
  {
    if (control == null) throw new ArgumentNullException(nameof(control));
    return RunWithHourglassInternalAsync(action, (val) => control.SafeInvoke(() => control.Enabled = val), control.FindForm());
  }

  /// <summary>
  /// Safely executes an action on the UI thread using BeginInvoke (Asynchronous).
  /// </summary>
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
      // sometimes there is an error accessing InvokeRequired
      // if so ignore it and try to preform the action anyway.
    }

    action();
  }

  /// <summary>
  /// Safely executes an action on the UI thread using Invoke (Synchronous).
  /// Forces handle creation if necessary.
  /// </summary>
  public static void SafeInvoke(this Control uiElement, Action action)
  {
    if (uiElement.IsDisposed)
      return;

    // Ensure control handle is created (lazy creation)
    if (!uiElement.IsHandleCreated)
      try { _ = uiElement.Handle; }
      catch
      {
        // Any exception while checking handle => skip
        return;
      }
    SafeInvokeNoHandleNeeded(uiElement, action);

    ProcessUIElements();
  }

  /// <summary>
  /// Executes an action on the UI thread safely and synchronously.
  /// Assumes the control handle is already created (no handle check).
  /// </summary>
  /// <param name="uiElement">The control on which to invoke the action.</param>
  /// <param name="action">The action to execute.</param>
  public static void SafeInvokeNoHandleNeeded(this Control uiElement, Action action)
  {
    if (uiElement.IsDisposed)
      return;
    try
    {
      if (uiElement.InvokeRequired)
      {
        // Synchronous invoke on UI thread
        uiElement.Invoke(action);
      }
      else
      {
        // Already on UI thread
        action();
      }
    }
    catch (ObjectDisposedException)
    {
      // Control disposed mid-invoke, safely ignore
    }
    catch (InvalidOperationException) when (!uiElement.IsHandleCreated || uiElement.IsDisposed)
    {
      // Control handle invalid or disposed
    }
  }
  public static void SetClipboard(this DataObject dataObject, int timeoutMilliseconds = 120000)
      => RunStaThread(() =>
      {
        Clipboard.Clear();
        Clipboard.SetDataObject(dataObject, false, 5, 200);
      }, timeoutMilliseconds);

  public static void SetClipboard(this string text) => RunStaThread(() => Clipboard.SetText(text));

  public static void SetEnumDataSource<T>(this ComboBox cbo, T currentValue, IReadOnlyCollection<T>? doNotShow = null)
      where T : Enum
  {
    cbo.SuspendLayout();

    cbo.DisplayMember = nameof(DisplayItem<T>.Display);
    cbo.ValueMember = nameof(DisplayItem<T>.ID);

    cbo.DataSource = Enum.GetValues(typeof(T))
      .Cast<T>()
      .Where(item => doNotShow is null || !doNotShow.Contains(item))
      .Select(item => new DisplayItem<T>(item, item.Display()))
      .ToList();
    cbo.SelectedValue = currentValue;
    cbo.ResumeLayout();
  }

  /// <summary>
  ///   Show error information to a user, and logs the message
  /// </summary>
  /// <param name="ex">the Exception</param>
  /// <param name="additionalTitle">Title Bar information</param>
  /// <param name="timeout">Timeout in Seconds</param>
  public static void ShowError(Exception ex, string? additionalTitle = "", double timeout = 60.0)
  {
    Cursor.Current = Cursors.Default;
    MessageBox.Show(
      ex.ExceptionMessages(),
      string.IsNullOrEmpty(additionalTitle) ? "Error" : $"Error {additionalTitle}",
      MessageBoxButtons.OK,
      MessageBoxIcon.Error,
      timeout: timeout);
  }

  public static DialogResult ShowWithFont(this ResizeForm newForm, in Control? ctrl, bool dialog = false)
  {
    Form? parentFrm = null;

    if (ctrl != null)
    {
      parentFrm = ctrl.FindForm();
      if (parentFrm !=null)
        newForm.FontConfig = new FontConfig(parentFrm.Font.Name, parentFrm.Font.Size);
    }
    if (dialog)
      return newForm.ShowDialog(parentFrm);
    newForm.Show(parentFrm);
    return DialogResult.None;
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
  /// Core logic for synchronous UI state management and exception reporting.
  /// </summary>
  private static void RunWithHourglassInternal(Action action, Action<bool> setEnabled, Form? frm)
  {
    // Capture the current cursor state
    var previousCursor = Cursor.Current;
    try
    {
      setEnabled(false);
      frm?.SafeInvoke(() => Cursor.Current=Cursors.WaitCursor);
      action.InvokeWithHourglass();
    }
    catch (ObjectDisposedException) { /* Component was closed; ignore */ }
    catch (Exception ex)
    {
      // Restore previous state
      frm?.SafeInvoke(() =>
      {
        Cursor.Current=previousCursor ?? Cursors.Default;
        ShowError(ex);
      });
    }
    finally
    {
      try
      {
        // Restore previous state
        frm?.SafeInvoke(() => Cursor.Current=previousCursor ?? Cursors.Default);
        setEnabled(true);
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
    }
  }

  /// <summary>
  /// Core logic for asynchronous UI state management and exception reporting.
  /// </summary>
  private static async Task RunWithHourglassInternalAsync(Func<Task> action, Action<bool> setEnabled, Form? frm)
  {
    // Capture the current cursor state
    var previousCursor = Cursor.Current;
    try
    {
      setEnabled(false);
      frm?.SafeInvoke(() => Cursor.Current=Cursors.WaitCursor);
      await action.InvokeWithHourglassAsync().ConfigureAwait(true);
    }
    catch (ObjectDisposedException) { /* UI was closed during await; ignore */ }
    catch (Exception ex)
    {
      // Restore previous state
      frm?.SafeInvoke(() =>
      {
        Cursor.Current = previousCursor ?? Cursors.Default;
        ShowError(ex);
      });
    }
    finally
    {
      try
      {
        // Restore previous state
        frm?.SafeInvoke(() => Cursor.Current=previousCursor ?? Cursors.Default);
        setEnabled(true);
      }
      catch (ObjectDisposedException)
      {
        // ignore
      }
    }
  }
}