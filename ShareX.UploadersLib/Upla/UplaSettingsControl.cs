#region License Information (GPL v3)

/*
    upla.com.tr additions to ShareX
    Copyright (c) 2026 upla.com.tr

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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareX.UploadersLib
{
    // upla.com.tr account and upload options, built in code so the Uploaders settings designer stays as upstream.
    [DesignerCategory("Code")]
    public class UplaSettingsControl : UserControl
    {
        private readonly UploadersConfig config;
        private readonly UplaSettings settings;
        private readonly TableLayoutPanel tlpMain;
        private Label lblAccount;
        private Button btnSignIn, btnSignOut;
        private LinkLabel llProfile, llDevices, llSignUp, llGuest, llManualKey;
        private Control[] manualKeyControls;
        private TextBox txtAPIKey;
        private Button btnVerify;
        private Label lblStatus;
        private bool isVerifying, isUpdatingAccountUI, showManualKey;

        public static void AttachTo(TabPage tabPage, UploadersConfig config)
        {
            if (tabPage.Controls.OfType<UplaSettingsControl>().Any())
            {
                return;
            }

            UplaSettingsControl control = new UplaSettingsControl(config)
            {
                Dock = DockStyle.Fill
            };

            tabPage.SuspendLayout();
            tabPage.Controls.Clear();
            tabPage.Controls.Add(control);
            tabPage.Text = "upla.com.tr";
            tabPage.ResumeLayout();

            ShareXResources.ApplyCustomThemeToControl(control);
        }

        public UplaSettingsControl(UploadersConfig config)
        {
            this.config = config;
            settings = Upla.GetSettings(config);
            showManualKey = Upla.HasPersonalAPIKey(config) && !Upla.IsSignedIn(settings);

            AutoScroll = true;
            Padding = new Padding(6);

            tlpMain = new TableLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Dock = DockStyle.Top
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            SuspendLayout();
            CreateAccountRows();
            CreateOptionRows();
            Controls.Add(tlpMain);
            ResumeLayout();

            UpdateAccountUI();
            UpdateStatus();
            Upla.AccountChanged += Upla_AccountChanged;
            CheckAccountAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Upla.AccountChanged -= Upla_AccountChanged;
            }

            base.Dispose(disposing);
        }

        private void Upla_AccountChanged()
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(Upla_AccountChanged);
                return;
            }

            isUpdatingAccountUI = true;
            txtAPIKey.Text = Upla.IsSignedIn(settings) ? "" : settings.PersonalAPIKey;
            isUpdatingAccountUI = false;
            UpdateAccountUI();
            UpdateStatus();
        }

        // Refreshes the shown account (the username may have changed) and notices a computer that was removed on the website.
        private async void CheckAccountAsync()
        {
            if (!Upla.IsSignedIn(settings))
            {
                return;
            }

            string key = settings.PersonalAPIKey;
            UplaAccountResult result;

            try
            {
                result = await new UplaAccountClient().GetAccountAsync(key);
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
                return;
            }

            if (IsDisposed || key != settings.PersonalAPIKey)
            {
                return;
            }

            if (result.Status == UplaAccountStatus.Success)
            {
                Upla.UpdateAccount(settings, result);
            }
            else if (result.Status == UplaAccountStatus.InvalidKey)
            {
                // The key stays, so uploads fail with a clear message instead of silently becoming guest uploads.
                Upla.MarkSignInExpired();
            }
        }

        private void CreateAccountRows()
        {
            lblAccount = CreateLabel("", true);
            AddRow(CreateLabel(UplaStrings.AccountMenu + ":"), lblAccount);

            btnSignIn = CreateButton(UplaStrings.SignInButton);
            btnSignIn.Click += (sender, e) => UplaAccountMenu.ShowSignIn(config, null, FindForm());
            btnSignOut = CreateButton(UplaStrings.SignOutButton);
            btnSignOut.Click += (sender, e) => UplaAccountMenu.SignOut(config, null, FindForm());
            llProfile = CreateLink(UplaStrings.MyProfile, null);
            llProfile.LinkClicked += (sender, e) => URLHelpers.OpenURL(Upla.GetProfileURL(settings.AccountURL));
            llDevices = CreateLink(UplaStrings.ConnectedDevices, Upla.ConnectedDevicesURL);
            llSignUp = CreateLink(UplaStrings.SignUp, Upla.SignUpURL);
            llGuest = CreateLink(UplaStrings.ContinueAsGuest, null);
            llGuest.LinkClicked += (sender, e) => UplaAccountMenu.ContinueAsGuest(config, null);

            FlowLayoutPanel flpAccount = CreateFlowPanel();
            flpAccount.Controls.Add(btnSignIn);
            flpAccount.Controls.Add(btnSignOut);
            flpAccount.Controls.Add(llProfile);
            flpAccount.Controls.Add(llDevices);
            flpAccount.Controls.Add(llSignUp);
            flpAccount.Controls.Add(llGuest);
            AddRow(null, flpAccount);

            llManualKey = CreateLink(UplaStrings.ManualAPIKey, null);
            llManualKey.LinkClicked += (sender, e) =>
            {
                showManualKey = !showManualKey;
                UpdateAccountUI();
            };
            AddRow(null, llManualKey);

            txtAPIKey = new TextBox()
            {
                Text = Upla.IsSignedIn(settings) ? "" : settings.PersonalAPIKey,
                UseSystemPasswordChar = true,
                Dock = DockStyle.Fill
            };
            txtAPIKey.TextChanged += (sender, e) =>
            {
                if (isUpdatingAccountUI)
                {
                    return;
                }

                // A key entered by hand replaces the app sign-in.
                settings.PersonalAPIKey = Upla.NormalizeAPIKey(txtAPIKey.Text);
                settings.AccountUsername = settings.AccountName = settings.AccountURL = "";
                UpdateAccountUI();
                UpdateStatus();
            };
            Label lblAPIKey = CreateLabel(UplaStrings.PersonalAPIKey);
            AddRow(lblAPIKey, txtAPIKey);

            CheckBox cbShowAPIKey = new CheckBox()
            {
                Text = UplaStrings.ShowAPIKey,
                AutoSize = true,
                Margin = new Padding(3, 6, 6, 3)
            };
            cbShowAPIKey.CheckedChanged += (sender, e) => txtAPIKey.UseSystemPasswordChar = !cbShowAPIKey.Checked;

            btnVerify = new Button()
            {
                Text = UplaStrings.VerifyAPIKey,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(6, 0, 6, 0)
            };
            btnVerify.Click += BtnVerify_Click;

            FlowLayoutPanel flpKeyActions = CreateFlowPanel();
            flpKeyActions.Controls.Add(cbShowAPIKey);
            flpKeyActions.Controls.Add(btnVerify);
            flpKeyActions.Controls.Add(CreateLink(UplaStrings.GetAPIKey, Upla.ConnectedDevicesURL));
            AddRow(null, flpKeyActions);

            lblStatus = CreateLabel("", true);
            AddRow(null, lblStatus);

            Label lblHint = CreateLabel(UplaStrings.PersonalAPIKeyHint, true);
            AddRow(null, lblHint);

            manualKeyControls = new Control[] { lblAPIKey, txtAPIKey, flpKeyActions, lblStatus, lblHint };
        }

        private void UpdateAccountUI()
        {
            bool signedIn = Upla.IsSignedIn(settings);
            bool lost = Upla.IsSignInLost(settings);
            bool expired = signedIn && Upla.SignInExpired;
            bool manualKey = !signedIn && !lost && Upla.NormalizeAPIKey(settings.PersonalAPIKey).Length > 0;

            if (lost)
            {
                lblAccount.Text = string.Format(UplaStrings.AccountStatusLost, settings.AccountUsername);
            }
            else if (signedIn)
            {
                string account = string.IsNullOrEmpty(settings.AccountName) || settings.AccountName == settings.AccountUsername ?
                    settings.AccountUsername : $"{settings.AccountName} ({settings.AccountUsername})";
                lblAccount.Text = expired ? UplaStrings.SignInDeviceSignedOut : string.Format(UplaStrings.AccountStatusSignedIn, account);
            }
            else
            {
                lblAccount.Text = manualKey ? UplaStrings.AccountStatusManualKey : UplaStrings.AccountStatusGuest;
            }

            btnSignIn.Text = lost || expired ? UplaStrings.SignInAgain : UplaStrings.SignInButton;
            btnSignIn.Visible = !signedIn || expired;
            btnSignOut.Visible = signedIn || manualKey;
            llProfile.Visible = signedIn && !string.IsNullOrEmpty(Upla.GetProfileURL(settings.AccountURL));
            llDevices.Visible = signedIn || lost;
            llSignUp.Visible = !signedIn && !manualKey && !lost;
            llGuest.Visible = lost;
            llManualKey.Visible = !signedIn && !lost;

            foreach (Control control in manualKeyControls)
            {
                control.Visible = !signedIn && !lost && (showManualKey || manualKey);
            }
        }

        private void CreateOptionRows()
        {
            ComboBox cbLinkType = new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 260,
                Anchor = AnchorStyles.Left
            };
            cbLinkType.Items.AddRange(new object[] { UplaStrings.LinkTypeViewerPage, UplaStrings.LinkTypeDirectLink, UplaStrings.LinkTypeShortLink });
            cbLinkType.SelectedIndex = ((int)settings.LinkType).Clamp(0, cbLinkType.Items.Count - 1);
            cbLinkType.SelectedIndexChanged += (sender, e) => settings.LinkType = (UplaLinkType)cbLinkType.SelectedIndex;
            AddRow(CreateLabel(UplaStrings.LinkType), cbLinkType);

            TextBox txtAlbum = new TextBox()
            {
                Text = settings.Album,
                Dock = DockStyle.Fill
            };
            txtAlbum.TextChanged += (sender, e) => settings.Album = txtAlbum.Text.Trim();
            AddRow(CreateLabel(UplaStrings.Album), txtAlbum);
            AddRow(null, CreateLabel(UplaStrings.AlbumHint, true));

            TextBox txtTags = new TextBox()
            {
                Text = settings.Tags,
                Dock = DockStyle.Fill
            };
            txtTags.TextChanged += (sender, e) => settings.Tags = txtTags.Text;
            AddRow(CreateLabel(UplaStrings.Tags), txtTags);

            NumericUpDown nudCategoryID = new NumericUpDown()
            {
                Minimum = 0,
                Maximum = 1000000,
                Width = 100,
                Anchor = AnchorStyles.Left
            };
            nudCategoryID.Value = Math.Max(0, Math.Min(settings.CategoryID, (int)nudCategoryID.Maximum));
            nudCategoryID.ValueChanged += (sender, e) => settings.CategoryID = (int)nudCategoryID.Value;
            AddRow(CreateLabel(UplaStrings.CategoryID), nudCategoryID);

            ComboBox cbExpiration = new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 160,
                Anchor = AnchorStyles.Left
            };
            cbExpiration.Items.Add(UplaStrings.AutoDeleteNever);
            cbExpiration.Items.AddRange(Upla.ExpirationPresets.Select(UplaStrings.FormatDuration).ToArray<object>());
            cbExpiration.SelectedIndex = Array.IndexOf(Upla.ExpirationPresets, settings.Expiration) + 1;
            cbExpiration.SelectedIndexChanged += (sender, e) =>
                settings.Expiration = cbExpiration.SelectedIndex > 0 ? Upla.ExpirationPresets[cbExpiration.SelectedIndex - 1] : "";
            AddRow(CreateLabel(UplaStrings.AutoDelete), cbExpiration);

            NumericUpDown nudMaxWidth = new NumericUpDown()
            {
                Minimum = 0,
                Maximum = 20000,
                Increment = 100,
                Width = 100,
                Anchor = AnchorStyles.Left
            };
            nudMaxWidth.Value = Math.Max(0, Math.Min(settings.MaxWidth, (int)nudMaxWidth.Maximum));
            nudMaxWidth.ValueChanged += (sender, e) => settings.MaxWidth = (int)nudMaxWidth.Value;
            AddRow(CreateLabel(UplaStrings.MaxWidth), nudMaxWidth);

            AddRow(null, CreateLabel(UplaStrings.MemberOnlyNote, true));
            AddRow(null, CreateLabel(UplaStrings.VideoNote, true));

            CheckBox cbStopRecording = new CheckBox()
            {
                Text = UplaStrings.StopRecordingAtUploadLimit,
                AutoSize = true,
                Checked = settings.StopRecordingAtUploadLimit
            };
            cbStopRecording.CheckedChanged += (sender, e) => settings.StopRecordingAtUploadLimit = cbStopRecording.Checked;
            AddRow(null, cbStopRecording);

            AddRow(null, CreateLink(UplaStrings.APIDocumentation, Upla.APIDocumentationURL));
        }

        private async void BtnVerify_Click(object sender, EventArgs e)
        {
            if (isVerifying)
            {
                return;
            }

            isVerifying = true;
            btnVerify.Enabled = false;
            lblStatus.Text = UplaStrings.Verifying;

            string personalKey = Upla.NormalizeAPIKey(txtAPIKey.Text);
            bool isMember = personalKey.Length > 0;
            string key = isMember ? personalKey : APIKeys.UplaAPIKey;
            string statusText;

            try
            {
                UplaKeyCheckResult result = await Task.Run(() => new UplaUploader(key, isMember, settings).CheckAPIKey());
                statusText = GetKeyCheckText(result, isMember);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex);
                statusText = string.Format(UplaStrings.KeyCheckFailed, ex.Message);
            }

            isVerifying = false;

            if (!IsDisposed)
            {
                btnVerify.Enabled = true;
                lblStatus.Text = statusText;
            }
        }

        private static string GetKeyCheckText(UplaKeyCheckResult result, bool isMember)
        {
            switch (result.Status)
            {
                case UplaKeyStatus.Valid:
                    return isMember ? UplaStrings.KeyValid : UplaStrings.GuestUploadAvailable;
                case UplaKeyStatus.Invalid:
                    return isMember ? UplaStrings.KeyInvalid : UplaStrings.GuestUploadUnavailable;
                case UplaKeyStatus.OldFormat:
                    return UplaStrings.KeyOldFormat;
                case UplaKeyStatus.NoUploadPermission:
                    return UplaStrings.KeyNoUploadPermission;
                default:
                    return string.Format(UplaStrings.KeyCheckFailed, result.Message);
            }
        }

        private void UpdateStatus()
        {
            if (!isVerifying)
            {
                lblStatus.Text = Upla.NormalizeAPIKey(txtAPIKey.Text).Length == 0 ? UplaStrings.StatusGuest : UplaStrings.StatusMember;
            }
        }

        private static Button CreateButton(string text)
        {
            return new Button()
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(6, 0, 6, 0)
            };
        }

        private void AddRow(Control left, Control right)
        {
            int row = tlpMain.RowCount++;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            if (left != null)
            {
                tlpMain.Controls.Add(left, 0, row);
            }

            if (right != null)
            {
                tlpMain.Controls.Add(right, 1, row);
            }
        }

        private static Label CreateLabel(string text, bool wrap = false)
        {
            Label label = new Label()
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 3, 3)
            };

            if (wrap)
            {
                label.MaximumSize = new Size(520, 0);
            }

            return label;
        }

        private static LinkLabel CreateLink(string text, string url)
        {
            LinkLabel link = new LinkLabel()
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(6, 8, 3, 3)
            };
            if (url != null)
            {
                link.LinkClicked += (sender, e) => URLHelpers.OpenURL(url);
            }

            return link;
        }

        private static FlowLayoutPanel CreateFlowPanel()
        {
            return new FlowLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Margin = new Padding(0)
            };
        }
    }
}
