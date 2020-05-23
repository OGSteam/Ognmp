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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ognmp.Programs;
using Ognmp.Properties; //using System.Text.RegularExpressions;

namespace Ognmp.UI
{
    public partial class MainFrm : Form
    {
        private readonly NotifyIcon _ni = new NotifyIcon();
        private MariaDbProgram _mariaDb;
        private ContextMenuStrip _mariaDbConfigContextMenuStrip, _mariaDbLogContextMenuStrip;

        private NginxProgram _nginx;

        private ContextMenuStrip _nginxConfigContextMenuStrip, _nginxLogContextMenuStrip;
        private PhpProgram _php;
        private ContextMenuStrip _phpConfigContextMenuStrip, _phpLogContextMenuStrip;
        private bool _visiblecore = true;

        public MainFrm()
        {
            if (Settings.Default.StartMinimizedToTray)
            {
                Visible = false;
                Hide();
            }

            InitializeComponent();
            Log.SetLogComponent(logRichTextBox);
            Log.Notice("Initializing Control Panel");
            Log.Notice("Ognmp Version: " + Application.ProductVersion);
            Log.Notice("Ognmp Directory: " + Program.StartupPath);
            SetupNginx();
            SetupMariaDb();
            SetupPhp();
            SetupConfigAndLogMenuStrips();
            SetupTrayMenu();

            if (Settings.Default.StartMinimizedToTray)
            {
                _visiblecore = false;
                base.SetVisibleCore(false);
            }

            if (Settings.Default.StartNginxOnLaunch) _nginx.Start();

            if (Settings.Default.StartMariaDBOnLaunch) _mariaDb.Start();

            if (Settings.Default.StartPHPOnLaunch) _php.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style &= ~0x00040000; // Remove WS_THICKFRAME (Disables resizing)
                return cp;
            }
        }

        private void SetupNginx()
        {
            _nginx = new NginxProgram(Program.StartupPath + "\\nginx.exe")
            {
                ProgLogSection = Log.LogSection.Nginx,
                StartArgs = "",
                StopArgs = "-s stop",
                ConfDir = Program.StartupPath + "\\conf\\",
                LogDir = Program.StartupPath + "\\logs\\"
            };
        }

        private void SetupMariaDb()
        {
            _mariaDb = new MariaDbProgram(Program.StartupPath + "\\mariadb\\bin\\mysqld.exe")
            {
                ProgLogSection = Log.LogSection.MariaDb,
                StartArgs = "--install-manual Ognmp-MariaDB",
                StopArgs = "/c sc delete Ognmp-MariaDB",
                ConfDir = Program.StartupPath + "\\mariadb\\",
                LogDir = Program.StartupPath + "\\mariadb\\data\\"
            };
        }

        /*private void SetCurlCaPath()
        {
            string phpini = Program.StartupPath + "/php/" + Properties.Settings.Default.PHPVersion + "php.ini";
            if (!File.Exists(phpini))
                return;

            string[] file = File.ReadAllLines(phpini);
            for (int i = 0; i < file.Length; i++)
            {
                if (file[i].Contains("curl.cainfo") == false)
                    continue;

                Regex reg = new Regex("(curl\\.cainfo).*?(=)");
                string original = reg.Match(file[i]).ToString();
                if (original == String.Empty)
                    continue;
                string replace = "curl.cainfo = " + "\"" + Program.StartupPath + @"\contrib\cacert.pem" + "\"";

                file[i] = replace;
            }

            using (var sw = new StreamWriter(phpini))
            {
                foreach (var line in file)
                    sw.WriteLine(line);
            }
        }*/

        /// <summary>
        ///     Adds configuration files or log files to a context menu strip
        /// </summary>
        private void DirFiles(string path, string getFiles, ContextMenuStrip cms)
        {
            var dInfo = new DirectoryInfo(path);

            if (!dInfo.Exists)
                return;

            var files = dInfo.GetFiles(getFiles);
            foreach (var file in files) cms.Items.Add(file.Name);
        }

