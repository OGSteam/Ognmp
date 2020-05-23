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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Ognmp.Properties;

namespace Ognmp.Programs
{
    public class PhpProgram : OgnmpProgram
    {
        private Socket _sock;

        public PhpProgram(string exeFile) : base(exeFile)
        {
        }

        private static string GetPhpIniPath()
        {
            return Program.StartupPath + "/php/" + Settings.Default.PHPVersion + "/php.ini";
        }

        public override void Start()
        {
            var processCount = Settings.Default.PHPProcessCount;
            var port = Settings.Default.PHPPort;
            var phpini = GetPhpIniPath();

            if (IsRunning())
            {
                Log.Error("Already running.", ProgLogSection);
                return;
            }

            _sock?.Close();
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sock.Bind(new IPEndPoint(IPAddress.Any, port));
            _sock.Listen(16384);
            var envVars = new Dictionary<string, string>
            {
                {"PHP_FCGI_MAX_REQUESTS", "0"}
            };

            try
            {
                for (var i = 1; i <= processCount; i++)
                {
                    StartProcess(ExeFileName, $"-b localhost:{port} -c {phpini}", false, envVars);
                    Log.Notice("Starting PHP " + i + "/" + processCount, ProgLogSection);
                }

                Log.Notice("PHP started", ProgLogSection);
            }
            catch (Exception ex)
            {
                Log.Error("StartPHP(): " + ex.Message, ProgLogSection);
            }
        }
    }
}