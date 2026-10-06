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

using System;
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
        public const string APIDocumentationURL = WebsiteURL + "/api-v1";

        // Guest uploads are limited to 20 MB on upla.com.tr. Member limits are decided by the server, but a
        // single request can never exceed Cloudflare's 100 MB request body limit.
        public const long GuestMaxFileSize = 20L * 1024 * 1024;
        public const long MaxRequestSize = 100L * 1000 * 1000;

        public static readonly string[] ImageExtensions = { "jpg", "jpeg", "png", "bmp", "gif", "webp" };
        // Accepted by Chevereto 4.1+ when video uploads are enabled on the server.
        public static readonly string[] VideoExtensions = { "mp4", "webm", "mov" };

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
            }

            return config.UplaSettings;
        }

        public static bool HasPersonalAPIKey(UploadersConfig config)
        {
            return !string.IsNullOrWhiteSpace(GetSettings(config).PersonalAPIKey);
        }

        public static string GetAPIKey(UploadersConfig config)
        {
            return HasPersonalAPIKey(config) ? GetSettings(config).PersonalAPIKey.Trim() : APIKeys.UplaAPIKey;
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
