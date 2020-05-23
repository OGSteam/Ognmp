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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Ognmp
{
    /// <summary>
    ///     Logs information and errors to a RichTextBox
    /// </summary>
    public static class Log
    {
        public enum LogSection
        {
            Ognmp,
            Nginx,
            MariaDb,
            Php
        }

        private static RichTextBox _rtfLog;

        public static string LogSectionToString(LogSection logSection)
        {
            switch (logSection)
            {
                case LogSection.Ognmp:
                    return "Ognmp";
                case LogSection.Nginx:
                    return "Nginx";
                case LogSection.MariaDb:
                    return "MariaDB";
                case LogSection.Php:
                    return "PHP";
                default:
                    return "";
            }
        }

        private static void OgnmpLog(string message, Color color, LogSection logSection)
        {
            var sectionName = LogSectionToString(logSection);
            var dateNow = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            var str = $"{dateNow} [{sectionName}] - {message}\n";
            var textLength = _rtfLog.TextLength;
            _rtfLog.AppendText(str);
            if (_rtfLog.Find(sectionName, textLength, RichTextBoxFinds.MatchCase) != -1)
            {
                _rtfLog.SelectionLength = sectionName.Length;
                _rtfLog.SelectionColor = color;
            }

            _rtfLog.ScrollToCaret();
            _rtfLog.SelectionLength = 0;
        }

        /// <summary>
        ///     Log error
        /// </summary>
        public static void Error(string message, LogSection logSection = LogSection.Ognmp)
        {
            OgnmpLog(message, Color.Red, logSection);
        }

        /// <summary>
        ///     Log information
        /// </summary>
        public static void Notice(string message, LogSection section = LogSection.Ognmp)
        {
            OgnmpLog(message, Color.DarkBlue, section);
        }

        public static void SetLogComponent(RichTextBox logRichTextBox)
        {
            _rtfLog = logRichTextBox;
            var logContextMenu = new ContextMenu();
            var copyItem = new MenuItem("&Copy");
            copyItem.Click += (s, e) =>
            {
                if (_rtfLog.SelectedText != string.Empty)
                    Clipboard.SetText(_rtfLog.SelectedText);
            };
            logContextMenu.MenuItems.Add(copyItem);
            _rtfLog.ContextMenu = logContextMenu;
        }
    }
}