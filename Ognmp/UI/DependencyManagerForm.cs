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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ognmp.Components;

namespace Ognmp.UI
{
    public partial class DependencyManagerForm : Form
    {
        private readonly DependencyManager _dependencyManager;
        private CancellationTokenSource _cancellationTokenSource;

        public DependencyManagerForm()
        {
            InitializeComponent();
            _dependencyManager = new DependencyManager(Program.StartupPath);
            
            _dependencyManager.DownloadProgressChanged += OnDownloadProgressChanged;
            _dependencyManager.ComponentInstalled += OnComponentInstalled;
            _dependencyManager.ErrorOccurred += OnErrorOccurred;
            
            LoadComponents();
        }

        private void InitializeComponent()
        {
            this.componentsListView = new ListView();
            this.nameColumn = new ColumnHeader();
            this.versionColumn = new ColumnHeader();
            this.statusColumn = new ColumnHeader();
            this.descriptionColumn = new ColumnHeader();
            this.installButton = new Button();
            this.installAllButton = new Button();
            this.progressBar = new ProgressBar();
            this.statusLabel = new Label();
            this.cancelButton = new Button();
            this.closeButton = new Button();
            
            this.SuspendLayout();
            
            // componentsListView
            this.componentsListView.Columns.AddRange(new[] {
                this.nameColumn, this.versionColumn, this.statusColumn, this.descriptionColumn
            });
            this.componentsListView.FullRowSelect = true;
            this.componentsListView.GridLines = true;
            this.componentsListView.Location = new Point(12, 12);
            this.componentsListView.Name = "componentsListView";
            this.componentsListView.Size = new Size(760, 250);
            this.componentsListView.TabIndex = 0;
            this.componentsListView.View = View.Details;
            this.componentsListView.SelectedIndexChanged += ComponentsListView_SelectedIndexChanged;
            
            // nameColumn
            this.nameColumn.Text = "Component";
            this.nameColumn.Width = 120;
            
            // versionColumn
            this.versionColumn.Text = "Version";
            this.versionColumn.Width = 80;
            
            // statusColumn
            this.statusColumn.Text = "Status";
            this.statusColumn.Width = 100;
            
            // descriptionColumn
            this.descriptionColumn.Text = "Description";
            this.descriptionColumn.Width = 440;
            
            // installButton
            this.installButton.Enabled = false;
            this.installButton.Location = new Point(12, 275);
            this.installButton.Name = "installButton";
            this.installButton.Size = new Size(120, 30);
            this.installButton.TabIndex = 1;
            this.installButton.Text = "Install Selected";
            this.installButton.Click += InstallButton_Click;
            
            // installAllButton
            this.installAllButton.Location = new Point(145, 275);
            this.installAllButton.Name = "installAllButton";
            this.installAllButton.Size = new Size(120, 30);
            this.installAllButton.TabIndex = 2;
            this.installAllButton.Text = "Install All";
            this.installAllButton.Click += InstallAllButton_Click;
            
            // cancelButton
            this.cancelButton.Enabled = false;
            this.cancelButton.Location = new Point(280, 275);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(120, 30);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += CancelButton_Click;
            
            // closeButton
            this.closeButton.Location = new Point(652, 275);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new Size(120, 30);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Close";
            this.closeButton.Click += (s, e) => Close();
            
            // progressBar
            this.progressBar.Location = new Point(12, 320);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(760, 23);
            this.progressBar.TabIndex = 5;
            
            // statusLabel
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new Point(12, 350);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(0, 15);
            this.statusLabel.TabIndex = 6;
            
            // DependencyManagerForm
            this.ClientSize = new Size(784, 381);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.installAllButton);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.componentsListView);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DependencyManagerForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Ognmp - Dependency Manager";
            this.FormClosing += DependencyManagerForm_FormClosing;
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private ListView componentsListView;
        private ColumnHeader nameColumn;
        private ColumnHeader versionColumn;
        private ColumnHeader statusColumn;
        private ColumnHeader descriptionColumn;
        private Button installButton;
        private Button installAllButton;
        private Button cancelButton;
        private Button closeButton;
        private ProgressBar progressBar;
        private Label statusLabel;

