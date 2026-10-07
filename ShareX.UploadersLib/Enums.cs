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

using System.ComponentModel;

namespace ShareX.UploadersLib
{
    // upla.com.tr: upla.com.tr (Chevereto) is the only upload destination. Images can also be sent through the
    // file uploader, which is upla.com.tr as well; text is sent to upla.com.tr as a file, which it rejects.

    [Description("Image uploaders"), DefaultValue(Chevereto)]
    public enum ImageDestination
    {
        [Description("upla.com.tr")]
        Chevereto,
        FileUploader // Localized
    }

    [Description("Text uploaders"), DefaultValue(FileUploader)]
    public enum TextDestination
    {
        FileUploader // Localized
    }

    [Description("File uploaders"), DefaultValue(Chevereto)]
    public enum FileDestination
    {
        [Description("upla.com.tr")]
        Chevereto
    }

    [Description("URL sharing services"), DefaultValue(Facebook)]
    public enum URLSharingServices
    {
        [Description("Facebook")]
        Facebook,
        [Description("Reddit")]
        Reddit,
        [Description("Pinterest")]
        Pinterest,
        [Description("Tumblr")]
        Tumblr,
        [Description("LinkedIn")]
        LinkedIn,
        [Description("VK")]
        VK
    }

    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    public enum LinkFormatEnum
    {
        [Description("Full URL")]
        URL,
        [Description("Full Image for Forums")]
        ForumImage,
        [Description("Full Image as HTML")]
        HTMLImage,
        [Description("Full Image for Wiki")]
        WikiImage,
        [Description("Shortened URL")]
        ShortenedURL,
        [Description("Linked Thumbnail for Forums")]
        ForumLinkedImage,
        [Description("Linked Thumbnail as HTML")]
        HTMLLinkedImage,
        [Description("Linked Thumbnail for Wiki")]
        WikiLinkedImage,
        [Description("Thumbnail")]
        ThumbnailURL,
        [Description("Local File path")]
        LocalFilePath,
        [Description("Local File path as URI")]
        LocalFilePathUri
    }
}
