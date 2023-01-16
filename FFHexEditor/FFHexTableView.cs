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
using System.IO;
using System.Linq;
using System.Text;

namespace File_Forge.HexEditor
{
    // Bridge away the actual id. The idea is to write the selection manager once and reuse it everywhere.
    internal abstract class SelectionId
    {
        public static bool operator <=(SelectionId l, SelectionId r) { return l.Equals (r) || l.LessThan (r); }
        public static bool operator >=(SelectionId l, SelectionId r) { return l.Equals (r) || l.GreaterThan (r); }
        public static bool operator <(SelectionId l, SelectionId r) { return l.LessThan (r); }
        public static bool operator >(SelectionId l, SelectionId r) { return l.GreaterThan (r); }
        protected abstract bool LessThan(SelectionId other);
        protected abstract bool GreaterThan(SelectionId other);
    }
    internal class SelectionRange
    {
        private SelectionId _a = null, _b = null;
        private List<SelectionId> _set = new List<SelectionId> ();
        public bool Contains(SelectionId id) { return (id >= _a && id <= _b) || _set.Contains (id); }
        public SelectionRange Union(SelectionId id, bool range)
        {
            if (null == _a) _a = id;
            else if (null == _b) _b = id;
            else
            {
                if (id < _a) { _b = _a; _a = id; } // thats how "Windows" "Explorer" behaves
                else if (id > _b) _b = id;         //
                else                               //
                { // you selected inside a selection
                    if (range) _b = id; // when the Shift is down, "b" is modified
                    else SplitRange (id);//TODO bool deselect?
                }
            }
            return this;
        }
        private void SplitRange(SelectionId id)
        {
            // either I inform the manager I'm becoming two ranges; or I'm a sub-manager - a.k.a SelectionRangeTreeNode
            throw new NotImplementedException ();
        }
        public SelectionRange Add(SelectionId id)
        {
            // this has manager issues as well - a Ctrl-select could join two ranges; smells like a tree
            throw new NotImplementedException ();
        }
    }
    // Maintains a list of SelectionRange. The idea is to minimize memory usage: SelectAll in 193TB file for example,
    // will result in the creation of one SelectionRange. Selecting random bytes from it, will cause OutOfMemoryException
    // at some point.
    sealed internal class SelectionManager //TODO compress me
    {
        //TODONT optimize; no point as the user rarely selects more than 1 block at a time
        private List<SelectionRange> _ranges = new List<SelectionRange> ();

        internal bool Selected(SelectionId selection_id)
        {
            if (null == selection_id) return false;
            return _ranges.Where (r => r.Contains (selection_id)).Count () > 0;
        }

        private int _stage = -1; // -1 - uninformed; 0 - began; 1 - ended as range; 2 - ended as a set
        SelectionRange _range = null;
        // These shall be called by UI the event handlers.
        public void BeginSelection() // Ctrl is held down, or an item is selected
        {
            _stage = 0;
        }
        public void EndSelection(bool range) // Ctrl is released, or an item is selected, or Shift is held down and an item is selected
        {
            _stage = range ? 1 : 2;
        }
        internal void Select(SelectionId selection_id)
        {
            if (null == selection_id) return;
            switch (_stage)
            {
                case 0:
                    {
                        _range = _ranges.Where (r => r.Contains (selection_id)).FirstOrDefault ();
                        if (null == _range) { _range = new SelectionRange (); _ranges.Add (_range); }
                        _range.Union (selection_id, false);
                    }
                    break;
                case 1: _range.Union (selection_id, true); _stage = -1; _range = null; break;
                case 2: _range.Add (selection_id); _stage = -1; _range = null; break;
                default: throw new Exception ("Fixme: your forgot to inform the SelectionManager whats happening.");
            }
        }
    }// SelectionManager

    sealed internal class FFHexTableViewSelectionId : SelectionId
    {
        public long ByteOffset { get; set; }
        public override bool Equals(object obj) { return ((FFHexTableViewSelectionId)obj).ByteOffset == ByteOffset; }
        public override int GetHashCode() { return ByteOffset.GetHashCode (); }
        protected override bool LessThan(SelectionId other) { return ByteOffset < ((FFHexTableViewSelectionId)other).ByteOffset; }
        protected override bool GreaterThan(SelectionId other) { return ByteOffset > ((FFHexTableViewSelectionId)other).ByteOffset; }
    }

