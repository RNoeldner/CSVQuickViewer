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
using System.Windows.Forms;

namespace CsvTools
{
  public static class Program
  {
    /// <summary>
    ///   The main entry point for the application.
    /// </summary>
#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

    [STAThread]
    public static void Main(string[] args)
    {
      Application.ThreadException += (s, e) => UnhandledException(e.Exception);
      AppDomain.CurrentDomain.UnhandledException += (s, e) => UnhandledException((Exception) e.ExceptionObject);

      var fileName = string.Empty;
#if NET5_0_OR_GREATER
      Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      WinAppLogging.Init();
      // ReSharper disable once CoVariantArrayConversion
      Logger.Debug("Application started {@args}", args);

      // read the command line parameter
      if (args.Length == 1)
        fileName = args[0];

      // in case we have multiple arguments assume the path was split at space
      else if (args.Length > 1)
        fileName = args.Join(' ');

      var viewSettings = ViewSettingHelper.LoadViewSettingsAsync().GetAwaiter().GetResult();

#if NET5_0_OR_GREATER
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif

      var frm = new FormMain(viewSettings);
      frm.Show();

      if (FileSystemUtils.FileExists(fileName))
        // Start this but do not wait for completion.
        // Can not use await as this need async main (not supported for Windows Form executable)
        // If using GetAwaiter().GetResult() it gets stuck when opening the progress bar.
#pragma warning disable CS4014
        frm.LoadCsvOrZipFileAsync(FileSystemUtils.GetFullPath(fileName), frm.CancellationToken);
#pragma warning restore CS4014
      Application.Run(frm);
    }

    /// <summary>
    ///   Handle's any not yet handled exception
    /// </summary>
    /// <param name="ex">The exception.</param>
    private static void UnhandledException(Exception ex)
    {
      switch (ex)
      {
        // Most likely disposing something which is still being used by a different thread its very
        // hard to track down as the stack frame is not useful, in 99% its updating progress or UI
        case ObjectDisposedException _ when ex.HResult == -2146232798:
        case InvalidOperationException _ when ex.HResult == -2146233079:
          return;
      }

      Logger.Error(ex, "Not handled Exception");
      var message = $"{ex.GetType()}\n\n{ex.ExceptionMessages()}";
#if DEBUG
      System.Diagnostics.Debug.Assert(false, @"Not handled Exception", message);
#else
      if (MessageBox.Show(message, @"Not handled Exception", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop)
          == DialogResult.Abort)
        Application.Exit();
#endif
    }
  }
}