// /*
//  * Copyright (c) 2012 - 2017, Kurt Cancemi (kurt@x64architecture.com)
//  * Copyright (c) 2017 - 2020, OGSteam.fr (darknoon@darkcity.fr)
//  *
//  * This file is part of Ognmp.
//  *
//  *  Ognmp is free software: you can redistribute it and/or modify
//  *  it under the terms of the GNU General Public License as published by
//  *  the Free Software Foundation, either version 3 of the License, or
//  *  (at your option) any later version.
//  *
//  *  Ognmp is distributed in the hope that it will be useful,
//  *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  *  GNU General Public License for more details.
//  *
//  *  You should have received a copy of the GNU General Public License
//  *  along with Ognmp.  If not, see <http://www.gnu.org/licenses/>.
// */

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Ognmp.Programs
{
    public class PhpConfigurationManager
    {
        private string _iniFilePath;
        private string[] _tmpIniFile;

        public List<PhpExtension> PhpExtensions;

        private void LoadPhpIni()
        {
            _tmpIniFile = File.ReadAllLines(_iniFilePath);
        }

        public void LoadPhpExtensions(string phpBinPath)
        {
            if (phpBinPath == "Default")
                _iniFilePath = Program.StartupPath + "/php/php.ini";
            else
                _iniFilePath = Program.StartupPath + "/php/" + phpBinPath + "/php.ini";

            LoadPhpIni();
            PhpExtensions = new List<PhpExtension>();

            for (var linenum = 0; linenum < _tmpIniFile.Length; linenum++)
            {
                var str = _tmpIniFile[linenum].Trim();
                if (str == string.Empty)
                    continue;
                if (str[0] == ';')
                {
                    var tmp = str.Substring(1);
                    if (!tmp.StartsWith("extension") && !tmp.StartsWith("zend_extension"))
                        continue;
                }

                // (zend_extension|extension)\s*\=\s*["]?(.*?\.dll)
                var m = Regex.Match(str, @"(zend_extension|extension)(=)((?:[a-z][a-z0-9_]*))");
                if (m.Success)
                {
                    var ext = new PhpExtension()
                    {
                        Name = m.Groups[3].Value,
                        ZendExtension = m.Groups[1].Value == "zend_extension",
                        Enabled = str[0] != ';',
                        LineNum = linenum
                    };
                    PhpExtensions.Add(ext);
                }
            }
        }

        public void SavePhpIniOptions()
        {
            foreach (var ext in PhpExtensions)
            {
                var extensionToken = ext.ZendExtension ? "zend_extension" : "extension";
                _tmpIniFile[ext.LineNum] = $"{(ext.Enabled ? "" : ";")}{extensionToken}={ext.Name}";
            }

            File.WriteAllLines(_iniFilePath, _tmpIniFile);
        }

        public class PhpExtension
        {
            public bool Enabled;
            public int LineNum;
            public string Name;
            public bool ZendExtension;
        }
    }
}