    // Bridges my cell to a SelectionManager.
    internal abstract class FFHexTableViewSelectableCell : Wind.Controls.IWTableViewCell
    {
        private SelectionManager _selection_manager = null;
        private SelectionId _selection_id = null;
        protected FFHexTableViewSelectableCell(SelectionManager selection_manager, SelectionId selection_id)
        {
            _selection_manager = selection_manager;
            _selection_id = selection_id;
        }
        #region Wind.Controls.IWTableViewCell
        public abstract bool Changed { get; set; }
        public abstract Wind.Controls.WTableViewRange HRange { get; }
        public abstract Wind.Controls.WTableViewRange VRange { get; }
        public abstract System.Drawing.Size Measure(Wind.Controls.WGraphicsContext gc);
        public abstract bool New { get; set; }
        public abstract void Render(Wind.Controls.WGraphicsContext gc);
        public bool Selected
        {
            get { return _selection_manager.Selected (_selection_id); }
            set { _selection_manager.Select (_selection_id); }
        }
        #endregion
        public SelectionId SelectionId { get { return _selection_id; } }
    }

    internal class FFHexTableViewCell : FFHexTableViewSelectableCell, IDisposable
    {
        protected Wind.Controls.WTableViewRange _h_range = null, _v_range = null;
        private bool _changed = true;
        private string _text = "";
        protected int _tx = 0, _ty = 0, _tw = 0, _th = 0;
        protected StringFormat _text_fmt = new StringFormat ();
        private static Font _text_font = SystemFonts.DefaultFont; //TODO FontChanged ; this will be fun
        static bool _column_SizeChanged_the_way_is_shut = false; // an event handler shouldn't cause events it has to handle
        protected static StringAlignment WTableViewAlignment2StringAlignment(Wind.Controls.WTableViewAlignment a)
        {
            StringAlignment result;
            switch (a)
            {
                case Wind.Controls.WTableViewAlignment.Bottom:
                case Wind.Controls.WTableViewAlignment.Right: result = StringAlignment.Far; break;
                case Wind.Controls.WTableViewAlignment.Top:
                case Wind.Controls.WTableViewAlignment.Left: result = StringAlignment.Near; break;
                case Wind.Controls.WTableViewAlignment.Center: result = StringAlignment.Center; break;
                default: throw new Exception ("Fixme: Unknown Wind.Controls.WTableViewAlignment");
            }
            return result;
        }

        public FFHexTableViewCell(string text, Wind.Controls.WTableViewRange h_range, Wind.Controls.WTableViewRange v_range,
            SelectionManager selection_manager, SelectionId selection_id)
            : base (selection_manager, selection_id)
        {
            _text = text;
            _h_range = h_range;
            _v_range = v_range;

            _text_fmt.Alignment = WTableViewAlignment2StringAlignment (_v_range.Alignment);
            _text_fmt.LineAlignment = WTableViewAlignment2StringAlignment (_h_range.Alignment);

            _v_range.SizeChanged += HandleSizeChanged;
            _h_range.SizeChanged += HandleSizeChanged;
        }

        private void HandleSizeChanged(int delta)
        {
            if (_column_SizeChanged_the_way_is_shut) throw new Exception ("Fixme: event handler re-entry.");
            _column_SizeChanged_the_way_is_shut = true;
            try { UpdateLayout (); }
            finally { _column_SizeChanged_the_way_is_shut = false; }
        }

        private void UpdateLayout()
        {
            int tw = _v_range.Size - _tw, th = _h_range.Size - _th;
            switch (_text_fmt.Alignment)
            {
                case StringAlignment.Near: _tx = 0; break;
                case StringAlignment.Far: _tx = tw; break;
                default: _tx = tw / 2; break;
            }
            switch (_text_fmt.LineAlignment)
            {
                case StringAlignment.Near: _ty = 0; break;
                case StringAlignment.Far: _ty = th; break;
                default: _ty = th / 2; break;
            }
        }

        public override bool Changed { get { return _changed; } set { _changed = value; } } // do not interfere with the view, for now
        public override Wind.Controls.WTableViewRange HRange { get { return _h_range; } }
        public override Wind.Controls.WTableViewRange VRange { get { return _v_range; } }
        public override System.Drawing.Size Measure(Wind.Controls.WGraphicsContext gc)
        {
            if (!_changed) return new Size (_v_range.Size, _h_range.Size);
            var ts = gc.MeasureText (_text, SystemFonts.DefaultFont);
            _tw = ts.Width;
            _th = ts.Height;
            _changed = false;
            _v_range.UpdateSize (Wind.Controls.WTableView.Align2 (_tw));
            _h_range.UpdateSize (Wind.Controls.WTableView.Align2 (_th));
            UpdateLayout ();
            return new Size (_v_range.Size, _h_range.Size); // update the range - a.k.a. layout
        }
        public override bool New { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
        public override void Render(Wind.Controls.WGraphicsContext gc)
        {
            gc.FillRectangle (Brushes.LightBlue, 0, 0, _v_range.Size, _h_range.Size);
            RenderText (gc);
        }
        protected void RenderText(Wind.Controls.WGraphicsContext gc)
        {
            if (_tx < 0 || _ty < 0)
                gc.DrawString (_text, _text_font, Brushes.Black, new RectangleF (0, 0, _v_range.Size, _h_range.Size), _text_fmt);
            else gc.DrawString (_text, _text_font, Brushes.Black, _tx, _ty);
        }

        void IDisposable.Dispose()
        {
            _v_range.SizeChanged -= HandleSizeChanged;
            _h_range.SizeChanged -= HandleSizeChanged;
            _text_font.Dispose ();
            _text_fmt.Dispose ();
        }
    }// FFHexTableViewCell

