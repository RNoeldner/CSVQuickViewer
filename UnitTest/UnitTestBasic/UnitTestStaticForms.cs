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
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools.Tests;

public static class UnitTestStaticForms
{
  public static void InitThreadException()
  {
    Application.ThreadException += (obj, args) => args.Exception.ToString().WriteToContext();
    AppDomain.CurrentDomain.UnhandledException += (obj, args) => args.ExceptionObject.ToString().WriteToContext();
    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
  }

  public static void ShowControl<T>(Func<T> createControl, double waitBeforeActionSeconds = 0, Action<T>? toDoControl = null)
    where T : Control
  {
    ShowForm(() => new TestForm(createControl, waitBeforeActionSeconds * 1000 + 1000), waitBeforeActionSeconds,
      f => toDoControl?.Invoke((T) f.CreatedControl));
  }

  public static void ShowControlAsync<T>(Func<T> createControl, Func<T, Task> toDoControl, double waitBeforeActionSeconds = 0.1,
    double closeAfterSeconds = .5)
    where T : Control
  {
    ShowFormAsync(() => new TestForm(createControl, closeAfterSeconds * 1000),
      async f => await toDoControl.Invoke((T) f.CreatedControl), waitBeforeActionSeconds);
  }

  public static void ShowForm<T>(Func<T> createFunc, double waitBeforeActionSeconds = 0, Action<T>? formAction = null, double waitAfterAction = 0)
    where T : Form
  {
    Extensions.RunStaThread(() =>
    {
      try
      {
        using T frm = createFunc();
        var isClosed = false;
        frm.FormClosed += (s, o) =>
          isClosed = true;

        frm.ShowInTaskbar = false;

        try
        {
          frm.Show();
          Application.DoEvents();
        }
        catch (Exception)
        {
          // ignore the form might be shown already
        }

        if (waitBeforeActionSeconds > 0 && !isClosed)
          WaitSomeTime(waitBeforeActionSeconds, CancellationToken.None);

        Application.DoEvents();

        if (!isClosed && formAction!=null)
        {
          formAction.Invoke(frm);
          Application.DoEvents();

          if (waitAfterAction > 0)
            WaitSomeTime(waitAfterAction, CancellationToken.None);
        }

        if (!isClosed)
          frm.Close();
      }
      catch (Exception e)
      {
        Logger.Error(e);
      }
    });
  }

  public static void ShowFormAsync<T>(Func<T> createFunc, Func<T, Task> toDo,
    double waitBeforeActionSeconds = 0.1)
    where T : Form
  {
    Extensions.RunStaThread(async void () =>
    {
      try
      {
        using T frm = createFunc();
        var isClosed = false;
        frm.FormClosed += (s, o) =>
          isClosed = true;

        frm.ShowInTaskbar = false;

        try
        {
          frm.Show();
        }
        catch (Exception)
        {
          // ignore the form might be shown already
        }

        if (waitBeforeActionSeconds > 0 && !isClosed)
          WaitSomeTime(waitBeforeActionSeconds, CancellationToken.None);

        if (!isClosed)
          await toDo.Invoke(frm);

        if (!isClosed)
          frm.Close();
      }
      catch (Exception e)
      {
        Logger.Error(e);
      }
    });
  }


  [DebuggerStepThrough]
  public static void WaitSomeTime(double seconds, in CancellationToken token)
  {
    if (seconds <= 0)
      return;
    var sw = Stopwatch.StartNew();
    while (sw.Elapsed.TotalSeconds < seconds && !token.IsCancellationRequested)
    {
      Application.DoEvents();
      Thread.Sleep(10);
    }
  }
}


public sealed class TestForm : Form
{
  public readonly Control CreatedControl;
  private readonly CancellationTokenSource m_CancellationTokenSource;
  private readonly Timer m_TimerAutoClose = new Timer();
  public TestForm(Func<Control>? createControl = null, double autoCloseMilliseconds = 5000)
  {
    SuspendLayout();
    m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(UnitTestStatic.Token);
    AutoScaleMode = AutoScaleMode.Dpi;
    BackColor = SystemColors.Control;
    ClientSize = new Size(895, 445);
    FormBorderStyle = FormBorderStyle.SizableToolWindow;
    Name = "TestForm";
    ShowInTaskbar = false;
    StartPosition = FormStartPosition.CenterScreen;
    // ReSharper disable once LocalizableElement
    Text = "TestForm";
    TopMost = true;
    FormClosing += TestForm_FormClosing;
    CreatedControl = createControl?.Invoke() ?? new Label();

    Text = CreatedControl.GetType().FullName;
    CreatedControl.Dock = DockStyle.Fill;
    CreatedControl.Location = new Point(0, 0);
    CreatedControl.Size = new Size(790, 790);
    Controls.Add(CreatedControl);

    ResumeLayout(false);

    if (!(autoCloseMilliseconds > 0))
      return;
    m_TimerAutoClose.Interval = autoCloseMilliseconds;
    m_TimerAutoClose.Enabled = true;
    m_TimerAutoClose.Start();
    m_TimerAutoClose.Elapsed += (sender, args) =>
    {
      if (InvokeRequired)
        BeginInvoke((MethodInvoker) Close);
      else
        Close();
    };
  }

  public CancellationToken CancellationToken => m_CancellationTokenSource.Token;
  /// <summary>
  /// Clean up any resources being used.
  /// </summary>
  /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
  protected override void Dispose(bool disposing)
  {
    try { m_CancellationTokenSource.Cancel(); } catch { /* ignore */ }
    if (disposing)
    {
      m_CancellationTokenSource.Dispose();
      if (CreatedControl is IDisposable dispControl)
        dispControl.Dispose();
    }

    base.Dispose(disposing);
  }

  private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
  {
    m_TimerAutoClose.Stop();
    m_CancellationTokenSource.Cancel();
  }
}