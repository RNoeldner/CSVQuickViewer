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

using log4net;
using Pri.LongPath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class Extensions
  {
    public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public static void ShowError(this Form from, Exception ex, string additionalTitle = "")
    {
      Log.Warn($"Issue in UI {nameof(from)}", ex);
      Cursor.Current = Cursors.Default;
      MessageBox.Show(from, ex.ExceptionMessages(), string.IsNullOrEmpty(additionalTitle) ? "Error" : $"Error {additionalTitle}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static void TimeOutWait(Func<bool> whileTrue, int millisecondsSleep = 0, double timeoutMinutes = 5.0, CancellationToken cancellationToken = default(CancellationToken))
    {
      var start = DateTime.Now;
      while (!cancellationToken.IsCancellationRequested && whileTrue())
      {
        if ((DateTime.Now - start).TotalMinutes > timeoutMinutes)
          throw new ApplicationException($"Waited longer than {timeoutMinutes} minutes, assuming something is wrong");
        ProcessUIElements(millisecondsSleep);
      }
    }

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
    ///   Handles a CTRL-A select all in the form.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">
    ///   The <see cref="KeyEventArgs" /> instance containing the event data.
    /// </param>
    public static void CtrlA(this Form frm, object sender, KeyEventArgs e)
    {
      if (e == null || !e.Control || e.KeyCode.ToString() != "A") return;
      var tb = sender as TextBox;
      if (sender == frm) tb = frm.ActiveControl as TextBox;

      if (tb != null)
      {
        tb.SelectAll();
        return;
      }

      var lv = sender as ListView;
      if (sender == frm) lv = frm.ActiveControl as ListView;

      if (lv == null || !lv.MultiSelect) return;
      foreach (ListViewItem item in lv.Items)
        item.Selected = true;
    }

    public static void WriteBinding(this Control ctrl)
    {
      var bind = ctrl.GetTextBindng();
      bind?.WriteValue();
    }

    public static bool DeleteFileQuestion(this string fileName, bool ask)
    {
      if (!FileSystemUtils.FileExists(fileName)) return true;
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

      if (_MessageBox.Show(null,
            $"The file {disp} does exist, do you want to remove it?", "File", MessageBoxButtons.YesNo) == DialogResult.Yes)
      {
        retry:
        try
        {
          File.Delete(fileName);
          return true;
        }
        catch (Exception ex)
        {
          if (_MessageBox.Show(null,
                $"The file {disp} could not be deleted.\n{ex.ExceptionMessages()}", "File",
                MessageBoxButtons.RetryCancel, MessageBoxIcon.Question) == DialogResult.Retry)
            goto retry;
        }
      }
      return false;
    }

    /// <summary>
    /// Gets the encrypted passphrase of a setting, if needed it will open a windows form
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <returns></returns>
    /// <exception cref="ApplicationException">
    /// The private key for decryption has not been setup
    /// or
    /// A passphrase is needed for decryption.
    /// </exception>
    public static string GetEncryptedPassphraseOpenForm(this IFileSetting setting)
    {
      if (ApplicationSetting.ToolSetting?.PGPInformation?.PrivateKeys?.IsEmpty() ?? true)
        throw new ApplicationException("The private key for decryption has not been setup");

      if (!string.IsNullOrEmpty(setting?.Passphrase))
        return setting.Passphrase;

      if (!string.IsNullOrEmpty(ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase))
        return ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase;

      // Need to enter Passphrase
      using (var frm = new FormPassphrase())
      {
        if (frm.ShowDialog() == DialogResult.OK)
          return frm.EncryptedPassphrase;
        else
          throw new ApplicationException("A passphrase is needed for decryption.");
      }
    }

    public static string GetProcessDisplayTitle(this IFileSetting fileSetting)
    {
      return string.IsNullOrEmpty(fileSetting.ID)
        ? FileSystemUtils.GetShortDisplayFileName(fileSetting.FileName, 80)
        : fileSetting.ID;
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
      if (!fileSetting.ShowProgress) return new DummyProcessDisplay(cancellationToken);

      if (withLogger)
      {
        var processDisplay = new FormProcessDisplayLogger(fileSetting.GetProcessDisplayTitle(), cancellationToken);
        processDisplay.Show(owner);
        return processDisplay;
      }
      else
      {
        var processDisplay = new FormProcessDisplay(fileSetting.GetProcessDisplayTitle(), cancellationToken);
        processDisplay.Show(owner);
        return processDisplay;
      }
    }

    public static Binding GetTextBindng(this Control ctrl)
    {
      foreach (Binding bind in ctrl.DataBindings)
        if (bind.PropertyName == "Text" || bind.PropertyName == "Value")
          return bind;
      return null;
    }

    public static void LoadWindowState(this Form form, WindowState windowPosition)
    {
      if (windowPosition == null || windowPosition.Width == 0 || windowPosition.Height == 0)
        return;
      form.StartPosition = FormStartPosition.Manual;

      var screen = Screen.FromRectangle(new Rectangle(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height));
      var width = Math.Min(windowPosition.Width, screen.WorkingArea.Width);
      var height = Math.Min(windowPosition.Height, screen.WorkingArea.Height);
      var left = Math.Min(screen.WorkingArea.Right - width, Math.Max(windowPosition.Left, screen.WorkingArea.Left));
      var top = Math.Min(screen.WorkingArea.Bottom - height, Math.Max(windowPosition.Top, screen.WorkingArea.Top));

      form.DesktopBounds = new Rectangle(left, top, width, height);
      form.WindowState = (FormWindowState)windowPosition.State;
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

    public static WindowState StoreWindowState(this Form form)
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
        return new WindowState { Left = windowPosition.Left, Top = windowPosition.Top, Height = windowPosition.Height, Width = windowPosition.Width, State = (int)windowState };
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
          if (oldSelItem.Count > 0) ni.Selected |= format.Name == oldSelItem[0].Text;
          ni.SubItems.Add(format.GetTypeAndFormatDescription());
        }

        listView.EndUpdate();
      }
    }

    /// <summary>
    ///   Writes the file ans displays performance information
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="showSummary">
    ///   Flag indicating that a message Box should be displayed. If <c>true</c> a message box will be
    ///   shown
    /// </param>
    /// <param name="cancellationToken">A CancellationToken</param>
    /// <param name="fileSettingSourcesCurrent">The file setting sources current.</param>
    /// <param name="settingLaterThanSources">The setting later than sources.</param>
    /// <param name="ask">if set to <c>true</c> [ask].</param>
    /// <returns>
    ///   Number of record written to file
    /// </returns>
    public static long WriteFileWithInfo(this IFileSetting fileSetting, bool showSummary,
      FileSettingChecker fileSettingSourcesCurrent,
      bool ask, bool onlyOlder, CancellationToken cancellationToken)
    {
      if (fileSetting == null)
        return 0;

      long written = 0;

      var fi = FileSystemUtils.FileInfo(fileSetting.FullPath);
      var dir = FileSystemUtils.GetDirectoryName(fi.FullName);
      if (!FileSystemUtils.DirectoryExists(dir))
        if (_MessageBox.Show(null,
              $"The directory {dir.RemovePrefix()} does not exist, should it be created?",
              "Directory", MessageBoxButtons.OKCancel,
              MessageBoxIcon.Question) == DialogResult.OK)
          FileSystemUtils.CreateDirectory(dir);
        else
          return 0;

      var fileInfo = new FileInfo(fileSetting.FullPath);
      if (fileInfo.Exists)
      {
        fileSetting.FileLastWriteTimeUtc = fi.LastWriteTimeUtc;
      }

      var stringBuilder = new System.Text.StringBuilder();
      using (var processDisplay = fileSetting.GetProcessDisplay(null, true, cancellationToken))
      {
        fileSettingSourcesCurrent?.Invoke(fileSetting, processDisplay);

        if (onlyOlder && fileSetting.SettingLaterThanSources(processDisplay.CancellationToken))
          return 0;

        fileSetting.FullPath.DeleteFileQuestion(ask);

        var errors = new RowErrorCollection(50);
        var writer = fileSetting.GetFileWriter(processDisplay.CancellationToken);
        writer.ProcessDisplay = processDisplay;
        writer.Warning += errors.Add;
        written = writer.Write();

        var hasIssues = !string.IsNullOrEmpty(writer.ErrorMessage) || (errors.CountRows > 0);

        if (showSummary || hasIssues)
        {
          fi = FileSystemUtils.FileInfo(fileSetting.FullPath);

          // if all source settings are file settings, get the latest file time and set this fileTime
          var latest = DateTime.MinValue;
          fileSetting.GetSourceFileSettings(delegate (IFileSetting setting)
          {
            if (!(setting is IFileSettingRemoteDownload))
            {
              if (latest < setting.FileLastWriteTimeUtc)
                latest = setting.FileLastWriteTimeUtc;
            }
            else
            {
              var fiSrc = FileSystemUtils.FileInfo(setting.FullPath);
              if (fiSrc.Exists && latest < fiSrc.LastWriteTimeUtc)
                latest = fiSrc.LastWriteTimeUtc;
            }
            return false;
          }, cancellationToken);
          stringBuilder.Append($"Finished writing file\r\rRecords: {written:N0}\rFile size: {fi.Length / 1048576.0:N} MB");
          if (latest < DateTime.MaxValue && latest > DateTime.MinValue)
          {
            stringBuilder.Append($"\rTime adjusted to latest source file: {latest.ToLocalTime():D}");
            fi.LastWriteTimeUtc = latest;
            fileSetting.FileLastWriteTimeUtc = latest;
          }

          if (hasIssues)
            stringBuilder.Append("\rIssues:\r");

          if (!string.IsNullOrEmpty(writer.ErrorMessage))
            stringBuilder.Append(writer.ErrorMessage);

          if (errors.CountRows > 0)
            stringBuilder.Append(errors.DisplayByRecordNumber);
        }
      }
      if (stringBuilder.Length > 0)
        _MessageBox.Show(null, stringBuilder.ToString(), FileSystemUtils.GetShortDisplayFileName(fileSetting.FileName, 80), MessageBoxButtons.OK);

      return written;
    }
  }
}