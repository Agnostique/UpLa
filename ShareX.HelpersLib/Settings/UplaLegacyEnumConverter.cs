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
using Newtonsoft.Json.Converters;
using System;

namespace ShareX.HelpersLib
{
    // Reads enum values that older settings files contain:
    // - UpLa 1.0 named some values after itself (e.g. the "ExitUpLa" hotkey); they are read as their ShareX names.
    // - Flags removed in upla.com.tr builds (e.g. the AnalyzeImage after capture task) are dropped. StringEnumConverter
    //   rejects such a value as a whole, and the setting then fell back to its default, which for example turned
    //   "upload after capture" back on for someone who had switched it off.
    // Anything else that cannot be read still fails like before (SettingsBase uses the default value).
    public class UplaLegacyEnumConverter : StringEnumConverter
    {
        public override bool CanConvert(Type objectType)
        {
            Type type = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return type.IsEnum && type.Namespace != null && type.Namespace.StartsWith("ShareX", StringComparison.Ordinal);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Type enumType = Nullable.GetUnderlyingType(objectType) ?? objectType;

            if (reader.TokenType == JsonToken.String)
            {
                bool isFlags = enumType.IsDefined(typeof(FlagsAttribute), false);
                string[] names = ((string)reader.Value).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                ulong value = 0;
                int known = 0;

                foreach (string name in names)
                {
                    if (TryParseName(enumType, name, out ulong bits))
                    {
                        value |= bits;
                        known++;
                    }
                    else if (isFlags)
                    {
                        DebugHelper.WriteLine($"Unknown {enumType.Name} value ignored: {name}");
                    }
                }

                if (names.Length > 0 && (isFlags || known == names.Length))
                {
                    return Enum.ToObject(enumType, value);
                }
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        private static bool TryParseName(Type enumType, string name, out ulong bits)
        {
            if (Enum.TryParse(enumType, name, true, out object result) ||
                (name.Contains("UpLa", StringComparison.Ordinal) && Enum.TryParse(enumType, name.Replace("UpLa", "ShareX"), true, out result)))
            {
                object underlying = Convert.ChangeType(result, Enum.GetUnderlyingType(enumType));
                bits = underlying is ulong unsigned ? unsigned : unchecked((ulong)Convert.ToInt64(underlying));
                return true;
            }

            bits = 0;
            return false;
        }
    }
}
