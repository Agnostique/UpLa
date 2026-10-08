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
using System.Drawing;
using System.Linq;

namespace ShareX.UploadersLib
{
    // upla.com.tr runs Chevereto 4.5.7, whose only API for users is API V1.1 (POST /api/1/upload).
    public static class Upla
    {
        public const string WebsiteURL = "https://upla.com.tr";
        public const string UploadURL = WebsiteURL + "/api/1/upload";
        public const string APIKeySettingsURL = WebsiteURL + "/settings/api";
        public const string SignUpURL = WebsiteURL + "/signup";
        public const string PasswordForgotURL = WebsiteURL + "/account/password-forgot";
        public const string ConnectedDevicesURL = WebsiteURL + "/upla-app/devices";
        public const string APIDocumentationURL = WebsiteURL + "/api-v1";
        public const string SourceCodeURL = "https://github.com/Agnostique/UpLa";

        // Guest uploads are limited to 20 MB on upla.com.tr. Member limits are decided by the server, but a
        // single request can never exceed Cloudflare's 100 MB request body limit.
        public const long GuestMaxFileSize = 20L * 1024 * 1024;
        public const long MaxRequestSize = 100L * 1000 * 1000;

        // Largest file one upload can send: the guest limit, or for members the request limit (upla.com.tr allows them 100 MB).
        public static long GetMaxUploadSize(bool isMember)
        {
            return isMember ? MaxRequestSize : GuestMaxFileSize;
        }

        public static string GetMaxUploadSizeText(bool isMember)
        {
            return isMember ? $"{MaxRequestSize / 1000000} MB" : $"{GuestMaxFileSize / 1048576} MB";
        }

        // Size at which a screen recording that will be uploaded stops. After reaching its size limit FFmpeg still writes
        // what it has buffered and closes the file, so room is left below the upload limit (measured with FFmpeg 8.1:
        // up to about 0.6 MB with WEBM written in one second blocks, much less with MP4 and GIF).
        public static long GetRecordingSizeLimit(bool isMember)
        {
            long limit = GetMaxUploadSize(isMember);
            return limit - Math.Max(2L * 1024 * 1024, limit * 5 / 100);
        }

        private static Image image;

        // The upla.com.tr destination and account menu show UpLa's own icon.
        public static Image Image => image ??= ShareXResources.GetAppImage(16);

        public static readonly string[] ImageExtensions = { "jpg", "jpeg", "png", "bmp", "gif", "webp" };
        // The video formats enabled on upla.com.tr (Chevereto 4.1+). mov is left out: browsers often cannot play it.
        public static readonly string[] VideoExtensions = { "mp4", "webm" };

        // Chevereto expiration values (ISO 8601 durations), same as upla.com.tr's own upload form.
        public static readonly string[] ExpirationPresets =
        {
            "PT5M", "PT15M", "PT30M", "PT1H", "PT3H", "PT6H", "PT12H", "P1D", "P2D", "P3D", "P4D", "P5D", "P6D",
            "P1W", "P2W", "P3W", "P1M", "P2M", "P3M", "P4M", "P5M", "P6M", "P1Y"
        };

        public static UplaSettings GetSettings(UploadersConfig config)
        {
            if (config.UplaSettings == null)
            {
                config.UplaSettings = new UplaSettings()
                {
                    // Earlier upla builds only had the Chevereto "direct link" checkbox.
                    LinkType = config.CheveretoDirectURL ? UplaLinkType.DirectLink : UplaLinkType.ViewerPage
                };

                // UpLa 1.0 kept the key in the Chevereto settings, where its default was the shared guest key. Only a
                // personal key (Chevereto user keys start with "chv_") is carried over, as a key entered by hand.
                string oldAPIKey = NormalizeAPIKey(config.CheveretoUploader?.APIKey);

                if (oldAPIKey.StartsWith("chv_", StringComparison.Ordinal) && oldAPIKey != NormalizeAPIKey(APIKeys.UplaAPIKey))
                {
                    config.UplaSettings.PersonalAPIKey = oldAPIKey;
                }
            }

            return config.UplaSettings;
        }

        public static bool HasPersonalAPIKey(UploadersConfig config)
        {
            return NormalizeAPIKey(GetSettings(config).PersonalAPIKey).Length > 0;
        }

        // Raised when the account changes (sign in, sign out, expired sign-in). May be raised on an upload thread, so
        // UI handlers must marshal to their own thread.
        public static event Action AccountChanged;

        // The server no longer accepts this computer's key (removed on "Connected devices", by the 10 key limit, or by an
        // admin). Not saved: the next start checks again when an upload fails or the settings tab is opened.
        public static bool SignInExpired { get; private set; }

