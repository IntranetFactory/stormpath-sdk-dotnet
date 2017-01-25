﻿// <copyright file="IProvider.cs" company="Stormpath, Inc.">
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

namespace Stormpath.SDK.Provider
{
    /// <summary>
    /// A provider resource holds specific information needed to work with Provider-based Directories (e.g, Google and Facebook).
    /// </summary>
    public interface IProvider : IResource, IAuditable
    {
        /// <summary>
        /// Gets the ID of the Provider (e.g. "facebook" or "google").
        /// </summary>
        /// <remarks>For generic OAuth 2.0 providers, this can be any unique string.</remarks>
        /// <value>The ID of the Provider.</value>
        string ProviderId { get; }

        /// <summary>
        /// Gets the type of the Provider (e.g. "facebook" or "google").
        /// </summary>
        /// <remarks>For generic OAuth 2.0 providers, this will always be "oauth2".</remarks>
        /// <value>The type of the Provider.</value>
        string ProviderType { get; }
    }
}
