﻿/*
 * Copyright (c) 2012 - 2017, Kurt Cancemi (kurt@x64architecture.com)
 *
 * This file is part of Wnmp.
 *
 *  Wnmp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Wnmp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Wnmp.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ognmp
{
    /// <summary>
    /// Logs information and errors to a RichTextBox
    /// </summary>
    public static class Log
    {
        private static RichTextBox rtfLog;

        public enum LogSection
        {
            Ognmp,
            Nginx,
            MariaDB,
            PHP,
        }

        public static string LogSectionToString(LogSection logSection)
        {
            switch (logSection) {
                case LogSection.Ognmp:
                    return "Ognmp";
                case LogSection.Nginx:
                    return "Nginx";
                case LogSection.MariaDB:
                    return "MariaDB";
                case LogSection.PHP:
                    return "PHP";
                default:
                    return "";
            }
        }

        private static void OgnmpLog(string message, Color color, LogSection logSection)
        {
            string SectionName = LogSectionToString(logSection);
            string DateNow = DateTime.Now.ToString();
            string str = $"{DateNow} [{SectionName}] - {message}\n";
            int textLength = rtfLog.TextLength;
            rtfLog.AppendText(str);
            if (rtfLog.Find(SectionName, textLength, RichTextBoxFinds.MatchCase) != -1) {
                rtfLog.SelectionLength = SectionName.Length;
                rtfLog.SelectionColor = color;
            }

            rtfLog.ScrollToCaret();
            rtfLog.SelectionLength = 0;
        }
        /// <summary>
        /// Log error
        /// </summary>
        public static void Error(string message, LogSection logSection = LogSection.Ognmp)
        {
            OgnmpLog(message, Color.Red, logSection);
        }
        /// <summary>
        /// Log information
        /// </summary>
        public static void Notice(string message, LogSection section = LogSection.Ognmp)
        {
            OgnmpLog(message, Color.DarkBlue, section);
        }

        public static void SetLogComponent(RichTextBox logRichTextBox)
        {
            rtfLog = logRichTextBox;
            var logContextMenu = new ContextMenu();
            var CopyItem = new MenuItem("&Copy");
            CopyItem.Click += (s, e) => {
                if (rtfLog.SelectedText != String.Empty)
                    Clipboard.SetText(rtfLog.SelectedText);
            };
            logContextMenu.MenuItems.Add(CopyItem);
            rtfLog.ContextMenu = logContextMenu;
        }
    }
}
