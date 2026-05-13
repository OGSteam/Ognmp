using System;
using System.IO;
using System.IO.Compression;

namespace Ognmp.Components.Installation
{
    /// <summary>
    /// Handles extraction and installation of component files.
    /// </summary>
    public class ComponentInstaller
    {
        /// <summary>
        /// Extracts a ZIP file to the specified directory.
        /// </summary>
        /// <param name="zipPath">Path to the ZIP file</param>
        /// <param name="extractPath">Destination directory for extraction</param>
        /// <returns>True if extraction succeeded, false otherwise</returns>
        public bool ExtractArchive(string zipPath, string extractPath)
        {
            if (string.IsNullOrWhiteSpace(zipPath))
            {
                Log.Error("ZIP path cannot be null or empty");
                return false;
            }
            if (string.IsNullOrWhiteSpace(extractPath))
            {
                Log.Error("Extract path cannot be null or empty");
                return false;
            }
            if (!File.Exists(zipPath))
            {
                Log.Error($"ZIP file not found: {zipPath}");
                return false;
            }

            try
            {
                Directory.CreateDirectory(extractPath);
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Extraction failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Installs component files from source directory to target directory.
        /// </summary>
        /// <param name="sourceFolder">Source directory containing the files</param>
        /// <param name="targetPath">Target installation directory</param>
        /// <returns>True if installation succeeded, false otherwise</returns>
        public bool InstallFiles(string sourceFolder, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(sourceFolder))
            {
                Log.Error("Source folder cannot be null or empty");
                return false;
            }
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                Log.Error("Target path cannot be null or empty");
                return false;
            }
            if (!Directory.Exists(sourceFolder))
            {
                Log.Error($"Source folder not found: {sourceFolder}");
                return false;
            }

            try
            {
                // Ensure target directory exists
                Directory.CreateDirectory(targetPath);

                // Copy files and overwrite if necessary
                foreach (var sourceFile in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(sourceFolder, sourceFile);
                    var targetFile = Path.Combine(targetPath, relativePath);

                    var targetDir = Path.GetDirectoryName(targetFile);
                    if (targetDir != null && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    File.Copy(sourceFile, targetFile, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Installation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cleans up temporary files and directories.
        /// </summary>
        /// <param name="filePaths">Paths to delete (files or directories)</param>
        public void Cleanup(params string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
                return;

            foreach (var path in filePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"Failed to clean up {path}: {ex.Message}");
                }
            }
        }
    }
}
