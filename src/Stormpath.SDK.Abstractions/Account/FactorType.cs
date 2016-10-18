﻿// <copyright file="FactorType.cs" company="Stormpath, Inc.">
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

using Stormpath.SDK.Resource;

namespace Stormpath.SDK.Account
{
    /// <summary>
    /// Represents the various <see cref="IFactor">Factor</see> types.
    /// </summary>
    public sealed class FactorType : AbstractEnumProperty
    {
        /// <summary>
        /// An SMS-based factor.
        /// </summary>
        public static FactorType Sms = new FactorType("SMS");

        /// <summary>
        /// A Google Authenticator/TOTP-based factor.
        /// </summary>
        public static FactorType GoogleAuthenticator = new FactorType("GOOGLE-AUTHENTICATOR");

        public FactorType(string value)
            : base(value)
        {
        }
    }
}