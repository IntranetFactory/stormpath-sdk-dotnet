﻿// <copyright file="ApplicationStatus.cs" company="Stormpath, Inc.">
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
using Stormpath.SDK.Shared;

namespace Stormpath.SDK.Application
{
    /// <summary>
    /// Represents the states an <see cref="IApplication">Application</see> may be in.
    /// </summary>
    public sealed class ApplicationStatus : AbstractEnumProperty
    {
        /// <summary>
        /// Accounts can log into this application.
        /// </summary>
        public static ApplicationStatus Enabled = new ApplicationStatus("ENABLED");

        /// <summary>
        /// Accounts are prevented from logging into this application.
        /// </summary>
        public static ApplicationStatus Disabled = new ApplicationStatus("DISABLED");

        /// <summary>
        /// Creates a new <see cref="ApplicationStatus"/> instance.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public ApplicationStatus(string value)
            : base(value)
        {
        }

        /// <summary>
        /// Parses a string to an <see cref="ApplicationStatus">Application Status</see>.
        /// </summary>
        /// <param name="status">A string containing "enabled" or "disabled" (matching is case-insensitive).</param>
        /// <returns>The <see cref="ApplicationStatus">Application Status</see> with the specified name.</returns>
        [Obsolete("Use constructor")]
        public static ApplicationStatus Parse(string status)
        {
            switch (status.ToUpper())
            {
                case "ENABLED": return Enabled;
                case "DISABLED": return Disabled;
                default:
                    throw new Exception($"Could not parse application status value '{status.ToUpper()}'");
            }
        }
    }
}