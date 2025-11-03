<#
.SYNOPSIS
    Downloads Nginx, PHP, MariaDB, and phpMyAdmin binaries for Ognmp.

.DESCRIPTION
    This script downloads the required server binaries and extracts them to the appropriate directories
    for the Ognmp application to function properly.

.PARAMETER TargetDirectory
    The base directory where binaries will be extracted. Defaults to the parent directory of the script.

.EXAMPLE
    .\Download-Dependencies.ps1
    .\Download-Dependencies.ps1 -TargetDirectory "C:\Ognmp"
#>

param(
    [string]$TargetDirectory = (Split-Path -Parent $PSScriptRoot),
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Configuration
$config = @{
    Nginx = @{
        Url = "https://nginx.org/download/nginx-1.25.3.zip"
        FileName = "nginx-1.25.3.zip"
        ExtractFolder = "nginx-1.25.3"
        TargetFolder = "nginx"
    }
    PHP = @{
        Url = "https://windows.php.net/downloads/releases/php-8.3.1-Win32-vs16-x64.zip"
        FileName = "php-8.3.1-nts-Win32-vs16-x64.zip"
        ExtractFolder = "php-8.3.1"
        TargetFolder = "php\8.3"
    }
    MariaDB = @{
        Url = "https://downloads.mariadb.org/rest-api/mariadb/11.2.2/mariadb-11.2.2-winx64.zip"
        FileName = "mariadb-11.2.2-winx64.zip"
        ExtractFolder = "mariadb-11.2.2-winx64"
        TargetFolder = "mariadb"
    }
    PhpMyAdmin = @{
        Url = "https://files.phpmyadmin.net/phpMyAdmin/5.2.1/phpMyAdmin-5.2.1-all-languages.zip"
        FileName = "phpMyAdmin-5.2.1-all-languages.zip"
        ExtractFolder = "phpMyAdmin-5.2.1-all-languages"
        TargetFolder = "www\phpmyadmin"
    }
}

function Write-Status {
    param([string]$Message, [string]$Type = "Info")
    
    $color = switch ($Type) {
        "Success" { "Green" }
        "Error" { "Red" }
        "Warning" { "Yellow" }
        default { "White" }
    }
    
    Write-Host "[$Type] $Message" -ForegroundColor $color
}

function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Download-File {
    param(
        [string]$Url,
        [string]$OutputPath
    )
    
    Write-Status "Downloading from $Url..."
    
    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.DownloadFile($Url, $OutputPath)
        Write-Status "Downloaded successfully" "Success"
        return $true
    }
    catch {
        Write-Status "Failed to download: $_" "Error"
        return $false
    }
}

function Expand-ZipFile {
    param(
        [string]$ZipFile,
        [string]$Destination
    )
    
    Write-Status "Extracting $ZipFile to $Destination..."
    
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($ZipFile, $Destination)
        Write-Status "Extracted successfully" "Success"
        return $true
    }
    catch {
        Write-Status "Failed to extract: $_" "Error"
        return $false
    }
}

