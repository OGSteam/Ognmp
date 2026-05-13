# Dependency Manager User Guide

## Opening the Dependency Manager

1. Launch Ognmp application
2. Click **Tools** ? **Download Components...**

## Understanding the Interface

### Component List (Top Section)
Shows all available components with columns:
- **Component**: Name (Nginx, MariaDB, PHP, PhpMyAdmin)
- **Installed Version**: Current version if installed, or "Not installed"
- **Status**: ? Installed (green) or ? Not Installed (red)
- **Description**: Brief description of the component

### Version Selection (Middle Section)
- **Refresh List**: Reload available versions from providers
- **Select Version**: Dropdown showing all available versions for selected component
  - Versions are fetched dynamically from official sources
  - Latest versions appear first
  - Shows version number and description (Latest, Stable, LTS, etc.)

### Action Buttons
- **Install Selected**: Install the chosen version of the selected component
- **Install All (Latest)**: Install latest version of all components
- **Cancel**: Stop current download/installation
- **Close**: Close the Dependency Manager window

### Progress Section (Bottom)
- **Progress Bar**: Visual indication of download progress
- **Status Label**: Current operation status and detailed progress

## How to Install a Component

### Install Specific Version
1. Select a component from the list (click on it)
2. Choose desired version from "Select Version" dropdown
3. Click "Install Selected"
4. Wait for download and installation to complete

### Install All Components (Latest Versions)
1. Click "Install All (Latest)"
2. All components will be downloaded and installed sequentially
3. Each component gets the latest available version

## Version Information

The Dependency Manager dynamically fetches versions from:
- **Nginx**: https://nginx.org/en/download.html
- **PHP**: https://windows.php.net/downloads/releases/
- **MariaDB**: https://downloads.mariadb.org/
- **PhpMyAdmin**: https://www.phpmyadmin.net/files/

### Version Labels
- **Latest**: Most recent release
- **Stable**: Stable branch (even minor version for Nginx)
- **LTS**: Long-term support version
- **Legacy**: Older but still supported version

## Refreshing Version List

If new versions are released:
1. Click "Refresh List"
2. Wait for versions to reload
3. New versions will appear in dropdowns

## Installation Details

### What Happens During Installation
1. **Download**: Component ZIP file downloaded to temp folder
2. **Extract**: Files extracted from ZIP
3. **Install**: Files moved to Ognmp directory structure
4. **Configure**: Initial configuration files created

### Installation Paths
- **Nginx**: `Ognmp\nginx\`
- **PHP**: `Ognmp\php\{version}\` (e.g., `php\8.3\`)
- **MariaDB**: `Ognmp\mariadb\`
- **PhpMyAdmin**: `Ognmp\www\phpmyadmin\`

## Troubleshooting

### "Failed to fetch versions"
- Check internet connection
- Click "Refresh List" to retry
- Fallback versions will be used if remote fetch fails

### "Download failed"
- Check internet connection
- Verify firewall isn't blocking downloads
- Try again after a few minutes

### "Installation failed"
- Check disk space
- Ensure Ognmp directory is writable
- Close any running instances of components
- Check logs in main window for details

### Component Already Installed
- Existing installation will be replaced
- Stop the component before reinstalling
- Configuration files may be preserved

## Tips

1. **Always stop services** before reinstalling
2. **Use latest versions** for security updates
3. **Backup configurations** before major upgrades
4. **Check logs** in main window for detailed progress
5. **Cancel anytime** using Cancel button during download

## Network Requirements

- Active internet connection required
- Firewall must allow HTTPS connections to:
  - nginx.org
  - windows.php.net
  - downloads.mariadb.org
  - files.phpmyadmin.net

## After Installation

1. Return to main Ognmp window
2. Start newly installed components
3. Check running status indicators
4. Review logs for any startup issues
