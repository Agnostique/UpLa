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

        // Account
        public static string AccountMenu => T("upla.com.tr hesabı", "upla.com.tr account");
        public static string AccountStatusGuest => T("Misafir olarak yüklüyorsunuz; dosyalar bir hesaba bağlanmaz. Hesabınızla yüklemek için giriş yapın.",
            "You upload as a guest; files are not linked to an account. Sign in to upload to your account.");
        public static string AccountStatusSignedIn => T("Giriş yapıldı: {0}. Dosyalar hesabınıza yüklenir.", "Signed in as {0}. Files are uploaded to your account.");
        public static string AccountStatusManualKey => T("Elle girilen API anahtarı kullanılıyor; dosyalar anahtarın sahibi olan hesaba yüklenir.",
            "Using an API key entered by hand; files are uploaded to the account that owns the key.");
        public static string AccountChecking => T("Hesap kontrol ediliyor...", "Checking the account...");
        public static string SignInButton => T("Giriş yap...", "Sign in...");
        public static string SignOutButton => T("Çıkış yap", "Sign out");
        public static string SignOutConfirm => T("upla.com.tr hesabınızdan çıkış yapılsın mı? Bu bilgisayarın bağlantısı sunucudan da silinir; sonraki yüklemeler misafir olarak yapılır.",
            "Sign out of your upla.com.tr account? This computer's connection is removed from the server too; later uploads are made as a guest.");
        public static string ManualKeyRemoveConfirm => T("Elle girilen API anahtarı bu bilgisayardan kaldırılsın mı? Sonraki yüklemeler misafir olarak yapılır.",
            "Remove the API key entered by hand from this computer? Later uploads are made as a guest.");
        public static string MyProfile => T("Profilim", "My profile");
        public static string ConnectedDevices => T("Bağlı cihazlar", "Connected devices");
        public static string ReportAbuse => T("Kötüye kullanımı bildir", "Report abuse");
        public static string ForgotPassword => T("Şifremi unuttum", "Forgot password");
        public static string ManualAPIKey => T("API anahtarını elle gir (gelişmiş)", "Enter an API key by hand (advanced)");
        public static string SignInAgain => T("Tekrar giriş yap...", "Sign in again...");
        public static string ContinueAsGuest => T("Misafir olarak devam et", "Continue as a guest");
        public static string AccountMenuNeedsSignIn => T("{0} (tekrar giriş yapın)", "{0} (sign in again)");
        public static string AccountStatusLost => T("{0} hesabının oturumu bu bilgisayarda okunamadı (ayarlar başka bir bilgisayardan veya yedekten gelmiş olabilir). Tekrar giriş yapın veya misafir olarak devam edin.",
            "The sign-in of {0} could not be read on this computer (the settings may come from another computer or a backup). Sign in again or continue as a guest.");
        public static string SignOutServerFailed => T("Bu bilgisayarda çıkış yapıldı ancak bağlantısı upla.com.tr'den kaldırılamadı ({0}). Kaldırılana kadar bu bağlantı hesabınıza yükleme yapabilir. \"Bağlı cihazlar\" sayfası açılsın mı?",
            "You are signed out on this computer, but its connection could not be removed from upla.com.tr ({0}). Until it is removed it can still upload to your account. Open the \"Connected devices\" page?");

        // Sign-in window
        public static string SignInTitle => T("upla.com.tr'ye giriş yap", "Sign in to upla.com.tr");
        public static string UsernameOrEmail => T("Kullanıcı adı veya e-posta:", "Username or email:");
        public static string Password => T("Şifre:", "Password:");
        public static string TwoFactorCode => T("Doğrulama kodu:", "Verification code:");
        public static string SignInSubmit => T("Giriş yap", "Sign in");
        public static string Cancel => T("İptal", "Cancel");
        public static string SigningIn => T("Giriş yapılıyor...", "Signing in...");
        public static string SignInPrivacy => T("Şifreniz yalnızca upla.com.tr'ye gönderilir ve bu bilgisayarda saklanmaz. Hesabınızda bu bilgisayar için ayrı bir bağlantı oluşturulur; upla.com.tr/upla-app/devices adresindeki \"Bağlı cihazlar\" sayfasından kaldırabilirsiniz. Şifrenizi değiştirmek bu bağlantıyı kaldırmaz.",
            "Your password is only sent to upla.com.tr and is not stored on this computer. A separate connection is created in your account for this computer; you can remove it on the \"Connected devices\" page at upla.com.tr/upla-app/devices. Changing your password does not remove it.");
        public static string SignInFieldsRequired => T("Kullanıcı adınızı veya e-postanızı ve şifrenizi girin.", "Enter your username or email and your password.");
        public static string SignInTwoFactorRequired => T("Hesabınızda iki adımlı doğrulama açık. Kimlik doğrulama uygulamanızdaki 6 haneli kodu girin.",
            "Two-step verification is on for your account. Enter the 6-digit code from your authenticator app.");
        public static string SignInInvalidCredentials => T("Kullanıcı adı/e-posta veya şifre hatalı. Hesabınızı bir sosyal ağ ile açtıysanız önce web sitesinde şifre oluşturun.",
            "Wrong username/email or password. If you created your account with a social network, create a password on the website first.");
        public static string SignInInvalidTwoFactorCode => T("Doğrulama kodu hatalı.", "Wrong verification code.");
        public static string SignInTooManyAttemptsHour => T("Çok fazla hatalı deneme yapıldı. Bir saat sonra tekrar deneyin veya web sitesinden giriş yapın.",
            "Too many failed attempts. Try again in an hour or sign in on the website.");
        public static string SignInTooManyAttemptsDay => T("Çok fazla hatalı deneme yapıldı. 24 saat sonra tekrar deneyin veya web sitesinden giriş yapın.",
            "Too many failed attempts. Try again in 24 hours or sign in on the website.");
        public static string SignInTooManyRequests => T("upla.com.tr'ye çok fazla istek gönderildi. Biraz bekleyip tekrar deneyin.", "Too many requests to upla.com.tr. Wait a little and try again.");
        public static string SignInUnexpectedPage => T("upla.com.tr giriş isteğine beklenmeyen bir sayfayla yanıt verdi (site bakımda olabilir). Daha sonra tekrar deneyin.",
            "upla.com.tr answered the sign-in with an unexpected page (the site may be under maintenance). Try again later.");
        public static string SignInLost => T("Bu bilgisayarda kayıtlı upla.com.tr oturumu okunamadığı için dosya yüklenmedi (ayarlar başka bir bilgisayardan veya yedekten gelmiş olabilir). \"upla.com.tr hesabı\" menüsünden tekrar giriş yapın veya misafir olarak devam edin.",
            "The file was not uploaded because the upla.com.tr sign-in saved on this computer could not be read (the settings may come from another computer or a backup). Sign in again or continue as a guest from the \"upla.com.tr account\" menu.");
        public static string SignInBlocked => T("upla.com.tr bu bilgisayardan gelen istekleri geçici olarak engelledi (çok fazla hatalı deneme olabilir). Daha sonra tekrar deneyin.",
            "upla.com.tr is temporarily blocking requests from this computer (possibly too many failed attempts). Try again later.");
        public static string SignInAccountBanned => T("Bu hesap engellenmiş.", "This account is banned.");
        public static string SignInAccountAwaitingConfirmation => T("Hesabınız henüz onaylanmamış. E-postanıza gönderilen onay linkine tıklayın.",
            "Your account is not confirmed yet. Click the confirmation link sent to your email.");
        public static string SignInAccountAwaitingEmail => T("Hesabınızın bir e-posta adresine ihtiyacı var. upla.com.tr'de giriş yapıp e-posta adresinizi ekleyin.",
            "Your account needs an email address. Sign in on upla.com.tr and add your email address.");
        public static string SignInAccountNotValid => T("Bu hesapla giriş yapılamıyor.", "This account cannot sign in.");
        public static string SignInDeviceSignedOut => T("Bu bilgisayarın bağlantısı kaldırılmış (web sitesindeki \"Bağlı cihazlar\" sayfasından silinmiş olabilir). Tekrar giriş yapın.",
            "This computer's connection was removed (possibly from the website's \"Connected devices\" page). Sign in again.");
        public static string SignInAPIDisabled => T("upla.com.tr üye yüklemelerini şu anda kabul etmiyor.", "upla.com.tr does not accept member uploads right now.");
        public static string SignInNotSupported => T("upla.com.tr henüz uygulamadan girişi desteklemiyor. Gelişmiş seçenekten, upla.com.tr › Ayarlar › API sayfasından aldığınız anahtarı elle girebilirsiniz.",
            "upla.com.tr does not support signing in from the app yet. In the advanced option you can enter the key from upla.com.tr › Settings › API by hand.");
        public static string SignInFailed => T("Giriş yapılamadı: {0}", "Could not sign in: {0}");

        // Settings panel
        public static string PersonalAPIKey => T("API anahtarı:", "API key:");
        public static string PersonalAPIKeyHint => T("Yalnızca uygulamadan giriş yapamıyorsanız gerekir. Anahtarı \"Bağlı cihazlar\" sayfasındaki \"Yeni API anahtarı oluştur\" düğmesiyle alın. Ayarlar › API sayfasındaki \"Regen key\" en yeni anahtarı siler; bu, giriş yaptığınız başka bir bilgisayarın bağlantısı olabilir.",
            "Only needed if you cannot sign in from the app. Get a key with \"Create a new API key\" on the \"Connected devices\" page. \"Regen key\" on Settings › API deletes the newest key, which may be the connection of another computer you signed in on.");
        public static string ShowAPIKey => T("Göster", "Show");
        public static string VerifyAPIKey => T("Doğrula", "Verify");
        public static string GetAPIKey => T("Anahtar al", "Get key");
        public static string SignUp => T("Hesap oluştur", "Sign up");
        public static string APIDocumentation => T("API belgesi", "API documentation");
        public static string StatusGuest => T("Misafir yükleme: dosyalar bir hesaba bağlanmaz.", "Guest upload: files are not linked to an account.");
        public static string StatusMember => T("Dosyalar bu anahtarın sahibi olan hesaba yüklenir.", "Files are uploaded to the account that owns this key.");
        public static string Verifying => T("Doğrulanıyor...", "Verifying...");
        public static string KeyValid => T("Anahtar geçerli; dosyalar hesabınıza yüklenecek.", "The key is valid; files will be uploaded to your account.");
        public static string KeyInvalid => T("Anahtar geçersiz. Uygulamadan giriş yapın veya \"Bağlı cihazlar\" sayfasından yeni bir anahtar oluşturun.",
            "Invalid key. Sign in from the app or create a new key on the \"Connected devices\" page.");
        public static string KeyOldFormat => T("Bu anahtar eski formatta ve artık desteklenmiyor. Uygulamadan giriş yapın veya \"Bağlı cihazlar\" sayfasından yeni bir anahtar oluşturun.",
            "This key uses an old format that is no longer supported. Sign in from the app or create a new key on the \"Connected devices\" page.");
        public static string KeyNoUploadPermission => T("Anahtar geçerli ancak bu hesabın yükleme izni yok.", "The key is valid but this account is not allowed to upload.");
        public static string GuestUploadAvailable => T("Misafir yükleme kullanılabilir.", "Guest upload is available.");
        public static string GuestUploadUnavailable => T("Misafir yükleme şu anda kapalı. Hesabınızla giriş yapın.", "Guest upload is currently disabled. Sign in with your account.");
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
        public static string MaxWidth => T("Sunucuda en fazla genişlik (px, 0 = kapalı):", "Max width on server (px, 0 = off):");
        public static string MemberOnlyNote => T("Albüm ve etiketler yalnızca giriş yaptığınızda çalışır. Otomatik silme, upla.com.tr'de etkinse uygulanır.",
            "Album and tags only work when you are signed in. Auto delete is applied when it is enabled on upla.com.tr.");
        public static string VideoNote => T("Ekran kayıtları (MP4, WEBM) da upla.com.tr'ye yüklenir. Sınır misafirlerde 20 MB, giriş yapan üyelerde 100 MB.",
            "Screen recordings (MP4, WEBM) are uploaded to upla.com.tr too. The limit is 20 MB for guests and 100 MB for signed in members.");
        public static string UpdateNotVerified => T("İndirilen güncelleme doğrulanamadı (SHA-256 eşleşmedi), bu yüzden kurulmadı.",
            "The downloaded update could not be verified (SHA-256 mismatch), so it was not installed.");
        public static string StopRecordingAtUploadLimit => T("Yüklenecek ekran kayıtlarını yükleme sınırına gelince durdur",
            "Stop screen recordings that will be uploaded at the upload limit");
        public static string RecordingStoppedAtUploadLimit => T("Kayıt {0} yükleme sınırına ulaştığı için durduruldu ve yükleniyor.",
            "The recording reached the {0} upload limit, so it was stopped and is being uploaded.");
        public static string RecordingShortenedToUploadLimit => T("Kayıt {0} yükleme sınırını aştığı için sonu kısaltıldı.",
            "The recording was longer than the {0} upload limit, so its end was cut.");

        // Upload errors
        public static string ErrorUnsupportedFileType => T("\"{0}\" türündeki dosyalar upla.com.tr'ye yüklenemez. Desteklenen türler: {1}.",
            "\"{0}\" files cannot be uploaded to upla.com.tr. Supported types: {1}.");
        public static string ErrorFileTooLargeGuest => T("Dosya çok büyük ({0}). Misafir yüklemelerde sınır {1}; daha büyük dosyalar için hesabınızla giriş yapın.",
            "The file is too large ({0}). Guest uploads are limited to {1}; sign in with your account for larger files.");
        public static string ErrorFileTooLargeMember => T("Dosya çok büyük ({0}); tek seferde en fazla {1} yüklenebilir.", "The file is too large ({0}); at most {1} can be uploaded at once.");
        public static string ErrorInvalidKey => T("upla.com.tr hesap bağlantınız artık geçerli değil (bu bilgisayarın bağlantısı kaldırılmış olabilir). \"upla.com.tr hesabı\" menüsünden \"Tekrar giriş yap\"ı seçin.",
            "Your upla.com.tr account connection is no longer valid (this computer's connection may have been removed). Choose \"Sign in again\" in the \"upla.com.tr account\" menu.");
        public static string ErrorInvalidManualKey => T("Elle girilen upla.com.tr API anahtarı geçersiz. Hedef ayarları › upla.com.tr bölümünden uygulamayla giriş yapın veya anahtarı kontrol edin.",
            "The upla.com.tr API key entered by hand is invalid. Sign in with the app or check the key in Destination settings › upla.com.tr.");
        public static string ErrorGuestUploadUnavailable => T("upla.com.tr'de misafir yükleme şu anda kapalı. \"upla.com.tr hesabı\" menüsünden hesabınızla giriş yapın.",
            "Guest upload is currently disabled on upla.com.tr. Sign in with your account from the \"upla.com.tr account\" menu.");
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
        public static string FirstUploadText => T("Ekran görüntüleriniz ve kayıtlarınız yakalandıktan sonra otomatik olarak upla.com.tr'ye yüklenir ve linke sahip herkesin görebileceği bir bağlantı oluşturulur.\r\n\r\nOtomatik yükleme açık kalsın mı?\r\n\r\nHayır derseniz bu dosya yüklenmez ve otomatik yükleme kapatılır; daha sonra \"Yakalama sonrası\" menüsünden tekrar açabilirsiniz.",
            "Your screenshots and recordings are uploaded to upla.com.tr automatically after capture, and a link that anyone who has it can open is created.\r\n\r\nKeep automatic upload on?\r\n\r\nIf you choose No, this file is not uploaded and automatic upload is turned off; you can turn it back on from the \"After capture tasks\" menu.");

        // Deletion links
        public static string DeletionURLConfirmTitle => T("Silme linki", "Deletion link");
        public static string DeletionURLConfirmText => T("Silme linki açıldığında yüklenen dosya sunucudan hemen ve kalıcı olarak silinebilir. Devam etmek istiyor musunuz?",
            "Opening the deletion link can delete the uploaded file from the server immediately and permanently. Do you want to continue?");

        // About window
        public static string AboutDescription => T("UpLa, upla.com.tr'nin ekran görüntüsü alma ve yükleme uygulamasıdır. ShareX Ekibi'nin geliştirdiği özgür ve açık kaynaklı ShareX programını temel alır ve GNU Genel Kamu Lisansı sürüm 3 (GPL v3) ile dağıtılır. UpLa resmi bir ShareX sürümü değildir; ShareX Ekibi tarafından desteklenmez.",
            "UpLa is the screenshot and upload app of upla.com.tr. It is based on ShareX, the free and open source program made by the ShareX Team, and is distributed under the GNU General Public License version 3 (GPL v3). UpLa is not an official ShareX release and is not supported by the ShareX Team.");
        public static string AboutLicense => T("Lisans", "License");
        public static string AboutShareXTeam => T("ShareX Ekibi", "ShareX Team");
        public static string AboutSourceCode => T("Kaynak kod", "Source code");

        // FFmpeg download for screen recording
        public static string FFmpegDownloadInfo => T("Ekran kaydı için gereken FFmpeg indiriliyor (yaklaşık {0} MB). Bu yalnızca bir kez yapılır.",
            "Downloading FFmpeg, which screen recording needs (about {0} MB). This is done only once.");
        public static string FFmpegDownloadProgress => T("{0} / {1} MB indirildi", "{0} / {1} MB downloaded");
        public static string FFmpegDownloadInstalling => T("Doğrulanıyor ve kuruluyor...", "Verifying and installing...");
        public static string FFmpegDownloadFailed => T("FFmpeg indirilemedi: {0}\r\n\r\nİnternet bağlantınızı kontrol edip kaydı yeniden başlatın.",
            "FFmpeg could not be downloaded: {0}\r\n\r\nCheck your internet connection and start the recording again.");
        public static string FFmpegVerifyFailed => T("İndirilen FFmpeg dosyası beklenen dosya değil, kullanılmadı. Kaydı yeniden başlatarak tekrar deneyin.",
            "The downloaded FFmpeg file is not the expected one and was not used. Start the recording again to retry.");

        // Texts ShareX has only in English: opening a file from the task list or image history, the history menu and its dialogs
        public static string OpenFileQuestion => T("Bu dosyayı açmak ister misiniz?", "Would you like to open this file?");
        public static string ConfirmationTitle => ShareXResources.Name + " - " + T("Onay", "Confirmation");
        public static string HistoryFavorite => T("Sık kullanılanlara ekle", "Favorite");
        public static string HistoryUnfavorite => T("Sık kullanılanlardan çıkar", "Unfavorite");
        public static string HistoryEditTag => T("Etiketi düzenle...", "Edit tag...");
        public static string HistoryEditTagTitle => T("Etiketi düzenle", "Edit tag");
        public static string HistoryEditItem => T("Öğeyi düzenle...", "Edit item...");
        public static string HistoryRenameFile => T("Dosyayı yeniden adlandır...", "Rename file...");
        public static string HistoryRenameFileTitle => T("Dosyayı yeniden adlandır", "Rename file");
        public static string HistoryDeleteItem => T("Öğeyi sil...", "Delete item...");
        public static string HistoryDeleteFileAndItem => T("Dosyayı ve öğeyi sil...", "Delete file && item...");
        public static string HistoryDeleteItemsConfirm(int count) => count > 1 ?
            T("Bu öğeleri gerçekten silmek istiyor musunuz?", "Do you really want to delete these items?") :
            T("Bu öğeyi gerçekten silmek istiyor musunuz?", "Do you really want to delete this item?");
        public static string HistoryDeleteFilesConfirm(int count) => count > 1 ?
            T("Bu dosyaları gerçekten silmek istiyor musunuz?", "Do you really want to delete these files?") :
            T("Bu dosyayı gerçekten silmek istiyor musunuz?", "Do you really want to delete this file?");

        // History item editor
        public static string HistoryItemEditTitle => ShareXResources.Name + " - " + T("Öğeyi düzenle", "Edit item");
        public static string HistoryItemFileName => T("Dosya adı:", "File name:");
        public static string HistoryItemFilePath => T("Dosya yolu:", "File path:");
        public static string HistoryItemDateTime => T("Tarih ve saat:", "Date time:");
        public static string HistoryItemType => T("Tür:", "Type:");
        public static string HistoryItemHost => T("Sunucu:", "Host:");
        public static string HistoryItemURL => T("Adres:", "URL:");
        public static string HistoryItemThumbnailURL => T("Küçük resim adresi:", "Thumbnail URL:");
        public static string HistoryItemDeletionURL => T("Silme adresi:", "Deletion URL:");
        public static string HistoryItemShortenedURL => T("Kısaltılmış adres:", "Shortened URL:");
        public static string HistoryItemTags => T("Etiketler:", "Tags:");
        public static string HistoryItemTagName => T("Ad", "Name");
        public static string HistoryItemTagValue => T("Değer", "Value");
        public static string OK => T("Tamam", "OK");

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
