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

using System.Linq;

namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Forms;

  /// <summary>
  ///   Helper class
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    ///   Handles a CTRL-A select all in the form.
    /// </summary>
    /// <param name="frm">The calling form</param>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
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
    ///   Gets the process display.
    /// </summary>
    /// <param name="fileSetting">The setting.</param>
    /// <param name="owner">
    ///   The owner form, in case the owner is minimized or closed this progress will do the same
    /// </param>
    /// <param name="withLogger">if set to <c>true</c> [with logger].</param>
    /// <param name="cancellationToken">A Cancellation token</param>
    /// <returns>A process display, if the stetting want a process</returns>
    public static IProcessDisplay GetProcessDisplay(
      this IFileSetting fileSetting,
      Form owner,
      bool withLogger,
      CancellationToken cancellationToken)
    {
      if (!fileSetting.ShowProgress)
        return new DummyProcessDisplay(cancellationToken);

      var processDisplay = new FormProcessDisplay(fileSetting.ToString(), withLogger, cancellationToken);
      processDisplay.Show(owner);
      return processDisplay;
    }

    public static Binding GetTextBindng(this Control ctrl)
    {
      return ctrl.DataBindings.Cast<Binding>().FirstOrDefault(bind => bind.PropertyName == "Text" || bind.PropertyName == "Value");
    }

    public static void LoadWindowState(
      this Form form,
      WindowState windowPosition,
      Action<int> setCustomValue1 = null,
      Action<string> setCustomValue2 = null)
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

      var screen = Screen.FromRectangle(
        new Rectangle(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height));
      var width = Math.Min(windowPosition.Width, screen.WorkingArea.Width);
      var height = Math.Min(windowPosition.Height, screen.WorkingArea.Height);
      var left = Math.Min(screen.WorkingArea.Right - width, Math.Max(windowPosition.Left, screen.WorkingArea.Left));
      var top = Math.Min(screen.WorkingArea.Bottom - height, Math.Max(windowPosition.Top, screen.WorkingArea.Top));

      form.DesktopBounds = new Rectangle(left, top, width, height);
      form.WindowState = (FormWindowState)windowPosition.State;
      if (windowPosition.CustomInt != int.MinValue)
        setCustomValue1?.Invoke(windowPosition.CustomInt);
      if (!string.IsNullOrEmpty(windowPosition.CustomText))
        setCustomValue2?.Invoke(windowPosition.CustomText);
    }

    /// <summary>
    ///   Handles UI elements while waiting on something
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
      UiElementInvoke(uiElement, action, TimeSpan.TicksPerSecond / 10);

      ProcessUIElements();
    }

    private static void UiElementInvoke(Control uiElement, Action action, long timeoutTicks)
    {
      if (uiElement.InvokeRequired)
      {
        var result = uiElement.BeginInvoke(action);
        result.AsyncWaitHandle.WaitOne(new TimeSpan(timeoutTicks));
        result.AsyncWaitHandle.Close();
        // uiElement.EndInvoke(result);
      }
      else
        action();
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
      UiElementInvoke(uiElement, action, TimeSpan.TicksPerSecond / 10);
    }

    /// <summary>
    ///   Show error information to a user, and logs the message
    /// </summary>
    /// <param name="from">The current Form</param>
    /// <param name="ex">the Exception</param>
    /// <param name="additionalTitle">Title Bar information</param>
    public static void ShowError(this Form from, Exception ex, string additionalTitle = "")
    {
      Logger.Warning(ex, "Error in {form} : {message}", from.GetType().Name, ex.SourceExceptionMessage());
      Cursor.Current = Cursors.Default;
#if DEBUG
      _MessageBox.ShowBig(from, ex.ExceptionMessages() + "\n\nMethod:\n" + ex.StackTrace, string.IsNullOrEmpty(additionalTitle) ? "Error" : $"Error {additionalTitle}", MessageBoxButtons.OK, MessageBoxIcon.Warning, timeout: 20);
#else
      _MessageBox.Show(
        from,
        ex.ExceptionMessages(),
        string.IsNullOrEmpty(additionalTitle) ? "Error" : $"Error {additionalTitle}",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning,
        timeout: 60);
#endif
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

        return new WindowState
        {
          Left = windowPosition.Left,
          Top = windowPosition.Top,
          Height = windowPosition.Height,
          Width = windowPosition.Width,
          State = (int)windowState,
          CustomInt = customInt,
          CustomText = customText
        };
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
        listView.Invoke((MethodInvoker)delegate { listView.UpdateListViewColumnFormat(columnFormat); });
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

    /// <summary>
    ///   Extends regular ValidateChildren with Timeout and Cancellation
    /// </summary>
    /// <param name="container">Control with validate able children</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns><c>True</c> if children where validated, <c>false</c> otherwise</returns>
    public static bool ValidateChildren(this ContainerControl container, CancellationToken cancellationToken) =>
      Task.Run(container.ValidateChildren, cancellationToken).WaitToCompleteTask(1);

    /// <summary>
    ///   Store a bound value
    /// </summary>
    /// <param name="ctrl">The control</param>
    public static void WriteBinding(this Control ctrl) => ctrl.GetTextBindng()?.WriteValue();
  }
}