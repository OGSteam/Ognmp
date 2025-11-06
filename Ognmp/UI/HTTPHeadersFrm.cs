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
using System.Net.Http;
using System.Windows.Forms;

namespace Ognmp.UI
{
    public partial class HttpHeadersFrm : Form
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public HttpHeadersFrm()
        {
            InitializeComponent();
        }

        private async void GetHeadersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            httpHeadersListView.Items.Clear();
            try
            {
                using (var response = await httpClient.GetAsync(urlTextBox.Text))
                {
                    foreach (var header in response.Headers)
                    {
                        var item = new ListViewItem(header.Key);
                        item.SubItems.Add(string.Join(", ", header.Value));
                        httpHeadersListView.Items.Add(item);
                    }

                    // Also include content headers
                    if (response.Content != null)
                    {
                        foreach (var header in response.Content.Headers)
                        {
                            var item = new ListViewItem(header.Key);
                            item.SubItems.Add(string.Join(", ", header.Value));
                            httpHeadersListView.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}