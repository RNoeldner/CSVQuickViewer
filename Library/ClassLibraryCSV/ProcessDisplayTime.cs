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
  public class ProcessDisplayTime : CustomProcessDisplay, IProcessDisplayTime
  {
    public ProcessDisplayTime(CancellationToken token)
      : base(token) =>
      TimeToCompletion = new TimeToCompletion();

    public event EventHandler<ProgressEventArgsTime>? ProgressTime;

    public event EventHandler<long>? SetMaximum;

    /// <summary>
    ///   Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>The maximum value.</value>
    public override long Maximum
    {
      get => TimeToCompletion.TargetValue;
      set
      {
        TimeToCompletion.TargetValue = value > 1 ? value : 1;
        SetMaximum?.Invoke(this, TimeToCompletion.TargetValue);
      }
    }

    public TimeToCompletion TimeToCompletion { get; }

    protected override void Handle(in object? sender, string text, long value, bool log)
    {
      base.Handle(sender, text, value, log);
      ProgressTime?.Invoke(
        sender,
        new ProgressEventArgsTime(text, value, TimeToCompletion.EstimatedTimeRemaining, TimeToCompletion.Percent));
      TimeToCompletion.Value = value;
    }
  }
}