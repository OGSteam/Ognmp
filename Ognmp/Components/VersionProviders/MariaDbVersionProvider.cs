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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ognmp.Components.VersionProviders
{
    public class MariaDbVersionProvider : VersionProviderBase
    {
        private const string MariaDbReleasesApi = "https://downloads.mariadb.org/rest-api/mariadb/";
        private static readonly string[] KnownBranches = { "11.2", "11.1", "10.11", "10.6" };

        public override string ComponentName => "MariaDB";
        protected override string FetchPageUrl => MariaDbReleasesApi; // Not used, but required by base

        public MariaDbVersionProvider(HttpClient httpClient) : base(httpClient)
        {
        }

        // Override because MariaDB uses direct HEAD requests instead of fetching a page
        public override async Task<List<ComponentInfo>> GetAvailableVersionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Notice($"Fetching available versions for {ComponentName}...");
                
                var versions = new List<ComponentInfo>();

                foreach (var branch in KnownBranches)
                {
                    try
                    {
                        var latestVersion = await GetLatestVersionInBranch(branch, cancellationToken);
                        if (latestVersion != null)
                        {
                            versions.Add(latestVersion);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"Failed to check MariaDB branch {branch}: {ex.Message}");
                    }
                }

                if (versions.Count > 0)
                {
                    Log.Notice($"Found {versions.Count} versions for {ComponentName}");
                    return SortVersions(versions);
                }

                Log.Warning($"No {ComponentName} versions found via API, using fallback versions");
                return GetFallbackVersions();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to fetch {ComponentName} versions: {ex.Message}");
                Log.Notice($"Using fallback versions for {ComponentName}");
                return GetFallbackVersions();
            }
        }

        private async Task<ComponentInfo> GetLatestVersionInBranch(string branch, CancellationToken cancellationToken)
        {
            // Try recent patch versions in descending order
            for (int patch = 10; patch >= 0; patch--)
            {
                var version = $"{branch}.{patch}";
                var url = $"{MariaDbReleasesApi}{version}/mariadb-{version}-winx64.zip";

                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Head, url);
                    var response = await HttpClient.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var description = GetMariaDbVersionDescription(version);
                        return new ComponentInfo
                        {
                            Type = ComponentType.MariaDB,
                            Name = "MariaDB",
                            Version = version,
                            Url = url,
                            FileName = $"mariadb-{version}-winx64.zip",
                            ExtractFolder = $"mariadb-{version}-winx64",
                            TargetFolder = "mariadb",
                            Description = description
                        };
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"Failed to check MariaDB version {version}: {ex.Message}");
                }
            }

            return null;
        }

        private string GetMariaDbVersionDescription(string version)
        {
            var parts = version.Split('.');
            if (parts.Length < 2)
                return "MySQL-compatible database server";

            var major = parts[0];
            var minor = parts[1];

            return $"MariaDB {major}.{minor} - " + (major, minor) switch
            {
                ("11", "2") => "Latest version",
                ("11", "1") => "Stable version",
                ("10", "11") => "LTS version",
                ("10", "6") => "Long-term support",
                _ => "MySQL-compatible database server"
            };
        }

        // These are not used for MariaDB, but required by base class
        protected override List<ComponentInfo> ParseVersionsFromResponse(string response)
        {
            throw new NotSupportedException("MariaDB provider uses direct API requests, not response parsing");
        }

        protected override List<ComponentInfo> GetFallbackVersions()
        {
            return new List<ComponentInfo>
            {
                new ComponentInfo
                {
                    Type = ComponentType.MariaDB,
                    Name = "MariaDB",
                    Version = "11.2.2",
                    Url = "https://downloads.mariadb.org/rest-api/mariadb/11.2.2/mariadb-11.2.2-winx64.zip",
                    FileName = "mariadb-11.2.2-winx64.zip",
                    ExtractFolder = "mariadb-11.2.2-winx64",
                    TargetFolder = "mariadb",
                    Description = "MariaDB 11.2 - Latest version"
                },
                new ComponentInfo
                {
                    Type = ComponentType.MariaDB,
                    Name = "MariaDB",
                    Version = "10.11.6",
                    Url = "https://downloads.mariadb.org/rest-api/mariadb/10.11.6/mariadb-10.11.6-winx64.zip",
                    FileName = "mariadb-10.11.6-winx64.zip",
                    ExtractFolder = "mariadb-10.11.6-winx64",
                    TargetFolder = "mariadb",
                    Description = "MariaDB 10.11 - LTS version"
                }
            };
        }
    }
}
