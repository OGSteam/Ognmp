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

namespace Ognmp.Components.Events
{
    /// <summary>
    /// Event arguments for download progress updates.
    /// </summary>
    public class DownloadProgressEventArgs : EventArgs
    {
        public string ComponentName { get; init; }
        public int ProgressPercentage { get; init; }
        public long BytesReceived { get; init; }
        public long TotalBytes { get; init; }

        public DownloadProgressEventArgs()
        {
        }

        public DownloadProgressEventArgs(string componentName, int progressPercentage, long bytesReceived, long totalBytes)
        {
            ComponentName = componentName;
            ProgressPercentage = progressPercentage;
            BytesReceived = bytesReceived;
            TotalBytes = totalBytes;
        }
    }
}