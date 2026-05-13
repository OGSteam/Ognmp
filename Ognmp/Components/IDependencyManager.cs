using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ognmp.Components.Events;

namespace Ognmp.Components
{
    /// <summary>
    /// Interface for managing server component dependencies (Nginx, PHP, MariaDB, phpMyAdmin)
    /// Provides methods for loading versions, querying installation status, and installing components.
    /// </summary>
    public interface IDependencyManager : IDisposable
    {
        /// <summary>
        /// Fired when a download progresses, with percentage and bytes information.
        /// </summary>
        event EventHandler<DownloadProgressEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Fired when a component is successfully installed.
        /// </summary>
        event EventHandler<ComponentEventArgs> ComponentInstalled;

        /// <summary>
        /// Fired when an error occurs during download or installation.
        /// </summary>
        event EventHandler<DependencyErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Gets the collection of available versions for all installed components.
        /// Dictionary key is component name, value is list of available ComponentInfo.
        /// </summary>
        Dictionary<string, List<ComponentInfo>> AvailableVersions { get; }

        /// <summary>
        /// Loads available versions for all components from remote sources.
        /// </summary>
        Task LoadAvailableVersionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of available versions for a specific component.
        /// </summary>
        List<ComponentInfo> GetVersionsForComponent(string componentName);

        /// <summary>
        /// Checks if a component is currently installed.
        /// </summary>
        bool IsComponentInstalled(string componentName);

        /// <summary>
        /// Downloads and installs a specific component version.
        /// If version is null, installs the latest/first available version.
        /// </summary>
        Task<bool> DownloadAndInstallComponentAsync(
            string componentName,
            string version = null,
            CancellationToken cancellationToken = default);
    }
}
