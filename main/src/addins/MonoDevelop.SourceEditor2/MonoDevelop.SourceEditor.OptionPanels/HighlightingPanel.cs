// HighlightingPanel.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Gtk;

using MonoDevelop.Core.Gui.Dialogs;
using MonoDevelop.Core.Gui;
using Mono.TextEditor.Highlighting;
using MonoDevelop.Core;

namespace MonoDevelop.SourceEditor.OptionPanels
{
	public partial class HighlightingPanel : Gtk.Bin, IDialogPanel
	{
		ListStore styleStore = new ListStore (typeof (string), typeof (string));
		
		public HighlightingPanel()
		{
			this.Build();
			styleTreeview.AppendColumn ("", new CellRendererText (), "markup", 0);
			styleTreeview.Model = styleStore;
		}
		
		bool   wasActivated = false;
		bool   isFinished   = true;
		object customizationObject = null;
		
		public Gtk.Widget Control {
			get {
				return this;
			}
		}

		public virtual Gtk.Image Icon {
			get {
				return null;
			}
		}
		
		public bool WasActivated {
			get {
				return wasActivated;
			}
		}
		
		public virtual object CustomizationObject {
			get {
				return customizationObject;
			}
			set {
				customizationObject = value;
				OnCustomizationObjectChanged();
			}
		}
		
		public virtual bool EnableFinish {
			get {
				return isFinished;
			}
			set {
				if (isFinished != value) {
					isFinished = value;
					OnEnableFinishChanged();
				}
			}
		}
		
		public virtual bool ReceiveDialogMessage(DialogMessage message)
		{
			try {
				switch (message) {
					case DialogMessage.Activated:
						if (!wasActivated) {
							LoadPanelContents();
							wasActivated = true;
						}
						break;
					case DialogMessage.OK:
						if (wasActivated) {
							return StorePanelContents();
						}
						break;
				}
			} catch (Exception ex) {
				Services.MessageService.ShowError (ex);
			}
			
			return true;
		}
		
		string GetMarkup (string name, string description)
		{
			return String.Format ("<b>{0}</b> - {1}", name, description);
		}
		
		public virtual void LoadPanelContents()
		{
			this.enableHighlightingCheckbutton.Active = SourceEditorOptions.Options.EnableSyntaxHighlighting;
			this.enableSemanticHighlightingCheckbutton.Active = SourceEditorOptions.Options.EnableSemanticHighlighting;
			this.enableHighlightingCheckbutton.Toggled += EnableHighlightingCheckbuttonToggled;
			EnableHighlightingCheckbuttonToggled (this, EventArgs.Empty);
			styleStore.Clear ();
			TreeIter selectedIter = styleStore.AppendValues (GetMarkup (GettextCatalog.GetString ("Default"), GettextCatalog.GetString ("The default color sheme.")), "Default");
			foreach (string styleName in SyntaxModeService.Styles) {
				Mono.TextEditor.Highlighting.Style style = SyntaxModeService.GetColorStyle (null, styleName);
				TreeIter iter = styleStore.AppendValues (GetMarkup (GettextCatalog.GetString (style.Name), GettextCatalog.GetString (style.Description)), style.Name);
				if (style.Name == SourceEditorOptions.Options.ColorSheme)
					selectedIter = iter;
			}
			styleTreeview.Selection.SelectIter (selectedIter); 
		}
		
		void EnableHighlightingCheckbuttonToggled (object sender, EventArgs e)
		{
			this.enableSemanticHighlightingCheckbutton.Sensitive = this.enableHighlightingCheckbutton.Active;
		}
		
		public virtual bool StorePanelContents()
		{
			SourceEditorOptions.Options.EnableSyntaxHighlighting = this.enableHighlightingCheckbutton.Active;
			SourceEditorOptions.Options.EnableSemanticHighlighting = this.enableSemanticHighlightingCheckbutton.Active;
			TreeIter selectedIter;
			if (styleTreeview.Selection.GetSelected (out selectedIter)) {
				SourceEditorOptions.Options.ColorSheme = (string)this.styleStore.GetValue (selectedIter, 1);
			}
			return true;
		}
		
		protected virtual void OnEnableFinishChanged()
		{
			if (EnableFinishChanged != null) {
				EnableFinishChanged(this, null);
			}
		}
		protected virtual void OnCustomizationObjectChanged()
		{
			if (CustomizationObjectChanged != null) {
				CustomizationObjectChanged(this, null);
			}
		}
		
		public event EventHandler CustomizationObjectChanged;
		public event EventHandler EnableFinishChanged;
	}
}
