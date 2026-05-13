using System;
using System.IO;

namespace Ognmp.Components.Installation
{
    /// <summary>
    /// Validates that a component has been correctly installed by checking for critical files.
    /// </summary>
    public class InstallationValidator
    {
        /// <summary>
        /// Validates that the component is properly installed by checking for critical files.
        /// </summary>
        /// <param name="componentType">The type of component to validate</param>
        /// <param name="targetPath">The installation path of the component</param>
        /// <returns>True if the component is valid, false otherwise</returns>
        public bool Validate(ComponentType componentType, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                Log.Error("Target path cannot be null or empty");
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
    }
}
