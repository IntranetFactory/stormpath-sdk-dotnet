﻿// <copyright file="AccountStatus.cs" company="Stormpath, Inc.">
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

namespace Stormpath.SDK.Account
{
    /// <summary>
    /// Represents the various states an <see cref="IAccount">Account</see> may be in.
    /// </summary>
    public sealed class AccountStatus : AbstractEnumProperty
    {
        /// <summary>
        /// A disabled account may not login to applications.
        /// </summary>
        public static AccountStatus Enabled = new AccountStatus("ENABLED");

        /// <summary>
        /// An enabled account may login to applications.
        /// </summary>
        public static AccountStatus Disabled = new AccountStatus("DISABLED");

        /// <summary>
        /// An unverified account is a disabled account that does not have a verified email address.
        /// </summary>
        public static AccountStatus Unverified = new AccountStatus("UNVERIFIED");

        /// <summary>
        /// Creates a new <see cref="AccountStatus"/> instance.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public AccountStatus(string value)
            : base(value)
        {
        }

        /// <summary>
        /// Parses a string to an <see cref="AccountStatus">Account Status</see>.
        /// </summary>
        /// <param name="status">A string containing "enabled", "disabled", or "unverified" (matching is case-insensitive).</param>
        /// <returns>The <see cref="AccountStatus">Account Status</see> with the specified name.</returns>
        /// <exception cref="Exception">No match is found.</exception>.
        [Obsolete("Use constructor")]
        public static AccountStatus Parse(string status)
        {
            switch (status.ToUpper())
            {
                case "ENABLED":
                    return Enabled;
                case "DISABLED":
                    return Disabled;
                case "UNVERIFIED":
                    return Unverified;
                default:
                    throw new Exception($"Could not parse account status value '{status.ToUpper()}'");
            }
        }
    }
}