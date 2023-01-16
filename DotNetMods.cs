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
using System.Windows.Forms;

namespace File_Forge
{
    internal class FFToolStrip : ToolStrip // when I click on something I expect it to register the click
    {
        protected override void WndProc(ref Message m)
        {
            const int MA_ACTIVATE = 1;         // WinUser.h:#define MA_ACTIVATE         1
            const int MA_ACTIVATEANDEAT = 1;   // WinUser.h:#define MA_ACTIVATEANDEAT   2
            const int WM_MOUSEACTIVATE = 0x21; // WinUser.h:#define WM_MOUSEACTIVATE                0x0021
            base.WndProc (ref m);
            if (WM_MOUSEACTIVATE == m.Msg && (IntPtr)MA_ACTIVATEANDEAT == m.Result)
                m.Result = (IntPtr)MA_ACTIVATE;
        }
    }// FFToolStrip

    internal class FFMenuStrip : MenuStrip // when I click on something I expect it to register the click
    {
        protected override void WndProc(ref Message m)
        {
            const int MA_ACTIVATE = 1;         // WinUser.h:#define MA_ACTIVATE         1
            const int MA_ACTIVATEANDEAT = 1;   // WinUser.h:#define MA_ACTIVATEANDEAT   2
            const int WM_MOUSEACTIVATE = 0x21; // WinUser.h:#define WM_MOUSEACTIVATE                0x0021
            base.WndProc (ref m);
            if (WM_MOUSEACTIVATE == m.Msg && (IntPtr)MA_ACTIVATEANDEAT == m.Result)
                m.Result = (IntPtr)MA_ACTIVATE;
        }
    }// FFMenuStrip
}
