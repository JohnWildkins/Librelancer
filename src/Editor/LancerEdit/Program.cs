﻿/* The contents of this file are subject to the Mozilla Public License
 * Version 1.1 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS"
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
 * License for the specific language governing rights and limitations
 * under the License.
 * 
 * 
 * The Initial Developer of the Original Code is Callum McGing (mailto:callum.mcging@gmail.com).
 * Portions created by the Initial Developer are Copyright (C) 2013-2018
 * the Initial Developer. All Rights Reserved.
 */
using System;
using System.IO;
using System.Runtime.InteropServices;
using LibreLancer;
namespace LancerEdit
{
	class Program
	{
        [DllImport("kernel32.dll")]
        static extern bool SetDllDirectory(string directory);

        [STAThread]
        static void Main(string[] args)
		{
            if (Platform.RunningOS == OS.Windows)
            {
                string bindir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                var fullpath = Path.Combine(bindir, IntPtr.Size == 8 ? "x64" : "x86");
                SetDllDirectory(fullpath);
            }
            //Apparently some windows installs are screwy enough that they won't fail on
            //unsupported configs. So allow DX9 to be first choice.
            bool useDX9 = (args.Length > 0 && args[0] == "--dx9");
            new MainWindow(useDX9).Run();
		}
	}
}
