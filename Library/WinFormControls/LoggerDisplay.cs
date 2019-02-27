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
using log4net.Repository.Hierarchy;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace CsvTools
{
  public class LoggerDisplay : RichTextBox
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly LogAppenderTextBox m_LogAppenderTextBox;
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        var hierarchy = (Hierarchy)LogManager.GetRepository();
        hierarchy.Root.RemoveAppender(m_LogAppenderTextBox);
        m_LogAppenderTextBox.Close();
      }

      base.Dispose(disposing);
    }

    public void Clear()
    {
      m_LogAppenderTextBox.Clear();
    }


    public void BeginSection(string text)
    {
      AppendText($"\n{text}\n");
      Extensions.ProcessUIElements();
    }

    public void Pause()
    {
      m_LogAppenderTextBox.Threshold = log4net.Core.Level.Off;
    }

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
        Log.Error("Error setting the log file", ex);
      }
    }
  }
}