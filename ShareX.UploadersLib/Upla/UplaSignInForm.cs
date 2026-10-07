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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ShareX.UploadersLib
{
    // upla.com.tr sign-in with username/email and password (and the two-step verification code when the account
    // has one). Built in code like UplaSettingsControl.
    [DesignerCategory("Code")]
    public class UplaSignInForm : Form
    {
        private readonly UplaSettings settings;
        private readonly string websiteURL;
        private readonly TableLayoutPanel tlpMain;
        private TextBox txtLoginSubject, txtPassword, txtTwoFactorCode;
        private Label lblTwoFactorCode, lblStatus;
        private Button btnSignIn, btnCancel;
        private CancellationTokenSource signInCancellation;

        public UplaAccountResult Result { get; private set; }

        public UplaSignInForm(UplaSettings settings, string websiteURL = Upla.WebsiteURL)
        {
            this.settings = settings;
            this.websiteURL = websiteURL;

            Text = UplaStrings.SignInTitle;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);
            // Screenshots taken while this window is open (by this app or any other) do not show the password.
            FormScreenCaptureMode = ScreenCaptureMode.HideContent;

            tlpMain = new TableLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Dock = DockStyle.Fill
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            SuspendLayout();
            CreateRows();
            Controls.Add(tlpMain);
            ResumeLayout();

            AcceptButton = btnSignIn;
            CancelButton = btnCancel;

            ShareXResources.ApplyTheme(this, true);
        }

        private void CreateRows()
        {
            txtLoginSubject = new TextBox() { Width = 260, Text = settings.AccountUsername ?? "" };
            AddRow(CreateLabel(UplaStrings.UsernameOrEmail), txtLoginSubject);

            txtPassword = new TextBox() { Width = 260, UseSystemPasswordChar = true };
            AddRow(CreateLabel(UplaStrings.Password), txtPassword);

            lblTwoFactorCode = CreateLabel(UplaStrings.TwoFactorCode);
            txtTwoFactorCode = new TextBox() { Width = 120, MaxLength = 10 };
            lblTwoFactorCode.Visible = txtTwoFactorCode.Visible = false;
            AddRow(lblTwoFactorCode, txtTwoFactorCode);

            lblStatus = new Label()
            {
                AutoSize = true,
                MaximumSize = new Size(380, 0),
                Margin = new Padding(3, 6, 3, 3),
                Text = UplaStrings.SignInPrivacy
            };
            tlpMain.Controls.Add(lblStatus, 0, tlpMain.RowCount);
            tlpMain.SetColumnSpan(lblStatus, 2);
            tlpMain.RowCount++;

            FlowLayoutPanel flpLinks = new FlowLayoutPanel() { AutoSize = true, Margin = new Padding(0) };
            flpLinks.Controls.Add(CreateLink(UplaStrings.ForgotPassword, websiteURL + "/account/password-forgot"));
            flpLinks.Controls.Add(CreateLink(UplaStrings.SignUp, websiteURL + "/signup"));
            flpLinks.Controls.Add(CreateLink(UplaStrings.ConnectedDevices, websiteURL + "/upla-app/devices"));
            tlpMain.Controls.Add(flpLinks, 0, tlpMain.RowCount);
            tlpMain.SetColumnSpan(flpLinks, 2);
            tlpMain.RowCount++;

            btnSignIn = new Button() { Text = UplaStrings.SignInSubmit, AutoSize = true, Padding = new Padding(8, 0, 8, 0) };
            btnSignIn.Click += BtnSignIn_Click;
            btnCancel = new Button() { Text = UplaStrings.Cancel, AutoSize = true, Padding = new Padding(8, 0, 8, 0), DialogResult = DialogResult.Cancel };

            FlowLayoutPanel flpButtons = new FlowLayoutPanel()
            {
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 0)
            };
            flpButtons.Controls.Add(btnCancel);
            flpButtons.Controls.Add(btnSignIn);
            tlpMain.Controls.Add(flpButtons, 0, tlpMain.RowCount);
            tlpMain.SetColumnSpan(flpButtons, 2);
            tlpMain.RowCount++;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (txtLoginSubject.TextLength > 0)
            {
                txtPassword.Focus();
            }
            else
            {
                txtLoginSubject.Focus();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Closing (Cancel, Esc, X, Windows shutdown) abandons a running sign-in.
            signInCancellation?.Cancel();

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                signInCancellation?.Dispose();
            }

            base.Dispose(disposing);
        }

        private async void BtnSignIn_Click(object sender, EventArgs e)
        {
            if (signInCancellation != null)
            {
                return;
            }

            string loginSubject = txtLoginSubject.Text.Trim();
            string password = txtPassword.Text;
            string twoFactorCode = txtTwoFactorCode.Visible ? txtTwoFactorCode.Text : "";

            if (loginSubject.Length == 0 || password.Length == 0)
            {
                lblStatus.Text = UplaStrings.SignInFieldsRequired;
                return;
            }

            signInCancellation = new CancellationTokenSource();
            SetBusy(true);
            lblStatus.Text = UplaStrings.SigningIn;

            string deviceName = UplaAccountClient.GetDeviceName(Upla.GetInstallID(settings));
            UplaAccountResult result;

            try
            {
                result = await new UplaAccountClient(websiteURL).SignInAsync(loginSubject, password, twoFactorCode, deviceName, signInCancellation.Token);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex);
                result = new UplaAccountResult { Status = UplaAccountStatus.Unknown, Message = string.Format(UplaStrings.SignInFailed, ex.Message) };
            }

            bool cancelled = signInCancellation.IsCancellationRequested;
            signInCancellation.Dispose();
            signInCancellation = null;

            if (IsDisposed || cancelled || result.Status == UplaAccountStatus.Cancelled)
            {
                return;
            }

            SetBusy(false);

            lblStatus.Text = result.Message;

            switch (result.Status)
            {
                case UplaAccountStatus.Success:
                    txtPassword.Clear();
                    Upla.ApplySignIn(settings, result);
                    Result = result;
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
                case UplaAccountStatus.TwoFactorRequired:
                    lblTwoFactorCode.Visible = txtTwoFactorCode.Visible = true;
                    txtTwoFactorCode.Focus();
                    break;
                case UplaAccountStatus.InvalidTwoFactorCode:
                    txtTwoFactorCode.SelectAll();
                    txtTwoFactorCode.Focus();
                    break;
                case UplaAccountStatus.InvalidCredentials:
                    txtPassword.Clear();
                    txtTwoFactorCode.Clear();
                    lblTwoFactorCode.Visible = txtTwoFactorCode.Visible = false;
                    txtPassword.Focus();
                    break;
            }
        }

        // Cancel stays enabled so a slow or stuck request can always be abandoned.
        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            txtLoginSubject.Enabled = txtPassword.Enabled = txtTwoFactorCode.Enabled = btnSignIn.Enabled = !busy;
        }

        private void AddRow(Control left, Control right)
        {
            int row = tlpMain.RowCount++;
            tlpMain.Controls.Add(left, 0, row);
            tlpMain.Controls.Add(right, 1, row);
        }

        private static Label CreateLabel(string text)
        {
            return new Label()
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 3, 3)
            };
        }

        private static LinkLabel CreateLink(string text, string url)
        {
            LinkLabel link = new LinkLabel()
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(3, 6, 9, 3)
            };
            link.LinkClicked += (sender, e) => URLHelpers.OpenURL(url);
            return link;
        }
    }
}
