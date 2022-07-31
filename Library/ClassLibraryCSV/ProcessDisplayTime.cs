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

namespace CsvTools
{
  [DebuggerStepThrough]
  public sealed class ProcessDisplayTime : CustomProcessDisplay, IProcessDisplayTime
  {
    public ProcessDisplayTime() =>
      TimeToCompletion = new TimeToCompletion();

    public Action<ProgressWithTimeEventArgs> ProgressTime { get; set; }= delegate  { };

    /// <inheritdoc cref="IProcessDisplayTime.Maximum" />
    public long Maximum
    {
      get => TimeToCompletion.TargetValue;
      set
      {
        TimeToCompletion.TargetValue = value > 1 ? value : 1;
      }
    }

    public TimeToCompletion TimeToCompletion { get; }

    public override void SetProcess(string text, long value)
    {
      base.SetProcess(text, value);

      TimeToCompletion.Value = value;
      try
      {
        ProgressTime.Invoke(new ProgressWithTimeEventArgs(text, value, TimeToCompletion.EstimatedTimeRemaining, TimeToCompletion.Percent));
      }
      catch
      {
        // ignore
      }
    }
  }
}