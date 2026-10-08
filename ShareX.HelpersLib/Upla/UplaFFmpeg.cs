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

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ShareX.HelpersLib
{
    // Screen recording needs FFmpeg. Setup and the portable zip ship ffmpeg.exe next to UpLa.exe; when it is missing it
    // is downloaded on first use. Setup and the app use the same FFmpeg build and accept it only with its known SHA-256.
    public static class UplaFFmpeg
    {
        public const string Version = "8.1";

        // SHA-256 of the release assets of https://github.com/ShareX/FFmpeg/releases/tag/v8.1, as published by GitHub.
        private const string SHA256x64 = "1838932e32f01e0e7b65fde5ca0ebb2ab131cb6a9ff1a84025ee5b784075576c";
        private const string SHA256Arm64 = "7ac6c5dbbe61037bcce97482549294238d87400d8b7b1cb559949293c3b06dd5";

        // Download sizes in MB, for the progress text before the server reports the length.
        private const int SizeMBx64 = 69;
        private const int SizeMBArm64 = 51;

        public static string Platform => RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";

        public static string GetDownloadURL(string platform)
        {
            return $"https://github.com/ShareX/FFmpeg/releases/download/v{Version}/ffmpeg-{Version}-win-{platform}.zip";
        }

        public static string GetSHA256(string platform)
        {
            return platform == "arm64" ? SHA256Arm64 : SHA256x64;
        }

        public static int GetSizeMB(string platform)
        {
            return platform == "arm64" ? SizeMBArm64 : SizeMBx64;
        }

        // Where setup puts it.
        public static string AppFolderPath => FileHelpers.GetAbsolutePath("ffmpeg.exe");

        // Used when the application folder cannot be written, e.g. an install in Program Files without admin rights.
        public static string UserFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ShareXResources.Name, "FFmpeg", "ffmpeg.exe");

        public static string FindExisting()
        {
            if (File.Exists(AppFolderPath))
            {
                return AppFolderPath;
            }

            if (File.Exists(UserFolderPath))
            {
                return UserFolderPath;
            }

            return null;
        }

        public static string GetInstallPath()
        {
            return CanWriteFolder(Path.GetDirectoryName(AppFolderPath)) ? AppFolderPath : UserFolderPath;
        }

        // Downloads the FFmpeg build for this computer and returns the path of ffmpeg.exe.
        public static async Task<string> DownloadAsync(IProgress<(long Downloaded, long Total)> progress, CancellationToken cancellationToken)
        {
            string platform = Platform;
            string installPath = GetInstallPath();
            string zipPath = Path.Combine(Path.GetTempPath(), $"{ShareXResources.Name}-ffmpeg-{Version}-{platform}-{Guid.NewGuid():N}.zip");

            try
            {
                using (HttpResponseMessage response = await HttpClientFactory.Create().GetAsync(GetDownloadURL(platform),
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    long length = response.Content.Headers.ContentLength ?? GetSizeMB(platform) * 1048576L;

                    using (Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                    using (FileStream target = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[81920];
                        long total = 0;
                        int read;

                        while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                        {
                            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                            total += read;
                            progress?.Report((total, length));
                        }
                    }
                }

                InstallFromZip(zipPath, GetSHA256(platform), installPath);
                DebugHelper.WriteLine($"FFmpeg {Version} ({platform}) installed: {installPath}");
                return installPath;
            }
            finally
            {
                try
                {
                    File.Delete(zipPath);
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }
        }

        // Accepts the archive only with the expected SHA-256, then copies ffmpeg.exe out of it.
        public static void InstallFromZip(string zipPath, string expectedSHA256, string installPath)
        {
            string actualSHA256;

            using (FileStream stream = File.OpenRead(zipPath))
            {
                actualSHA256 = Convert.ToHexString(SHA256.HashData(stream));
            }

            if (!actualSHA256.Equals(expectedSHA256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(UplaStrings.FFmpegVerifyFailed);
            }

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                ZipArchiveEntry entry = archive.Entries.FirstOrDefault(x => x.Name.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                {
                    throw new InvalidDataException(UplaStrings.FFmpegVerifyFailed);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(installPath));
                string tempPath = installPath + ".tmp";
                entry.ExtractToFile(tempPath, true);
                File.Move(tempPath, installPath, true);
            }
        }

        private static bool CanWriteFolder(string folderPath)
        {
            try
            {
                string testPath = Path.Combine(folderPath, $".write-test-{Guid.NewGuid():N}");
                using (File.Create(testPath, 1, FileOptions.DeleteOnClose))
                {
                }

                return true;
            }
            catch (Exception e) when (e is UnauthorizedAccessException || e is IOException)
            {
                return false;
            }
        }
    }
}
