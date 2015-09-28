﻿// <copyright file="IExtendableSync.cs" company="Stormpath, Inc.">
//      Copyright (c) 2015 Stormpath, Inc.
// </copyright>
// <remarks>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </remarks>

using Stormpath.SDK.CustomData;

namespace Stormpath.SDK.Impl.Resource
{
    /// <summary>
    /// Represents resources that can be deleted synchronously.
    /// </summary>
    internal interface IExtendableSync
    {
        /// <summary>
        /// Provides access to convenience methods that can manipulate this resource's custom data.
        /// </summary>
        /// <value>
        /// Access to convenience methods that can manipulate this resource's custom data.
        /// </value>
        IEmbeddedCustomData CustomData { get; }

        /// <summary>
        /// Synchronously gets the custom data associated with this resource.
        /// </summary>
        /// <returns>The <see cref="ICustomData"/> associated with this resource.</returns>
        /// <exception cref="SDK.Error.ResourceException">The custom data could not be loaded.</exception>
        ICustomData GetCustomData();
    }
}