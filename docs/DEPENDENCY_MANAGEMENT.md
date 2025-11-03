# Ognmp Dependency Management

Ognmp requires several third-party components to function: Nginx, PHP, MariaDB, and phpMyAdmin. This guide explains how to download and manage these dependencies.

## Option 1: PowerShell Script (Quick Setup)

The fastest way to set up all dependencies is using the PowerShell script.

### Usage

```powershell
# Navigate to the Ognmp directory
cd C:\source\Ognmp

# Run the download script
.\scripts\Download-Dependencies.ps1

# Force reinstall all components
.\scripts\Download-Dependencies.ps1 -Force

# Install to a custom directory
.\scripts\Download-Dependencies.ps1 -TargetDirectory "C:\MyCustomPath"
```

### What the script does:
- Downloads the latest stable versions of all components
- Extracts them to the correct directories
- Creates necessary configuration files
- Sets up directory structure

## Option 2: In-Application Manager (User-Friendly)

The in-app dependency manager provides a GUI for managing components.

### How to use:

1. Launch Ognmp
2. Go to **Tools ? Dependency Manager** (or add this to your menu)
3. Select components to install
4. Click "Install Selected" or "Install All"
5. Monitor progress with the progress bar

### Features:
- Visual status of installed components
- Download progress tracking
- Individual or batch installation
- Cancel support
- Automatic configuration initialization

## Integration with MainFrm

To add the Dependency Manager to your main form, add this to `MainFrm.cs`:

```csharp
// Add to menu strip
private void DependencyManagerToolStripMenuItem_Click(object sender, EventArgs e)
{
    using var depManager = new DependencyManagerForm();
    depManager.ShowDialog(this);
}

// Or add a button in the main form
private void ManageDependenciesButton_Click(object sender, EventArgs e)
{
    using var depManager = new DependencyManagerForm();
    depManager.ShowDialog(this);
}
```

## Component Versions

Current default versions (can be updated in the code):

| Component   | Version | Platform |
|-------------|---------|----------|
| Nginx       | 1.25.3  | Windows  |
| PHP         | 8.3.1   | Windows x64 (VS16) |
| MariaDB     | 11.2.2  | Windows x64 |
| phpMyAdmin  | 5.2.1   | All Languages |

## Directory Structure

After installation, your directory structure will look like this:

```
Ognmp/
??? nginx/              # Nginx web server
?   ??? conf/
?   ??? ...
??? php/                # PHP runtime
?   ??? 8.3/
?       ??? php.ini
?       ??? ...
??? mariadb/            # MariaDB database
?   ??? my.ini
?   ??? data/
?   ??? ...
??? www/                # Web root
?   ??? phpmyadmin/     # phpMyAdmin installation
??? conf/               # Ognmp configuration
??? logs/               # Application logs
??? scripts/            # Utility scripts
```

## Updating Component Versions

To update component versions, modify the configuration in:

**PowerShell Script:** `scripts/Download-Dependencies.ps1`
```powershell
$config = @{
    Nginx = @{
        Url = "https://nginx.org/download/nginx-1.26.0.zip"
        Version = "1.26.0"
        # ...
    }
}
```

**C# Dependency Manager:** `Ognmp/Components/DependencyManager.cs`
```csharp
public Dictionary<string, ComponentInfo> AvailableComponents { get; } = new()
{
    ["Nginx"] = new ComponentInfo
    {
        Version = "1.26.0",
        Url = "https://nginx.org/download/nginx-1.26.0.zip",
        // ...
    }
};
```

## Troubleshooting

### Download fails
- Check your internet connection
- Verify the URLs are still valid (component download locations may change)
- Check firewall/antivirus settings

### Extraction fails
- Ensure you have write permissions to the target directory
- Check available disk space
- Try running as Administrator

### Component doesn't start after installation
- Check the logs in the `logs` directory
- Verify configuration files were created correctly
- Ensure required ports (80, 3306, 9000) are not in use

## Manual Installation

If automated installation doesn't work, you can manually download and extract components:

1. Download components from official websites:
   - Nginx: https://nginx.org/en/download.html
   - PHP: https://windows.php.net/download/
   - MariaDB: https://mariadb.org/download/
   - phpMyAdmin: https://www.phpmyadmin.net/downloads/

2. Extract to the appropriate directories as shown in the structure above

3. Copy configuration templates:
   - Copy `nginx/conf/nginx.conf` to `conf/nginx.conf`
   - Copy `php/8.3/php.ini-development` to `php/8.3/php.ini`
   - Create `mariadb/my.ini` with basic configuration

## Security Notes

- Always download components from official sources
- Keep components up to date with security patches
- Review and customize configuration files for your environment
- Use strong passwords for MariaDB root account
- Consider running components with restricted user permissions

## License

The dependency management code is part of Ognmp and follows the same GPL v3 license.
Individual components have their own licenses:
- Nginx: 2-clause BSD license
- PHP: PHP License
- MariaDB: GPL v2
- phpMyAdmin: GPL v2
