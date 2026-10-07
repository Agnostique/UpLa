#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2021 ShareX Team

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
namespace ShareX.UploadersLib
{
    partial class UploadersConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UploadersConfigForm));
            ttHelpTip = new System.Windows.Forms.ToolTip(components);
            btnCopyShowFiles = new System.Windows.Forms.Button();
            tpImageUploaders = new System.Windows.Forms.TabPage();
            tcImageUploaders = new System.Windows.Forms.TabControl();
            tpChevereto = new System.Windows.Forms.TabPage();
            lblCheveretoUploadURLExample = new System.Windows.Forms.Label();
            cbCheveretoDirectURL = new System.Windows.Forms.CheckBox();
            lblCheveretoUploadURL = new System.Windows.Forms.Label();
            txtCheveretoUploadURL = new System.Windows.Forms.TextBox();
            txtCheveretoAPIKey = new System.Windows.Forms.TextBox();
            lblCheveretoAPIKey = new System.Windows.Forms.Label();
            tcUploaders = new System.Windows.Forms.TabControl();
            tttvMain = new ShareX.HelpersLib.TabToTreeView();
            tpImageUploaders.SuspendLayout();
            tcImageUploaders.SuspendLayout();
            tpChevereto.SuspendLayout();
            tcUploaders.SuspendLayout();
            SuspendLayout();
            // 
            // ttHelpTip
            // 
            ttHelpTip.AutomaticDelay = 0;
            ttHelpTip.AutoPopDelay = 30000;
            ttHelpTip.BackColor = System.Drawing.SystemColors.Window;
            ttHelpTip.InitialDelay = 500;
            ttHelpTip.ReshowDelay = 100;
            ttHelpTip.UseAnimation = false;
            ttHelpTip.UseFading = false;
            // 
            // btnCopyShowFiles
            // 
            resources.ApplyResources(btnCopyShowFiles, "btnCopyShowFiles");
            btnCopyShowFiles.Name = "btnCopyShowFiles";
            // 
            // tpImageUploaders
            // 
            tpImageUploaders.BackColor = System.Drawing.SystemColors.Window;
            tpImageUploaders.Controls.Add(tcImageUploaders);
            resources.ApplyResources(tpImageUploaders, "tpImageUploaders");
            tpImageUploaders.Name = "tpImageUploaders";
            // 
            // tcImageUploaders
            // 
            tcImageUploaders.Controls.Add(tpChevereto);
            resources.ApplyResources(tcImageUploaders, "tcImageUploaders");
            tcImageUploaders.Name = "tcImageUploaders";
            tcImageUploaders.SelectedIndex = 0;
            // 
            // tpChevereto
            // 
            tpChevereto.BackColor = System.Drawing.SystemColors.Window;
            tpChevereto.Controls.Add(lblCheveretoUploadURLExample);
            tpChevereto.Controls.Add(cbCheveretoDirectURL);
            tpChevereto.Controls.Add(lblCheveretoUploadURL);
            tpChevereto.Controls.Add(txtCheveretoUploadURL);
            tpChevereto.Controls.Add(txtCheveretoAPIKey);
            tpChevereto.Controls.Add(lblCheveretoAPIKey);
            resources.ApplyResources(tpChevereto, "tpChevereto");
            tpChevereto.Name = "tpChevereto";
            // 
            // lblCheveretoUploadURLExample
            // 
            resources.ApplyResources(lblCheveretoUploadURLExample, "lblCheveretoUploadURLExample");
            lblCheveretoUploadURLExample.Name = "lblCheveretoUploadURLExample";
            // 
            // cbCheveretoDirectURL
            // 
            resources.ApplyResources(cbCheveretoDirectURL, "cbCheveretoDirectURL");
            cbCheveretoDirectURL.Name = "cbCheveretoDirectURL";
            cbCheveretoDirectURL.UseVisualStyleBackColor = true;
            cbCheveretoDirectURL.CheckedChanged += cbCheveretoDirectURL_CheckedChanged;
            // 
            // lblCheveretoUploadURL
            // 
            resources.ApplyResources(lblCheveretoUploadURL, "lblCheveretoUploadURL");
            lblCheveretoUploadURL.Name = "lblCheveretoUploadURL";
            // 
            // txtCheveretoUploadURL
            // 
            resources.ApplyResources(txtCheveretoUploadURL, "txtCheveretoUploadURL");
            txtCheveretoUploadURL.Name = "txtCheveretoUploadURL";
            txtCheveretoUploadURL.TextChanged += txtCheveretoWebsite_TextChanged;
            // 
            // txtCheveretoAPIKey
            // 
            resources.ApplyResources(txtCheveretoAPIKey, "txtCheveretoAPIKey");
            txtCheveretoAPIKey.Name = "txtCheveretoAPIKey";
            txtCheveretoAPIKey.UseSystemPasswordChar = true;
            txtCheveretoAPIKey.TextChanged += txtCheveretoAPIKey_TextChanged;
            // 
            // lblCheveretoAPIKey
            // 
            resources.ApplyResources(lblCheveretoAPIKey, "lblCheveretoAPIKey");
            lblCheveretoAPIKey.Name = "lblCheveretoAPIKey";
            // 
            // tcUploaders
            // 
            tcUploaders.Controls.Add(tpImageUploaders);
            resources.ApplyResources(tcUploaders, "tcUploaders");
            tcUploaders.Name = "tcUploaders";
            tcUploaders.SelectedIndex = 0;
            // 
            // tttvMain
            // 
            tttvMain.AutoSelectChild = true;
            resources.ApplyResources(tttvMain, "tttvMain");
            tttvMain.ImageList = null;
            tttvMain.LeftPanelBackColor = System.Drawing.SystemColors.Window;
            tttvMain.MainTabControl = null;
            tttvMain.Name = "tttvMain";
            tttvMain.SeparatorColor = System.Drawing.SystemColors.ControlDark;
            tttvMain.TreeViewFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
            tttvMain.TreeViewSize = 230;
            // 
            // UploadersConfigForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.SystemColors.Window;
            Controls.Add(tcUploaders);
            Controls.Add(tttvMain);
            Name = "UploadersConfigForm";
            Shown += UploadersConfigForm_Shown;
            Resize += UploadersConfigForm_Resize;
            tpImageUploaders.ResumeLayout(false);
            tcImageUploaders.ResumeLayout(false);
            tpChevereto.ResumeLayout(false);
            tpChevereto.PerformLayout();
            tcUploaders.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion Windows Form Designer generated code

        private System.Windows.Forms.ToolTip ttHelpTip;
        private System.Windows.Forms.Button btnCopyShowFiles;
        private System.Windows.Forms.TabPage tpImageUploaders;
        private System.Windows.Forms.TabControl tcImageUploaders;
        private System.Windows.Forms.TabControl tcUploaders;
        private System.Windows.Forms.Label lblCheveretoUploadURL;
        private System.Windows.Forms.TextBox txtCheveretoUploadURL;
        private System.Windows.Forms.TextBox txtCheveretoAPIKey;
        private System.Windows.Forms.Label lblCheveretoAPIKey;
        private System.Windows.Forms.CheckBox cbCheveretoDirectURL;
        private System.Windows.Forms.Label lblCheveretoUploadURLExample;
        internal System.Windows.Forms.TabPage tpChevereto;
        private HelpersLib.TabToTreeView tttvMain;
    }
}