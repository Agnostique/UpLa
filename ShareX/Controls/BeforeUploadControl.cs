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
using ShareX.UploadersLib;
using System;
using System.Linq;
using System.Windows.Forms;

namespace ShareX
{
    public partial class BeforeUploadControl : UserControl
    {
        public delegate void EventHandler(string currentDestination);
        public event EventHandler InitCompleted;

        public BeforeUploadControl()
        {
            InitializeComponent();
        }

        public void Init(TaskInfo info)
        {
            switch (info.DataType)
            {
                case EDataType.Image:
                    InitCapture(info.TaskSettings);
                    break;
                case EDataType.Text:
                    Helpers.GetEnums<TextDestination>().ForEach(x =>
                    {
                        if (x != TextDestination.FileUploader)
                        {
                            AddDestination<TextDestination>((int)x, EDataType.Text, info.TaskSettings);
                        }
                    });

                    Helpers.GetEnums<FileDestination>().ForEach(x =>
                    {
                        AddDestination<FileDestination>((int)x, EDataType.Text, info.TaskSettings);
                    });

                    flp.Controls.OfType<RadioButton>().ForEach(x =>
                    {
                        if (info.TaskSettings.TextDestination != TextDestination.FileUploader)
                        {
                            x.Checked = x.Tag is TextDestination textDestination && textDestination == info.TaskSettings.TextDestination;
                        }
                        else
                        {
                            x.Checked = x.Tag is FileDestination fileDestination && fileDestination == info.TaskSettings.TextFileDestination;
                        }
                    });
                    break;
                case EDataType.File:
                    Helpers.GetEnums<FileDestination>().ForEach(x =>
                    {
                        AddDestination<FileDestination>((int)x, EDataType.File, info.TaskSettings);
                    });

                    flp.Controls.OfType<RadioButton>().ForEach(x =>
                    {
                        x.Checked = x.Tag is FileDestination fileDestination && fileDestination == info.TaskSettings.FileDestination;
                    });
                    break;
            }

            OnInitCompleted();
        }

        public void InitCapture(TaskSettings taskSettings)
        {
            Helpers.GetEnums<ImageDestination>().ForEach(x =>
            {
                if (x != ImageDestination.FileUploader)
                {
                    AddDestination<ImageDestination>((int)x, EDataType.Image, taskSettings);
                }
            });

            Helpers.GetEnums<FileDestination>().ForEach(x =>
            {
                AddDestination<FileDestination>((int)x, EDataType.File, taskSettings);
            });

            flp.Controls.OfType<RadioButton>().ForEach(x =>
            {
                if (taskSettings.ImageDestination != ImageDestination.FileUploader)
                {
                    x.Checked = x.Tag is ImageDestination imageDestination && imageDestination == taskSettings.ImageDestination;
                }
                else
                {
                    x.Checked = x.Tag is FileDestination fileDestination && fileDestination == taskSettings.ImageFileDestination;
                }
            });
        }

        private void OnInitCompleted()
        {
            if (InitCompleted != null)
            {
                RadioButton rbDestination = flp.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked);
                string currentDestination = "";
                if (rbDestination != null)
                {
                    currentDestination = rbDestination.Text;
                }
                InitCompleted(currentDestination);
            }
        }

        private void AddDestination<T>(int index, EDataType dataType, TaskSettings taskSettings)
        {
            Enum destination = (Enum)Enum.ToObject(typeof(T), index);

            if (UploadersConfigValidator.Validate<T>(index, Program.UploadersConfig))
            {
                RadioButton rb = new RadioButton() { AutoSize = true };

                rb.Text = destination.GetLocalizedDescription();
                rb.Tag = destination;
                rb.CheckedChanged += (sender, e) => SetDestinations(rb.Checked, dataType, rb.Tag, taskSettings);

                flp.Controls.Add(rb);
            }
        }

        private void SetDestinations(bool isActive, EDataType dataType, object destination, TaskSettings taskSettings)
        {
            if (!isActive) return;

            switch (dataType)
            {
                case EDataType.Image:
                    if (destination is ImageDestination imageDestination)
                    {
                        taskSettings.ImageDestination = imageDestination;
                    }
                    else if (destination is FileDestination imageFileDestination)
                    {
                        taskSettings.ImageDestination = ImageDestination.FileUploader;
                        taskSettings.ImageFileDestination = imageFileDestination;
                    }
                    break;
                case EDataType.Text:
                    if (destination is TextDestination textDestination)
                    {
                        taskSettings.TextDestination = textDestination;
                    }
                    else if (destination is FileDestination textFileDestination)
                    {
                        taskSettings.TextDestination = TextDestination.FileUploader;
                        taskSettings.TextFileDestination = textFileDestination;
                    }
                    break;
                case EDataType.File:
                    if (destination is FileDestination fileDestination)
                    {
                        taskSettings.ImageDestination = ImageDestination.FileUploader;
                        taskSettings.TextDestination = TextDestination.FileUploader;
                        taskSettings.ImageFileDestination = taskSettings.TextFileDestination = taskSettings.FileDestination = fileDestination;
                    }
                    break;
            }
        }

    }
}