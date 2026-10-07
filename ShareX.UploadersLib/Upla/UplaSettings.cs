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

namespace ShareX.UploadersLib
{
    public enum UplaLinkType
    {
        ViewerPage,
        DirectLink,
        ShortLink
    }

    public class UplaSettings
    {
        // Upload key of the member: the per-computer key from the in-app sign-in, or a key entered by hand from
        // upla.com.tr/settings/api. Empty means guest upload with the shared key.
        [JsonEncrypt]
        public string PersonalAPIKey { get; set; } = "";

        // Account of the in-app sign-in; empty for guests and for a key entered by hand.
        public string AccountUsername { get; set; } = "";
        public string AccountName { get; set; } = "";
        public string AccountURL { get; set; } = "";

        // Random per installation; names this computer's key on the website's "Connected devices" page.
        public string InstallID { get; set; } = "";

        public UplaLinkType LinkType { get; set; } = UplaLinkType.ViewerPage;

        // Album link or encoded album ID; only used with a personal key.
        public string Album { get; set; } = "";

        // Comma separated; only used with a personal key.
        public string Tags { get; set; } = "";

        public int CategoryID { get; set; } = 0;

        // One of Upla.ExpirationPresets, empty for no automatic deletion.
        public string Expiration { get; set; } = "";

        public bool NSFW { get; set; } = false;

        // Server side resize of images wider than this, 0 to disable.
        public int MaxWidth { get; set; } = 0;
    }
}
