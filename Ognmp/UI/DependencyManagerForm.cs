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
using Ognmp.Components.Events;

namespace Ognmp.UI
{
    public partial class DependencyManagerForm : Form
    {
        private readonly IDependencyManager _dependencyManager;
        private CancellationTokenSource _cancellationTokenSource;

        public DependencyManagerForm(IDependencyManager dependencyManager)
        {
            InitializeComponent();
            _dependencyManager = dependencyManager ?? throw new ArgumentNullException(nameof(dependencyManager));
            
            _dependencyManager.DownloadProgressChanged += OnDownloadProgressChanged;
            _dependencyManager.ComponentInstalled += OnComponentInstalled;
            _dependencyManager.ErrorOccurred += OnErrorOccurred;
            
            Load += async (s, e) => await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            UpdateStatus("Loading available versions...");
            SetControlsEnabled(false);

            try
            {
                await _dependencyManager.LoadAvailableVersionsAsync();
                LoadComponents();
                UpdateStatus("Ready");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading versions: {ex.Message}");
                Log.Error($"Failed to initialize dependency manager: {ex.Message}");
            }
            finally
            {
                SetControlsEnabled(true);
            }
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
            this.versionLabel = new Label();
            this.versionComboBox = new ComboBox();
            this.refreshButton = new Button();
            
            this.SuspendLayout();
            
            // componentsListView
            this.componentsListView.Columns.AddRange(new[] {
                this.nameColumn, this.versionColumn, this.statusColumn, this.descriptionColumn
            });
            this.componentsListView.FullRowSelect = true;
            this.componentsListView.GridLines = true;
            this.componentsListView.Location = new Point(12, 12);
            this.componentsListView.MultiSelect = false;
            this.componentsListView.Name = "componentsListView";
            this.componentsListView.Size = new Size(760, 250);
            this.componentsListView.TabIndex = 0;
            this.componentsListView.View = View.Details;
            this.componentsListView.SelectedIndexChanged += ComponentsListView_SelectedIndexChanged;
            
            // nameColumn
            this.nameColumn.Text = "Component";
            this.nameColumn.Width = 120;
            
            // versionColumn
            this.versionColumn.Text = "Installed Version";
            this.versionColumn.Width = 120;
            
            // statusColumn
            this.statusColumn.Text = "Status";
            this.statusColumn.Width = 100;
            
            // descriptionColumn
            this.descriptionColumn.Text = "Description";
            this.descriptionColumn.Width = 400;
            
            // refreshButton
            this.refreshButton.Location = new Point(12, 271);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new Size(90, 30);
            this.refreshButton.TabIndex = 9;
            this.refreshButton.Text = "Refresh List";
            this.refreshButton.Click += RefreshButton_Click;
            
            // versionLabel
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new Point(110, 278);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new Size(90, 15);
            this.versionLabel.TabIndex = 7;
            this.versionLabel.Text = "Select Version:";
            
            // versionComboBox
            this.versionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.versionComboBox.Enabled = false;
            this.versionComboBox.FormattingEnabled = true;
            this.versionComboBox.Location = new Point(200, 275);
            this.versionComboBox.Name = "versionComboBox";
            this.versionComboBox.Size = new Size(350, 23);
            this.versionComboBox.TabIndex = 8;
            
            // installButton
            this.installButton.Enabled = false;
            this.installButton.Location = new Point(560, 273);
            this.installButton.Name = "installButton";
            this.installButton.Size = new Size(100, 28);
            this.installButton.TabIndex = 1;
            this.installButton.Text = "Install Selected";
            this.installButton.Click += InstallButton_Click;
            
            // installAllButton
            this.installAllButton.Location = new Point(12, 310);
            this.installAllButton.Name = "installAllButton";
            this.installAllButton.Size = new Size(120, 28);
            this.installAllButton.TabIndex = 2;
            this.installAllButton.Text = "Install All (Latest)";
            this.installAllButton.Click += InstallAllButton_Click;
            
            // cancelButton
            this.cancelButton.Enabled = false;
            this.cancelButton.Location = new Point(672, 273);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(100, 28);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += CancelButton_Click;
            
            // closeButton
            this.closeButton.Location = new Point(672, 310);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new Size(100, 28);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Close";
            this.closeButton.Click += (s, e) => Close();
            
            // progressBar
            this.progressBar.Location = new Point(12, 348);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(760, 23);
            this.progressBar.TabIndex = 5;
            
            // statusLabel
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new Point(12, 378);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(39, 15);
            this.statusLabel.TabIndex = 6;
            this.statusLabel.Text = "Ready";
            
            // DependencyManagerForm
            this.ClientSize = new Size(784, 410);
            this.Controls.Add(this.versionComboBox);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.installAllButton);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.componentsListView);
            this.Controls.Add(this.refreshButton);
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
        private Label versionLabel;
        private ComboBox versionComboBox;
        private Button refreshButton;

