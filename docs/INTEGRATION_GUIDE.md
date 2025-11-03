# Integration Guide: Adding Dependency Manager to MainFrm

## Step 1: Add Menu Item

Add a new menu item to your existing menu strip in `MainFrm.cs`:

```csharp
// In the InitializeComponent method or designer, add this to your Tools menu:
private ToolStripMenuItem dependencyManagerToolStripMenuItem;

// Initialize it:
this.dependencyManagerToolStripMenuItem = new ToolStripMenuItem();
this.dependencyManagerToolStripMenuItem.Name = "dependencyManagerToolStripMenuItem";
this.dependencyManagerToolStripMenuItem.Size = new Size(200, 22);
this.dependencyManagerToolStripMenuItem.Text = "Dependency Manager...";
this.dependencyManagerToolStripMenuItem.Click += DependencyManagerToolStripMenuItem_Click;

// Add to your Tools menu (or create one if it doesn't exist)
this.toolsToolStripMenuItem.DropDownItems.Add(this.dependencyManagerToolStripMenuItem);
```

## Step 2: Add Event Handler

Add the click event handler in `MainFrm.cs`:

```csharp
private void DependencyManagerToolStripMenuItem_Click(object sender, EventArgs e)
{
    using var depManager = new DependencyManagerForm();
    depManager.ShowDialog(this);
}
```

## Step 3: Check Dependencies on Startup (Optional)

Add a check in the MainFrm constructor to warn users if dependencies are missing:

```csharp
public MainFrm()
{
    // ... existing initialization code ...
    
    CheckDependencies();
}

private void CheckDependencies()
{
    using var depManager = new DependencyManager(Program.StartupPath);
    
    var missingComponents = depManager.AvailableComponents
        .Where(kvp => !depManager.IsComponentInstalled(kvp.Key))
        .Select(kvp => kvp.Key)
        .ToList();
    
    if (missingComponents.Any())
    {
        var message = $"The following components are not installed:\n\n" +
                     $"{string.Join("\n", missingComponents)}\n\n" +
                     $"Would you like to open the Dependency Manager to install them?";
        
        var result = MessageBox.Show(
            message,
            "Missing Dependencies",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        
        if (result == DialogResult.Yes)
        {
            using var depManagerForm = new DependencyManagerForm();
            depManagerForm.ShowDialog(this);
        }
    }
}
```

## Step 4: Smart Start Buttons (Optional Enhancement)

Enhance your start buttons to check if components are installed:

```csharp
private void NginxStartButton_Click(object sender, EventArgs e)
{
    if (!IsNginxInstalled())
    {
        var result = MessageBox.Show(
            "Nginx is not installed. Would you like to install it now?",
            "Nginx Not Found",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            using var depManager = new DependencyManagerForm();
            depManager.ShowDialog(this);
        }
        return;
    }
    
    _nginx.Start();
}

private bool IsNginxInstalled()
{
    using var depManager = new DependencyManager(Program.StartupPath);
    return depManager.IsComponentInstalled("Nginx");
}
```

## Complete Menu Structure Example

Here's a complete example of a Tools menu with the Dependency Manager:

```csharp
// Create Tools menu if it doesn't exist
private ToolStripMenuItem toolsToolStripMenuItem;
private ToolStripMenuItem dependencyManagerToolStripMenuItem;
private ToolStripMenuItem checkForUpdatesToolStripMenuItem;

private void InitializeToolsMenu()
{
    // Tools menu
    this.toolsToolStripMenuItem = new ToolStripMenuItem();
    this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
    this.toolsToolStripMenuItem.Size = new Size(47, 20);
    this.toolsToolStripMenuItem.Text = "&Tools";
    
    // Dependency Manager
    this.dependencyManagerToolStripMenuItem = new ToolStripMenuItem();
    this.dependencyManagerToolStripMenuItem.Name = "dependencyManagerToolStripMenuItem";
    this.dependencyManagerToolStripMenuItem.Size = new Size(200, 22);
    this.dependencyManagerToolStripMenuItem.Text = "&Dependency Manager...";
    this.dependencyManagerToolStripMenuItem.Click += DependencyManagerToolStripMenuItem_Click;
    
    // Check for Updates
    this.checkForUpdatesToolStripMenuItem = new ToolStripMenuItem();
    this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
    this.checkForUpdatesToolStripMenuItem.Size = new Size(200, 22);
    this.checkForUpdatesToolStripMenuItem.Text = "Check for &Updates...";
    this.checkForUpdatesToolStripMenuItem.Click += CheckForUpdatesToolStripMenuItem_Click;
    
    // Add items to Tools menu
    this.toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
        this.dependencyManagerToolStripMenuItem,
        new ToolStripSeparator(),
        this.checkForUpdatesToolStripMenuItem
    });
    
    // Add Tools menu to menu strip (assuming you have wnmpMenuStrip)
    // Insert before Help menu or at the end
    int helpIndex = this.wnmpMenuStrip.Items.IndexOf(this.helpToolStripMenuItem);
    if (helpIndex >= 0)
        this.wnmpMenuStrip.Items.Insert(helpIndex, this.toolsToolStripMenuItem);
    else
        this.wnmpMenuStrip.Items.Add(this.toolsToolStripMenuItem);
}

private void DependencyManagerToolStripMenuItem_Click(object sender, EventArgs e)
{
    using var depManager = new DependencyManagerForm();
    depManager.ShowDialog(this);
}

private void CheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
{
    // TODO: Implement update checking
    MessageBox.Show(
        "Check for updates functionality coming soon!",
        "Check for Updates",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
}
```

## Alternative: Add Button to Main Form

If you prefer a button instead of (or in addition to) a menu item:

```csharp
// Add to your form designer or InitializeComponent:
private Button manageDependenciesButton;

this.manageDependenciesButton = new Button();
this.manageDependenciesButton.Location = new Point(12, 450);
this.manageDependenciesButton.Name = "manageDependenciesButton";
this.manageDependenciesButton.Size = new Size(150, 30);
this.manageDependenciesButton.TabIndex = 20;
this.manageDependenciesButton.Text = "Manage Dependencies";
this.manageDependenciesButton.UseVisualStyleBackColor = true;
this.manageDependenciesButton.Click += ManageDependenciesButton_Click;

this.Controls.Add(this.manageDependenciesButton);

// Event handler:
private void ManageDependenciesButton_Click(object sender, EventArgs e)
{
    using var depManager = new DependencyManagerForm();
    depManager.ShowDialog(this);
}
```

## Testing

After integration, test the following scenarios:

1. ? Open Dependency Manager from menu
2. ? Install a single component
3. ? Install all components at once
4. ? Cancel installation mid-download
5. ? Try to start services without components installed
6. ? Reinstall components (with -Force or after manual deletion)
7. ? Check dependency status on startup

## Next Steps

Consider adding these enhancements:

- **Auto-update checking**: Check for newer component versions online
- **Version selection**: Allow users to select specific versions
- **Backup/Restore**: Backup configurations before reinstalling
- **Custom URLs**: Allow users to specify custom download URLs
- **Offline installation**: Support installation from local ZIP files
