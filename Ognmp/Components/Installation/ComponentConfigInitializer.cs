using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ognmp.Components.Installation
{
    /// <summary>
    /// Responsible for initializing component-specific configuration files and settings.
    /// Handles component configuration for Nginx, PHP, MariaDB, and Xdebug extensions.
    /// </summary>
    public class ComponentConfigInitializer
    {
        private readonly string _baseDirectory;

        public ComponentConfigInitializer(string baseDirectory)
        {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
        }

        /// <summary>
        /// Initializes configuration for the specified component based on its type.
        /// </summary>
        public async Task InitializeComponentConfigAsync(string componentName, ComponentInfo component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            await Task.Run(() =>
            {
                switch (component.Type)
                {
                    case ComponentType.Nginx:
                        InitializeNginxConfig(component);
                        break;
                    case ComponentType.PHP:
                        InitializePhpConfig(component);
                        break;
                    case ComponentType.MariaDB:
                        InitializeMariaDbConfig(component);
                        break;
                }
            });
        }

        /// <summary>
        /// Initializes Nginx configuration by copying nginx.conf to the conf directory.
        /// </summary>
        private void InitializeNginxConfig(ComponentInfo component)
        {
            if (component == null)
                return;

            try
            {
                var confDir = Path.Combine(_baseDirectory, "conf");
            Directory.CreateDirectory(confDir);

            // Use installation path from component
            var nginxConfSource = Path.Combine(_baseDirectory, component.TargetFolder, "conf", "nginx.conf");
            var nginxConfTarget = Path.Combine(confDir, "nginx.conf");

                if (File.Exists(nginxConfSource) && !File.Exists(nginxConfTarget))
                {
                    File.Copy(nginxConfSource, nginxConfTarget);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize Nginx config: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes PHP configuration by creating php.ini from template and logs directory.
        /// </summary>
        private void InitializePhpConfig(ComponentInfo component)
        {
            try
            {
                var phpDir = Path.Combine(_baseDirectory, component.TargetFolder);
                Log.Notice($"Initializing PHP configuration in {phpDir}...", Log.LogSection.Ognmp);

                var phpIniSource = Path.Combine(phpDir, "php.ini-development");
                var phpIniTarget = Path.Combine(phpDir, "php.ini");

                if (File.Exists(phpIniSource))
                {
                    if (!File.Exists(phpIniTarget))
                    {
                        File.Copy(phpIniSource, phpIniTarget);
                        Log.Notice("Created php.ini from template", Log.LogSection.Ognmp);
                    }
                    else
                    {
                        Log.Notice("php.ini already exists, skipping creation", Log.LogSection.Ognmp);
                    }
                }
                else
                {
                    Log.Warning($"Template php.ini-development not found at {phpIniSource}", Log.LogSection.Ognmp);
                }

                // Create logs directory
                var logsDir = Path.Combine(phpDir, "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                    Log.Notice("Created PHP logs directory", Log.LogSection.Ognmp);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize PHP configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initializes MariaDB configuration by creating data directory and my.ini.
        /// </summary>
        private void InitializeMariaDbConfig(ComponentInfo component)
        {
            if (component == null)
                return;

            try
            {
                var mariaDbDir = Path.Combine(_baseDirectory, component.TargetFolder);
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
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize MariaDB config: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures Xdebug in the PHP ini file for the specified component.
        /// Extracts PHP version from target folder path and updates php.ini accordingly.
        /// </summary>
        public async Task ConfigureXdebugAsync(ComponentInfo component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            await Task.Run(() =>
            {
                // Extract PHP version from target folder (e.g., php\8.3\ext)
                var targetPath = component.TargetFolder.Replace('\\', '/');
                var parts = targetPath.Split('/');
                string phpVersion = null;

                // Find "php" in path and take the next part as version
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (string.Equals(parts[i], "php", StringComparison.OrdinalIgnoreCase))
                    {
                        phpVersion = parts[i + 1];
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(phpVersion))
                {
                    var phpIniPath = Path.Combine(_baseDirectory, "php", phpVersion, "php.ini");

                    if (File.Exists(phpIniPath))
                    {
                        var lines = File.ReadAllLines(phpIniPath).ToList();
                        
                        // Check if Xdebug is already configured
                        var xdebugConfigured = lines.Any(line => 
                            line.Contains("zend_extension") && line.Contains("xdebug"));

                        if (!xdebugConfigured)
                        {
                            // Add Xdebug configuration
                            lines.Add("");
                            lines.Add("; Xdebug Configuration");
                            lines.Add($"zend_extension={component.FileName}");
                            lines.Add("xdebug.mode=debug");
                            lines.Add("xdebug.start_with_request=yes");
                            lines.Add("xdebug.client_port=9003");
                            lines.Add("xdebug.client_host=127.0.0.1");

                            File.WriteAllLines(phpIniPath, lines);
                            Log.Notice($"Xdebug configured in php.ini for PHP {phpVersion}");
                        }
                    }
                }
            });
        }
    }
}