        // Signed in from the app (as opposed to a key entered by hand).
        public static bool IsSignedIn(UplaSettings settings)
        {
            return NormalizeAPIKey(settings.PersonalAPIKey).Length > 0 && !string.IsNullOrEmpty(settings.AccountUsername);
        }

        // The account is remembered but its key could not be read (DPAPI only decrypts it for the same Windows user on
        // the same computer, e.g. settings copied from another PC). Uploads must not silently become guest uploads.
        public static bool IsSignInLost(UplaSettings settings)
        {
            return NormalizeAPIKey(settings.PersonalAPIKey).Length == 0 && !string.IsNullOrEmpty(settings.AccountUsername);
        }

        public static bool NeedsSignIn(UplaSettings settings)
        {
            return IsSignInLost(settings) || (IsSignedIn(settings) && SignInExpired);
        }

        public static void MarkSignInExpired()
        {
            if (!SignInExpired)
            {
                SignInExpired = true;
                RaiseAccountChanged();
            }
        }

        public static string GetInstallID(UplaSettings settings)
        {
            if (string.IsNullOrEmpty(settings.InstallID))
            {
                settings.InstallID = Guid.NewGuid().ToString("N");
            }

            return settings.InstallID;
        }

        public static void ApplySignIn(UplaSettings settings, UplaAccountResult result)
        {
            settings.PersonalAPIKey = NormalizeAPIKey(result.APIKey);
            UpdateAccount(settings, result);
        }

        public static void UpdateAccount(UplaSettings settings, UplaAccountResult result)
        {
            settings.AccountUsername = result.Username ?? "";
            settings.AccountName = result.Name ?? "";
            settings.AccountURL = result.ProfileURL ?? "";
            SignInExpired = false;
            RaiseAccountChanged();
        }

        public static void ClearAccount(UplaSettings settings)
        {
            settings.PersonalAPIKey = "";
            settings.AccountUsername = "";
            settings.AccountName = "";
            settings.AccountURL = "";
            SignInExpired = false;
            RaiseAccountChanged();
        }

        // The account change itself is done; a window that is closing and cannot be updated any more must not fail the
        // upload or the sign-in that caused it, nor stop the other windows from updating.
        private static void RaiseAccountChanged()
        {
            Action handlers = AccountChanged;

            if (handlers == null)
            {
                return;
            }

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }
        }

        public static string GetAPIKey(UploadersConfig config)
        {
            return HasPersonalAPIKey(config) ? NormalizeAPIKey(GetSettings(config).PersonalAPIKey) : APIKeys.UplaAPIKey;
        }

        // Keys never contain whitespace, but a key copied from a web page can carry spaces or line breaks,
        // which would make the X-API-Key header invalid.
        public static string NormalizeAPIKey(string key)
        {
            return string.IsNullOrEmpty(key) ? "" : new string(key.Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c)).ToArray());
        }

        public static UplaUploader CreateUploader(UploadersConfig config)
        {
            return new UplaUploader(GetAPIKey(config), HasPersonalAPIKey(config), GetSettings(config));
        }

        public static bool IsVideoExtension(string extension)
        {
            return VideoExtensions.Contains(NormalizeExtension(extension));
        }

        public static bool IsSupportedExtension(string extension)
        {
            string ext = NormalizeExtension(extension);
            return ImageExtensions.Contains(ext) || VideoExtensions.Contains(ext);
        }

        private static string NormalizeExtension(string extension)
        {
            return (extension ?? "").Trim().TrimStart('.').ToLowerInvariant();
        }

        // Accepts an album link (https://upla.com.tr/album/Name.AbCd, .../album/AbCd) or the encoded album ID.
        public static string ParseAlbumID(string album)
        {
            if (string.IsNullOrWhiteSpace(album))
            {
                return "";
            }

            string value = album.Trim();

            int queryIndex = value.IndexOfAny(new char[] { '?', '#' });

            if (queryIndex >= 0)
            {
                value = value.Substring(0, queryIndex);
            }

            value = value.TrimEnd('/');

            int slashIndex = value.LastIndexOf('/');

            if (slashIndex >= 0)
            {
                value = value.Substring(slashIndex + 1);
            }

            int dotIndex = value.LastIndexOf('.');

            if (dotIndex >= 0)
            {
                value = value.Substring(dotIndex + 1);
            }

            return value;
        }

        // Chevereto drops tags longer than 32 characters or containing ',', '/' or '#'.
        public static string NormalizeTags(string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return "";
            }

            return string.Join(",", tags.Split(',')
                .Select(x => x.Replace("/", "").Replace("#", "").Trim())
                .Where(x => x.Length > 0)
                .Select(x => x.Length > 32 ? x.Substring(0, 32).Trim() : x)
                .Distinct(StringComparer.OrdinalIgnoreCase));
        }
    }
}
