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
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   A Po pup Form to display progress information
  /// </summary>
  public class FormProcessDisplayLogger : FormProcessDisplay
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public FormProcessDisplayLogger(string windowTitle) : this(windowTitle, CancellationToken.None)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormProcessDisplayLogger" /> class.
    /// </summary>
    /// <param name="windowTitle">The description / form title</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public FormProcessDisplayLogger(string windowTitle, CancellationToken cancellationToken) : base(windowTitle, cancellationToken)
    {
      InitializeComponent();
      Progress += delegate (object sender, ProgressEventArgs e) { Log.Info(e.Text); };
    }

    public FormProcessDisplayLogger() : this(string.Empty, default(CancellationToken))
    {
    }

    private void InitializeComponent()
    {
      SuspendLayout();
      Width = 400;
      Height = 300;

      var logger = new LoggerDisplay
      {
        Dock = DockStyle.Fill,
        Multiline = true,
        // ScrollBars = ScrollBars.Both,
        TabIndex = 8
      };
      tableLayoutPanel.SetColumnSpan(logger, 2);
      tableLayoutPanel.Controls.Add(logger, 0, 3);
      ResumeLayout(false);
    }
  }
}