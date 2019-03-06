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
using System.Threading;

namespace CsvTools
{
  public class ProcessDisplayTime : DummyProcessDisplay, IProcessDisplayTime
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="DummyProcessDisplay" /> class.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public ProcessDisplayTime(CancellationToken cancellationToken) : base(cancellationToken)
    {
      TimeToCompletion = new TimeToCompletion();
    }        

    public virtual event EventHandler<int> SetMaximum;

    /// <summary>
    ///   Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>
    ///   The maximum value.
    /// </value>
    public override int Maximum
    {
      get => TimeToCompletion.TargetValue;
      set
      {
        TimeToCompletion.TargetValue = value > 1 ? value : -1;
        SetMaximum?.Invoke(this, TimeToCompletion.TargetValue);
      }
    }

    public TimeToCompletion TimeToCompletion { get; }

    /// <summary>
    ///   Sets the process.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The value.</param>
    public override void SetProcess(string text, int value = -1)
    {
      TimeToCompletion.Value = value;
      base.SetProcess(text, value);
    }
  }
}