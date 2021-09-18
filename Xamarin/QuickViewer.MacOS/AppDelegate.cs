// /*
//  * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
//  *
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
//  * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
//  * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
//  *
//  * You should have received a copy of the GNU Lesser Public License along with this program.
//  * If not, see http://www.gnu.org/licenses/ .
//  *
//  */
using AppKit;
using Foundation;
using System;
using System.IO;

namespace QuickViewer.MacOS
{
  [Register("AppDelegate")]
  public class AppDelegate : NSApplicationDelegate
  {
    public AppDelegate()
    {
    }

    public override void DidFinishLaunching(NSNotification notification)
    {
      // Insert code here to initialize your application
    }

    [Export("openDocument:")]
    void OpenDialog(NSObject sender)
    {
      var dlg = NSOpenPanel.OpenPanel;
      dlg.CanChooseFiles = true;
      dlg.CanChooseDirectories = false;
      dlg.AllowsOtherFileTypes = true;
      
      if (dlg.RunModal() == 1)
      {
        var url = dlg.Urls[0];
        if (url != null)
        {         
          OpenFile(url);
        }
      }
    }

    public override bool OpenFile(NSApplication sender, string filename)
    {
      // Trap all errors
      try
      {
        filename = filename.Replace(" ", "%20");
        var url = new NSUrl("file://"+filename);
        
        return OpenFile(url);
      }
      catch
      {
        return false;
      }
    }


    #region Private Methods
    private bool OpenFile(NSUrl url)
    {
      var good = false;

      // Trap all errors
      try
      {
        var path = url.Path;
        NSDocumentController.SharedDocumentController.NoteNewRecentDocumentURL(url);
        // Is the file already open?
        //for (var n = 0; n<NSApplication.SharedApplication.Windows.Length; ++n)
        //{
        //  var content = NSApplication.SharedApplication.Windows[n].ContentViewController as ViewController;
        //  if (content != null && path == content.FilePath)
        //  {
        //    // Bring window to front
        //    NSApplication.SharedApplication.Windows[n].MakeKeyAndOrderFront(this);
        //    return true;
        //  }
        //}
        // Add document to the Open Recent menu
        NSDocumentController.SharedDocumentController.NoteNewRecentDocumentURL(url);
        
        // Get new window
        var storyboard = NSStoryboard.FromName("Main", null);
        var controller = storyboard.InstantiateControllerWithIdentifier("DocumentWindowController") as NSWindowController;

        // Display
        controller.ShowWindow(this);
        controller.WindowTitleForDocumentDisplayName(Path.GetFileName(path));
        var viewController = controller.ContentViewController as ViewController;
        // Load the text into the window
        
        // viewController.TextView.TextStorage.SetString(File.ReadAllText(path));
        
        //viewController.SetLanguageFromPath(path);
        //viewController.SetLanguageFromPath(path);
        //viewController.View.Window.SetTitleWithRepresentedFilename(path.GetFileName(path));
        //viewController.View.Window.RepresentedUrl = url;

        // Add document to the Open Recent menu
        NSDocumentController.SharedDocumentController.NoteNewRecentDocumentURL(url);

        // Make as successful
        good = true;
      }
      catch (Exception ex)
      { 
        var alert = new NSAlert()
        {
          AlertStyle = NSAlertStyle.Warning,
          InformativeText = $"{ex.Message}",
          MessageText = "Error"
        };
        alert.RunModal();
        // Mark as bad file on error
        good = false;
      }

      // Return results
      return good;
    }
    #endregion

    public override void WillTerminate(NSNotification notification)
    {
      // Insert code here to tear down your application
    }
  }
}
