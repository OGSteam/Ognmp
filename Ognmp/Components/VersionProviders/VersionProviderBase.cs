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
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ognmp.Components.VersionProviders
{
    /// <summary>
    /// Abstract base class for version providers that implements common functionality
    /// such as HTTP fetching, error handling, fallback logic, and version sorting.
    /// </summary>
    public abstract class VersionProviderBase : IVersionProvider
    {
        protected readonly HttpClient HttpClient;

        /// <summary>
        /// Gets the name of the component this provider serves.
        /// </summary>
        public abstract string ComponentName { get; }

        /// <summary>
        /// Gets the URL to fetch version information from.
        /// </summary>
        protected abstract string FetchPageUrl { get; }

        protected VersionProviderBase(HttpClient httpClient)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Template method that orchestrates version fetching: fetch → parse → dedupe → sort → fallback on error.
        /// Can be overridden for custom fetching logic (e.g., MariaDB's direct API approach).
        /// </summary>
        public virtual async Task<List<ComponentInfo>> GetAvailableVersionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Notice($"Fetching available versions for {ComponentName}...");
                
                var response = await FetchVersionDataAsync(cancellationToken);
                var versions = ParseVersionsFromResponse(response);
                
                // Deduplicate by version string
                var deduplicatedVersions = DeduplicateVersions(versions);
                
                if (deduplicatedVersions.Any())
                {
                    Log.Notice($"Found {deduplicatedVersions.Count} versions for {ComponentName}");
                    return SortVersions(deduplicatedVersions);
                }

                Log.Warning($"No {ComponentName} versions found via scraping, using fallback versions");
                return GetFallbackVersions();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to fetch {ComponentName} versions: {ex.Message}");
                Log.Notice($"Using fallback versions for {ComponentName}");
                return GetFallbackVersions();
            }
        }

        /// <summary>
        /// Fetches the raw data (HTML/JSON) from the source URL.
        /// Can be overridden for custom fetching logic (e.g., HEAD requests, API calls).
        /// </summary>
        protected virtual async Task<string> FetchVersionDataAsync(CancellationToken cancellationToken)
        {
            return await HttpClient.GetStringAsync(FetchPageUrl, cancellationToken);
        }

        /// <summary>
        /// Parses component information from the fetched response data.
        /// Must be implemented by derived classes to extract version-specific data.
        /// </summary>
        protected abstract List<ComponentInfo> ParseVersionsFromResponse(string response);

        /// <summary>
        /// Returns a list of hardcoded fallback versions to use when scraping fails.
        /// Must be implemented by derived classes.
        /// </summary>
        protected abstract List<ComponentInfo> GetFallbackVersions();

        /// <summary>
        /// Deduplicates versions by version string.
        /// Can be overridden for custom deduplication logic.
        /// </summary>
        protected virtual List<ComponentInfo> DeduplicateVersions(List<ComponentInfo> versions)
        {
            var processedVersions = new HashSet<string>();
            var result = new List<ComponentInfo>();

            foreach (var version in versions)
            {
                if (processedVersions.Add(version.Version))
                {
                    result.Add(version);
                }
            }

            return result;
        }

        /// <summary>
        /// Sorts versions in descending order (latest first).
        /// Can be overridden for custom sorting logic.
        /// </summary>
        protected virtual List<ComponentInfo> SortVersions(List<ComponentInfo> versions)
        {
            try
            {
                return versions.OrderByDescending(v => new Version(v.Version)).ToList();
            }
            catch
            {
                // If version parsing fails, return as-is
                Log.Warning($"Could not sort {ComponentName} versions by System.Version, returning unsorted");
                return versions;
            }
        }
    }
}