        private void LoadComponents()
        {
            componentsListView.Items.Clear();
            
            foreach (var componentGroup in _dependencyManager.AvailableVersions)
            {
                var componentName = componentGroup.Key;
                var versions = componentGroup.Value;
                var latestVersion = versions.First();

                var item = new ListViewItem(componentName);
                
                var isInstalled = _dependencyManager.IsComponentInstalled(componentName);
                var versionText = isInstalled ? latestVersion.Version : "Not installed";
                item.SubItems.Add(versionText);
                
                var statusText = isInstalled ? "Installed" : "Not Installed";
                var statusItem = item.SubItems.Add(statusText);
                
                if (isInstalled)
                    statusItem.ForeColor = Color.Green;
                else
                    statusItem.ForeColor = Color.Red;
                
                item.SubItems.Add(latestVersion.Description);
                item.Tag = componentName;
                
                componentsListView.Items.Add(item);
            }
        }

        private void ComponentsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var hasSelection = componentsListView.SelectedItems.Count > 0;
            installButton.Enabled = hasSelection;
            versionComboBox.Enabled = hasSelection;
            
            if (hasSelection)
            {
                var componentName = componentsListView.SelectedItems[0].Tag.ToString();
                LoadVersionsForComponent(componentName);
            }
            else
            {
                versionComboBox.Items.Clear();
            }
        }

        private void LoadVersionsForComponent(string componentName)
        {
            versionComboBox.Items.Clear();
            
            var versions = _dependencyManager.GetVersionsForComponent(componentName);
            foreach (var version in versions)
            {
                versionComboBox.Items.Add($"{version.Version} - {version.Description}");
            }
            
            if (versionComboBox.Items.Count > 0)
            {
                versionComboBox.SelectedIndex = 0;
            }
        }

        private async void InstallButton_Click(object sender, EventArgs e)
        {
            if (componentsListView.SelectedItems.Count == 0)
                return;

            var componentName = componentsListView.SelectedItems[0].Tag.ToString();
            var versions = _dependencyManager.GetVersionsForComponent(componentName);
            
            if (versionComboBox.SelectedIndex < 0 || versionComboBox.SelectedIndex >= versions.Count)
            {
                MessageBox.Show("Please select a version", "Version Required", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedVersion = versions[versionComboBox.SelectedIndex].Version;
            await InstallComponentAsync(componentName, selectedVersion);
        }

        private async void InstallAllButton_Click(object sender, EventArgs e)
        {
            var componentNames = _dependencyManager.AvailableVersions.Keys.ToList();
            
            foreach (var componentName in componentNames)
            {
                await InstallComponentAsync(componentName, null); // null = use latest
                if (_cancellationTokenSource?.Token.IsCancellationRequested == true)
                    break;
            }
        }

        private async Task InstallComponentAsync(string componentName, string version)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            SetControlsEnabled(false);
            
            try
            {
                var versionText = version ?? "latest";
                UpdateStatus($"Installing {componentName} {versionText}...");
                progressBar.Value = 0;
                
                var success = await _dependencyManager.DownloadAndInstallComponentAsync(
                    componentName,
                    version,
                    _cancellationTokenSource.Token);
                
                if (!success && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    MessageBox.Show(
                        $"Failed to install {componentName}. Check the log for details.",
                        "Installation Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                
                LoadComponents();
                UpdateStatus($"{componentName} installation completed");
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

        private void OnErrorOccurred(object sender, DependencyErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => OnErrorOccurred(sender, e));
                return;
            }
            
            UpdateStatus($"Error: {e.Message}");
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
            refreshButton.Enabled = enabled;
            cancelButton.Enabled = !enabled;
            closeButton.Enabled = enabled;
            componentsListView.Enabled = enabled;
            versionComboBox.Enabled = enabled && componentsListView.SelectedItems.Count > 0;
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

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            UpdateStatus("Refreshing component list...");

            try
            {
                await _dependencyManager.LoadAvailableVersionsAsync();
                LoadComponents();
                UpdateStatus("Component list refreshed");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error refreshing list: {ex.Message}");
                Log.Error($"Failed to refresh component list: {ex.Message}");
            }
        }
    }
}
