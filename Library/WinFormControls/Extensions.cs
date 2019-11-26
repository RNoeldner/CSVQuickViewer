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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using Pri.LongPath;

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class Extensions
  {
    public static bool ValidateChildren(this ContainerControl container, CancellationToken cancellationToken) => new Func<bool>(container.ValidateChildren).RunWithTimeout(1, cancellationToken);

    public static void ShowError(this Form from, Exception ex, string additionalTitle = "")
    {
      Logger.Warning(ex, "Issue in UI {form} : {message}", nameof(from), ex.Message);
      Cursor.Current = Cursors.Default;
      System.Windows.Forms.MessageBox.Show(from, ex.ExceptionMessages(), string.IsNullOrEmpty(additionalTitle) ? "Error" : $"Error {additionalTitle}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Checks if a condition is set but processes UI events while waiting
    /// </summary>
    /// <param name="waitWhileTrue">Function to determine if a process should still execute, once the function return <c>FASLE</c> this routine will return</param>
    /// <param name="millisecondsSleep">Waiting the amount of Milliseconds during tests</param>
    /// <param name="timeoutMinutes">Timeout in Minutes</param>
    /// <param name="raiseError"><c>TRUE</c> if an Exception should be raised on timeout, otherwise a log entry will be written</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns><c>true</c> if finished successfully, <c>false</c> if canceled or timeout has been reached</returns>
    public static bool TimeOutWait(Func<bool> waitWhileTrue, int millisecondsSleep, double timeoutMinutes, bool raiseError, CancellationToken cancellationToken)
    {
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      while (waitWhileTrue())
      {
        if (cancellationToken.IsCancellationRequested)
        {
          if (raiseError)
            cancellationToken.ThrowIfCancellationRequested();
          return false;
        }
        if (timeoutMinutes > 0.005 && stopwatch.Elapsed.TotalMinutes > timeoutMinutes)
        {
          var msg = $"Waited longer than {stopwatch.Elapsed.Seconds:N1} seconds, assuming something is wrong";
          if (raiseError)
            throw new TimeoutException(msg);
          else
            Logger.Warning(msg);
          return false;
        }
        ProcessUIElements(millisecondsSleep);
      }
      return true;
    }

    /// <summary>
    /// Handles UI elements while waiting on something
    /// </summary>
    /// <param name="milliseconds">number of milliseconds to not process the calling thread</param>
    public static void ProcessUIElements(int milliseconds = 0)
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

    /// <summary>
    /// Execute an action synchronously with timeout
    /// </summary>
    /// <param name="action">The delegate that does not return a value</param>
    /// <param name="timeoutSeconds">Timeout for the completion of the task, if more time is spent running / waiting the wait is finished</param>
    /// <param name="raiseError">if <c>true</c> an error is thrown in cause of an issue, otherwise its only logged </param>
    /// <param name="cancellationToken">Cancellation Token to be able to cancel the task</param>
    /// <returns><c>true</c> if finished successfully, <c>false</c> if canceled or timeout has been reached</returns>
    public static bool RunWithTimeout(this Action action, double timeoutSeconds = 2.0, bool raiseError = true, CancellationToken cancellationToken = default(CancellationToken))
    {
      if (action == null)
        throw new ArgumentNullException(nameof(action));
      try
      {
        var task = System.Threading.Tasks.Task.Factory.StartNew(action, cancellationToken);
        var result = task.WaitToCompleteTask(timeoutSeconds, false, cancellationToken);
        if (task.IsFaulted)
          throw task.Exception.InnerException;
        return result;
      }
      catch (Exception ex)
      {
        if (raiseError)
          throw ex;

        Logger.Warning(ex, "RunWithTimeout Error: {exception} Timeout: {timeout} Method: {method} {src}", ex.Message, timeoutSeconds, action.Method, ClassLibraryCsvExtensionMethods.UpmostStackTrace());
        return false;
      }
    }

    /// <summary>
    /// Execute an function synchronously with timeout
    /// </summary>
    /// <param name="action">The delegate that does return one value</param>
    /// <param name="timeoutSeconds">Timeout for the completion of the task, if more time is spent running / waiting the wait is finished</param>
    /// <param name="cancellationToken">Cancellation Token to be able to cancel the task</param>
    ///
    /// <returns>The return value of the delegate</returns>
    public static TResult RunWithTimeout<TResult>(this Func<TResult> action, double timeoutSeconds = 1d, CancellationToken cancellationToken = default(CancellationToken))
    {
      if (action == null)
        throw new ArgumentNullException(nameof(action));
      try
      {
        var task = System.Threading.Tasks.Task.Factory.StartNew(action, cancellationToken);
        task.WaitToCompleteTask(timeoutSeconds, false, cancellationToken);
        if (task.IsFaulted)
          throw task.Exception.InnerException;
        else
          return task.Result;
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "RunWithTimeout Error: {exception} Timeout: {timeout} Method: {method} {src}", ex.Message, timeoutSeconds, action.Method, ClassLibraryCsvExtensionMethods.UpmostStackTrace());
        throw;
      }
    }

    /// <summary>
    /// Run a task synchronously with timeout
    /// </summary>
    /// <param name="executeTask">The started <see cref="System.Threading.Tasks.Task"/></param>
    /// <param name="timeoutSeconds">Timeout for the completion of the task, if more time is spent running / waiting the wait is finished</param>
    /// <param name="raiseError">if <c>true</c> an error is thrown in cause of an issue, otherwise its only logged </param>
    /// <param name="cancellationToken">Cancellation Token to be able to cancel the task</param>
    /// <returns><c>true</c> if finished successfully, <c>false</c> if canceled or timeout has been reached</returns>
    public static bool WaitToCompleteTask(this System.Threading.Tasks.Task executeTask, double timeoutSeconds, bool raiseError, CancellationToken cancellationToken)
    {
      if (executeTask == null)
        throw new ArgumentNullException(nameof(executeTask));
      return TimeOutWait(() =>
        {
          switch (executeTask.Status)
          {
            // Wait / keep running
            case System.Threading.Tasks.TaskStatus.Created:
            case System.Threading.Tasks.TaskStatus.WaitingForActivation:
            case System.Threading.Tasks.TaskStatus.WaitingToRun:
            case System.Threading.Tasks.TaskStatus.Running:
            case System.Threading.Tasks.TaskStatus.WaitingForChildrenToComplete:
              return true;

            // Finished / no need running
            case System.Threading.Tasks.TaskStatus.Faulted:
            case System.Threading.Tasks.TaskStatus.Canceled:
            case System.Threading.Tasks.TaskStatus.RanToCompletion:
            default:
              return false;
          }
        }, 200, timeoutSeconds / 60d, raiseError, cancellationToken);
    }

    /// <summary>
    ///   Handles a CTRL-A select all in the form.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">
    ///   The <see cref="KeyEventArgs" /> instance containing the event data.
    /// </param>
    public static void CtrlA(this Form frm, object sender, KeyEventArgs e)
    {
      if (e == null || !e.Control || e.KeyCode.ToString() != "A")
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

      if (lv == null || !lv.MultiSelect)
        return;
      foreach (ListViewItem item in lv.Items)
        item.Selected = true;
    }

    /// <summary>
    /// Store a bound value
    /// </summary>
    /// <param name="ctrl">The control</param>
    public static void WriteBinding(this Control ctrl) => ctrl.GetTextBindng()?.WriteValue();

    /// <summary>
    /// Deleting a file, in case it exists it will ask if it should be deleted
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="ask"></param>
    /// <returns>true: the file does not exist, or it was deleted
    /// false: the file was not deleted it does still exist
    /// null: user pressed cancel</returns>
    public static bool? DeleteFileQuestion(this string fileName, bool ask)
    {
      if (!FileSystemUtils.FileExists(fileName))
        return true;
      if (!ask)
      {
        try
        {
          File.Delete(fileName);
          return true;
        }
        catch
        {
          // ignored
        }
        return false;
      }
      var res = FileSystemUtils.SplitPath(fileName);
      var disp = res.FileName;

      var diagRes = _MessageBox.Show(null,
            $"The file {disp} does exist already, do you want to delete the existing file?", "File", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

      if (diagRes == DialogResult.Yes)
      {
        retry:
        try
        {
          File.Delete(fileName);
          return true;
        }
        catch (Exception ex)
        {
          diagRes = DialogResult.No;
          if (_MessageBox.Show(null,
                $"The file {disp} could not be deleted.\n{ex.ExceptionMessages()}", "File",
                MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
            goto retry;
        }
      }
      if (diagRes == DialogResult.Cancel)
        return null;
      else
        return (diagRes == DialogResult.Yes);
    }

    /// <summary>
    /// Gets the encrypted passphrase of a setting, if needed it will open a windows form
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <returns></returns>
    /// The private key for decryption has not been setup
    /// or
    /// A passphrase is needed for decryption.
    /// </exception>
    public static string GetEncryptedPassphraseOpenForm(this IFileSetting setting)
    {
      if (ApplicationSetting.PGPKeyStorage?.PrivateKeys?.IsEmpty() ?? true)
        throw new EncryptionException("The private key for decryption has not been setup");

      if (!string.IsNullOrEmpty(setting?.Passphrase))
        return setting.Passphrase;

      if (!string.IsNullOrEmpty(ApplicationSetting.PGPKeyStorage?.EncryptedPassphase))
        return ApplicationSetting.PGPKeyStorage.EncryptedPassphase;

      // Need to enter Passphrase
      using (var frm = new FormPassphrase())
      {
        if (frm.ShowDialog() == DialogResult.OK)
          return frm.EncryptedPassphrase;
        else
          throw new EncryptionException("A passphrase is needed for decryption.");
      }
    }

    /// <summary>
    ///   Gets the process display.
    /// </summary>
    /// <param name="fileSetting">The setting.</param>
    /// <param name="cancellationToken">A Cancellation token</param>
    /// <param name="owner">
    ///   The owner form, in case the owner is minimized or closed this progress will do the same
    /// </param>
    /// <returns>A process display, if the stetting want a process</returns>
    public static IProcessDisplay GetProcessDisplay(this IFileSetting fileSetting, Form owner, bool withLogger, CancellationToken cancellationToken)
    {
      if (!fileSetting.ShowProgress)
        return new DummyProcessDisplay(cancellationToken);

      var processDisplay = new FormProcessDisplay(fileSetting.ToString(), withLogger, cancellationToken);
      processDisplay.Show(owner);
      return processDisplay;
    }

    public static Binding GetTextBindng(this Control ctrl)
    {
      foreach (Binding bind in ctrl.DataBindings)
        if (bind.PropertyName == "Text" || bind.PropertyName == "Value")
          return bind;
      return null;
    }

    public static void LoadWindowState(this Form form, WindowState windowPosition, Action<int> setCustomValue1 = null, Action<string> setCustomValue2 = null)
    {
      if (windowPosition == null || windowPosition.Width == 0 || windowPosition.Height == 0)
        return;
      if (form.IsDisposed)
        return;

      /*
      if (!form.Visible)
        form.Show();

      if (!form.Focused)
        form.Focus();
      */
      form.StartPosition = FormStartPosition.Manual;

      var screen = Screen.FromRectangle(new Rectangle(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height));
      var width = Math.Min(windowPosition.Width, screen.WorkingArea.Width);
      var height = Math.Min(windowPosition.Height, screen.WorkingArea.Height);
      var left = Math.Min(screen.WorkingArea.Right - width, Math.Max(windowPosition.Left, screen.WorkingArea.Left));
      var top = Math.Min(screen.WorkingArea.Bottom - height, Math.Max(windowPosition.Top, screen.WorkingArea.Top));

      form.DesktopBounds = new Rectangle(left, top, width, height);
      form.WindowState = (FormWindowState)windowPosition.State;
      if (windowPosition.CustomInt != int.MinValue && setCustomValue1 != null)
        setCustomValue1.Invoke(windowPosition.CustomInt);
      if (!string.IsNullOrEmpty(windowPosition.CustomText) && setCustomValue2 != null)
        setCustomValue2.Invoke(windowPosition.CustomText);
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    public static void SafeBeginInvoke(this Control uiElement, Action action)
    {
      if (uiElement == null || uiElement.IsDisposed || action == null)
        return;
      if (uiElement.InvokeRequired)
        uiElement.BeginInvoke(action);
      else
        action();
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    public static void SafeInvoke(this Control uiElement, Action action)
    {
      if (uiElement == null || uiElement.IsDisposed || action == null || !uiElement.IsHandleCreated)
        return;
      if (uiElement.InvokeRequired)
        uiElement.Invoke(action);
      else
        action();
      ProcessUIElements();
    }

    /// <summary>
    ///   Extensions Methods to Simplify WinForms Thread Invoking, start the action synchrony
    /// </summary>
    /// <param name="uiElement">Type of the Object that will get the extension</param>
    /// <param name="action">A delegate for the action</param>
    public static void SafeInvokeNoHandleNeeded(this Control uiElement, Action action)
    {
      if (uiElement == null || uiElement.IsDisposed || action == null)
        return;
      if (uiElement.InvokeRequired)
        uiElement.Invoke(action);
      else
        action();
    }

    public static WindowState StoreWindowState(this Form form, int customInt = int.MinValue, string customText = "")
    {
      try
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
        return new WindowState { Left = windowPosition.Left, Top = windowPosition.Top, Height = windowPosition.Height, Width = windowPosition.Width, State = (int)windowState, CustomInt = customInt, CustomText = customText };
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    ///   Updates the list view column format.
    /// </summary>
    /// <param name="columnFormat">The column format.</param>
    /// <param name="listView">The list view.</param>
    public static void UpdateListViewColumnFormat(this ListView listView, ICollection<Column> columnFormat)
    {
      if (listView == null || listView.IsDisposed)
        return;
      if (listView.InvokeRequired)
      {
        listView.Invoke((MethodInvoker)delegate
        { listView.UpdateListViewColumnFormat(columnFormat); });
      }
      else
      {
        var oldSelItem = listView.SelectedItems;
        listView.BeginUpdate();
        listView.Items.Clear();

        foreach (var format in columnFormat)
        {
          var ni = listView.Items.Add(format.Name);
          if (oldSelItem.Count > 0)
            ni.Selected |= format.Name == oldSelItem[0].Text;
          ni.SubItems.Add(format.GetTypeAndFormatDescription());
        }

        listView.EndUpdate();
      }
    }
  }
}