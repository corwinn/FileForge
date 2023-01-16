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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Universe { interface limit {} interface the { } abstract class Sky : limit { public abstract void hack(the limit); } }

namespace File_Forge
{
    // note: this is not the proper way of doing computer animation, so don't copy code from here.
    // note2: I don't know the proper way of doing computer animation, yet; but I'm sure this ain't it.
    public class FormAbout : Form
    {
        Timer _dontknow_timer;
        dontknow _dontknow;

        public FormAbout()
        {
            this.SetStyle (ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.DoubleBuffer, true);

            this.ShowIcon = false;
            this.ShowInTaskbar = false;

            Cursor.Hide ();
            this.Closed += (a, b) => { Cursor.Show (); };

            this.FormClosing += this.AboutForm_FormClosing;
            this.Shown += this.AboutForm_Shown;
            this.KeyUp += this.AboutForm_KeyUp;
            this.MouseUp += this.AboutForm_MouseUp;

            _dontknow = new dontknow ();
            _dontknow_timer = new Timer ();
            _dontknow_timer.Tick += (a, b) => { _dontknow.Move (); this.Invalidate (); };
            _dontknow_timer.Interval = 1000 / 25;
            _dontknow_timer.Start ();

            this.Opacity = 0;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);
            _dontknow.Width = this.Width;
            _dontknow.Height = this.Height;
            _dontknow.Reset ();
        }

        protected override void WndProc(ref Message messg)//TODO FFForm
        {
            const int WM_NULL = 0x0000; // "WinUser.h"
            const int WM_ERASEBKGND = 0x0014; // "WinUser.h"
            if (WM_ERASEBKGND == messg.Msg) messg.Msg = WM_NULL;
            base.WndProc (ref messg); // never stand on the way of WndProc and its prey ;)
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var current = base.CreateParams;
                current.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                //current.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return current;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _dontknow.Render (e.Graphics);
        }

        private void AboutForm_Shown(object sender, EventArgs e)
        {
            Timer t = new Timer ();
            t.Tick += (a, b) => { if (this.Opacity < 1.0d) this.Opacity += 0.05d; else { this.Opacity = 1.0d; (a as Timer).Stop (); } };
            t.Interval = 1000 / 100;
            t.Start ();
        }

