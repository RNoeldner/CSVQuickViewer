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

namespace CsvTools.Tests
{
  public class MockProgress : Progress<ProgressInfo>
  {
    private bool m_Disposed;
    private bool m_Visible = true;
    public string Text = string.Empty;

    public CancellationToken CancellationToken => UnitTestStatic.Token;

    public virtual void Dispose()
    {
      if (!m_Disposed)
      {
        m_Visible = true;
        m_Disposed = true;
      }
    }

    public void Cancel()
    {
    }

    public event EventHandler? ProgressStopEvent;

    public void Close()
    {
      m_Visible = !m_Visible;
      m_Disposed = true;
      ProgressStopEvent?.Invoke(this, null);
    }

  }
}