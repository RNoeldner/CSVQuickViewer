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
  public class CustomProcessDisplay : Progress<ProgressEventArgs>, IProcessDisplay
  {
    
    public Action<ProgressEventArgs> Progress { get; set; } = delegate {  };

    public virtual void SetProcess(string text, long value)
    {
      base.OnReport(new ProgressEventArgs(text, value));
      try
      {
        Progress?.Invoke(new ProgressEventArgs(text, value));
      }
      catch
      {
        // ignore all errors in process display
      }
    }
  }
}