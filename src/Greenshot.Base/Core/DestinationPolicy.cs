/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2004-2026 Thomas Braun, Jens Klingen, Robin Krom
 * 
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Greenshot.Base.Core
{
    /// <summary>
    /// JDubb17WRX: central policy for the reduced destination surface exposed by this build.
    /// Keeping the allowlist here prevents removed destinations from being reintroduced through
    /// config, picker enumeration, or direct designation lookups elsewhere in the app.
    /// </summary>
    public static class DestinationPolicy
    {
        public const string EditorDesignation = "Editor";
        public const string OutlookDesignation = "Outlook";

        private static readonly ISet<string> AllowedDestinationDesignations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            EditorDesignation,
            nameof(WellKnownDestinations.FileDialog),
            nameof(WellKnownDestinations.Clipboard),
            nameof(WellKnownDestinations.Picker),
            OutlookDesignation
        };

        public static List<string> DefaultOutputDestinations => new List<string>
        {
            nameof(WellKnownDestinations.Picker)
        };

        public static bool IsAllowedDestination(string designation)
        {
            return !string.IsNullOrWhiteSpace(designation) && AllowedDestinationDesignations.Contains(designation);
        }

        public static List<string> SanitizeDestinations(IEnumerable<string> designations)
        {
            return designations?
                       .Where(IsAllowedDestination)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .ToList()
                   ?? new List<string>();
        }
    }
}
