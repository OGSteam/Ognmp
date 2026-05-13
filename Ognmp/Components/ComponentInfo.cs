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

namespace Ognmp.Components
{
    /// <summary>
    /// Represents metadata about a component that can be downloaded and installed.
    /// This class is immutable after construction.
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// Gets the type of the component.
        /// </summary>
        public ComponentType Type { get; init; }

        /// <summary>
        /// Gets the display name of the component.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the version string of the component.
        /// </summary>
        public string Version { get; init; }

        /// <summary>
        /// Gets the download URL for the component.
        /// </summary>
        public string Url { get; init; }

        /// <summary>
        /// Gets the filename of the downloaded file.
        /// </summary>
        public string FileName { get; init; }

        /// <summary>
        /// Gets the folder name to extract from the archive (if applicable).
        /// </summary>
        public string ExtractFolder { get; init; }

        /// <summary>
        /// Gets the target installation folder path.
        /// </summary>
        public string TargetFolder { get; init; }

        /// <summary>
        /// Gets the description of the component.
        /// </summary>
        public string Description { get; init; }
    }
}
