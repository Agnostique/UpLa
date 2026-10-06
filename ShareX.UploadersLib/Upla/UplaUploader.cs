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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareX.HelpersLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ShareX.UploadersLib
{
    public enum UplaKeyStatus
    {
        Valid,
        Invalid,
        OldFormat,
        NoUploadPermission,
        Unknown
    }

    public class UplaKeyCheckResult
    {
        public UplaKeyStatus Status { get; set; }
        public string Message { get; set; }
    }

    public class UplaResponse
    {
        public bool Success { get; set; }
        public string URL { get; set; }
        public string ThumbnailURL { get; set; }
        public string DeletionURL { get; set; }
        public bool AwaitingModeration { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    // Uploads images and videos to upla.com.tr through Chevereto API V1.1.
    public sealed class UplaUploader : ImageUploader
    {
        public string APIKey { get; private set; }
        public bool IsMember { get; private set; }
        public UplaSettings Settings { get; private set; }

        public UplaUploader(string apiKey, bool isMember, UplaSettings settings)
        {
            APIKey = apiKey ?? "";
            IsMember = isMember;
            Settings = settings ?? new UplaSettings();
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            string fileError = CheckFile(stream, fileName);

            if (fileError != null)
            {
                Errors.Add(fileError);
                return new UploadResult();
            }

            Dictionary<string, string> args = CreateArguments(stream, fileName);

            // Chevereto reads the header first; the "key" field is kept for proxies that drop custom headers.
            NameValueCollection headers = new NameValueCollection
            {
                { "X-API-Key", APIKey }
            };

            ReturnResponseOnError = true;

            UploadResult result = SendRequestFile(Upla.UploadURL, stream, fileName, "source", args, headers);

            if (result != null && !StopUploadRequested)
            {
                UplaResponse response = ParseResponse(result.Response, result.IsSuccess, LastResponseInfo?.StatusCode, Settings.LinkType, IsMember);

                if (response.Success)
                {
                    result.URL = response.URL;
                    result.ThumbnailURL = response.ThumbnailURL;
                    result.DeletionURL = response.DeletionURL;
                }
                else if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    Errors.AddFirst(response.ErrorMessage);
                }
            }

            return result;
        }

        // Sends a request without a file: Chevereto checks the key before the source, so error 130 (empty source)
        // means the key is accepted, 100 means it is invalid and 403 means the account cannot upload.
        public UplaKeyCheckResult CheckAPIKey()
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "X-API-Key", APIKey }
            };

            byte[] body = Encoding.UTF8.GetBytes("format=json&key=" + Uri.EscapeDataString(APIKey));

            using (MemoryStream stream = new MemoryStream(body))
            using (HttpWebResponse response = GetResponse(HttpMethod.POST, Upla.UploadURL, stream, RequestHelpers.ContentTypeURLEncoded, null, headers, null, true))
            {
                string responseText = null;
                HttpStatusCode? statusCode = null;

                if (response != null)
                {
                    statusCode = response.StatusCode;

                    using (Stream responseStream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }

                return ParseKeyCheckResponse(responseText, statusCode);
            }
        }

        private string CheckFile(Stream stream, string fileName)
        {
            string extension = FileHelpers.GetFileNameExtension(fileName);

            if (!Upla.IsSupportedExtension(extension))
            {
                return string.Format(UplaStrings.ErrorUnsupportedFileType, string.IsNullOrEmpty(extension) ? fileName : extension,
                    string.Join(", ", Upla.ImageExtensions.Concat(Upla.VideoExtensions)));
            }

            if (stream != null && stream.CanSeek)
            {
                long limit = IsMember ? Upla.MaxRequestSize : Upla.GuestMaxFileSize;

                if (stream.Length > limit)
                {
                    return string.Format(IsMember ? UplaStrings.ErrorFileTooLargeMember : UplaStrings.ErrorFileTooLargeGuest,
                        FormatMegabytes(stream.Length, !IsMember), FormatMegabytes(limit, !IsMember));
                }
            }

            return null;
        }

        private Dictionary<string, string> CreateArguments(Stream stream, string fileName)
        {
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                { "key", APIKey },
                { "format", "json" }
            };

            // Chevereto ignores albums and tags for guest uploads.
            if (IsMember)
            {
                string albumID = Upla.ParseAlbumID(Settings.Album);

                if (!string.IsNullOrEmpty(albumID))
                {
                    args.Add("album_id", albumID);
                }

                string tags = Upla.NormalizeTags(Settings.Tags);

                if (!string.IsNullOrEmpty(tags))
                {
                    args.Add("tags", tags);
                }
            }

            if (Settings.CategoryID > 0)
            {
                args.Add("category_id", Settings.CategoryID.ToString(CultureInfo.InvariantCulture));
            }

            if (!string.IsNullOrEmpty(Settings.Expiration) && Upla.ExpirationPresets.Contains(Settings.Expiration))
            {
                args.Add("expiration", Settings.Expiration);
            }

            if (Settings.NSFW)
            {
                args.Add("nsfw", "1");
            }

            // Chevereto fails the whole upload (error 610) when the resize width is larger than the image.
            if (Settings.MaxWidth > 0 && !Upla.IsVideoExtension(FileHelpers.GetFileNameExtension(fileName)) &&
                TryGetImageWidth(stream, out int width) && width > Settings.MaxWidth)
            {
                args.Add("width", Settings.MaxWidth.ToString(CultureInfo.InvariantCulture));
            }

            return args;
        }

        private static bool TryGetImageWidth(Stream stream, out int width)
        {
            width = 0;

            if (stream == null || !stream.CanSeek)
            {
                return false;
            }

            long position = stream.Position;

            try
            {
                using (Image image = Image.FromStream(stream, false, false))
                {
                    width = image.Width;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                stream.Position = position;
            }
        }

        private static string FormatMegabytes(long bytes, bool binary)
        {
            double megabytes = bytes / (binary ? 1024d * 1024d : 1000d * 1000d);
            return megabytes.ToString("0.#", CultureInfo.CurrentCulture) + " MB";
        }

        public static UplaResponse ParseResponse(string responseText, bool requestSucceeded, HttpStatusCode? statusCode, UplaLinkType linkType, bool isMember)
        {
            UplaResponse response = new UplaResponse();
            JObject json = TryParseJson(responseText);

            if (requestSucceeded && json?["image"] is JObject image)
            {
                string viewerPage = GetString(image, "url_viewer");
                string directLink = GetString(image, "url");
                string shortLink = GetString(image, "url_short");
                string preferred;

                switch (linkType)
                {
                    case UplaLinkType.DirectLink:
                        preferred = directLink;
                        break;
                    case UplaLinkType.ShortLink:
                        preferred = shortLink;
                        break;
                    default:
                        preferred = viewerPage;
                        break;
                }

                // Uploads waiting for moderation have no direct or thumbnail links, only the page link.
                response.URL = new[] { preferred, viewerPage, shortLink, directLink }.FirstOrDefault(x => !string.IsNullOrEmpty(x));
                response.ThumbnailURL = GetString(image, "thumb", "url");
                response.DeletionURL = GetString(image, "delete_url");
                response.AwaitingModeration = GetBool(image["is_approved"]) == false;
                response.Success = !string.IsNullOrEmpty(response.URL);

                if (!response.Success)
                {
                    response.ErrorMessage = UplaStrings.ErrorUnexpectedResponse;
                }

                return response;
            }

            response.ErrorCode = GetErrorCode(json);
            response.ErrorMessage = GetErrorMessage(response.ErrorCode, GetString(json, "error", "message"), statusCode, isMember);
            return response;
        }

        public static UplaKeyCheckResult ParseKeyCheckResponse(string responseText, HttpStatusCode? statusCode)
        {
            JObject json = TryParseJson(responseText);
            int code = GetErrorCode(json);
            string message = GetString(json, "error", "message");

            switch (code)
            {
                case 130:
                    return new UplaKeyCheckResult() { Status = UplaKeyStatus.Valid, Message = message };
                case 100:
                    return new UplaKeyCheckResult() { Status = IsOldKeyFormatMessage(message) ? UplaKeyStatus.OldFormat : UplaKeyStatus.Invalid, Message = message };
                case 403:
                    return new UplaKeyCheckResult() { Status = UplaKeyStatus.NoUploadPermission, Message = message };
            }

            if (string.IsNullOrEmpty(message))
            {
                message = statusCode.HasValue ? "HTTP " + (int)statusCode.Value : UplaStrings.ErrorConnection;
            }

            return new UplaKeyCheckResult() { Status = UplaKeyStatus.Unknown, Message = message };
        }

        // Error codes come from Chevereto 4.5.7. Messages may be translated to the site language, so the codes decide.
        private static string GetErrorMessage(int code, string message, HttpStatusCode? statusCode, bool isMember)
        {
            switch (code)
            {
                case 100:
                    if (IsOldKeyFormatMessage(message)) return UplaStrings.KeyOldFormat;
                    return isMember ? UplaStrings.ErrorInvalidKey : UplaStrings.ErrorGuestUploadUnavailable;
                case 101:
                    return UplaStrings.ErrorDuplicate;
                case 130:
                    if (ContainsText(message, "flood")) return UplaStrings.ErrorFlood;
                    return string.IsNullOrEmpty(message) ? UplaStrings.ErrorEmptySource : string.Format(UplaStrings.ErrorRejected, message);
                case 403:
                    return UplaStrings.ErrorForbidden;
                case 600:
                    return UplaStrings.ErrorVideoProcessing;
                case 610:
                    return UplaStrings.ErrorWidth;
                case 614:
                    return UplaStrings.ErrorFileTypeRejected;
            }

            if (ContainsText(message, "too big") || statusCode == HttpStatusCode.RequestEntityTooLarge)
            {
                return UplaStrings.ErrorTooBig;
            }

            if (!string.IsNullOrEmpty(message))
            {
                return string.Format(UplaStrings.ErrorRejected, message);
            }

            if (statusCode == HttpStatusCode.NotFound)
            {
                return UplaStrings.ErrorAPIDisabled;
            }

            if (statusCode.HasValue && (int)statusCode.Value >= 500)
            {
                return string.Format(UplaStrings.ErrorServer, (int)statusCode.Value);
            }

            if (!statusCode.HasValue)
            {
                return UplaStrings.ErrorConnection;
            }

            return UplaStrings.ErrorUnexpectedResponse;
        }

        private static bool IsOldKeyFormatMessage(string message)
        {
            return ContainsText(message, "no longer supported");
        }

        private static bool ContainsText(string text, string value)
        {
            return text != null && text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static JObject TryParseJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            try
            {
                return JToken.Parse(text) as JObject;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static int GetErrorCode(JObject json)
        {
            return int.TryParse(GetString(json, "error", "code"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int code) ? code : 0;
        }

        private static string GetString(JToken token, params string[] path)
        {
            foreach (string name in path)
            {
                token = (token as JObject)?[name];

                if (token == null)
                {
                    return null;
                }
            }

            return token.Type == JTokenType.Null || token.Type == JTokenType.Undefined ? null : token.ToString();
        }

        private static bool? GetBool(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            switch (token.Type)
            {
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Integer:
                    return token.Value<long>() != 0;
                case JTokenType.String:
                    string text = token.Value<string>();
                    if (text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
                    if (text == "0" || text.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
                    return null;
                default:
                    return null;
            }
        }
    }
}
