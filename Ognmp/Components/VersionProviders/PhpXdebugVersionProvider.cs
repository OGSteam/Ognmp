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
    public class PhpXdebugVersionProvider : VersionProviderBase
    {
        public override string ComponentName => "PHP Xdebug";
        protected override string FetchPageUrl => "https://xdebug.org/download";

        public PhpXdebugVersionProvider(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override List<ComponentInfo> ParseVersionsFromResponse(string html)
        {
            // Parse Xdebug versions - looking for Windows x64 DLL files for PHP 8.x
            var versionPattern = @"php_xdebug-(\d+\.\d+\.\d+)-(\d+\.\d+)-vs16-x86_64\.dll";
            var matches = Regex.Matches(html, versionPattern);
            
            var versions = new List<ComponentInfo>();

            foreach (Match match in matches)
            {
                var xdebugVersion = match.Groups[1].Value;
                var phpVersion = match.Groups[2].Value;
                var description = GetXdebugDescription(xdebugVersion, phpVersion);

                versions.Add(new ComponentInfo
                {
                    Type = ComponentType.PhpXdebug,
                    Name = "PHP Xdebug",
                    Version = $"Xdebug {xdebugVersion} for PHP {phpVersion}",
                    Url = $"https://xdebug.org/files/php_xdebug-{xdebugVersion}-{phpVersion}-vs16-x86_64.dll",
                    FileName = $"php_xdebug-{xdebugVersion}-{phpVersion}-vs16-x86_64.dll",
                    ExtractFolder = "",
                    TargetFolder = $"php\\{phpVersion}\\ext",
                    Description = description
                });
            }

            return versions;
        }

        protected override List<ComponentInfo> DeduplicateVersions(List<ComponentInfo> versions)
        {
            // Custom deduplication: key is xdebugVersion_phpVersion
            var processedVersions = new HashSet<string>();
            var result = new List<ComponentInfo>();

            foreach (var version in versions)
            {
                // Extract xdebugVersion and phpVersion from the Version string
                var match = Regex.Match(version.Version, @"Xdebug (\d+\.\d+\.\d+) for PHP (\d+\.\d+)");
                if (match.Success)
                {
                    var key = $"{match.Groups[1].Value}_{match.Groups[2].Value}";
                    if (processedVersions.Add(key))
                    {
                        result.Add(version);
                    }
                }
                else
                {
                    result.Add(version);
                }
            }

            return result;
        }

        protected override List<ComponentInfo> SortVersions(List<ComponentInfo> versions)
        {
            // Custom sorting: extract and sort by Xdebug version
            return versions.OrderByDescending(v => ExtractXdebugVersion(v.Version)).ToList();
        }

        private string ExtractXdebugVersion(string versionString)
        {
            var match = Regex.Match(versionString, @"Xdebug (\d+\.\d+\.\d+)");
            return match.Success ? match.Groups[1].Value : "0.0.0";
        }

        private string GetXdebugDescription(string xdebugVersion, string phpVersion)
        {
            return $"Xdebug {xdebugVersion} debugger extension for PHP {phpVersion}";
        }

        protected override List<ComponentInfo> GetFallbackVersions()
        {
            return new List<ComponentInfo>
            {
                new ComponentInfo
                {
                    Type = ComponentType.PhpXdebug,
                    Name = "PHP Xdebug",
                    Version = "Xdebug 3.4.0 for PHP 8.5",
                    Url = "https://xdebug.org/files/php_xdebug-3.4.0-8.5-vs16-x86_64.dll",
                    FileName = "php_xdebug-3.4.0-8.5-vs16-x86_64.dll",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.5\\ext",
                    Description = "Xdebug 3.4.0 debugger extension for PHP 8.5"
                },
                new ComponentInfo
                {
                    Type = ComponentType.PhpXdebug,
                    Name = "PHP Xdebug",
                    Version = "Xdebug 3.4.0 for PHP 8.4",
                    Url = "https://xdebug.org/files/php_xdebug-3.4.0-8.4-vs16-x86_64.dll",
                    FileName = "php_xdebug-3.4.0-8.4-vs16-x86_64.dll",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.4\\ext",
                    Description = "Xdebug 3.4.0 debugger extension for PHP 8.4"
                },
                new ComponentInfo
                {
                    Type = ComponentType.PhpXdebug,
                    Name = "PHP Xdebug",
                    Version = "Xdebug 3.3.2 for PHP 8.3",
                    Url = "https://xdebug.org/files/php_xdebug-3.3.2-8.3-vs16-x86_64.dll",
                    FileName = "php_xdebug-3.3.2-8.3-vs16-x86_64.dll",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.3\\ext",
                    Description = "Xdebug 3.3.2 debugger extension for PHP 8.3"
                },
                new ComponentInfo
                {
                    Type = ComponentType.PhpXdebug,
                    Name = "PHP Xdebug",
                    Version = "Xdebug 3.3.2 for PHP 8.2",
                    Url = "https://xdebug.org/files/php_xdebug-3.3.2-8.2-vs16-x86_64.dll",
                    FileName = "php_xdebug-3.3.2-8.2-vs16-x86_64.dll",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.2\\ext",
                    Description = "Xdebug 3.3.2 debugger extension for PHP 8.2"
                },
                new ComponentInfo
                {
                    Type = ComponentType.PhpXdebug,
                    Name = "PHP Xdebug",
                    Version = "Xdebug 3.3.2 for PHP 8.1",
                    Url = "https://xdebug.org/files/php_xdebug-3.3.2-8.1-vs16-x86_64.dll",
                    FileName = "php_xdebug-3.3.2-8.1-vs16-x86_64.dll",
                    ExtractFolder = "",
                    TargetFolder = "php\\8.1\\ext",
                    Description = "Xdebug 3.3.2 debugger extension for PHP 8.1"
                }
            };
        }
    }
}
