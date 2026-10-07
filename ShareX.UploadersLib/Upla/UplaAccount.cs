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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareX.HelpersLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShareX.UploadersLib
{
    public enum UplaAccountStatus
    {
        Success,
        TwoFactorRequired,
        InvalidCredentials,
        InvalidTwoFactorCode,
        TooManyAttempts,
        Blocked,
        AccountBanned,
        AccountAwaitingConfirmation,
        AccountAwaitingEmail,
        AccountNotValid,
        InvalidKey,
        APIDisabled,
        NotSupported,
        ConnectionError,
        Cancelled,
        Unknown
    }

    public class UplaAccountResult
    {
        public UplaAccountStatus Status { get; set; }
        public string APIKey { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string ProfileURL { get; set; }
        public int RetryAfterSeconds { get; set; }
        public string Message { get; set; }

        public bool IsSuccess => Status == UplaAccountStatus.Success;
    }

    // Client of the upla-app route (Server/chevereto/app/legacy/routes/overrides/upla-app.php) that signs a member in
    // with username/email and password and gets an upload key for this computer. The password is only sent to the
    // server over HTTPS and never stored or logged; the app keeps the per-computer key (UplaSettings.PersonalAPIKey, DPAPI).
    public sealed class UplaAccountClient
    {
        // Each request gives up after this long (the sign-in window can also be cancelled at any time).
        public static TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public string WebsiteURL { get; }

        public UplaAccountClient(string websiteURL = Upla.WebsiteURL)
        {
            WebsiteURL = websiteURL.TrimEnd('/');
        }

        public Task<UplaAccountResult> SignInAsync(string loginSubject, string password, string twoFactorCode, string deviceName,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>
            {
                { "login-subject", (loginSubject ?? "").Trim() },
                { "password", password ?? "" },
                { "device", deviceName ?? "" }
            };

            string code = new string((twoFactorCode ?? "").Where(char.IsDigit).ToArray());

            if (code.Length > 0)
            {
                fields.Add("two-factor-code", code);
            }

            return PostAsync("login", fields, null, cancellationToken);
        }

        public Task<UplaAccountResult> GetAccountAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            return PostAsync("me", null, Upla.NormalizeAPIKey(apiKey), cancellationToken);
        }

        // Deletes this computer's key on the server; the caller forgets the key locally whatever the result is.
        public Task<UplaAccountResult> SignOutAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            return PostAsync("logout", null, Upla.NormalizeAPIKey(apiKey), cancellationToken);
        }

        public UplaAccountResult SignIn(string loginSubject, string password, string twoFactorCode, string deviceName)
        {
            return SignInAsync(loginSubject, password, twoFactorCode, deviceName).GetAwaiter().GetResult();
        }

        public UplaAccountResult GetAccount(string apiKey)
        {
            return GetAccountAsync(apiKey).GetAwaiter().GetResult();
        }

        public UplaAccountResult SignOut(string apiKey)
        {
            return SignOutAsync(apiKey).GetAwaiter().GetResult();
        }

        private async Task<UplaAccountResult> PostAsync(string action, Dictionary<string, string> fields, string apiKey, CancellationToken cancellationToken)
        {
            using (CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                timeout.CancelAfter(RequestTimeout);

                try
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, $"{WebsiteURL}/upla-app/{action}"))
                    {
                        request.Content = new FormUrlEncodedContent(fields ?? new Dictionary<string, string>());
                        request.Headers.Add("X-Upla-App", "1");

                        if (!string.IsNullOrEmpty(apiKey))
                        {
                            request.Headers.Add("X-API-Key", apiKey);
                        }

                        using (HttpResponseMessage response = await HttpClientFactory.Create().SendAsync(request, timeout.Token).ConfigureAwait(false))
                        {
                            string responseText = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
                            TimeSpan? retryAfter = response.Headers.RetryAfter?.Delta;

                            return ParseSignInResponse(responseText, response.StatusCode, action,
                                retryAfter.HasValue ? (int)retryAfter.Value.TotalSeconds : 0);
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return new UplaAccountResult { Status = UplaAccountStatus.Cancelled, Message = "" };
                }
                catch (Exception e) when (e is HttpRequestException || e is OperationCanceledException)
                {
                    // Exception messages never contain the request body, so the password cannot end up in the log.
                    DebugHelper.WriteLine($"upla.com.tr {action} request failed: {e.Message}");
                    return new UplaAccountResult { Status = UplaAccountStatus.ConnectionError, Message = UplaStrings.ErrorConnection };
                }
            }
        }

        // action: "login", "me" or "logout"; a successful login must carry a key and a username, "me" a username.
        public static UplaAccountResult ParseSignInResponse(string responseText, HttpStatusCode? statusCode, string action = "login", int retryAfterHeader = 0)
        {
            if (statusCode == null)
            {
                return new UplaAccountResult { Status = UplaAccountStatus.ConnectionError, Message = UplaStrings.ErrorConnection };
            }

            int status = (int)statusCode.Value;
            JObject json = TryParseJson(responseText);

            if (json == null)
            {
                switch (status)
                {
                    case 404:
                    case 405:
                        // The site does not have the upla-app route (yet).
                        return new UplaAccountResult { Status = UplaAccountStatus.NotSupported, Message = UplaStrings.SignInNotSupported };
                    case 403:
                        // A plain "403 Forbidden" is Chevereto's daily lockout after too many failed sign-ins, sign-ups or
                        // two-step codes from one IP; a Cloudflare block is an HTML 403 page.
                        return new UplaAccountResult { Status = UplaAccountStatus.Blocked, Message = UplaStrings.SignInBlocked };
                    case 429:
                        // Cloudflare rate limiting answers with an HTML page.
                        return TooManyRequests(retryAfterHeader);
                    default:
                        // 2xx HTML: maintenance, consent or private mode pages, or a Chevereto IP ban message.
                        return new UplaAccountResult
                        {
                            Status = UplaAccountStatus.Unknown,
                            Message = status >= 500 ? string.Format(UplaStrings.ErrorServer, status) :
                                status < 300 ? UplaStrings.SignInUnexpectedPage : UplaStrings.ErrorUnexpectedResponse
                        };
                }
            }

            JObject user = json["user"] as JObject;
            JObject error = json["error"] as JObject;

            UplaAccountResult result = new UplaAccountResult
            {
                APIKey = Upla.NormalizeAPIKey(GetString(json, "api_key")),
                Username = GetString(user, "username"),
                Name = GetString(user, "name"),
                ProfileURL = GetString(user, "url")
            };

            if (status == 200 && json["error"] == null)
            {
                bool complete = action == "login" ? result.APIKey.Length > 0 && result.Username.Length > 0 :
                    action == "me" ? result.Username.Length > 0 : true;

                if (complete)
                {
                    result.Status = UplaAccountStatus.Success;
                    return result;
                }

                return new UplaAccountResult { Status = UplaAccountStatus.Unknown, Message = UplaStrings.ErrorUnexpectedResponse };
            }

            string code = GetString(error, "code");
            result.Message = GetString(error, "message");
            result.RetryAfterSeconds = int.TryParse(GetString(json, "retry_after"), out int retryAfter) ? retryAfter : retryAfterHeader;
            result.APIKey = "";

            switch (code)
            {
                case "two_factor_required":
                    result.Status = UplaAccountStatus.TwoFactorRequired;
                    result.Message = UplaStrings.SignInTwoFactorRequired;
                    break;
                case "invalid_credentials":
                case "missing_fields":
                    result.Status = UplaAccountStatus.InvalidCredentials;
                    result.Message = UplaStrings.SignInInvalidCredentials;
                    break;
                case "invalid_two_factor_code":
                    result.Status = UplaAccountStatus.InvalidTwoFactorCode;
                    result.Message = UplaStrings.SignInInvalidTwoFactorCode;
                    break;
                case "too_many_attempts":
                    result.Status = UplaAccountStatus.TooManyAttempts;
                    result.Message = TooManyRequests(result.RetryAfterSeconds).Message;
                    break;
                case "account_banned":
                    result.Status = UplaAccountStatus.AccountBanned;
                    result.Message = UplaStrings.SignInAccountBanned;
                    break;
                case "account_awaiting_confirmation":
                    result.Status = UplaAccountStatus.AccountAwaitingConfirmation;
                    result.Message = UplaStrings.SignInAccountAwaitingConfirmation;
                    break;
                case "account_awaiting_email":
                    result.Status = UplaAccountStatus.AccountAwaitingEmail;
                    result.Message = UplaStrings.SignInAccountAwaitingEmail;
                    break;
                case "account_not_valid":
                    result.Status = UplaAccountStatus.AccountNotValid;
                    result.Message = UplaStrings.SignInAccountNotValid;
                    break;
                case "invalid_key":
                    result.Status = UplaAccountStatus.InvalidKey;
                    result.Message = UplaStrings.SignInDeviceSignedOut;
                    break;
                case "api_disabled":
                    result.Status = UplaAccountStatus.APIDisabled;
                    result.Message = UplaStrings.SignInAPIDisabled;
                    break;
                default:
                    if (status == 429)
                    {
                        return TooManyRequests(result.RetryAfterSeconds);
                    }

                    result.Status = UplaAccountStatus.Unknown;
                    result.Message = string.IsNullOrEmpty(result.Message) ? UplaStrings.ErrorUnexpectedResponse : string.Format(UplaStrings.SignInFailed, result.Message);
                    break;
            }

            return result;
        }

        private static UplaAccountResult TooManyRequests(int retryAfterSeconds)
        {
            return new UplaAccountResult
            {
                Status = UplaAccountStatus.TooManyAttempts,
                RetryAfterSeconds = retryAfterSeconds,
                Message = retryAfterSeconds >= 86400 ? UplaStrings.SignInTooManyAttemptsDay :
                    retryAfterSeconds >= 3600 ? UplaStrings.SignInTooManyAttemptsHour : UplaStrings.SignInTooManyRequests
            };
        }

        // Shown on the "Connected devices" page of the website, e.g. "DESKTOP-1A2B3C (5f3e9a1c)".
        public static string GetDeviceName(string installID)
        {
            string machine = Environment.MachineName;
            string id = string.IsNullOrEmpty(installID) ? "" : installID.Replace("-", "");

            return id.Length >= 8 ? $"{machine} ({id.Substring(0, 8)})" : machine;
        }

        private static string GetString(JObject json, string name)
        {
            JToken token = json?[name];
            return token is JValue value && value.Value != null ? Convert.ToString(value.Value, System.Globalization.CultureInfo.InvariantCulture) : "";
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
    }
}
