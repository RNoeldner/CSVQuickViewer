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
using System.Reflection;
using System.Windows.Forms;
using log4net;
using log4net.Repository.Hierarchy;

namespace CsvTools
{
  public class LoggerDisplay : RichTextBox
  {
    private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly LogAppenderTextBox m_LogAppenderTextBox;

    public LoggerDisplay()
    {
      m_LogAppenderTextBox = new LogAppenderTextBox(this);
      Multiline = true;
      KeyUp += base.FindForm().CtrlA;

      try
      {
        var hierarchy = (Hierarchy)LogManager.GetRepository();
        m_LogAppenderTextBox.Threshold = log4net.Core.Level.Debug;
        m_LogAppenderTextBox.ActivateOptions();
        hierarchy.Root.AddAppender(m_LogAppenderTextBox);
        hierarchy.Configured = true;
      }
      catch (Exception ex)
      {
        m_Log.Error("Error setting the log file", ex);
      }
    }

    public log4net.Core.Level Threshold
    {
      get => m_LogAppenderTextBox.Threshold;
      set
      {
        m_LogAppenderTextBox.Threshold = value;
        m_LogAppenderTextBox.ActivateOptions();
      }
    }

    public new void Clear() => m_LogAppenderTextBox.Clear();

    public void Pause() => m_LogAppenderTextBox.Threshold = log4net.Core.Level.Off;

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        try
        {
          m_LogAppenderTextBox.Close();
          var hierarchy = (Hierarchy)LogManager.GetRepository();
          hierarchy.Root.RemoveAppender(m_LogAppenderTextBox);
        }
        catch
        {
          // ignore all
        }
      }

      base.Dispose(disposing);
    }
  }
}