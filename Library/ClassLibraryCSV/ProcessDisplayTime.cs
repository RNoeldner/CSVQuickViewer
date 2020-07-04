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
using System.Diagnostics;
using System.Threading;

namespace CsvTools
{
  [DebuggerStepThrough]
  public class ProcessDisplayTime : IProcessDisplayTime
  {
    public ProcessDisplayTime(CancellationToken token) => CancellationToken = token;

    public CancellationToken CancellationToken { get; }

    /// <summary>
    ///   Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>
    ///   The maximum value.
    /// </value>
    public long Maximum
    {
      get => TimeToCompletion.TargetValue;
      set
      {
        TimeToCompletion.TargetValue = value > 1 ? value : 1;
        SetMaximum?.Invoke(this, TimeToCompletion.TargetValue);
      }
    }

    public void Dispose()
    {
    }

    public event EventHandler<ProgressEventArgs> Progress;
    public event EventHandler<ProgressEventArgsTime> ProgressTime;

    public bool LogAsDebug { get; set; }

    public void Cancel()
    {
    }

    public void SetProcess(object sender, ProgressEventArgs e)
    {
      if (e == null)
        return;
      Handle(sender, e.Text, e.Value, e.Log);
    }

    public string Title { get; set; }

    public void SetProcess(string text, long value, bool log) => Handle(this, text, value, log);

    public TimeToCompletion TimeToCompletion { get; } = new TimeToCompletion();
    public event EventHandler<long> SetMaximum;

    private void Handle(object sender, string text, long value, bool log)
    {
      TimeToCompletion.Value = value;
      Progress?.Invoke(sender, new ProgressEventArgs(text, value, log));
      ProgressTime?.Invoke(sender,
        new ProgressEventArgsTime(text, value, TimeToCompletion.EstimatedTimeRemaining, TimeToCompletion.Percent));
    }
  }
}