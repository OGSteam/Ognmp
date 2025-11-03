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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ognmp.Components
{
    /// <summary>
    /// Manages downloading and installation of server components (Nginx, PHP, MariaDB, phpMyAdmin)
    /// </summary>
    public class DependencyManager : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseDirectory;
        private bool _disposed;

        public event EventHandler<DownloadProgressEventArgs> DownloadProgressChanged;
        public event EventHandler<ComponentEventArgs> ComponentInstalled;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        public DependencyManager(string baseDirectory)
        {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
        }

        public Dictionary<string, ComponentInfo> AvailableComponents { get; } = new()
        {
            ["Nginx"] = new ComponentInfo
            {
                Name = "Nginx",
                Version = "1.25.3",
                Url = "https://nginx.org/download/nginx-1.25.3.zip",
                FileName = "nginx-1.25.3.zip",
                ExtractFolder = "nginx-1.25.3",
                TargetFolder = "nginx",
                Description = "High-performance HTTP server and reverse proxy"
            },
            ["PHP"] = new ComponentInfo
            {
                Name = "PHP",
                Version = "8.3.1",
                Url = "https://windows.php.net/downloads/releases/php-8.3.1-Win32-vs16-x64.zip",
                FileName = "php-8.3.1-Win32-vs16-x64.zip",
                ExtractFolder = "php-8.3.1",
                TargetFolder = "php\\8.3",
                Description = "PHP scripting language runtime"
            },
            ["MariaDB"] = new ComponentInfo
            {
                Name = "MariaDB",
                Version = "11.2.2",
                Url = "https://downloads.mariadb.org/rest-api/mariadb/11.2.2/mariadb-11.2.2-winx64.zip",
                FileName = "mariadb-11.2.2-winx64.zip",
                ExtractFolder = "mariadb-11.2.2-winx64",
                TargetFolder = "mariadb",
                Description = "MySQL-compatible database server"
            },
            ["PhpMyAdmin"] = new ComponentInfo
            {
                Name = "phpMyAdmin",
                Version = "5.2.1",
                Url = "https://files.phpmyadmin.net/phpMyAdmin/5.2.1/phpMyAdmin-5.2.1-all-languages.zip",
                FileName = "phpMyAdmin-5.2.1-all-languages.zip",
                ExtractFolder = "phpMyAdmin-5.2.1-all-languages",
                TargetFolder = "www\\phpmyadmin",
                Description = "Web-based MySQL administration tool"
            }
        };

        public bool IsComponentInstalled(string componentName)
        {
            if (!AvailableComponents.TryGetValue(componentName, out var component))
                return false;

            var targetPath = Path.Combine(_baseDirectory, component.TargetFolder);
            return Directory.Exists(targetPath);
        }

        public async Task<bool> DownloadAndInstallComponentAsync(
            string componentName, 
            CancellationToken cancellationToken = default)
        {
            if (!AvailableComponents.TryGetValue(componentName, out var component))
            {
                OnErrorOccurred($"Unknown component: {componentName}");
                return false;
            }

            try
            {
                var tempPath = Path.GetTempPath();
                var downloadPath = Path.Combine(tempPath, component.FileName);
                var extractPath = Path.Combine(tempPath, "ognmp_extract_" + Guid.NewGuid());

                // Download
                Log.Notice($"Downloading {component.Name} {component.Version}...");
                if (!await DownloadFileAsync(component.Url, downloadPath, componentName, cancellationToken))
                    return false;

                // Extract
                Log.Notice($"Extracting {component.Name}...");
                if (!await Task.Run(() => ExtractZipFile(downloadPath, extractPath), cancellationToken))
                {
                    CleanupFiles(downloadPath, extractPath);
                    return false;
                }

                // Install
                Log.Notice($"Installing {component.Name}...");
                var extractedFolder = Path.Combine(extractPath, component.ExtractFolder);
                var targetPath = Path.Combine(_baseDirectory, component.TargetFolder);

                if (!await Task.Run(() => InstallComponent(extractedFolder, targetPath), cancellationToken))
                {
                    CleanupFiles(downloadPath, extractPath);
                    return false;
                }

                // Cleanup
                CleanupFiles(downloadPath, extractPath);

                OnComponentInstalled(component);
                Log.Notice($"{component.Name} installed successfully", Log.LogSection.Ognmp);

                // Initialize component-specific configuration
                await InitializeComponentConfigAsync(componentName);

                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Failed to install {componentName}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> DownloadFileAsync(
            string url, 
            string outputPath, 
            string componentName,
            CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                var canReportProgress = totalBytes > 0;

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalRead += bytesRead;

                    if (canReportProgress)
                    {
                        var progressPercentage = (int)((totalRead * 100) / totalBytes);
                        OnDownloadProgressChanged(componentName, progressPercentage, totalRead, totalBytes);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Download failed: {ex.Message}");
                return false;
            }
        }

        private bool ExtractZipFile(string zipPath, string extractPath)
        {
            try
            {
                Directory.CreateDirectory(extractPath);
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Extraction failed: {ex.Message}");
                return false;
            }
        }

        private bool InstallComponent(string sourcePath, string targetPath)
        {
            try
            {
                // Remove existing installation
                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, true);
                }

                // Create parent directory if needed
                var parentDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                // Move extracted files to target location
                Directory.Move(sourcePath, targetPath);
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Installation failed: {ex.Message}");
                return false;
            }
        }

        private async Task InitializeComponentConfigAsync(string componentName)
        {
            await Task.Run(() =>
            {
                switch (componentName)
                {
                    case "Nginx":
                        InitializeNginxConfig();
                        break;
                    case "PHP":
                        InitializePhpConfig();
                        break;
                    case "MariaDB":
                        InitializeMariaDbConfig();
                        break;
                }
            });
        }

        private void InitializeNginxConfig()
        {
            var confDir = Path.Combine(_baseDirectory, "conf");
            Directory.CreateDirectory(confDir);

            var nginxConfSource = Path.Combine(_baseDirectory, "nginx", "conf", "nginx.conf");
            var nginxConfTarget = Path.Combine(confDir, "nginx.conf");

            if (File.Exists(nginxConfSource) && !File.Exists(nginxConfTarget))
            {
                File.Copy(nginxConfSource, nginxConfTarget);
            }
        }

        private void InitializePhpConfig()
        {
            var phpDir = Path.Combine(_baseDirectory, "php", "8.3");
            var phpIniSource = Path.Combine(phpDir, "php.ini-development");
            var phpIniTarget = Path.Combine(phpDir, "php.ini");

            if (File.Exists(phpIniSource) && !File.Exists(phpIniTarget))
            {
                File.Copy(phpIniSource, phpIniTarget);
            }

            // Create logs directory
            var logsDir = Path.Combine(phpDir, "logs");
            Directory.CreateDirectory(logsDir);
        }

        private void InitializeMariaDbConfig()
        {
            var mariaDbDir = Path.Combine(_baseDirectory, "mariadb");
            var dataDir = Path.Combine(mariaDbDir, "data");
            Directory.CreateDirectory(dataDir);

            var myIniPath = Path.Combine(mariaDbDir, "my.ini");
            if (!File.Exists(myIniPath))
            {
                var iniContent = $@"[mysqld]
port=3306
datadir={dataDir.Replace("\\", "/")}
";
                File.WriteAllText(myIniPath, iniContent);
            }
        }

        private void CleanupFiles(params string[] paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    else if (Directory.Exists(path))
                        Directory.Delete(path, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        protected virtual void OnDownloadProgressChanged(string componentName, int percentage, long bytesReceived, long totalBytes)
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs
            {
                ComponentName = componentName,
                ProgressPercentage = percentage,
                BytesReceived = bytesReceived,
                TotalBytes = totalBytes
            });
        }

        protected virtual void OnComponentInstalled(ComponentInfo component)
        {
            ComponentInstalled?.Invoke(this, new ComponentEventArgs { Component = component });
        }

        protected virtual void OnErrorOccurred(string message)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(new Exception(message)));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class ComponentInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string ExtractFolder { get; set; }
        public string TargetFolder { get; set; }
        public string Description { get; set; }
    }

    public class DownloadProgressEventArgs : EventArgs
    {
        public string ComponentName { get; set; }
        public int ProgressPercentage { get; set; }
        public long BytesReceived { get; set; }
        public long TotalBytes { get; set; }
    }

    public class ComponentEventArgs : EventArgs
    {
        public ComponentInfo Component { get; set; }
    }
}
