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
using System;

using AppKit;
using Foundation;

namespace QuickViewer.MacOS
{
  [Register("Document")]
  public class Document : NSDocument
  {
    public Document(IntPtr handle) : base(handle)
    {
      // Add your subclass-specific initialization here.
    }

    public override void WindowControllerDidLoadNib(NSWindowController windowController)
    {
      base.WindowControllerDidLoadNib(windowController);
      // Add any code here that needs to be executed once the windowController has loaded the document's window.
    }

    [Export("autosavesInPlace")]
    public static bool AutosaveInPlace()
    {
      return true;
    }

    public override void MakeWindowControllers()
    {
      // Override to return the Storyboard file name of the document.
      AddWindowController((NSWindowController) NSStoryboard.FromName("Main", null).InstantiateControllerWithIdentifier("Document Window Controller"));
    }

    public override NSData GetAsData(string typeName, out NSError outError)
    {
      // Insert code here to write your document to data of the specified type. 
      // If outError != NULL, ensure that you create and set an appropriate error when returning nil.
      throw new NotImplementedException();
    }

    public override bool ReadFromData(NSData data, string typeName, out NSError outError)
    {
      // Insert code here to read your document from the given data of the specified type. 
      // If outError != NULL, ensure that you create and set an appropriate error when returning NO.
      throw new NotImplementedException();
    }
  }
}
