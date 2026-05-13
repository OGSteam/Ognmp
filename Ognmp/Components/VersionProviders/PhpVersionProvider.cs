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
using System.Text.RegularExpressions;

namespace Ognmp.Components.VersionProviders
{
    public class PhpVersionProvider : VersionProviderBase
    {
        public override string ComponentName => "PHP";
        protected override string FetchPageUrl => "https://windows.php.net/downloads/releases/";

        public PhpVersionProvider(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override List<ComponentInfo> ParseVersionsFromResponse(string html)
        {
            var versions = new Dictionary<string, ComponentInfo>();
            
            // Multiple regex patterns to find different link formats for x64 NTS builds
            var patterns = new List<string>
            {
                @"href=[""']([^""']*php-(\d+\.\d+\.\d+)-Win32-vs16-x64\.zip)[""']",
                @"href=[""']([^""']*php-(\d+\.\d+\.\d+)-nts-Win32-vs16-x64\.zip)[""']",
                @"php-(\d+\.\d+\.\d+)-Win32-vs16-x64\.zip",
                @"php-(\d+\.\d+\.\d+)-nts-Win32-vs16-x64\.zip"
            };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);
                
                Log.Notice($"Pattern '{pattern.Substring(0, Math.Min(50, pattern.Length))}...' found {matches.Count} matches");
                
                foreach (Match match in matches)
                {
                    // Extract version number (group 2 for href patterns, group 1 for filename-only patterns)
                    string version;
                    if (match.Groups.Count > 2 && !string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        version = match.Groups[2].Value;
                    }
                    else
                    {
                        version = match.Groups[1].Value;
                        // If group 1 contains the full filename, extract just the version
                        var versionMatch = Regex.Match(version, @"(\d+\.\d+\.\d+)");
                        if (versionMatch.Success)
                        {
                            version = versionMatch.Groups[1].Value;
                        }
                    }
                    
                    // Validate version format
                    if (!IsValidVersion(version))
                    {
                        Log.Warning($"Skipping invalid version: {version}");
                        continue;
                    }

                    // Only process PHP 8.x versions
                    if (!version.StartsWith("8."))
                    {
                        continue;
                    }

                    // Only add if we haven't seen this version yet
                    if (!versions.ContainsKey(version))
                    {
                        var majorMinor = GetMajorMinorVersion(version);
                        var description = GetPhpVersionDescription(version);

                        versions[version] = new ComponentInfo
                        {
                            Type = ComponentType.PHP,
                            Name = "PHP",
                            Version = version,
                            Url = $"https://windows.php.net/downloads/releases/php-{version}-Win32-vs16-x64.zip",
                            FileName = $"php-{version}-Win32-vs16-x64.zip",
                            ExtractFolder = "",
                            TargetFolder = $"php\\{majorMinor}",
                            Description = description
                        };
                        
                        Log.Notice($"Added PHP version {version}");
                    }
                }
            }

            // Log summary
            var result = versions.Values.ToList();
            if (result.Count > 0 && result.Count <= 5)
            {
                foreach (var ver in result)
                {
                    Log.Notice($"  - PHP {ver.Version}");
                }
            }
            else if (result.Count > 5)
            {
                Log.Notice($"  Found {result.Count} PHP versions");
            }
            
            return result;
        }

        protected override List<ComponentInfo> GetFallbackVersions()
        {
            return new List<ComponentInfo>
            {
                new ComponentInfo
                {
                    Type = ComponentType.PHP,
                    Name = "PHP",
                    Version = "8.5.0",
                    Url = "https://windows.php.net/downloads/releases/php-8.5.0-Win32-vs16-x64.zip",
                    FileName = "php-8.5.0-Win32-vs16-x64.zip",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.5",
                    Description = "PHP 8.5 - Latest version"
                },
                new ComponentInfo
                {
                    Type = ComponentType.PHP,
                    Name = "PHP",
                    Version = "8.4.2",
                    Url = "https://windows.php.net/downloads/releases/php-8.4.2-Win32-vs16-x64.zip",
                    FileName = "php-8.4.2-Win32-vs16-x64.zip",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.4",
                    Description = "PHP 8.4 - Stable version"
                },
                new ComponentInfo
                {
                    Type = ComponentType.PHP,
                    Name = "PHP",
                    Version = "8.3.16",
                    Url = "https://windows.php.net/downloads/releases/php-8.3.16-Win32-vs16-x64.zip",
                    FileName = "php-8.3.16-Win32-vs16-x64.zip",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.3",
                    Description = "PHP 8.3 - Active version"
                },
                new ComponentInfo
                {
                    Type = ComponentType.PHP,
                    Name = "PHP",
                    Version = "8.2.28",
                    Url = "https://windows.php.net/downloads/releases/php-8.2.28-Win32-vs16-x64.zip",
                    FileName = "php-8.2.28-Win32-vs16-x64.zip",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.2",
                    Description = "PHP 8.2 - Security fixes only"
                },
                new ComponentInfo
                {
                    Type = ComponentType.PHP,
                    Name = "PHP",
                    Version = "8.1.31",
                    Url = "https://windows.php.net/downloads/releases/php-8.1.31-Win32-vs16-x64.zip",
                    FileName = "php-8.1.31-Win32-vs16-x64.zip",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.1",
                    Description = "PHP 8.1 - End of life soon"
                }
            };
        }

        private bool IsValidVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;

            var parts = version.Split('.');
            if (parts.Length != 3)
                return false;

            // Check if all parts are numbers and within reasonable ranges
            if (!int.TryParse(parts[0], out var major) || major < 0 || major > 99)
                return false;
            if (!int.TryParse(parts[1], out var minor) || minor < 0 || minor > 99)
                return false;
            if (!int.TryParse(parts[2], out var patch) || patch < 0 || patch > 999)
                return false;

            return true;
        }

        private string GetMajorMinorVersion(string version)
        {
            var parts = version.Split('.');
            return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : version;
        }

        private string GetPhpVersionDescription(string version)
        {
            var parts = version.Split('.');
            if (parts.Length < 2)
                return "PHP scripting language runtime";

            var major = parts[0];
            var minor = parts[1];

            return $"PHP {major}.{minor} - " + (major, minor) switch
            {
                ("8", "5") => "Latest version (JIT improvements)",
                ("8", "4") => "Stable version (Property hooks)",
                ("8", "3") => "Active support",
                ("8", "2") => "Security fixes only",
                ("8", "1") => "End of life soon",
                ("8", "0") => "End of life",
                _ => "PHP scripting language runtime"
            };
        }
    }
}