    sealed internal class FFHexTableViewHeaderCell : FFHexTableViewCell
    {
        public FFHexTableViewHeaderCell(string text, Wind.Controls.WTableViewRange h_range, Wind.Controls.WTableViewRange v_range,
            SelectionManager selection_manager, SelectionId selection_id)
            : base (text, h_range, v_range, selection_manager, selection_id)
        {
            //TODO this is a little bit confusing; these should be header cell properties
            _text_fmt.Alignment = WTableViewAlignment2StringAlignment (_v_range.HeaderHorizontalAlignment);
            _text_fmt.LineAlignment = WTableViewAlignment2StringAlignment (_h_range.HeaderVerticalAlignment);
        }
        public override void Render(Wind.Controls.WGraphicsContext gc)
        {
            gc.FillRectangle (Brushes.PaleGreen, 0, 0, _v_range.Size, _h_range.Size);
            RenderText (gc);
        }
    }// FFHexTableViewHeaderCell

    sealed internal class FFHexTableViewModel : Wind.Controls.IWTableViewModel
    {
        private Wind.Controls.WTableViewRange _column_header_row = new Wind.Controls.WTableViewRange ();
        // Using one distinct row range.
        private Wind.Controls.WTableViewRange _rows = new Wind.Controls.WTableViewRange ();
        // Using n columns; default: n=16
        private List<Wind.Controls.WTableViewRange> _columns = new List<Wind.Controls.WTableViewRange> ();
        private int _column_num = 16;
        //
        private SelectionManager _selection = new SelectionManager ();

        class CellStream //TODO I should consider using ICellStream at the model
        {
            protected List<Wind.Controls.WTableViewRange> _columns = null; // ref (1:1 with FFHexTableViewModel)
            protected int _c_pointer = 0;
            public CellStream(List<Wind.Controls.WTableViewRange> columns, SelectionManager selection_manager)
            {
                _columns = columns;
                SelectionManager = selection_manager;
            }
            protected FFHexTableViewCell CreateCell(string text, Wind.Controls.WTableViewRange h_range)
            {
                // "selection_id: null" - column header cells are not selectable
                return new FFHexTableViewHeaderCell (text, h_range: h_range, v_range: _columns[_c_pointer],
                    selection_manager: SelectionManager, selection_id: null);
            }
            public virtual Wind.Controls.IWTableViewCell CurrentCell { get { return null; } } //TODO perhaps its a better name
            public virtual bool Move(int column_offset, int row_offset) { return false; }
            public SelectionManager SelectionManager { get; private set; }
        }
        CellStream _column_header_stream = null;
        Data_Stream _data_stream = null;
        CellStream _current_stream = null;
        sealed class ColumnHeader_Stream : CellStream
        {
            private Wind.Controls.WTableViewRange _colum_header_row = null; // ref (1:1 with FFHexTableViewModel)
            private List<FFHexTableViewCell> _header_cells = new List<FFHexTableViewCell> (); // cell cache
            public ColumnHeader_Stream(Wind.Controls.WTableViewRange column_header_row, List<Wind.Controls.WTableViewRange> columns,
                SelectionManager selection_manager)
                : base (columns, selection_manager)
            {
                _colum_header_row = column_header_row;
            }
            private string AutoHeaderName { get { return "+" + _c_pointer.ToString ("d2"); } }
            private void UpdateCache()
            {
                if (_c_pointer < _header_cells.Count && _c_pointer >= 0 && null == _header_cells[_c_pointer])
                    _header_cells[_c_pointer] = CreateCell (text: AutoHeaderName, h_range: _colum_header_row);
                else if (_c_pointer >= _header_cells.Count && _header_cells.Count >= 0 && _header_cells.Count < _columns.Count)
                    for (int i = _c_pointer - _header_cells.Count; _c_pointer >= _header_cells.Count; i++)
                        _header_cells.Add (CreateCell (text: AutoHeaderName, h_range: _colum_header_row));
            }
            public override Wind.Controls.IWTableViewCell CurrentCell
            {
                get
                {
                    UpdateCache ();
                    return _header_cells[_c_pointer];
                }
            }
            public override bool Move(int column_offset, int row_offset)
            {
                if (0 != row_offset) return false; // I'm 1 row
                var q = _c_pointer + column_offset;
                if (q < 0 || q >= _columns.Count) return false;
                _c_pointer = q; return true;
            }
        }// ColumnHeader_Stream
        sealed class Data_Stream : CellStream
        {
            private Wind.Controls.WTableViewRange _row = null; // ref (1:1 with FFHexTableViewModel)
            private Stream _data = null;
            private long _data_size = 0;
            private string _data_txt = "";
            private void UpdateDataTxt()
            {
                var sentinel = _data.Position;
                try { _data_txt = _data.ReadByte ().ToString ("X2"); }
                catch (Exception exc) { Wind.Log.WLog.Err (exc.ToString ()); }
                finally
                {
                    try { _data.Seek (sentinel, SeekOrigin.Begin); }
                    catch (Exception exc) { Wind.Log.WLog.Err (exc.ToString ()); }
                }
            }
            public Data_Stream(Wind.Controls.WTableViewRange row, List<Wind.Controls.WTableViewRange> columns,
                SelectionManager selection_manager)
                : base (columns, selection_manager)
            {
                _row = row;
            }
            public override Wind.Controls.IWTableViewCell CurrentCell { get { return null != _data ? CreateCell (_data_txt, _row) : null; } }
            public override bool Move(int column_offset, int row_offset)
            {
                if (null == _data) return false;

                var q = _c_pointer + column_offset;
                if (q < 0 || q >= _columns.Count) return false;
                _c_pointer = q;

                var sentinel = _data.Position;
                var new_pos = sentinel + (row_offset * _columns.Count + column_offset);//TODO overflow check
                if (new_pos < 0 || new_pos >= _data_size) return false;
                var result = sentinel;
                try { result = _data.Seek (new_pos, SeekOrigin.Begin); }
                catch (Exception e) { Wind.Log.WLog.Err (e.ToString ()); return false; }
                if (result != sentinel) { UpdateDataTxt (); return true; }
                return false;
            }

