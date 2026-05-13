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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ognmp.UI;

namespace Ognmp
{
    public static class Program
    {
        public static readonly string StartupPath = Application.StartupPath;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.ThreadException += ApplicationThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Log.Notice("Starting Ognmp");
            Log.Notice("Log file: " + Log.CurrentLogFilePath);
            Application.Run(new MainFrm());
            Log.Notice("Ognmp stopped");
        }

        private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Error(e.Exception);
            MessageBox.Show(
                "An unexpected error occurred. Details were written to:\n" + Log.CurrentLogFilePath,
                "Ognmp Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Log.Error(exception);
            }
            else
            {
                Log.Error("Unhandled non-exception error: " + e.ExceptionObject);
            }
        }

        private static void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception);
            e.SetObserved();
        }
    }
}
