/**** BEGIN LICENSE BLOCK ****

BSD 3-Clause License

Copyright (c) 2022-2023, the wind.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

**** END LICENCE BLOCK ****/

using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

// the undo/redo part

namespace File_Forge
{
    // implement this interface, and whatever state is needed, for an undo-able action
    // thread(s): 1 - the main UI one
    // persistence: none
    public interface IFFAction
    {
        IFFAction Do(IFFAction a = null);   // "a" has 2 roles: who invoked me; ActionList.Insert: FFAction.Do(a)
        IFFAction Undo(IFFAction a = null); // return the next action, or "a", or null - whatever suits your use-case
        string Text { get; set; }
        string ToolTipText { get; set; }
        bool CheckOnClick { get; set; }
        bool Checked { get; set; }
        bool Visible { get; set; } // action-only event
        bool Enabled { get; set; } // action-only event
        Image Image { get; set; }
    }

    public delegate void ActionNotify(FFAction a);

    public class FFBaseAction : IFFAction
    {
        public FFBaseAction() { _enabled = _visible = true; }
        Dictionary<object, ActionNotify> _listeners = new Dictionary<object, ActionNotify> ();
        private object _issuer;
        private FFAction _action;
        public FFAction Action { set { _action = value; } }
        public object Issuer { set { _issuer = value; } }
        public string Text { get; set; }
        public string ToolTipText { get; set; }
        public virtual bool CheckOnClick { get; set; }
        public Image Image { get; set; }
        bool _checked;
        public virtual bool Checked { get { return _checked; } set { if (value != _checked) { _checked = value; Changed (); } } }
        bool _visible;
        public virtual bool Visible { get { return _visible; } set { if (value != _visible) { _visible = value; Changed (); } } }
        bool _enabled;
        public virtual bool Enabled { get { return _enabled; } set { if (value != _enabled) { _enabled = value; Changed (); } } }
        public void Listen(object issuer, ActionNotify notification) { _listeners[issuer] = notification; }
        private void Changed() { foreach (var listener in _listeners.Where (x => x.Key != _issuer)) listener.Value (_action); }
        public virtual IFFAction Do(IFFAction a = null) { return a; }
        public virtual IFFAction Undo(IFFAction a = null) { return a; }
    }

    public sealed class FFExitAction : FFBaseAction
    {
        public FFExitAction() : base () { }
        public override IFFAction Do(IFFAction a = null) { Application.Exit (); return a; }
    }

    public sealed class FFHelpAboutAction : FFBaseAction
    {
        public FFHelpAboutAction() : base () { }
        public override IFFAction Do(IFFAction a = null) { using (var f = new FormAbout ()) f.ShowDialog (); return a; }
    }

    public sealed class FFAction // implement IFFAction, or override FFActionImpl TODO when it gets out of there
    {// the mess is because "event" won't let me filter, and there is no need to notify the issuer of the action
        private FFBaseAction _impl;
        public FFAction(FFBaseAction impl = null, string text = null)
        {
            _impl = impl ?? new FFBaseAction ();
            _impl.Action = this;
            _impl.Text = text;
        }
        public FFAction(string text, string tooltiptext = null, Image image = null)
            : this (null, text)
        {
            _impl.ToolTipText = string.IsNullOrEmpty (tooltiptext) ? text : tooltiptext;
            _impl.Image = image;
        }
        public FFAction(string text, string tooltiptext, bool checkonclick, Image image = null)
            : this (text, tooltiptext, image)
        {
            _impl.CheckOnClick = checkonclick;
        }
        public void Listen(object issuer, ActionNotify notification) { _impl.Listen (issuer, notification); }
        public IFFAction Issue(object issuer) { _impl.Issuer = issuer; return _impl; }
        public IFFAction Get(object issuer) { return Issue (issuer); }

        private void InitToolStripItem(ToolStripItem itm)
        {
            itm.Tag = this;
            itm.Text = _impl.Text;
            itm.ToolTipText = _impl.ToolTipText;
            itm.Image = _impl.Image;
            itm.Click += (a, b) => _impl.Do ();
        }

        public ToolStripButton AsToolStripButton
        {
            get
            {
                ToolStripButton btn = new ToolStripButton ();
                InitToolStripItem (btn);
                btn.CheckOnClick = _impl.CheckOnClick;
                if (btn.CheckOnClick) // if check on click, I'm triggering something, and something triggers me
                    btn.CheckedChanged += (sender, b) => { this.Issue (btn).Checked = btn.Checked; };// notify all interested
                this.Listen (btn, (aa) =>
                {
                    btn.Checked = aa.Get (btn).Checked;
                    btn.Visible = _impl.Visible;
                    btn.Enabled = _impl.Enabled;
                });//notify me
                btn.Checked = _impl.Checked;
                btn.Visible = _impl.Visible;
                btn.Enabled = _impl.Enabled;
                return btn;
            }
        }

        public ToolStripMenuItem AsToolStripMenuItem
        {
            get
            {
                ToolStripMenuItem itm = new ToolStripMenuItem ();
                InitToolStripItem (itm);
                itm.Name = itm.Text; // implicit foo.Items[key] - key is Name
                itm.CheckOnClick = _impl.CheckOnClick; // CheckOnClick is not on ToolStripItem
                if (itm.CheckOnClick)
                    itm.CheckedChanged += (sender, b) => { this.Issue (itm).Checked = itm.Checked; };
                this.Listen (itm, (aa) =>
                {
                    itm.Checked = aa.Get (itm).Checked;
                    itm.Visible = _impl.Visible;
                    itm.Enabled = _impl.Enabled;
                });
                itm.Checked = _impl.Checked;
                itm.Visible = _impl.Visible;
                itm.Enabled = _impl.Enabled;
                return itm;
            }
        }
    }// class FFAction
}
