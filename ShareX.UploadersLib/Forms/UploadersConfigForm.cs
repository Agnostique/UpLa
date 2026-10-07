#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using ShareX.UploadersLib.ImageUploaders;
using ShareX.UploadersLib.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX.UploadersLib
{
    public partial class UploadersConfigForm : Form
    {
        public static bool IsInstanceActive => instance != null && !instance.IsDisposed;

        private static UploadersConfigForm instance;

        public UploadersConfig Config { get; private set; }

        private ImageList uploadersImageList;

        private UploadersConfigForm(UploadersConfig config)
        {
            Config = config;

            InitializeComponent();
            ShareXResources.ApplyTheme(this, true);

            InitializeControls();
        }

        public static UploadersConfigForm GetFormInstance(UploadersConfig config)
        {
            if (!IsInstanceActive)
            {
                instance = new UploadersConfigForm(config);
            }

            return instance;
        }

        private void UploadersConfigForm_Shown(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void UploadersConfigForm_Resize(object sender, EventArgs e)
        {
            Refresh();
        }

        private void InitializeControls()
        {
            if (!string.IsNullOrEmpty(Config.FilePath))
            {
                Text += " - " + Config.FilePath;
            }

            AddIconToTabs();

            tttvMain.ImageList = uploadersImageList;
            tttvMain.MainTabControl = tcUploaders;
        }

        private void AddIconToTabs()
        {
            uploadersImageList = new ImageList();
            uploadersImageList.ColorDepth = ColorDepth.Depth32Bit;

            uploadersImageList.Images.Add(Resources.navigation_270_button_white);

            foreach (IUploaderService uploaderService in UploaderFactory.AllServices)
            {
                TabPage tp = uploaderService.GetUploadersConfigTabPage(this);

                if (tp != null && string.IsNullOrEmpty(tp.ImageKey))
                {
                    Icon icon = uploaderService.ServiceIcon;

                    if (icon != null)
                    {
                        uploadersImageList.Images.Add(tp.Name, icon);
                        tp.ImageKey = tp.Name;
                    }
                    else
                    {
                        Image img = uploaderService.ServiceImage;

                        if (img != null)
                        {
                            uploadersImageList.Images.Add(tp.Name, img);
                            tp.ImageKey = tp.Name;
                        }
                    }
                }
            }
        }

        public void NavigateToTabPage(TabPage tp)
        {
            if (tp != null)
            {
                tttvMain.NavigateToTabPage(tp);
            }
        }

        private void LoadSettings()
        {
            LoadImageUploaderSettings();
        }

        private void LoadImageUploaderSettings()
        {
            #region Chevereto

            // upla.com.tr: the Chevereto URL/API key fields are replaced by the upla account and upload options.
            UplaSettingsControl.AttachTo(tpChevereto, Config);

            #endregion Chevereto

        }

        #region Image uploaders

        #region Chevereto

        private void txtCheveretoWebsite_TextChanged(object sender, EventArgs e)
        {
            Config.CheveretoUploader.UploadURL = txtCheveretoUploadURL.Text;
        }

        private void txtCheveretoAPIKey_TextChanged(object sender, EventArgs e)
        {
            Config.CheveretoUploader.APIKey = txtCheveretoAPIKey.Text;
        }

        private void cbCheveretoDirectURL_CheckedChanged(object sender, EventArgs e)
        {
            Config.CheveretoDirectURL = cbCheveretoDirectURL.Checked;
        }

        #endregion Chevereto

        #endregion Image uploaders

    }
}