        private void LoadComponents()
        {
            componentsListView.Items.Clear();
            
            foreach (var component in _dependencyManager.AvailableComponents.Values)
            {
                var item = new ListViewItem(component.Name);
                item.SubItems.Add(component.Version);
                
                var isInstalled = _dependencyManager.IsComponentInstalled(component.Name);
                var statusText = isInstalled ? "? Installed" : "Not Installed";
                var statusItem = item.SubItems.Add(statusText);
                
                if (isInstalled)
                    statusItem.ForeColor = Color.Green;
                else
                    statusItem.ForeColor = Color.Red;
                
                item.SubItems.Add(component.Description);
                item.Tag = component.Name;
                
                componentsListView.Items.Add(item);
            }
        }

        private void ComponentsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            installButton.Enabled = componentsListView.SelectedItems.Count > 0;
        }

        private async void InstallButton_Click(object sender, EventArgs e)
        {
            if (componentsListView.SelectedItems.Count == 0)
                return;

            var componentNames = componentsListView.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag.ToString())
                .ToList();

            await InstallComponentsAsync(componentNames);
        }

        private async void InstallAllButton_Click(object sender, EventArgs e)
        {
            var componentNames = _dependencyManager.AvailableComponents.Keys.ToList();
            await InstallComponentsAsync(componentNames);
        }

        private async Task InstallComponentsAsync(System.Collections.Generic.List<string> componentNames)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            SetControlsEnabled(false);
            
            try
            {
                foreach (var componentName in componentNames)
                {
                    UpdateStatus($"Installing {componentName}...");
                    progressBar.Value = 0;
                    
                    var success = await _dependencyManager.DownloadAndInstallComponentAsync(
                        componentName, 
                        _cancellationTokenSource.Token);
                    
                    if (!success && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        MessageBox.Show(
                            $"Failed to install {componentName}. Check the log for details.",
                            "Installation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                
                LoadComponents();
                UpdateStatus("Installation completed");
                progressBar.Value = 100;
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Installation cancelled");
            }
            finally
            {
                SetControlsEnabled(true);
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            UpdateStatus("Cancelling...");
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnDownloadProgressChanged(sender, e));
                return;
            }
            
            progressBar.Value = e.ProgressPercentage;
            
            var mbReceived = e.BytesReceived / (1024.0 * 1024.0);
            var mbTotal = e.TotalBytes / (1024.0 * 1024.0);
            
            UpdateStatus($"Downloading {e.ComponentName}: {mbReceived:F2} MB / {mbTotal:F2} MB ({e.ProgressPercentage}%)");
        }

        private void OnComponentInstalled(object sender, ComponentEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnComponentInstalled(sender, e));
                return;
            }
            
            UpdateStatus($"{e.Component.Name} installed successfully");
            LoadComponents();
        }

        private void OnErrorOccurred(object sender, System.IO.ErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnErrorOccurred(sender, e));
                return;
            }
            
            UpdateStatus($"Error: {e.GetException().Message}");
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
            Log.Notice(message, Log.LogSection.Ognmp);
        }

        private void SetControlsEnabled(bool enabled)
        {
            installButton.Enabled = enabled && componentsListView.SelectedItems.Count > 0;
            installAllButton.Enabled = enabled;
            cancelButton.Enabled = !enabled;
            closeButton.Enabled = enabled;
            componentsListView.Enabled = enabled;
        }

        private void DependencyManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = MessageBox.Show(
                    "Installation is in progress. Are you sure you want to cancel and close?",
                    "Confirm Close",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                
                _cancellationTokenSource.Cancel();
            }
            
            _dependencyManager?.Dispose();
        }
    }
}
