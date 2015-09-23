﻿// <copyright file="IClientSync.cs" company="Stormpath, Inc.">
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

using Stormpath.SDK.Impl.DataStore;
using Stormpath.SDK.Impl.Tenant;
using Stormpath.SDK.Tenant;

namespace Stormpath.SDK.Impl.Client
{
    internal interface IClientSync : ITenantActionsSync, IDataStoreSync
    {
        /// <summary>
        /// Synchronously gets the sole <see cref="ITenant"/> associated to this <see cref="IClient"/>.
        /// </summary>
        /// <returns>The <see cref="ITenant"/> associated to this client.</returns>
        ITenant GetCurrentTenant();
    }
}