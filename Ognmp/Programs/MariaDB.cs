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
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace Ognmp.Programs
{
    public class MariaDbProgram : OgnmpProgram
    {
        private const string ServiceName = "Ognmp-MariaDB";
        private readonly ServiceController _mariaDbController = new ServiceController();

        public MariaDbProgram(string exeFile) : base(exeFile)
        {
            /* Set MariaDB service details */
            _mariaDbController.MachineName = Environment.MachineName;
            _mariaDbController.ServiceName = ServiceName;
        }

        private void RemoveService()
        {
            try
            {
                _mariaDbController.Close();
                StartProcess("cmd.exe", StopArgs, true);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        private void InstallService()
        {
            if (!File.Exists(ExeFileName))
            {
                Log.Error("File " + ExeFileName + " not found.", ProgLogSection);
                return;
            }

            if (ServiceExists())
                RemoveService();
            StartProcess(ExeFileName, StartArgs, true);
        }

        private bool ServiceExists()
        {
            var services = ServiceController.GetServices();
            foreach (var t in services)
                if (t.ServiceName == ServiceName)
                    return true;

            return false;
        }

        public void OpenShell()
        {
            if (IsRunning() == false)
                Start();

            Process.Start(Program.StartupPath + "/mariadb/bin/mysql.exe", "-u root -p");
            Log.Notice("Started MariaDB shell", ProgLogSection);
        }

        public override void Start()
        {
            try
            {
                InstallService();
                _mariaDbController.Start();
                Log.Notice("Started", ProgLogSection);
            }
            catch (Exception ex)
            {
                Log.Error("Start():" + ex.Message, ProgLogSection);
            }
        }

        public override void Stop()
        {
            try
            {
                _mariaDbController.Stop();
                RemoveService();
                Log.Notice("Stopped", ProgLogSection);
            }
            catch (Exception ex)
            {
                Log.Error("Stop():" + ex.Message, ProgLogSection);
            }
        }

        public override bool IsRunning()
        {
            try
            {
                _mariaDbController.Refresh();
                return _mariaDbController.Status == ServiceControllerStatus.Running;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}