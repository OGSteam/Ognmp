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
using System.Text.RegularExpressions;

namespace Ognmp.Components.VersionProviders
{
    public class NginxVersionProvider : VersionProviderBase
    {
        public override string ComponentName => "Nginx";
        protected override string FetchPageUrl => "https://nginx.org/en/download.html";

        public NginxVersionProvider(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override List<ComponentInfo> ParseVersionsFromResponse(string html)
        {
            var versionPattern = @"nginx-(\d+\.\d+\.\d+)\.zip";
            var matches = Regex.Matches(html, versionPattern);
            
            var versions = new List<ComponentInfo>();

            foreach (Match match in matches)
            {
                var version = match.Groups[1].Value;
                var isStable = IsStableVersion(version);
                var description = isStable 
                    ? "High-performance HTTP server and reverse proxy (Stable)" 
                    : "High-performance HTTP server and reverse proxy";

                versions.Add(new ComponentInfo
                {
                    Type = ComponentType.Nginx,
                    Name = "Nginx",
                    Version = version,
                    Url = $"https://nginx.org/download/nginx-{version}.zip",
                    FileName = $"nginx-{version}.zip",
                    ExtractFolder = $"nginx-{version}",
                    TargetFolder = "nginx",
                    Description = description
                });
            }

            return versions;
        }

        protected override List<ComponentInfo> GetFallbackVersions()
        {
            return new List<ComponentInfo>
            {
                new ComponentInfo
                {
                    Type = ComponentType.Nginx,
                    Name = "Nginx",
                    Version = "1.25.3",
                    Url = "https://nginx.org/download/nginx-1.25.3.zip",
                    FileName = "nginx-1.25.3.zip",
                    ExtractFolder = "nginx-1.25.3",
                    TargetFolder = "nginx",
                    Description = "High-performance HTTP server and reverse proxy"
                },
                new ComponentInfo
                {
                    Type = ComponentType.Nginx,
                    Name = "Nginx",
                    Version = "1.24.0",
                    Url = "https://nginx.org/download/nginx-1.24.0.zip",
                    FileName = "nginx-1.24.0.zip",
                    ExtractFolder = "nginx-1.24.0",
                    TargetFolder = "nginx",
                    Description = "High-performance HTTP server and reverse proxy (Stable)"
                }
            };
        }

        private bool IsStableVersion(string version)
        {
            var parts = version.Split('.');
            if (parts.Length >= 2 && int.TryParse(parts[1], out var minor))
            {
                return minor % 2 == 0; // Stable versions have even minor numbers
            }
            return false;
        }
    }
}