        bool _cancel_closing = true;
        private void AboutForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dontknow_timer.Stop ();
            e.Cancel = _cancel_closing; _cancel_closing = !_cancel_closing;
            Timer t = new Timer ();
            t.Tick += (a, b) => { if (this.Opacity > 0.0d) this.Opacity -= 0.05d; else { this.Opacity = 0.0d; (a as Timer).Stop (); Close (); } };
            t.Interval = 1000 / 100;
            t.Start ();
        }

        private void AboutForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) Close ();
        }

        private void AboutForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Q) Close ();
        }
    }// AboutForm

    class dontknow
    {
        class item
        {
            public static Random rnd = new Random ((int)DateTime.Now.Ticks);
            public float px;
            public float py;
            public float x;
            public float y;
            float dx;
            float dy;
            public void Reset(int w, int h)
            {
                var x1 = w >> 1;
                var y1 = h >> 1;
                px = x = rnd.Next (w); if (x == x1) px = x += (rnd.Next () % 2 == 0 ? -1 : 1);
                py = y = rnd.Next (h); if (y == y1) py = y += (rnd.Next () % 2 == 0 ? -1 : 1);
                var qx = x - x1;
                var qy = y - y1;
                if (Math.Abs (qx) > Math.Abs (qy))
                {
                    var a = qy / (float)qx;
                    dx = 2 + rnd.Next (4); if (qx < 0) dx = -dx;
                    dy = a * dx;
                }
                else
                {
                    var a = qx / (float)qy;
                    dy = 2 + rnd.Next (4); if (qy < 0) dy = -dy;
                    dx = a * dy;
                }
                LineColor = new Pen (Color.FromArgb (rnd.Next ()));
            }

            public bool Move(int w, int h)
            {
                px = x;
                py = y;
                x += dx; if (x < 0 || x >= w) return false;
                y += dy; if (y < 0 || y >= h) return false;
                return true;
            }
            public Pen LineColor { get; set; }
        }
        List<item> _items = new List<item> ();
        Bitmap _bmp1;
        Font _fnt;
        Rectangle _r1;
        Color _c1 = Color.Silver;
        ImageAttributes _img_attr;
        ColorMatrix _alpha_matrix;
        Rectangle _t1_rec;

        void GenTex(string text)
        {
            if (null != _bmp1) _bmp1.Dispose ();
            var s = TextRenderer.MeasureText (text, _fnt);
            _bmp1 = new Bitmap (s.Width, s.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gc = Graphics.FromImage (_bmp1);
            _t1_rec = new Rectangle (0, 0, s.Width, s.Height);
            TextRenderer.DrawText (gc, text, _fnt, _t1_rec, _c1, Color.Transparent,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.GlyphOverhangPadding);
        }

        public dontknow()
        {
            for (int i = 0; i < 200; i++) _items.Add (new item ());

            _fnt = new Font (SystemFonts.CaptionFont.FontFamily, SystemFonts.CaptionFont.Size + (float)42, FontStyle.Bold);
            GenTex ("Don't panic!");
            _alpha_matrix = new ColorMatrix ();
            _alpha_matrix.Matrix33 = 0.0f;
            _img_attr = new ImageAttributes ();
            _img_attr.SetColorMatrix (_alpha_matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            _ani_p1 = true;
        }

        int _author = 0; // global author index
        int _ani2_cnt = 2; // disable secondary animation - reserved for authors display only
        float _rdir = 1; // rotate direction - chosen randomly for each Part2 bitmap
        bool _rotate = true; // rotate or translate - 2 different "fx"
        float _txdir = 1; // translate x direction
        float _tydir = 1; // translate y direction
        void Part2()
        {
            if (_author < Program.AUTHORS.Count)
            {
                //Wind.Log.WLog.Info ("author");
                GenTex (Program.AUTHORS[_author++]);
                _alpha_matrix.Matrix33 = 0.0f;
                _img_attr.SetColorMatrix (_alpha_matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                // center
                _t1_rec.X = (Width - _t1_rec.Width) / 2;
                _t1_rec.Y = (Height - _t1_rec.Height) / 2;
                // start the anim
                _ani_p1 = true;
                _ani2_idx = 22;
                _ani2_cnt = 0;
                _ani2_idx_inc = -2;
                if (item.rnd.Next () % 2 == 0) _rdir = -_rdir;
                if (item.rnd.Next () % 2 == 0) _rotate = !_rotate;
                if (item.rnd.Next () % 2 == 0) _txdir = -_txdir;
                if (item.rnd.Next () % 2 == 0) _tydir = -_tydir;
            }
        }

        int _ani2_idx = 0; // anim path index
        int _ani2_idx_inc = -2; // anim path index increment
        float[] _ani2_path = new float[]
        {
             -1, 1.1f,
             -2, 1.2f,
             -4, 1.3f,
             -7, 1.5f,
             -11, 1.8f,
            -16, 2.13f,
            -22, 2.21f,
            -29, 2.34f,
            -37, 2.55f,
            -46, 2.89f,
            -56, 3.44f,
            -67, 4.33f,
            -79, 5.77f,
            -92, 7f,
            -106, 8f,
            -121, 9f,
            -137, 10f,
            -154, 11f,
            -172, 12f
        };
        // linear random color change
        float _some_inc0;
        float _some_inc1;
        float _some_inc2;
        int _some_steps = 101;
        int _some_steps_i = 0;
        public void Render(Graphics gc)
        {
            gc.ResetTransform ();
            foreach (var item in _items) gc.DrawLine (item.LineColor, item.x, item.y, item.px, item.py);
            if (_ani2_cnt < 2 && _ani2_idx < _ani2_path.Length - 1 && _ani2_idx >= 0)
            {
                Wind.Log.WLog.Info ("_ani2_idx: " + _ani2_idx);
                gc.TranslateTransform ((_t1_rec.X + _bmp1.Width / 2), (_t1_rec.Y + _bmp1.Height / 2));
                if (_rotate)
                {
                    gc.RotateTransform (-_rdir * _ani2_path[_ani2_idx]);
                    gc.ScaleTransform (_ani2_path[_ani2_idx + 1], _ani2_path[_ani2_idx + 1]);
                }
                else
                    gc.TranslateTransform (_txdir * _ani2_path[_ani2_idx] * 10, _tydir * _ani2_path[_ani2_idx] * 10);
                gc.TranslateTransform (-(_t1_rec.X + _bmp1.Width / 2), -(_t1_rec.Y + _bmp1.Height / 2));
                _ani2_idx += _ani2_idx_inc;
            }
            gc.DrawImage (_bmp1, _t1_rec, 0, 0, _bmp1.Width, _bmp1.Height, GraphicsUnit.Pixel, _img_attr);
        }

        public int Width { get; set; }
        public int Height { get; set; }

        // called when Width/Height changes
        public void Reset()
        {
            foreach (var item in _items) item.Reset (Width, Height);
            _r1 = new Rectangle (0, 0, Width, Height);
            _t1_rec.X = (Width - _t1_rec.Width) / 2;
            _t1_rec.Y = (Height - _t1_rec.Height) / 2;
        }

        bool _ani_p1 = false; // animation part1 - alpha from 0 to 1
        bool _ani_p2 = false; // animation part2 - alpha from 1 to 0
        // called repeatedly
        public void Move()
        {
            foreach (var item in _items) if (!item.Move (Width, Height)) item.Reset (Width, Height);

            if (0 == _some_steps_i)
            {
                var rnd = item.rnd;
                float r = (float)rnd.NextDouble (); if (rnd.Next () % 2 == 0) r = -r;
                float g = (float)rnd.NextDouble (); if (rnd.Next () % 2 == 0) g = -g;
                float b = (float)rnd.NextDouble (); if (rnd.Next () % 2 == 0) b = -b;
                _some_inc0 = (r - _alpha_matrix.Matrix40) / _some_steps;
                _some_inc1 = (g - _alpha_matrix.Matrix41) / _some_steps;
                _some_inc2 = (b - _alpha_matrix.Matrix42) / _some_steps;
            }
            _alpha_matrix.Matrix40 += _some_inc0;
            _alpha_matrix.Matrix41 += _some_inc1;
            _alpha_matrix.Matrix42 += _some_inc2;
            _some_steps_i = (_some_steps_i + 1) % _some_steps;
            _img_attr.SetColorMatrix (_alpha_matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            if (!_ani_p1) return;
            if (_alpha_matrix.Matrix33 < 1.0f)
            {
                _alpha_matrix.Matrix33 += _alpha_matrix.Matrix33 + 0.00001f;
                _img_attr.SetColorMatrix (_alpha_matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            }
            else
            {
                _ani_p1 = false;
                if (_ani_p2) return;
                _ani_p2 = true;
                Timer t = new Timer ();
                t.Tick += (a, b) =>
                {
                    if (0 == _ani2_cnt) { _ani2_idx = 0; _ani2_idx_inc = 2; _ani2_cnt++; }
                    t.Interval = 1000 / 25;
                    if (_alpha_matrix.Matrix33 > 0.0f)
                    {
                        _alpha_matrix.Matrix33 -= 0.1f;
                        _img_attr.SetColorMatrix (_alpha_matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    }
                    else
                    {
                        t.Stop ();
                        this.Part2 ();
                        _ani_p2 = false;
                    }
                };
                t.Interval = 5000; // let it stay 5 seconds on the screen
                t.Start ();
            }
        }
    }// class dontknow
}
