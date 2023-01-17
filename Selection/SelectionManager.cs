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
using System.Linq;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo ("Testworks")]

/*
    This code shall describe a selection decoupled from everything. //TODO to the windlib
    Let it be undo-able.
*/

namespace File_Forge.Selection
{
    // What is it: it is the A and B of a selection range [A;B] - whether it is a row in a list or a byte in a file - its up to you.
    // Bridge away the actual id. The idea is to write the selection manager once and reuse it everywhere.
    internal abstract class SelectionId
    {
        public static bool operator <=(SelectionId l, SelectionId r) { return l.Equals (r) || l.LessThan (r); }
        public static bool operator >=(SelectionId l, SelectionId r) { return l.Equals (r) || l.GreaterThan (r); }
        public static bool operator <(SelectionId l, SelectionId r) { return l.LessThan (r); }
        public static bool operator >(SelectionId l, SelectionId r) { return l.GreaterThan (r); }
        public static SelectionId operator ++(SelectionId obj) { return obj.Next (); }
        public static SelectionId operator --(SelectionId obj) { return obj.Prev (); }
        protected abstract bool LessThan(SelectionId other);
        protected abstract bool GreaterThan(SelectionId other);
        public abstract bool Sequential(SelectionId id); // true when "this" and "id' are sequential
        protected abstract SelectionId Next();
        protected abstract SelectionId Prev();
        public abstract SelectionId Create(SelectionId copy_me);
    }

    sealed internal class SelectionRange
    {
        private SelectionId _a = null, _b = null;
        private SelectionRange _next = null;
        private SelectionRange _prev = null;

        public SelectionRange() { }
        public SelectionRange(SelectionId a, SelectionId b) { Replace (a, b); }

#if TESTING_WORKS
        public SelectionId A { get { return _a; } }
        public SelectionId B { get { return _b; } }
        public SelectionRange Next { get { return _next; } }
        public SelectionRange Prev { get { return _prev; } }
#endif

        public SelectionRange Union(SelectionId id, bool range)
        {
            throw new FFNotTestedException ();
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
        // LL.Add
        private void Add(SelectionRange node)
        {
            node._prev = this;
            node._next = _next;
            if (null != _next) _next._prev = node;
            _next = node;
        }
        // LL.Insert
        private void Insert(SelectionRange node)
        {
            node._next = this;
            node._prev = _prev;
            if (null != _prev) _prev._next = node;
            _prev = node;
        }

        public void SplitRange(SelectionId id) //TODO temporary, for testing, until Union above is implemented
        {
            if (null == _a || null == _b || _a == _b) throw new ArgumentException ("Can't help you");
            if (id < _a || id > _b) throw new ArgumentException ("Id shall be [a;b]");
            // id was just de-selected
            if (id == _b) { _b--; return; }
            if (id == _a) { _a++; return; }
            //
            var b = id.Create (id); b--;
            var a = id.Create (id); a++;
            Insert (new SelectionRange (_a, b));
            _a = a;
        }

        public void Add(SelectionId id)//TODO has implicit "bool set = true"
        {
            if (null == _a) { _a = id; return; }
            // if (_a.Sequential (id)
            //   && (   (null != _next && _next._a.Sequential (id))   //LATER merge (memory usage optimization)
            //       || (null != _prev && _prev._a.Sequential (id)))) //
            // else
            if (id > _a) Add (new SelectionRange (id, null));
            else Insert (new SelectionRange (id, null));
        }

        private bool Has(SelectionId id)
        {
            if (null == _a) return false;
            if (id == _a) return true;
            return null != _b && (id >= _a && id <= _b);
        }
        private bool ContainsPrev(SelectionId id) // WalkPrev
        {
            if (null == _prev) return false;
            if (_prev.Has (id)) return true;
            else return _prev.ContainsPrev (id);
        }
        private bool ContainsNext(SelectionId id) // WalkNext
        {
            if (null == _next) return false;
            if (_next.Has (id)) return true;
            else return _next.ContainsPrev (id);
        }
        public bool Contains(SelectionId id)
        {
            if (Has (id)) return true;
            if (ContainsPrev (id)) return true;
            return ContainsNext (id);
        }

        public void Replace(SelectionId a, SelectionId b) { _a = a; _b = b; }
    }// SelectionRange

    // Maintains a SelectionRange. The idea is to minimize memory usage (its a requirement actually): SelectAll in
    // 193TB file for example, will result in the creation of one SelectionRange. Selecting random bytes from it, will cause
    // OutOfMemoryException at some point.
    sealed internal class SelectionManager //TODO compress me
    {
        //TODONT optimize; no point as the user rarely selects more than 1 block at a time
        private SelectionRange _range = new SelectionRange (); // DLL

        internal bool Selected(SelectionId selection_id)
        {
            return _range.Contains (selection_id);
        }

        // Use-cases:
        //  - initial: current_selection = empty
        //  - select: (no modifiers): current_selection = [A=B]
        //  - select: (union): current_selection = current_selection U [A;B]
        private int _stage = -1; // -1 - uninformed; 0 - began; 1 - ended as range;
        //TODO deselect
        //TODO FFUndoable

        // These shall be called by UI the event handlers.
        public SelectionManager BeginSelection() // Ctrl is held down, or an item is selected
        {
            _stage = 0;
            return this;
        }
        // Ctrl or mouse or key (up/down arrow for example - whatever your design) or Shift, or AI, or ... are released or down
        public SelectionManager EndSelection(bool select_many)
        {
            _stage = select_many ? 1 : 2;
            return this;
        }
        // BeginSelection ().Select (foo)
        // EndSelection ().Select (bar)
        internal void Select(SelectionId selection_id)
        {
            if (null == selection_id) return;
            switch (_stage)
            {
                case 0: _range.Replace (selection_id, selection_id); break;
                case 1: _range.Union (selection_id, true); _stage = -1; _range = null; break;
                case 2: _range.Add (selection_id); _stage = -1; _range = null; break;
                default: throw new Exception ("Fixme: your forgot to inform the SelectionManager whats happening.");
            }
        }
    }// SelectionManager
}
