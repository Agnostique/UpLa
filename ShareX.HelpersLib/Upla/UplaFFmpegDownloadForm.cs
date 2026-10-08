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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ShareX.HelpersLib
{
    // Downloads FFmpeg when a screen recording starts without it. DialogResult.OK once ffmpeg.exe is in place; errors are
    // shown here, so the caller only has to stop the recording otherwise.
    [DesignerCategory("Code")]
    public class UplaFFmpegDownloadForm : Form
    {
        private readonly Label lblInfo, lblProgress;
        private readonly ProgressBar pbProgress;
        private readonly Button btnCancel;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly int sizeMB = UplaFFmpeg.GetSizeMB(UplaFFmpeg.Platform);

        public string FFmpegPath { get; private set; }

        public UplaFFmpegDownloadForm()
        {
            Text = $"{ShareXResources.Name} - FFmpeg";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);

            TableLayoutPanel tlpMain = new TableLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Dock = DockStyle.Fill
            };

            lblInfo = new Label()
            {
                AutoSize = true,
                MaximumSize = new Size(400, 0),
                Margin = new Padding(3, 3, 3, 8),
                Text = string.Format(UplaStrings.FFmpegDownloadInfo, sizeMB)
            };
            pbProgress = new ProgressBar()
            {
                Width = 400,
                Height = 20,
                Maximum = 1000
            };
            lblProgress = new Label()
            {
                AutoSize = true,
                Margin = new Padding(3, 6, 3, 3),
                Text = string.Format(UplaStrings.FFmpegDownloadProgress, 0, sizeMB)
            };
            btnCancel = new Button()
            {
                Text = UplaStrings.Cancel,
                AutoSize = true,
                Padding = new Padding(8, 0, 8, 0),
                Anchor = AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };

            tlpMain.Controls.Add(lblInfo);
            tlpMain.Controls.Add(pbProgress);
            tlpMain.Controls.Add(lblProgress);
            tlpMain.Controls.Add(btnCancel);
            Controls.Add(tlpMain);

            CancelButton = btnCancel;

            ShareXResources.ApplyTheme(this, true);
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Progress<(long Downloaded, long Total)> progress = new Progress<(long Downloaded, long Total)>(ShowProgress);

            try
            {
                FFmpegPath = await UplaFFmpeg.DownloadAsync(progress, cancellation.Token);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex);

                if (!IsDisposed)
                {
                    string message = ex is InvalidDataException ? ex.Message : string.Format(UplaStrings.FFmpegDownloadFailed, ex.Message);
                    MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.Abort;
                }

                return;
            }

            if (!IsDisposed)
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void ShowProgress((long Downloaded, long Total) value)
        {
            if (IsDisposed || value.Total <= 0)
            {
                return;
            }

            pbProgress.Value = (int)Math.Min(pbProgress.Maximum, value.Downloaded * pbProgress.Maximum / value.Total);
            lblProgress.Text = value.Downloaded >= value.Total ? UplaStrings.FFmpegDownloadInstalling :
                string.Format(UplaStrings.FFmpegDownloadProgress, Math.Round(value.Downloaded / 1048576d, 1), Math.Round(value.Total / 1048576d, 1));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Closing (Cancel, Esc, X) stops a running download.
            cancellation.Cancel();

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cancellation.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
