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

using System.Globalization;
using System.Text.RegularExpressions;

namespace ShareX.HelpersLib
{
    // Texts of the upla.com.tr features. Turkish when the UI language is Turkish, English otherwise.
    public static class UplaStrings
    {
        private static bool IsTurkish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "tr";

        private static string T(string turkish, string english) => IsTurkish ? turkish : english;

        // Settings panel
        public static string PersonalAPIKey => T("Kişisel API anahtarı:", "Personal API key:");
        public static string PersonalAPIKeyHint => T("Boş bırakırsanız dosyalar misafir olarak (bir hesaba bağlanmadan) yüklenir. Anahtarınızı upla.com.tr › Ayarlar › API sayfasından alabilirsiniz.",
            "Leave empty to upload as a guest (not linked to an account). Get your key at upla.com.tr › Settings › API.");
        public static string ShowAPIKey => T("Göster", "Show");
        public static string VerifyAPIKey => T("Doğrula", "Verify");
        public static string GetAPIKey => T("Anahtar al", "Get key");
        public static string SignUp => T("Hesap oluştur", "Sign up");
        public static string APIDocumentation => T("API belgesi", "API documentation");
        public static string StatusGuest => T("Misafir yükleme: dosyalar bir hesaba bağlanmaz.", "Guest upload: files are not linked to an account.");
        public static string StatusMember => T("Dosyalar bu anahtarın sahibi olan hesaba yüklenir.", "Files are uploaded to the account that owns this key.");
        public static string Verifying => T("Doğrulanıyor...", "Verifying...");
        public static string KeyValid => T("Anahtar geçerli; dosyalar hesabınıza yüklenecek.", "The key is valid; files will be uploaded to your account.");
        public static string KeyInvalid => T("Anahtar geçersiz. upla.com.tr › Ayarlar › API sayfasından yeni bir anahtar oluşturun.",
            "Invalid key. Create a new key at upla.com.tr › Settings › API.");
        public static string KeyOldFormat => T("Bu anahtar eski formatta ve artık desteklenmiyor. upla.com.tr › Ayarlar › API sayfasından yeni bir anahtar oluşturun.",
            "This key uses an old format that is no longer supported. Create a new key at upla.com.tr › Settings › API.");
        public static string KeyNoUploadPermission => T("Anahtar geçerli ancak bu hesabın yükleme izni yok.", "The key is valid but this account is not allowed to upload.");
        public static string GuestUploadAvailable => T("Misafir yükleme kullanılabilir.", "Guest upload is available.");
        public static string GuestUploadUnavailable => T("Misafir yükleme şu anda kapalı. Kişisel API anahtarınızı girin.", "Guest upload is currently disabled. Enter your personal API key.");
        public static string KeyCheckFailed => T("Anahtar doğrulanamadı: {0}", "Could not verify the key: {0}");
        public static string LinkType => T("Kopyalanacak link:", "Link to copy:");
        public static string LinkTypeViewerPage => T("Sayfa linki (önerilen)", "Page link (recommended)");
        public static string LinkTypeDirectLink => T("Doğrudan dosya linki", "Direct file link");
        public static string LinkTypeShortLink => T("Kısa link", "Short link");
        public static string Album => T("Albüm (link veya kimlik):", "Album (link or ID):");
        public static string AlbumHint => T("Örnek: https://upla.com.tr/album/Tatil.AbCd (albüm sizin hesabınıza ait olmalıdır)",
            "Example: https://upla.com.tr/album/Holiday.AbCd (the album must belong to your account)");
        public static string Tags => T("Etiketler (virgülle ayırın):", "Tags (comma separated):");
        public static string CategoryID => T("Kategori kimliği (0 = yok):", "Category ID (0 = none):");
        public static string AutoDelete => T("Otomatik silme:", "Auto delete:");
        public static string AutoDeleteNever => T("Kapalı", "Never");
        public static string NSFW => T("Hassas içerik (NSFW) olarak işaretle", "Mark as NSFW");
        public static string MaxWidth => T("Sunucuda en fazla genişlik (px, 0 = kapalı):", "Max width on server (px, 0 = off):");
        public static string MemberOnlyNote => T("Albüm ve etiketler yalnızca kişisel anahtarla çalışır. Otomatik silme, upla.com.tr'de etkinse uygulanır.",
            "Album and tags only work with a personal key. Auto delete is applied when it is enabled on upla.com.tr.");
        public static string VideoNote => T("Ekran kayıtları (MP4, WEBM, MOV) da upla.com.tr'ye yüklenir; bunun için sunucuda video yüklemenin açık olması gerekir.",
            "Screen recordings (MP4, WEBM, MOV) are uploaded to upla.com.tr too; video uploads must be enabled on the server.");

