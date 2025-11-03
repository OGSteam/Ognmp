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
using System.Linq;
using System.ServiceProcess;

namespace Ognmp.Programs
{
    public class MariaDbProgram : OgnmpProgram, IDisposable
    {
        private const string ServiceName = "Ognmp-MariaDB";
        private ServiceController _mariaDbController;
        private bool _disposed;

        public MariaDbProgram(string exeFile) : base(exeFile)
        {
            InitializeServiceController();
        }

        private void InitializeServiceController()
        {
            _mariaDbController = new ServiceController
            {
                MachineName = Environment.MachineName,
                ServiceName = ServiceName
            };
        }

        private void RemoveService()
        {
            try
            {
                if (_mariaDbController != null)
                {
                    _mariaDbController.Refresh();
                    
                    if (_mariaDbController.Status != ServiceControllerStatus.Stopped)
                    {
                        _mariaDbController.Stop();
                        _mariaDbController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }

                StartProcess("cmd.exe", StopArgs, true);
                Log.Notice("Service removed successfully", ProgLogSection);
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                Log.Error("Service stop operation timed out", ProgLogSection);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to remove service: {e.Message}", ProgLogSection);
            }
        }

        private void InstallService()
        {
            if (!File.Exists(ExeFileName))
            {
                Log.Error($"File {ExeFileName} not found.", ProgLogSection);
                return;
            }

            if (ServiceExists())
            {
                Log.Notice("Service already exists, removing before reinstall", ProgLogSection);
                RemoveService();
            }

            StartProcess(ExeFileName, StartArgs, true);
            Log.Notice("Service installed successfully", ProgLogSection);
        }

        private bool ServiceExists()
        {
            try
            {
                return ServiceController.GetServices()
                    .Any(s => s.ServiceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception e)
            {
                Log.Error($"Error checking service existence: {e.Message}", ProgLogSection);
                return false;
            }
        }

        public void OpenShell()
        {
            if (!IsRunning())
            {
                Log.Notice("MariaDB is not running, starting service first", ProgLogSection);
                Start();
            }

            string mysqlPath = Path.Combine(Program.StartupPath, "mariadb", "bin", "mysql.exe");
            
            if (!File.Exists(mysqlPath))
            {
                Log.Error($"MySQL client not found at {mysqlPath}", ProgLogSection);
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = mysqlPath,
                    Arguments = "-u root -p",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Log.Notice("Started MariaDB shell", ProgLogSection);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to start MariaDB shell: {ex.Message}", ProgLogSection);
            }
        }

        public override void Start()
        {
            try
            {
                if (IsRunning())
                {
                    Log.Notice("Service is already running", ProgLogSection);
                    return;
                }

                InstallService();
                
                // Reinitialize controller to ensure it's fresh
                _mariaDbController?.Dispose();
                InitializeServiceController();
                
                _mariaDbController.Start();
                _mariaDbController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                
                Log.Notice("Started", ProgLogSection);
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                Log.Error("Service start operation timed out", ProgLogSection);
            }
            catch (Exception ex)
            {
                Log.Error($"Start(): {ex.Message}", ProgLogSection);
            }
        }

        public override void Stop()
        {
            try
            {
                if (!IsRunning())
                {
                    Log.Notice("Service is not running", ProgLogSection);
                    RemoveService();
                    return;
                }

                _mariaDbController.Stop();
                _mariaDbController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                
                RemoveService();
                Log.Notice("Stopped", ProgLogSection);
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                Log.Error("Service stop operation timed out", ProgLogSection);
            }
            catch (Exception ex)
            {
                Log.Error($"Stop(): {ex.Message}", ProgLogSection);
            }
        }

        public override bool IsRunning()
        {
            try
            {
                if (_mariaDbController == null)
                    return false;

                _mariaDbController.Refresh();
                return _mariaDbController.Status == ServiceControllerStatus.Running;
            }
            catch (InvalidOperationException)
            {
                // Service doesn't exist
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error checking service status: {ex.Message}", ProgLogSection);
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _mariaDbController?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}