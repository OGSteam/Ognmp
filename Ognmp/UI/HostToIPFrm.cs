﻿// /*
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
using System.Net;
using System.Windows.Forms;

namespace Ognmp.UI
{
    public partial class HostToIpFrm : Form
    {
        public HostToIpFrm()
        {
            InitializeComponent();
        }

        private void HostToIpButton_Click(object sender, EventArgs e)
        {
            ipAddressesListBox.Items.Clear();
            try
            {
                var IPs = Dns.GetHostAddresses(hostTextBox.Text);
                foreach (var IP in IPs)
                    ipAddressesListBox.Items.Add(IP.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}