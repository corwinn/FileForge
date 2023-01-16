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

// A tool for crafting file formats.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace File_Forge
{
    static class Program
    {
        [STAThread]
        static void Main() { The_main_entry_point_for_the_application (); }

        static void The_main_entry_point_for_the_application()
        {
            try
            {
                if (!CheckReferenceAssemblies ()) return; // don't scare the user

                FFSettings.FileLog.Start ();

                Application.ApplicationExit += (a, b) => Store ();
                Application.ThreadException += (a, e) => Bug ("Bug: Unhandled exception:\n\n" + e.Exception.ToString ());

                // because one GDI renders text better than another GDI
                Application.EnableVisualStyles ();
                Application.SetCompatibleTextRenderingDefault (false);

                Application.Run (Restore ());
            }
            finally
            {
                FFSettings.FileLog.Stop ();
                Wind.Log.WLog.Stop (); //TODO services (that need to stop no matter what)
            }
        }// The_main_entry_point_for_the_application

        //TODO ISerializable
        static Form _main_form = null;
        static void DontStore() { _main_form = null; }
        static void Store() { if (null == _main_form) return; }
        static Form Restore() { return Wind.Log.WLog.ProgramMainForm = _main_form = new FormMain (); }

        public static void Bug(string msg)
        {
            MessageBox.Show (text: msg, caption: "Bug", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Stop);
            DontStore ();
            Application.Exit ();
        }

        /// <summary>Returns true when all referred assemblies are availble.</summary>
        static bool CheckReferenceAssemblies()
        {
            foreach (var ra in Assembly.GetExecutingAssembly ().GetReferencedAssemblies ())
                try
                {
                    Assembly.ReflectionOnlyLoad (ra.FullName);
                }
                catch
                {
                    var msg = string.Format ("Failed to load: \"{0}\"{1}Either install it into the GAC, or copy the needed files in this directory",
                                             ra.FullName, Environment.NewLine);
                    Console.WriteLine (msg);
                    MessageBox.Show (text: msg,
                                     caption: "Missing reference assembly",
                                     buttons: MessageBoxButtons.OK,
                                     icon: MessageBoxIcon.Stop);
                    return false;
                }
            return true;
        }// CheckReferenceAssemblies()

        public static List<string> AUTHORS = new List<string> ()
        {
            "This program is being crafted by:",
            "the wind",
            "and many others I guess",
            "Thank you.",
            "You can press Esc now.",
            " ",
            " ",
            " ",
            "Well, obviously you didn't :)",
            " ",
            "Do you happen to know the story,",
            "about the humans",
            "that have built a starship,",
            "and they have broke free",
            "of the pale blue prison?"
        };
    }// Program
}
