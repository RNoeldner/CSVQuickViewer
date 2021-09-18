// WARNING
//
// This file has been generated automatically by Rider IDE
//   to store outlets and actions made in Xcode.
// If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace QuickViewer.MacOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSScrollView DocumentEditor { get; set; }

		[Outlet]
		public AppKit.NSTextView TextView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (TextView != null) {
				TextView.Dispose ();
				TextView = null;
			}

			if (DocumentEditor != null) {
				DocumentEditor.Dispose ();
				DocumentEditor = null;
			}

		}
	}
}
