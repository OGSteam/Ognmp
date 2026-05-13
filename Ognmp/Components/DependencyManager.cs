using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ognmp.Components.Events;
using Ognmp.Components.Installation;
using Ognmp.Components.VersionProviders;

namespace Ognmp.Components
{
    /// <summary>
    /// Manages downloading and installation of server components (Nginx, PHP, MariaDB, phpMyAdmin)
    /// </summary>
    public class DependencyManager : IDependencyManager
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseDirectory;
        private bool _disposed;
        private readonly Dictionary<string, IVersionProvider> _versionProviders;
        private readonly ComponentDownloader _componentDownloader;
        private readonly ComponentInstaller _componentInstaller;
        private readonly InstallationValidator _installationValidator;
        private readonly ComponentConfigInitializer _componentConfigInitializer;

        public event EventHandler<DownloadProgressEventArgs> DownloadProgressChanged;
        public event EventHandler<ComponentEventArgs> ComponentInstalled;
        public event EventHandler<DependencyErrorEventArgs> ErrorOccurred;

        public DependencyManager(string baseDirectory)
        {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10)
            };

            _versionProviders = new Dictionary<string, IVersionProvider>(StringComparer.OrdinalIgnoreCase)
            {
                ["Nginx"] = new NginxVersionProvider(_httpClient),
                ["PHP"] = new PhpVersionProvider(_httpClient),
                ["PHP Xdebug"] = new PhpXdebugVersionProvider(_httpClient),
                ["MariaDB"] = new MariaDbVersionProvider(_httpClient),
                ["PhpMyAdmin"] = new PhpMyAdminVersionProvider(_httpClient)
            };

            // Initialize extracted component handlers
            _componentDownloader = new ComponentDownloader(_httpClient);
            _componentInstaller = new ComponentInstaller();
            _installationValidator = new InstallationValidator();
            _componentConfigInitializer = new ComponentConfigInitializer(_baseDirectory);

            // Wire download progress event from downloader
            _componentDownloader.ProgressChanged += (s, e) => OnDownloadProgressChanged(e);
        }

        public Dictionary<string, List<ComponentInfo>> AvailableVersions { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

        public async Task LoadAvailableVersionsAsync(CancellationToken cancellationToken = default)
        {
            AvailableVersions.Clear();
            Log.Notice("Starting to load component versions in parallel...");

            // Load all version providers in parallel for better performance
            var loadTasks = _versionProviders.Values
                .Select(async provider =>
                {
                    try
                    {
                        Log.Notice($"Loading versions for {provider.ComponentName}...");
                        var versions = await provider.GetAvailableVersionsAsync(cancellationToken);
                        
                        if (versions != null && versions.Any())
                        {
                            Log.Notice($"Loaded {versions.Count} version(s) for {provider.ComponentName}");
                            return (provider.ComponentName, versions);
                        }
                        else
                        {
                            Log.Warning($"No versions returned for {provider.ComponentName}");
                            return (provider.ComponentName, new List<ComponentInfo>());
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to load versions for {provider.ComponentName}: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Log.Error($"  Inner exception: {ex.InnerException.Message}");
                        }
                        return (provider.ComponentName, new List<ComponentInfo>());
                    }
                })
                .ToList();

            // Wait for all providers to complete
            var results = await Task.WhenAll(loadTasks);

            // Add results to dictionary
            foreach (var (componentName, versions) in results)
            {
                if (versions.Any())
                {
                    AvailableVersions[componentName] = versions;
                }
            }

            Log.Notice($"Finished loading versions. Total components loaded: {AvailableVersions.Count}");
            
            // List all loaded components
            foreach (var component in AvailableVersions.Keys)
            {
                Log.Notice($"  - {component}");
            }
        }

        public List<ComponentInfo> GetVersionsForComponent(string componentName)
        {
            return AvailableVersions.TryGetValue(componentName, out var versions) 
                ? versions 
                : new List<ComponentInfo>();
        }

        public bool IsComponentInstalled(string componentName)
        {
            if (!AvailableVersions.TryGetValue(componentName, out var versions) || !versions.Any())
                return false;

            var component = versions.First();
            var targetPath = Path.Combine(_baseDirectory, component.TargetFolder);
            return Directory.Exists(targetPath);
        }

        public async Task<bool> DownloadAndInstallComponentAsync(
            string componentName, 
            string version = null,
            CancellationToken cancellationToken = default)
        {
            if (!AvailableVersions.TryGetValue(componentName, out var versions))
            {
                OnErrorOccurred($"Unknown component: {componentName}");
                return false;
            }

            ComponentInfo component;
            if (string.IsNullOrEmpty(version))
            {
                component = versions.First();
            }
            else
            {
                component = versions.FirstOrDefault(v => v.Version == version);
                if (component == null)
                {
                    OnErrorOccurred($"Version {version} not found for {componentName}");
                    return false;
                }
            }

            // Check if this is a DLL file (like Xdebug)
            if (component.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return await DownloadAndInstallDllAsync(component, componentName, cancellationToken);
            }

            // Standard ZIP installation
            var tempPath = Path.GetTempPath();
            var downloadPath = Path.Combine(tempPath, component.FileName);
            var extractPath = Path.Combine(tempPath, "ognmp_extract_" + Guid.NewGuid());

            try
            {
                // Download
                Log.Notice($"Downloading {component.Name} {component.Version}...");
                if (!await _componentDownloader.DownloadAsync(component.Url, downloadPath, componentName, cancellationToken))
                    return false;

                // Extract
                Log.Notice($"Extracting {component.Name}...");
                if (!await Task.Run(() => _componentInstaller.ExtractArchive(downloadPath, extractPath), cancellationToken))
                    return false;

                // Install
                Log.Notice($"Installing {component.Name}...");
                var sourceFolder = string.IsNullOrEmpty(component.ExtractFolder)
                    ? extractPath
                    : Path.Combine(extractPath, component.ExtractFolder);
                
                // Smart extract: Handle unexpected zip structures
                if (!Directory.Exists(sourceFolder))
                {
                    try
                    {
                        var entries = Directory.GetFileSystemEntries(extractPath);
                        if (entries.Length == 1 && Directory.Exists(entries[0]))
                        {
                            Log.Notice($"Expected source folder not found. Auto-detected root folder: {Path.GetFileName(entries[0])}");
                            sourceFolder = entries[0];
                        }
                        else if (entries.Length == 0)
                        {
                            OnErrorOccurred($"Extract directory is empty: {extractPath}");
                            return false;
                        }
                        else
                        {
                            OnErrorOccurred($"Expected source folder not found and multiple entries detected in: {extractPath}");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnErrorOccurred($"Failed to detect extract directory structure: {ex.Message}");
                        return false;
                    }
                }

                var targetPath = Path.Combine(_baseDirectory, component.TargetFolder);

                if (!await Task.Run(() => _componentInstaller.InstallFiles(sourceFolder, targetPath), cancellationToken))
                    return false;

                // Validate installation
                if (!_installationValidator.Validate(component.Type, targetPath))
                {
                    OnErrorOccurred($"Installation validation failed for {componentName}. Critical files missing.");
                    // Cleanup broken installation
                    if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                    return false;
                }

                OnComponentInstalled(component);
                Log.Notice($"{component.Name} installed successfully", Log.LogSection.Ognmp);

                // Initialize component-specific configuration
                await _componentConfigInitializer.InitializeComponentConfigAsync(componentName, component);

                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Failed to install {componentName}: {ex.Message}");
                return false;
            }
            finally
            {
                _componentInstaller.Cleanup(downloadPath, extractPath);
            }
        }

        private bool ValidateInstallation(ComponentType componentType, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                Log.Error("Target path cannot be null or empty for validation");
                return false;
            }
            if (!Directory.Exists(targetPath))
            {
                Log.Error($"Target directory not found: {targetPath}");
                return false;
            }

            string criticalFile = componentType switch
            {
                ComponentType.PHP => "php-cgi.exe",
                ComponentType.Nginx => "nginx.exe",
                ComponentType.MariaDB => Path.Combine("bin", "mysqld.exe"),
                ComponentType.PhpMyAdmin => "index.php",
                ComponentType.PhpXdebug => null, // DLLs validated differently
                _ => null
            };

            if (criticalFile != null)
            {
                var fullPath = Path.Combine(targetPath, criticalFile);
                if (!File.Exists(fullPath)) 
                {
                    // Fallback check for MariaDB (renamed in some versions)
                    if (componentType == ComponentType.MariaDB && File.Exists(Path.Combine(targetPath, "bin", "mariadbd.exe")))
                        return true;

                    Log.Error($"Critical file not found: {fullPath}");
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> DownloadAndInstallDllAsync(
            ComponentInfo component,
            string componentName,
            CancellationToken cancellationToken)
        {
            var tempPath = Path.GetTempPath();
            var tempFile = Path.Combine(tempPath, Guid.NewGuid() + ".tmp");

            try
            {
                var targetPath = Path.Combine(_baseDirectory, component.TargetFolder);
                Directory.CreateDirectory(targetPath);

                var targetFile = Path.Combine(targetPath, component.FileName);

                // Download to temp file first to prevent partial files
                Log.Notice($"Downloading {component.Name} {component.Version}...");
                if (!await _componentDownloader.DownloadAsync(component.Url, tempFile, componentName, cancellationToken))
                    return false;

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }
                
                File.Move(tempFile, targetFile);

                OnComponentInstalled(component);
                Log.Notice($"{component.Name} installed successfully", Log.LogSection.Ognmp);

                // Configure Xdebug in php.ini if applicable
                if (component.Type == ComponentType.PhpXdebug)
                {
                    await _componentConfigInitializer.ConfigureXdebugAsync(component);
                }

                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Failed to install {componentName}: {ex.Message}");
                return false;
            }
            finally
            {
                _componentInstaller.Cleanup(tempFile);
            }
        }

        private void OnDownloadProgressChanged(DownloadProgressEventArgs e)
        {
            DownloadProgressChanged?.Invoke(this, e);
        }

        private void OnComponentInstalled(ComponentInfo component)
        {
            ComponentInstalled?.Invoke(this, new ComponentEventArgs { Component = component });
        }

        private void OnErrorOccurred(string errorMessage)
        {
            ErrorOccurred?.Invoke(this, new DependencyErrorEventArgs(errorMessage));
            Log.Error(errorMessage);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
