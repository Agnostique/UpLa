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

using Newtonsoft.Json.Serialization;
using System;
using System.Text.RegularExpressions;

namespace ShareX.HelpersLib
{
    // UpLa 1.0 named its libraries "UpLa.<Lib>" instead of "ShareX.<Lib>", so its settings files contain "$type" values
    // such as "UpLa.ImageEffectsLib.Canvas, UpLa.ImageEffectsLib". Without mapping them the whole file fails to load.
    public class UplaLegacySerializationBinder : ISerializationBinder
    {
        // "UpLa." at the start of a type or assembly name, including the ones inside generic type arguments. The main
        // assembly is named just "UpLa" and must not be mapped.
        private static readonly Regex LegacyNameRegex = new Regex(@"(?<=^|\[|,\s*)UpLa\.", RegexOptions.CultureInvariant);

        private readonly ISerializationBinder defaultBinder = new DefaultSerializationBinder();

        public Type BindToType(string assemblyName, string typeName)
        {
            return defaultBinder.BindToType(MapLegacyName(assemblyName), MapLegacyName(typeName));
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
        }

        private static string MapLegacyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return LegacyNameRegex.Replace(name, "ShareX.");
        }
    }
}
