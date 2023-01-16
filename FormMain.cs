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

#define WIND_GL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace File_Forge
{
    public partial class FormMain : Form
    {
        private bool _init_failed = false;

        public FormMain()
        {
            InitializeComponent ();

            /*this.FormClosing += (a, b) =>
            {
                b.Cancel = null != _bw && _bw.ThreadState != System.Threading.ThreadState.Stopped;
                if (true == b.Cancel)
                {
                    if (DialogResult.Yes != MessageBox.Show (text: "I'm working. Stop?", caption: "Please confirm", buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Question))
                        return;
                    lock (_bw_stop_lock) _bw_stop_signal = true;
                    while (_bw.ThreadState != System.Threading.ThreadState.Stopped && _bw_stop_signal)
                    {
                        Application.DoEvents ();
                        Thread.Sleep (1);
                    }
                    b.Cancel = false;
                }
            };*/
            try // prevent "VS" segfaults
            {
                this.Text = Application.ProductName + " " + Application.ProductVersion;
                this.Size = new Size (800, 600);
                this.StartPosition = FormStartPosition.CenterScreen;
                SetWindowIcon ();
                InitDefaults ();
            }// try
            catch (Exception exc)
            {
                _init_failed = true;
                Wind.Log.WLog.Err (exc.ToString ()); // this will hardly work at design time; "log4net" will take care of it
            }
        }// FormMain()

        private DockPanel _dock_panel;
        private MenuStrip _mm; // main menu
        private StatusStrip _sb; // status bar
        private ToolStripStatusLabel _sb_label;
        private ToolStripPanel _tb; // tool bar - 1 for now TODO 4 toolbars
        private ToolStrip _dock_forms;
        private const string MI_FILE = "&File";
        private const string MI_FILE_EXIT = "E&xit";
        private const string MI_EDIT = "&Edit";
        private const string MI_VIEW = "&View";
        private const string MI_RENDER = "&Render";
        private const string MI_HELP = "&Help";
        private const string MI_HELP_ABOUT = "&About";
        private Dictionary<string, FFAction> _al = new Dictionary<string, FFAction> () // action list
        {
            {MI_FILE, new FFAction(MI_FILE)}, {MI_EDIT, new FFAction(MI_EDIT)}, {MI_VIEW, new FFAction(MI_VIEW)},
            {MI_RENDER, new FFAction(MI_RENDER)}, {MI_HELP, new FFAction(MI_HELP)},
            {MI_FILE_EXIT, new FFAction(new FFExitAction(), MI_FILE_EXIT)},
            {MI_HELP_ABOUT, new FFAction(new  FFHelpAboutAction(), MI_HELP_ABOUT)}
        };
        private List<string> _mm_actions = new List<string> () { MI_FILE, MI_EDIT, MI_VIEW, MI_RENDER, MI_HELP };
        public static List<FFBaseDockContent> DockForms = new List<FFBaseDockContent> ();

        private void InitDefaults()
        {
            // 1. dock panel site; the order of init is relevant, so they don't overlap, or be out of order
            _dock_panel = new DockPanel ();
            _dock_panel.Parent = this;
            _dock_panel.Dock = DockStyle.Fill;
            _dock_panel.DocumentStyle = DocumentStyle.DockingWindow;
            _dock_panel.BorderStyle = BorderStyle.FixedSingle;

            // 2. toolbar
            _tb = new ToolStripPanel ();
            _tb.Parent = this;
            _tb.Dock = DockStyle.Top;
            _dock_forms = new FFToolStrip ();
            _tb.Join (_dock_forms);

            // 3. main menu
            this.MainMenuStrip = _mm = new FFMenuStrip ();
            _mm.Parent = this; // its the this.MainMenuStrip you know

            // 4. statusbar
            _sb = new StatusStrip ();
            _sb.Parent = this;
            _sb_label = new ToolStripStatusLabel ("Hi");
            _sb.Items.Add (_sb_label);
            _sb_label.Spring = true;
            //_sb_label.BackColor = Color.Navy; _sb_label.ForeColor = Color.Yellow;
            _sb_label.TextAlign = ContentAlignment.MiddleLeft;

            // main menu
            foreach (var mm_item in _mm_actions) _mm.Items.Add (_al[mm_item].AsToolStripMenuItem);
            (_mm.Items[MI_FILE] as ToolStripMenuItem).DropDownItems.Add (_al[MI_FILE_EXIT].AsToolStripMenuItem);
            (_mm.Items[MI_HELP] as ToolStripMenuItem).DropDownItems.Add (_al[MI_HELP_ABOUT].AsToolStripMenuItem);

            // standard dock panels; create them (each one registers with FormMain.DockForms) prior the next enumeration
            new HexEditor.FFHexEditor ();
            foreach (var dp in FormMain.DockForms)
            {
                var action = new FFAction (dp.Text, tooltiptext: "Show/Hide \"" + dp.Text + "\"", checkonclick: true);
                (_mm.Items[MI_VIEW] as ToolStripMenuItem).DropDownItems.Add (action.AsToolStripMenuItem);

                dp.Tag = action;
                dp.VisibleChanged += (sender, b) =>
                {
                    if (dp.DockState == DockState.DockBottomAutoHide
                        || dp.DockState == DockState.DockLeftAutoHide
                        || dp.DockState == DockState.DockRightAutoHide
                        || dp.DockState == DockState.DockTopAutoHide) return;
                    action.Issue (dp).Checked = dp.Visible;
                };
                action.Listen (dp, (aa) => { if (aa.Get (dp).Checked) dp.Show (); else dp.Hide (); });

                dp.DockPanel = _dock_panel;
                _dock_forms.Items.Add (action.AsToolStripButton);
            }

            /*var gl_test = new Wind.Controls.WindGL.WindGLForm (); TODO it can't extend DockContent
            gl_test.DockPanel = _dock_panel;
            gl_test.Show ();*/

            //this.Load += Form1_Load;
        }// InitDefaults()

        private void SetWindowIcon() // generate random app icon
        {
            using (var sys = SystemIcons.Asterisk)
            {
                var mh = sys.Height;
                var mw = sys.Width;
                using (var bmp = new Bitmap (mw, mh, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var g = Graphics.FromImage (bmp);
                    g.FillRectangle (Brushes.Transparent, 0, 0, mw, mh);
                    mw -= 1; mh -= 1;
                    GraphicsPath path = new GraphicsPath ();
                    path.AddEllipse (0, 0, mw, mh);
                    PathGradientBrush pthGrBrush = new PathGradientBrush (path);
                    var rnd = new Random ();
                    pthGrBrush.CenterPoint = new PointF (rnd.Next (mw / 4, 3 * mw / 4), rnd.Next (mh / 4, 3 * mh / 4));
                    pthGrBrush.CenterColor = Color.Lime;
                    Color[] colors = { Color.DarkGreen };
                    pthGrBrush.SurroundColors = colors;
                    g.FillEllipse (pthGrBrush, 0, 0, mw, mh);
                    this.Icon = Icon.FromHandle (bmp.GetHicon ());
                }
            }
        }// SetWindowIcon()

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_init_failed) e.Graphics.FillRectangle (Brushes.Red, e.ClipRectangle);//TODO NoRender
            else base.OnPaint (e);
        }
    }// FormMain
}
