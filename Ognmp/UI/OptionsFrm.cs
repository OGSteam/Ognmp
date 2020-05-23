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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using Ognmp.Programs;
using Ognmp.Properties;

namespace Ognmp.UI
{
    /// <summary>
    ///     Form that allows configuring Wnmp options.
    /// </summary>
    public partial class OptionsFrm : Form
    {
        private readonly MainFrm mainForm;
        private readonly PhpConfigurationManager PHPConfigurationMgr = new PhpConfigurationManager();
        private string Editor;

        public OptionsFrm(MainFrm form)
        {
            mainForm = form;
            InitializeComponent();
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

        /* Options releated functions */

        /// <summary>
        ///     Populates the options with there saved values
        /// </summary>
        private void UpdateOptions()
        {
            editorTB.Text = Settings.Default.TextEditor;
            StartWnmpWithWindows.Checked = Settings.Default.StartWithWindows;
            StartNginxLaunchCB.Checked = Settings.Default.StartNginxOnLaunch;
            StartMySQLLaunchCB.Checked = Settings.Default.StartMariaDBOnLaunch;
            StartPHPLaunchCB.Checked = Settings.Default.StartPHPOnLaunch;
            StartMinimizedToTray.Checked = Settings.Default.StartMinimizedToTray;
            MinimizeWnmpToTray.Checked = Settings.Default.MinimizeToTray;
            PHP_PROCESSES.Value = Settings.Default.PHPProcessCount;
            PHP_PORT.Value = Settings.Default.PHPPort;
            MinimizeToTrayInsteadOfClosing.Checked = Settings.Default.MinimizeInsteadOfClosing;
            foreach (var str in PhpVersions()) phpBin.Items.Add(str);

            phpBin.SelectedIndex = phpBin.Items.IndexOf(Settings.Default.PHPVersion);
        }

        private void Options_Load(object sender, EventArgs e)
        {
            UpdateOptions();
        }

        private void SetSettings()
        {
            Settings.Default.TextEditor = editorTB.Text;
            Settings.Default.StartWithWindows = StartWnmpWithWindows.Checked;
            Settings.Default.StartNginxOnLaunch = StartNginxLaunchCB.Checked;
            Settings.Default.StartMariaDBOnLaunch = StartMySQLLaunchCB.Checked;
            Settings.Default.StartPHPOnLaunch = StartPHPLaunchCB.Checked;
            Settings.Default.StartMinimizedToTray = StartMinimizedToTray.Checked;
            Settings.Default.MinimizeToTray = MinimizeWnmpToTray.Checked;
            Settings.Default.MinimizeInsteadOfClosing = MinimizeToTrayInsteadOfClosing.Checked;
            Settings.Default.PHPProcessCount = (uint) PHP_PROCESSES.Value;
            Settings.Default.PHPPort = (ushort) PHP_PORT.Value;
            StartWithWindows();
            UpdateNgxPhpConfig();
            Settings.Default.PHPVersion = phpBin.Text;
            Save_PHPExtOptions();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            SetSettings();
            Settings.Default.Save();
            /* Setup custom PHP without restart */
            mainForm.SetupPhp();
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /* Editor releated functions */

        private void SetEditor()
        {
            var input = "";
            var dialog = new OpenFileDialog
            {
                Filter = "executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select a text editor"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
                input = dialog.FileName;

            editorTB.Text = dialog.FileName;
            Editor = dialog.FileName;

            if (input == "")
                Editor = "notepad.exe";
            editorTB.Text = Editor;
        }

        private void Selecteditor_Click(object sender, EventArgs e)
        {
            SetEditor();
        }

        private string[] PhpVersions()
        {
            if (Directory.Exists(Program.StartupPath + "/php") == false)
                return new string[0];
            return Directory.GetDirectories(Program.StartupPath + "/php").Select(d => new DirectoryInfo(d).Name)
                .ToArray();
        }

        private void UpdateNgxPhpConfig()
        {
            var port = (short) PHP_PORT.Value;

            using (var sw = new StreamWriter(Program.StartupPath + "/conf/php_processes.conf"))
            {
                sw.WriteLine("# DO NOT MODIFY!!! THIS FILE IS GENERATED AUTOMATICALLY.\r\n");
                sw.WriteLine("upstream php_processes {");
                sw.WriteLine("    server 127.0.0.1:" + port + " weight=1;");
                sw.WriteLine("}");
            }
        }


        private void StartWithWindows()
        {
            var root = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (root == null)
                return;
            if (StartWnmpWithWindows.Checked)
            {
                if (root.GetValue("Ognmp") == null)
                    root.SetValue("Ognmp", "\"" + Application.ExecutablePath + "\"");
            }
            else
            {
                if (root.GetValue("Ognmp") != null)
                    root.DeleteValue("Ognmp");
            }
        }

        /* PHP Extensions Manager */

        private void Save_PHPExtOptions()
        {
            for (var i = 0; i < phpExtListBox.Items.Count; i++)
                PHPConfigurationMgr.PhpExtensions[i].Enabled = phpExtListBox.GetItemChecked(i);

            PHPConfigurationMgr.SavePhpIniOptions();
        }

        private void PhpBin_SelectedIndexChanged(object sender, EventArgs e)
        {
            phpExtListBox.Items.Clear();
            PHPConfigurationMgr.LoadPhpExtensions(phpBin.Text);

            foreach (var ext in PHPConfigurationMgr.PhpExtensions)
                phpExtListBox.Items.Add(ext.Name, ext.Enabled);
        }
    }
}