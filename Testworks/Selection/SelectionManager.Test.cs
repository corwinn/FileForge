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
using NUnit.Framework;

namespace Testworks.Selection
{
    using TestNS = File_Forge.Selection;

    sealed internal class TestSelectionId : TestNS.SelectionId
    {
        public TestSelectionId() { }
        public TestSelectionId(int v) { Value = v; }
        public int Value { get; set; }
        public override bool Equals(object obj) { return ((TestSelectionId)obj).Value == Value; }
        public override int GetHashCode() { return Value.GetHashCode (); }
        protected override bool LessThan(TestNS.SelectionId other) { return Value < ((TestSelectionId)other).Value; }
        protected override bool GreaterThan(TestNS.SelectionId other) { return Value > ((TestSelectionId)other).Value; }
        public override bool Sequential(TestNS.SelectionId id) { return Math.Abs (((TestSelectionId)id).Value - Value) == 1; }
        protected override TestNS.SelectionId Next() { Value++; return this; }
        protected override TestNS.SelectionId Prev() { Value--; return this; }
        public override TestNS.SelectionId Create(TestNS.SelectionId copy_me)
        {
            var result = new TestSelectionId();
            result.Value = ((TestSelectionId)copy_me).Value;
            return result;
        }
    }

    [TestFixture]
    class SelectionRangeTest
    {
        TestNS.SelectionRange _r;

        [SetUp]
        public void setup() { _r = new TestNS.SelectionRange (); }

        [Test, Category ("InitialState")]
        public void Empty()
        {
            TestSelectionId foo = new TestSelectionId ();
            Assert.IsFalse (_r.Contains (foo));
            foo.Value = 0;
            Assert.IsFalse (_r.Contains (foo));
            foo.Value = int.MaxValue;
            Assert.IsFalse (_r.Contains (foo));
            foo.Value = int.MinValue;
            Assert.IsFalse (_r.Contains (foo));
        }

        [Test, Category ("InitialBugs - temporary")]
        public void SearchNextEvenIfPrevIsNull()
        {
            TestSelectionId a = new TestSelectionId (0);
            TestSelectionId b = new TestSelectionId (5);
            TestSelectionId c = new TestSelectionId (2);
            TestSelectionId d = new TestSelectionId (7);
            TestSelectionId e = new TestSelectionId (0);
            _r.Replace (a, b); // [0;5]
            _r.SplitRange (c);  // [0;1], _r->[3;5]
            _r.Add (d); // [0;1], _r->[3;5], [7;7]
            Assert.IsTrue (_r.Contains (d));
            Assert.IsTrue (_r.Contains (e));
        }
    }
}
