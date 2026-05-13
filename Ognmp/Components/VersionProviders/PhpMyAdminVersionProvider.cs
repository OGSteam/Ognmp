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

using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Ognmp.Components.VersionProviders
{
    public class PhpMyAdminVersionProvider : VersionProviderBase
    {
        public override string ComponentName => "PhpMyAdmin";
        protected override string FetchPageUrl => "https://www.phpmyadmin.net/files/";

        public PhpMyAdminVersionProvider(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override List<ComponentInfo> ParseVersionsFromResponse(string html)
        {
            var versionPattern = @"phpMyAdmin-(\d+\.\d+\.\d+)-all-languages\.zip";
            var matches = Regex.Matches(html, versionPattern);
            
            var versions = new List<ComponentInfo>();

            foreach (Match match in matches)
            {
                var version = match.Groups[1].Value;
                versions.Add(new ComponentInfo
                {
                    Type = ComponentType.PhpMyAdmin,
                    Name = "phpMyAdmin",
                    Version = version,
                    Url = $"https://files.phpmyadmin.net/phpMyAdmin/{version}/phpMyAdmin-{version}-all-languages.zip",
                    FileName = $"phpMyAdmin-{version}-all-languages.zip",
                    ExtractFolder = $"phpMyAdmin-{version}-all-languages",
                    TargetFolder = "www\\phpmyadmin",
                    Description = "Web-based MySQL administration tool"
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
                    Type = ComponentType.PhpMyAdmin,
                    Name = "phpMyAdmin",
                    Version = "5.2.1",
                    Url = "https://files.phpmyadmin.net/phpMyAdmin/5.2.1/phpMyAdmin-5.2.1-all-languages.zip",
                    FileName = "phpMyAdmin-5.2.1-all-languages.zip",
                    ExtractFolder = "phpMyAdmin-5.2.1-all-languages",
                    TargetFolder = "www\\phpmyadmin",
                    Description = "Web-based MySQL administration tool"
                }
            };
        }
    }
}
