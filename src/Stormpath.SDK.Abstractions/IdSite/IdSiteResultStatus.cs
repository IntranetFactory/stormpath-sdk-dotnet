﻿// <copyright file="IdSiteResultStatus.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using Stormpath.SDK.Resource;

namespace Stormpath.SDK.IdSite
{
    /// <summary>
    /// Represents the possible results of an ID Site invocation.
    /// </summary>
    public sealed class IdSiteResultStatus : AbstractEnumProperty
    {
        /// <summary>
        /// A new account registration via ID Site.
        /// </summary>
        public static IdSiteResultStatus Registered = new IdSiteResultStatus("REGISTERED");

        /// <summary>
        /// A successful account authentication via ID Site.
        /// </summary>
        public static IdSiteResultStatus Authenticated = new IdSiteResultStatus("AUTHENTICATED");

        /// <summary>
        /// A logout action via ID Site.
        /// </summary>
        public static IdSiteResultStatus Logout = new IdSiteResultStatus("LOGOUT");

        /// <summary>
        /// Creates a new <see cref="IdSiteResultStatus"/> instance.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public IdSiteResultStatus(string value)
            : base(value)
        {
        }

        /// <summary>
        /// Parses a string to an <see cref="IdSiteResultStatus"/>.
        /// </summary>
        /// <param name="status">A string containing "registered", "authenticated", or "logout" (matching is case-insensitive).</param>
        /// <returns>The <see cref="IdSiteResultStatus"/> with the specified name.</returns>
        /// <exception cref="Exception">No match is found.</exception>
        [Obsolete("Use constructor")]
        public static IdSiteResultStatus Parse(string status)
        {
            status = status.ToUpper();

            switch (status)
            {
                case "REGISTERED":
                    return Registered;
                case "AUTHENTICATED":
                    return Authenticated;
                case "LOGOUT":
                    return Logout;
                default:
                    throw new Exception($"Could not parse status value '{status}'");
            }
        }
    }
}