        private void SetupConfigAndLogMenuStrips()
        {
            _nginxConfigContextMenuStrip = new ContextMenuStrip();
            _nginxConfigContextMenuStrip.ItemClicked += (s, e) =>
            {
                Misc.OpenFileEditor(_nginx.ConfDir + e.ClickedItem);
            };
            _nginxLogContextMenuStrip = new ContextMenuStrip();
            _nginxLogContextMenuStrip.ItemClicked += (s, e) => { Misc.OpenFileEditor(_nginx.LogDir + e.ClickedItem); };
            _mariaDbConfigContextMenuStrip = new ContextMenuStrip();
            _mariaDbConfigContextMenuStrip.ItemClicked += (s, e) =>
            {
                Misc.OpenFileEditor(_mariaDb.ConfDir + e.ClickedItem);
            };
            _mariaDbLogContextMenuStrip = new ContextMenuStrip();
            _mariaDbLogContextMenuStrip.ItemClicked += (s, e) =>
            {
                Misc.OpenFileEditor(_mariaDb.LogDir + e.ClickedItem);
            };
            _phpConfigContextMenuStrip = new ContextMenuStrip();
            _phpConfigContextMenuStrip.ItemClicked += (s, e) => { Misc.OpenFileEditor(_php.ConfDir + e.ClickedItem); };
            _phpLogContextMenuStrip = new ContextMenuStrip();
            _phpLogContextMenuStrip.ItemClicked += (s, e) => { Misc.OpenFileEditor(_php.LogDir + e.ClickedItem); };
            DirFiles(_nginx.ConfDir, "*.conf", _nginxConfigContextMenuStrip);
            DirFiles(_mariaDb.ConfDir, "my.ini", _mariaDbConfigContextMenuStrip);
            DirFiles(_php.ConfDir, "php.ini", _phpConfigContextMenuStrip);
            DirFiles(_nginx.LogDir, "*.log", _nginxLogContextMenuStrip);
            DirFiles(_mariaDb.LogDir, "*.err", _mariaDbLogContextMenuStrip);
            DirFiles(_php.LogDir, "*.log", _phpLogContextMenuStrip);
        }

        public void SetupPhp()
        {
            var phpVersion = Settings.Default.PHPVersion;
            _php = new PhpProgram(Program.StartupPath + "\\php\\" + phpVersion + "\\php-cgi.exe")
            {
                ProgLogSection = Log.LogSection.Php,
                ConfDir = Program.StartupPath + "\\php\\" + phpVersion + "\\",
                LogDir = Program.StartupPath + "\\php\\" + phpVersion + "\\logs\\"
            };
        }


        private MenuItem CreateWnmpProgramMenuItem(OgnmpProgram prog)
        {
            var item = new MenuItem {Text = Log.LogSectionToString(prog.ProgLogSection)};

            var start = item.MenuItems.Add("Start");
            start.Click += (s, e) => { prog.Start(); };
            var stop = item.MenuItems.Add("Stop");
            stop.Click += (s, e) => { prog.Stop(); };
            var restart = item.MenuItems.Add("Restart");
            restart.Click += (s, e) => { prog.Restart(); };

            return item;
        }

        private void SetupTrayMenu()
        {
            var controlpanel = new MenuItem("Ognmp Control Panel");
            controlpanel.Click += (s, e) =>
            {
                _visiblecore = true;
                base.SetVisibleCore(true);
                WindowState = FormWindowState.Normal;
                Show();
            };
            var cm = new ContextMenu();
            cm.MenuItems.Add(controlpanel);
            cm.MenuItems.Add("-");
            cm.MenuItems.Add(CreateWnmpProgramMenuItem(_nginx));
            cm.MenuItems.Add(CreateWnmpProgramMenuItem(_mariaDb));
            cm.MenuItems.Add(CreateWnmpProgramMenuItem(_php));
            cm.MenuItems.Add("-");
            var exit = new MenuItem("Exit");
            exit.Click += (s, e) => { Application.Exit(); };
            cm.MenuItems.Add(exit);
            cm.MenuItems.Add("-");
            _ni.ContextMenu = cm;
            _ni.Icon = Resources.Ognmp;
            _ni.Click += (s, e) =>
            {
                _visiblecore = true;
                base.SetVisibleCore(true);
                WindowState = FormWindowState.Normal;
                Show();
            };
            _ni.Visible = true;
        }

        protected override void SetVisibleCore(bool value)
        {
            if (_visiblecore == false)
            {
                value = false;
                if (!IsHandleCreated)
                    CreateHandle();
            }

            base.SetVisibleCore(value);
        }

        /* Menu */

        /* File */

        private void WnmpOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var optionForm = new OptionsFrm(this);
            optionForm.ShowDialog(this);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /* Applications Group Box */

        private void CtxButton(object sender, ContextMenuStrip contextMenuStrip)
        {
            var btnSender = (Button) sender;
            var ptLowerLeft = new Point(0, btnSender.Height);
            ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
            contextMenuStrip.Show(ptLowerLeft);
        }

