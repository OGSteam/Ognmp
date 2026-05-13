# Dynamic Version Loading System

## Overview

The Dependency Manager now fetches available component versions dynamically from official sources instead of using hardcoded values. This ensures users always have access to the latest versions without needing to update the application.

## Architecture

### Version Providers

Each component has a dedicated version provider that implements the `IVersionProvider` interface:

- **NginxVersionProvider**: Scrapes https://nginx.org/en/download.html
- **PhpVersionProvider**: Scrapes https://windows.php.net/downloads/releases/
- **MariaDbVersionProvider**: Queries https://downloads.mariadb.org/rest-api/
- **PhpMyAdminVersionProvider**: Scrapes https://www.phpmyadmin.net/files/

### How It Works

1. **Initial Load**: When the Dependency Manager form opens, it calls `LoadAvailableVersionsAsync()` which:
   - Contacts each provider
   - Fetches available versions from official sources
   - Populates the `AvailableVersions` dictionary

2. **Fallback Mechanism**: If fetching fails (network issues, parsing errors, etc.), each provider has hardcoded fallback versions to ensure the application remains functional.

3. **Manual Refresh**: Users can click the "Refresh List" button to reload versions on demand.

## Version Provider Details

### NginxVersionProvider
- **Source**: HTML scraping of nginx.org download page
- **Pattern**: Looks for `nginx-X.X.X.zip` files
- **Stable Detection**: Identifies stable versions (even minor numbers)
- **Sorting**: Latest versions first

### PhpVersionProvider
- **Source**: HTML scraping of PHP Windows downloads
- **Pattern**: Looks for `php-X.X.X-Win32-vs16-x64.zip` (64-bit, non-thread-safe)
- **Version Types**: Latest (8.3), Stable (8.2), LTS (8.1)
- **Target Folders**: Each major.minor version in separate folder (`php\8.3`, `php\8.2`, etc.)

### MariaDbVersionProvider
- **Source**: Direct HEAD requests to MariaDB download API
- **Strategy**: Tests known stable branches (11.2, 11.1, 10.11, 10.6) to find latest patches
- **Optimization**: Only checks recent patch versions to minimize requests
- **LTS Detection**: Identifies long-term support versions (10.11, 10.6)

### PhpMyAdminVersionProvider
- **Source**: HTML scraping of phpMyAdmin files page
- **Pattern**: Looks for `phpMyAdmin-X.X.X-all-languages.zip`
- **Sorting**: Latest versions first

## Error Handling

- **Network Failures**: If a provider can't fetch versions, it logs an error and returns fallback versions
- **Parse Failures**: Regex parsing errors are caught and fallback versions are used
- **User Notification**: Status bar shows "Loading available versions..." and errors if they occur

## Benefits

1. **Always Up-to-Date**: Users get access to latest versions as soon as they're released
2. **No Application Updates Required**: Version lists update automatically
3. **Resilient**: Fallback versions ensure functionality even when sources are unavailable
4. **User Control**: Manual refresh button allows users to update the list on demand
5. **Performance**: Versions are cached after initial load (only reloaded on refresh)

## Future Enhancements

Potential improvements:
- Cache versions locally with expiration time
- Parallel loading of version providers
- Version filtering (show only LTS, stable, etc.)
- Automatic periodic refresh
- RSS/Atom feed parsing for update notifications
