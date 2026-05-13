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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Text;
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
        private static readonly object LogFileLock = new object();
        private static readonly string LogDirectory = Path.Combine(Program.StartupPath, "logs");

        public static string CurrentLogFilePath => Path.Combine(LogDirectory, $"ognmp-{DateTime.Now:yyyyMMdd}.log");

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

        private static string BuildLogLine(string level, string message, LogSection logSection)
        {
            var sectionName = LogSectionToString(logSection);
            var dateNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            return $"{dateNow} [{level}] [{sectionName}] - {message}";
        }

        private static void WriteFileLog(string line)
        {
            try
            {
                lock (LogFileLock)
                {
                    Directory.CreateDirectory(LogDirectory);
                    File.AppendAllText(CurrentLogFilePath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Logging must never crash the control panel.
            }
        }

        private static void OgnmpLog(string level, string message, Color color, LogSection logSection)
        {
            var str = BuildLogLine(level, message, logSection);
            WriteFileLog(str);

            if (_rtfLog == null || _rtfLog.IsDisposed)
                return;

            if (_rtfLog.InvokeRequired)
            {
                _rtfLog.Invoke(new Action(() => OgnmpLogToControl(str, color, logSection)));
                return;
            }

            OgnmpLogToControl(str, color, logSection);
        }

        private static void OgnmpLogToControl(string str, Color color, LogSection logSection)
        {
            var sectionName = LogSectionToString(logSection);
            var textLength = _rtfLog.TextLength;
            _rtfLog.AppendText(str + Environment.NewLine);
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
            OgnmpLog("ERROR", message, Color.Red, logSection);
        }

        public static void Error(Exception exception, LogSection logSection = LogSection.Ognmp)
        {
            if (exception == null)
            {
                Error("Unknown exception", logSection);
                return;
            }

            Error(exception.ToString(), logSection);
        }

        /// <summary>
        ///     Log warning
        /// </summary>
        public static void Warning(string message, LogSection logSection = LogSection.Ognmp)
        {
            OgnmpLog("WARN", message, Color.DarkOrange, logSection);
        }

        /// <summary>
        ///     Log information
        /// </summary>
        public static void Notice(string message, LogSection section = LogSection.Ognmp)
        {
            OgnmpLog("INFO", message, Color.DarkBlue, section);
        }

        public static void SetLogComponent(RichTextBox logRichTextBox)
        {
            _rtfLog = logRichTextBox;
            
            var logContextMenu = new ContextMenuStrip();
            var copyItem = new ToolStripMenuItem("&Copy");
            copyItem.Click += (s, e) =>
            {
                if (_rtfLog.SelectedText != string.Empty)
                    Clipboard.SetText(_rtfLog.SelectedText);
            };
            logContextMenu.Items.Add(copyItem);
            logContextMenu.Items.Add(new ToolStripSeparator());

            var openLogItem = new ToolStripMenuItem("Open Log File");
            openLogItem.Click += (s, e) => OpenLogTarget(CurrentLogFilePath);
            logContextMenu.Items.Add(openLogItem);

            var openLogFolderItem = new ToolStripMenuItem("Open Logs Folder");
            openLogFolderItem.Click += (s, e) => OpenLogTarget(LogDirectory);
            logContextMenu.Items.Add(openLogFolderItem);

            _rtfLog.ContextMenuStrip = logContextMenu;
        }

        private static void OpenLogTarget(string path)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
    }
}
