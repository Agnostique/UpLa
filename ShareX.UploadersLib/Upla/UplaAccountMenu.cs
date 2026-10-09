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
using System.Windows.Forms;

namespace ShareX.UploadersLib
{
    // upla.com.tr account actions shared by the main window, the tray menu and the upla.com.tr settings tab.
    public static class UplaAccountMenu
    {
        // Rebuilds an "upla.com.tr account" drop down: sign in / sign up for guests, profile / devices / sign out when signed in,
        // and "sign in again" when this computer's sign-in is no longer usable.
        public static void Update(ToolStripDropDownItem menu, UploadersConfig config, Action saveConfig, bool showUsernameOnly = false)
        {
            menu.Image = Upla.Image;
            menu.DropDownItems.Clear();

            if (config == null)
            {
                // Settings are still loading; a placeholder item keeps the drop down openable, opening it reloads the menu.
                menu.Text = showUsernameOnly ? UplaStrings.SignInSubmit : UplaStrings.AccountMenu;
                menu.DropDownItems.Add(new ToolStripMenuItem(UplaStrings.SignInButton) { Enabled = false });
                return;
            }

            UplaSettings settings = Upla.GetSettings(config);

            if (Upla.IsSignInLost(settings))
            {
                menu.Text = string.Format(UplaStrings.AccountMenuNeedsSignIn, showUsernameOnly ? settings.AccountUsername : $"{UplaStrings.AccountMenu}: {settings.AccountUsername}");
                menu.DropDownItems.Add(UplaStrings.SignInAgain, null, (sender, e) => ShowSignIn(config, saveConfig, null));
                menu.DropDownItems.Add(UplaStrings.ContinueAsGuest, null, (sender, e) => ContinueAsGuest(config, saveConfig));
                menu.DropDownItems.Add(UplaStrings.ConnectedDevices, null, (sender, e) => URLHelpers.OpenURL(Upla.ConnectedDevicesURL));
            }
            else if (Upla.IsSignedIn(settings))
            {
                string account = showUsernameOnly ? settings.AccountUsername : $"{UplaStrings.AccountMenu}: {settings.AccountUsername}";
                menu.Text = Upla.SignInExpired ? string.Format(UplaStrings.AccountMenuNeedsSignIn, account) : account;

                if (Upla.SignInExpired)
                {
                    menu.DropDownItems.Add(UplaStrings.SignInAgain, null, (sender, e) => ShowSignIn(config, saveConfig, null));
                    menu.DropDownItems.Add(new ToolStripSeparator());
                }

                string profileURL = Upla.GetProfileURL(settings.AccountURL);

                if (!string.IsNullOrEmpty(profileURL))
                {
                    menu.DropDownItems.Add(UplaStrings.MyProfile, null, (sender, e) => URLHelpers.OpenURL(profileURL));
                }

                menu.DropDownItems.Add(UplaStrings.ConnectedDevices, null, (sender, e) => URLHelpers.OpenURL(Upla.ConnectedDevicesURL));
                menu.DropDownItems.Add(new ToolStripSeparator());
                menu.DropDownItems.Add(UplaStrings.SignOutButton, null, (sender, e) => SignOut(config, saveConfig, null));
            }
            else
            {
                menu.Text = showUsernameOnly ? UplaStrings.SignInSubmit : UplaStrings.AccountMenu;
                menu.DropDownItems.Add(UplaStrings.SignInButton, null, (sender, e) => ShowSignIn(config, saveConfig, null));
                menu.DropDownItems.Add(UplaStrings.SignUp, null, (sender, e) => URLHelpers.OpenURL(Upla.SignUpURL));
            }

            // Microsoft Store policy for apps that share user content: a way to report content that breaks the rules.
            menu.DropDownItems.Add(new ToolStripSeparator());
            menu.DropDownItems.Add(UplaStrings.ReportAbuse, null, (sender, e) => URLHelpers.OpenURL(Upla.ReportAbuseURL));
        }

        public static bool ShowSignIn(UploadersConfig config, Action saveConfig, IWin32Window owner)
        {
            using (UplaSignInForm form = new UplaSignInForm(Upla.GetSettings(config)))
            {
                if (form.ShowDialog(owner) == DialogResult.OK)
                {
                    saveConfig?.Invoke();
                    return true;
                }
            }

            return false;
        }

        // Forgets an unreadable sign-in (Upla.IsSignInLost) so uploads continue as a guest.
        public static void ContinueAsGuest(UploadersConfig config, Action saveConfig)
        {
            Upla.ClearAccount(Upla.GetSettings(config));
            saveConfig?.Invoke();
        }

        // An app sign-in is removed from the server as well; a key entered by hand belongs to the website and is only forgotten here.
        public static async void SignOut(UploadersConfig config, Action saveConfig, IWin32Window owner)
        {
            UplaSettings settings = Upla.GetSettings(config);
            bool signedIn = Upla.IsSignedIn(settings);

            if (MessageBox.Show(owner, signedIn ? UplaStrings.SignOutConfirm : UplaStrings.ManualKeyRemoveConfirm, UplaStrings.AccountMenu,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            string key = settings.PersonalAPIKey;
            Upla.ClearAccount(settings);
            saveConfig?.Invoke();

            if (!signedIn || string.IsNullOrEmpty(key))
            {
                return;
            }

            UplaAccountResult result = null;

            try
            {
                result = await new UplaAccountClient().SignOutAsync(key);
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            // InvalidKey: the connection was already removed (e.g. on "Connected devices"), which is what sign out wants too.
            if (result == null || (result.Status != UplaAccountStatus.Success && result.Status != UplaAccountStatus.InvalidKey))
            {
                IWin32Window dialogOwner = owner is Control control && control.IsDisposed ? null : owner;
                string url = result?.Status == UplaAccountStatus.NotSupported ? Upla.APIKeySettingsURL : Upla.ConnectedDevicesURL;

                if (MessageBox.Show(dialogOwner, string.Format(UplaStrings.SignOutServerFailed, result?.Message ?? UplaStrings.ErrorUnexpectedResponse),
                    UplaStrings.AccountMenu, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    URLHelpers.OpenURL(url);
                }
            }
        }
    }
}
