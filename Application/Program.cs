/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com/
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
using System.Diagnostics.CodeAnalysis;

using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  internal static class Program
  {
    internal const string cPhrase = "R@pHa€l";

    /// <summary>
    ///   Handles the ThreadException event of the Application control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="ThreadExceptionEventArgs" /> instance containing the event data.
    /// </param>
    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => UnhandledException(e.Exception);

    /// <summary>
    ///   Handles the UnhandledException event of the CurrentDomain control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="UnhandledExceptionEventArgs" /> instance containing the event data.
    /// </param>
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => UnhandledException((Exception)e.ExceptionObject);

    /// <summary>
    ///   The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      Logger.Configure("CSVQuickViewer.log", Logger.Level.Info);
      Application.ThreadException += Application_ThreadException;
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
      Logger.Debug("Application start…");
      var fileName = string.Empty;

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      // read the command line parameter
      if (args.Length >= 1)
        fileName = args[0];
#if NETCOREAPP30
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif

      Application.Run(new FormMain(fileName));
      Application.Exit();
    }

    /// <summary>
    ///   Handle's any not yet handled exception
    /// </summary>
    /// <param name="ex">The exception.</param>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static void UnhandledException(Exception ex)
    {
      Logger.Error(ex, "Unhandled Exception");
      var message = $"{ex.GetType()}\n\n{ex.ExceptionMessages()}\nStack Trace:\n{ex.StackTrace}";
#if DEBUG
      System.Diagnostics.Debug.Assert(false, @"Unhandled Exception", message);
#else
      if (MessageBox.Show(message, @"Unhandled Exception", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop) ==
          DialogResult.Abort)
        Application.Exit();
#endif
    }
  }
}