        // Upload errors
        public static string ErrorUnsupportedFileType => T("\"{0}\" türündeki dosyalar upla.com.tr'ye yüklenemez. Desteklenen türler: {1}.",
            "\"{0}\" files cannot be uploaded to upla.com.tr. Supported types: {1}.");
        public static string ErrorFileTooLargeGuest => T("Dosya çok büyük ({0}). Misafir yüklemelerde sınır {1}; daha büyük dosyalar için kişisel API anahtarı kullanın.",
            "The file is too large ({0}). Guest uploads are limited to {1}; use a personal API key for larger files.");
        public static string ErrorFileTooLargeMember => T("Dosya çok büyük ({0}); tek seferde en fazla {1} yüklenebilir.", "The file is too large ({0}); at most {1} can be uploaded at once.");
        public static string ErrorInvalidKey => T("upla.com.tr API anahtarı geçersiz. Hedef ayarları › upla.com.tr bölümünden anahtarınızı kontrol edin.",
            "The upla.com.tr API key is invalid. Check your key in Destination settings › upla.com.tr.");
        public static string ErrorGuestUploadUnavailable => T("upla.com.tr'de misafir yükleme şu anda kapalı. Hedef ayarları › upla.com.tr bölümüne kişisel API anahtarınızı girin.",
            "Guest upload is currently disabled on upla.com.tr. Enter your personal API key in Destination settings › upla.com.tr.");
        public static string ErrorDuplicate => T("Bu dosya kısa süre önce zaten yüklendi; upla.com.tr aynı dosyanın 24 saat içinde tekrar yüklenmesine izin vermiyor.",
            "This file was already uploaded recently; upla.com.tr does not accept the same file again within 24 hours.");
        public static string ErrorFlood => T("Çok kısa sürede çok fazla yükleme yapıldı. Lütfen biraz bekleyip tekrar deneyin.", "Too many uploads in a short time. Please wait a little and try again.");
        public static string ErrorEmptySource => T("Dosya sunucuya ulaşmadı; dosya izin verilen boyuttan büyük olabilir.", "The file did not reach the server; it may be larger than allowed.");
        public static string ErrorForbidden => T("Bu hesabın upla.com.tr'ye yükleme izni yok veya yükleme geçici olarak kapalı.", "This account is not allowed to upload to upla.com.tr, or uploads are temporarily disabled.");
        public static string ErrorVideoProcessing => T("upla.com.tr videoyu işleyemedi.", "upla.com.tr could not process the video.");
        public static string ErrorWidth => T("Sunucuda yeniden boyutlandırma genişliği resmin kendi genişliğinden büyük.", "The resize width is larger than the image width.");
        public static string ErrorFileTypeRejected => T("Bu dosya türü upla.com.tr'de şu anda kabul edilmiyor.", "This file type is currently not accepted by upla.com.tr.");
        public static string ErrorTooBig => T("Dosya upla.com.tr için çok büyük.", "The file is too large for upla.com.tr.");
        public static string ErrorAPIDisabled => T("upla.com.tr yükleme API'si şu anda kapalı.", "The upla.com.tr upload API is currently disabled.");
        public static string ErrorServer => T("upla.com.tr şu anda yanıt vermiyor (HTTP {0}). Lütfen daha sonra tekrar deneyin.", "upla.com.tr is not responding right now (HTTP {0}). Please try again later.");
        public static string ErrorConnection => T("upla.com.tr'ye bağlanılamadı. İnternet bağlantınızı kontrol edin.", "Could not connect to upla.com.tr. Check your internet connection.");
        public static string ErrorRejected => T("upla.com.tr yüklemeyi kabul etmedi: {0}", "upla.com.tr rejected the upload: {0}");
        public static string ErrorUnexpectedResponse => T("upla.com.tr'den beklenmeyen bir yanıt alındı.", "Unexpected response from upla.com.tr.");

        // First upload
        public static string FirstUploadTitle => T("upla.com.tr'ye otomatik yükleme", "Automatic upload to upla.com.tr");
        public static string FirstUploadText => T("Ekran görüntüleriniz ve kayıtlarınız yakalandıktan sonra otomatik olarak upla.com.tr'ye yüklenir ve linke sahip herkesin görebileceği bir bağlantı oluşturulur.\r\n\r\nOtomatik yükleme açık kalsın mı?\r\n\r\nHayır derseniz bu dosya yüklenmez ve otomatik yükleme kapatılır; daha sonra \"Yakalama sonrası görevler\" menüsünden tekrar açabilirsiniz.",
            "Your screenshots and recordings are uploaded to upla.com.tr automatically after capture, and a link that anyone who has it can open is created.\r\n\r\nKeep automatic upload on?\r\n\r\nIf you choose No, this file is not uploaded and automatic upload is turned off; you can turn it back on from the \"After capture tasks\" menu.");

        // Deletion links
        public static string DeletionURLConfirmTitle => T("Silme linki", "Deletion link");
        public static string DeletionURLConfirmText => T("Silme linki açıldığında yüklenen dosya sunucudan hemen ve kalıcı olarak silinebilir. Devam etmek istiyor musunuz?",
            "Opening the deletion link can delete the uploaded file from the server immediately and permanently. Do you want to continue?");

        // "PT5M", "PT1H", "P2D", "P1W", "P3M", "P1Y" -> "5 dakika", "1 saat", "2 gün"...
        public static string FormatDuration(string isoDuration)
        {
            Match match = Regex.Match(isoDuration ?? "", @"^P(T?)(\d+)([MHDWY])$");

            if (!match.Success)
            {
                return isoDuration;
            }

            bool time = match.Groups[1].Value == "T";
            int count = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            string unit;

            switch (match.Groups[3].Value)
            {
                case "M":
                    unit = time ? T("dakika", count == 1 ? "minute" : "minutes") : T("ay", count == 1 ? "month" : "months");
                    break;
                case "H":
                    unit = T("saat", count == 1 ? "hour" : "hours");
                    break;
                case "D":
                    unit = T("gün", count == 1 ? "day" : "days");
                    break;
                case "W":
                    unit = T("hafta", count == 1 ? "week" : "weeks");
                    break;
                default:
                    unit = T("yıl", count == 1 ? "year" : "years");
                    break;
            }

            return $"{count} {unit}";
        }
    }
}