        private void NginxStartButton_Click(object sender, EventArgs e)
        {
            _nginx.Start();
        }

        private void MariadbStartButton_Click(object sender, EventArgs e)
        {
            _mariaDb.Start();
        }

        private void PhpStartButton_Click(object sender, EventArgs e)
        {
            _php.Start();
        }

        private void NginxStopButton_Click(object sender, EventArgs e)
        {
            _nginx.Stop();
        }

        private void MariadbStopButton_Click(object sender, EventArgs e)
        {
            _mariaDb.Stop();
        }

        private void PhpStopButton_Click(object sender, EventArgs e)
        {
            _php.Stop();
        }

        private void NginxRestartButton_Click(object sender, EventArgs e)
        {
            _nginx.Restart();
        }

        private void MariadbRestartButton_Click(object sender, EventArgs e)
        {
            _mariaDb.Restart();
        }

        private void PhpRestartButton_Click(object sender, EventArgs e)
        {
            _php.Restart();
        }

        private void NginxConfigButton_Click(object sender, EventArgs e)
        {
            CtxButton(sender, _nginxConfigContextMenuStrip);
        }

        private void MariadbConfigButton_Click(object sender, EventArgs e)
        {
            CtxButton(sender, _mariaDbConfigContextMenuStrip);
        }

        private void PhpConfigButton_Click(object sender, EventArgs e)
        {
            CtxButton(sender, _phpConfigContextMenuStrip);
        }

        private void NginxLogButton_Click(object sender, EventArgs e)
        {
            CtxButton(sender, _nginxLogContextMenuStrip);
        }

        private void MariadbLogButton_Click(object sender, EventArgs e)
        {
            CtxButton(sender, _mariaDbLogContextMenuStrip);
        }

        private void PhpLogButton_Click(object sender, EventArgs e)
        {
            CtxButton(sender, _phpLogContextMenuStrip);
        }

        /* */

        private void StopAll()
        {
            _nginx.Stop();
            _mariaDb.Stop();
            _php.Stop();
        }

        private void StartAllButton_Click(object sender, EventArgs e)
        {
            _nginx.Start();
            _mariaDb.Start();
            _php.Start();
        }

        private void StopAllButton_Click(object sender, EventArgs e)
        {
            StopAll();
        }

        private void GetHTTPHeadersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var httpHeadersFrm = new HttpHeadersFrm()
            {
                StartPosition = FormStartPosition.CenterParent
            };
            httpHeadersFrm.Show(this);
        }

        private void HostToIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var hostToIpFrm = new HostToIpFrm()
            {
                StartPosition = FormStartPosition.CenterParent
            };
            hostToIpFrm.Show(this);
        }

        private void WebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Misc.StartProcessAsync("https://ogsteam.fr");
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutFrm = new AboutFrm()
            {
                StartPosition = FormStartPosition.CenterParent
            };
            aboutFrm.ShowDialog(this);
        }

        private void ReportBugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Misc.StartProcessAsync("https://github.com/darknoon29/onmp/issues/new");
        }

        private void SetRunningStatusLabel(Label label, bool running)
        {
            if (running)
            {
                label.Text = @"✓";
                label.ForeColor = Color.Green;
            }
            else
            {
                label.Text = @"X";
                label.ForeColor = Color.DarkRed;
            }
        }

        private void AppsRunningTimer_Tick(object sender, EventArgs e)
        {
            SetRunningStatusLabel(nginxrunning, _nginx.IsRunning());
            SetRunningStatusLabel(phprunning, _php.IsRunning());
            SetRunningStatusLabel(mariadbrunning, _mariaDb.IsRunning());
        }

        private void LocalhostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Misc.StartProcessAsync("http://localhost");
        }

        private void OpenMariaDBShellButton_Click(object sender, EventArgs e)
        {
            _mariaDb.OpenShell();
        }

        private void WnmpMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void WnmpDirButton_Click(object sender, EventArgs e)
        {
            Misc.StartProcessAsync("explorer.exe", Program.StartupPath);
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && Settings.Default.MinimizeInsteadOfClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                Settings.Default.Save();
            }
        }

        private void MainFrm_Resize(object sender, EventArgs e)
        {
            if (Settings.Default.MinimizeToTray == false)
                return;

            if (WindowState == FormWindowState.Minimized)
                Hide();
        }
    }
}