            public long DataSize { get { return _data_size; } }

            public Stream Data
            {
                get { return _data; }
                set { if (!object.ReferenceEquals (_data, value) && null != value) DataChanged (value); }
            }

            private void DataChanged(Stream value)
            {
                _data = value;
                _data_size = value.Length;
                UpdateDataTxt ();
            }
        }// Data_Stream

        public FFHexTableViewModel()
        {
            _column_header_row.HeaderVerticalAlignment = Wind.Controls.WTableViewAlignment.Center; // header_cell.VerticalAlignment
            for (int i = 0; i < _column_num; i++)
            {
                var column = new Wind.Controls.WTableViewRange ();
                column.HeaderHorizontalAlignment = Wind.Controls.WTableViewAlignment.Left; // header_cell.HorizontalAlignment
                column.Alignment = Wind.Controls.WTableViewAlignment.Center;
                column.AutoSize = true;
                _columns.Add (column);
            }
            _column_header_stream = new ColumnHeader_Stream (
                column_header_row: _column_header_row, columns: _columns, selection_manager: _selection);

            _rows.Alignment = Wind.Controls.WTableViewAlignment.Center;
            _rows.AutoSize = true;
            _data_stream = new Data_Stream (row: _rows, columns: _columns, selection_manager: _selection);
        }

        private long StreamSize() { return _data_stream.DataSize; }
        private long ColumnCount() { return _columns.Count; }
        private long RowCount() { var p1 = StreamSize () % _columns.Count != 0 ? 1 : 0; return StreamSize () / _columns.Count + p1; }

        #region IWTableViewModel Members
        public Wind.Controls.IWTableViewModel ColumnHeaderStream() { _current_stream = _column_header_stream; return this; }
        public Wind.Controls.IWTableViewModel DataStream() { _current_stream = _data_stream; return this; }
        public Wind.Controls.IWTableViewCell Current { get { return _current_stream.CurrentCell; } }
        public bool Move(int column_offset, int row_offset) { return _current_stream.Move (column_offset, row_offset); }
        public void Set(IEnumerable<Wind.Controls.IWTableViewCell> cells) { throw new NotImplementedException (); }
        public long SizeHorizontal { get { return ColumnCount (); } }
        public long SizeVertical { get { return RowCount (); } }
        #endregion

        public Stream Data { get { return _data_stream.Data; } set { _data_stream.Data = value; } }
    }// FFHexTableViewModel

    internal class FFHexTableView : Wind.Controls.WControl
    {
        private Wind.Controls.WTableView _tv = new Wind.Controls.WTableView ();
        private FFHexTableViewModel _model = new FFHexTableViewModel ();
        public FFHexTableView()
        {
            _tv.Parent = this;
            _tv.Dock = System.Windows.Forms.DockStyle.Fill;
        }
        public void SetTheModel() { _tv.Model = _model; }
    }
}