function Install-Component {
    param(
        [string]$Name,
        [hashtable]$Config
    )
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Status "Installing $Name" "Info"
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    $downloadPath = Join-Path $env:TEMP $Config.FileName
    $extractPath = Join-Path $TargetDirectory "temp_extract"
    $targetPath = Join-Path $TargetDirectory $Config.TargetFolder
    
    # Check if already exists
    if ((Test-Path $targetPath) -and -not $Force) {
        Write-Status "$Name already exists at $targetPath. Use -Force to reinstall." "Warning"
        return $true
    }
    
    # Download
    if (-not (Download-File -Url $Config.Url -OutputPath $downloadPath)) {
        return $false
    }
    
    # Create temp extract directory
    if (Test-Path $extractPath) {
        Remove-Item $extractPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $extractPath -Force | Out-Null
    
    # Extract
    if (-not (Expand-ZipFile -ZipFile $downloadPath -Destination $extractPath)) {
        return $false
    }
    
    # Move to target location
    $extractedFolder = Join-Path $extractPath $Config.ExtractFolder
    
    if (Test-Path $targetPath) {
        Write-Status "Removing existing installation..." "Warning"
        Remove-Item $targetPath -Recurse -Force
    }
    
    # Create parent directory if it doesn't exist
    $parentDir = Split-Path $targetPath -Parent
    if (-not (Test-Path $parentDir)) {
        New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
    }
    
    Move-Item $extractedFolder $targetPath -Force
    Write-Status "$Name installed to $targetPath" "Success"
    
    # Cleanup
    Remove-Item $downloadPath -Force -ErrorAction SilentlyContinue
    Remove-Item $extractPath -Recurse -Force -ErrorAction SilentlyContinue
    
    return $true
}

function Initialize-Configuration {
    param([string]$BaseDir)
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Status "Initializing Configuration Files" "Info"
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    # Create necessary directories
    $dirs = @("conf", "logs", "www", "mariadb\data", "php\8.3\logs")
    foreach ($dir in $dirs) {
        $path = Join-Path $BaseDir $dir
        if (-not (Test-Path $path)) {
            New-Item -ItemType Directory -Path $path -Force | Out-Null
            Write-Status "Created directory: $path" "Success"
        }
    }
    
    # Copy nginx config if doesn't exist
    $nginxConfSource = Join-Path $BaseDir "nginx\conf\nginx.conf"
    $nginxConfTarget = Join-Path $BaseDir "conf\nginx.conf"
    if ((Test-Path $nginxConfSource) -and -not (Test-Path $nginxConfTarget)) {
        Copy-Item $nginxConfSource $nginxConfTarget
        Write-Status "Created nginx.conf template" "Success"
    }
    
    # Copy PHP config if doesn't exist
    $phpIniSource = Join-Path $BaseDir "php\8.3\php.ini-development"
    $phpIniTarget = Join-Path $BaseDir "php\8.3\php.ini"
    if ((Test-Path $phpIniSource) -and -not (Test-Path $phpIniTarget)) {
        Copy-Item $phpIniSource $phpIniTarget
        Write-Status "Created php.ini from template" "Success"
    }
    
    # Copy MariaDB config if doesn't exist
    $mariaDbIniTarget = Join-Path $BaseDir "mariadb\my.ini"
    if (-not (Test-Path $mariaDbIniTarget)) {
        $mariaDbIniContent = @"
[mysqld]
port=3306
datadir=$($BaseDir -replace '\\', '/')/mariadb/data
"@
        Set-Content -Path $mariaDbIniTarget -Value $mariaDbIniContent
        Write-Status "Created MariaDB my.ini" "Success"
    }
}

# Main execution
Write-Host @"

??????????????????????????????????????????????????????????????
?                                                            ?
?        Ognmp Dependencies Download Script                 ?
?        .NET 8.0 Version                                   ?
?                                                            ?
??????????????????????????????????????????????????????????????

"@ -ForegroundColor Cyan

Write-Status "Target Directory: $TargetDirectory"

if (-not (Test-Administrator)) {
    Write-Status "Warning: Not running as administrator. Some operations might fail." "Warning"
}

# Create target directory if it doesn't exist
if (-not (Test-Path $TargetDirectory)) {
    New-Item -ItemType Directory -Path $TargetDirectory -Force | Out-Null
}

$success = $true

# Install each component
foreach ($component in $config.Keys) {
    if (-not (Install-Component -Name $component -Config $config[$component])) {
        $success = $false
        Write-Status "Failed to install $component" "Error"
    }
}

if ($success) {
    Initialize-Configuration -BaseDir $TargetDirectory
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Status "All components installed successfully!" "Success"
    Write-Host "========================================`n" -ForegroundColor Green
    
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Review and customize configuration files in the 'conf' directory"
    Write-Host "  2. Initialize MariaDB database"
    Write-Host "  3. Run the Ognmp application"
    Write-Host ""
}
else {
    Write-Status "Installation completed with errors. Please check the output above." "Error"
    exit 1
}
