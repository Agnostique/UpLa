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

namespace ShareX
{
    // upla.com.tr: UpLa's own releases (github.com/Agnostique/UpLa, tags vX.Y.Z), checked at most once a day. ShareX's
    // update channels and dev builds do not apply.
    internal class ShareXUpdateManager : GitHubUpdateManager
    {
        public const string GitHubOwnerName = "Agnostique";
        public const string GitHubRepoName = "UpLa";

        private static readonly TimeSpan DailyCheck = TimeSpan.FromDays(1);

        public UpdateChannel UpdateChannel { get; set; }

        public override GitHubUpdateChecker CreateUpdateChecker()
        {
            return new GitHubUpdateChecker(GitHubOwnerName, GitHubRepoName)
            {
                IsPortable = Program.Portable,
                IgnoreRevision = true
            };
        }

        // The timer still runs every hour, so a check that failed (no connection) is tried again soon.
        protected override bool IsUpdateCheckDue()
        {
            DateTime lastCheck = Program.Settings.LastUpdateCheck;
            return lastCheck > DateTime.UtcNow || DateTime.UtcNow - lastCheck >= DailyCheck;
        }

        protected override void OnUpdateChecked()
        {
            Program.Settings.LastUpdateCheck = DateTime.UtcNow;
        }
    }
}