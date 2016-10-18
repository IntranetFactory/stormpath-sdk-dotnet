﻿// <copyright file="DefaultAccount.Collections.cs" company="Stormpath, Inc.">
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
using Stormpath.SDK.Account;
using Stormpath.SDK.Api;
using Stormpath.SDK.Application;
using Stormpath.SDK.Group;
using Stormpath.SDK.Impl.Linq;
using Stormpath.SDK.Linq;
using Stormpath.SDK.Oauth;

namespace Stormpath.SDK.Impl.Account
{
    internal sealed partial class DefaultAccount
    {
        IAsyncQueryable<IApplication> IAccount.GetApplications()
            => new CollectionResourceQueryable<IApplication>(this.Applications.Href, this.GetInternalAsyncDataStore());

        IAsyncQueryable<IGroup> IAccount.GetGroups()
            => new CollectionResourceQueryable<IGroup>(this.Groups.Href, this.GetInternalAsyncDataStore());

        IAsyncQueryable<IGroupMembership> IAccount.GetGroupMemberships()
            => new CollectionResourceQueryable<IGroupMembership>(this.GroupMemberships.Href, this.GetInternalAsyncDataStore());

        IAsyncQueryable<IAccessToken> IAccount.GetAccessTokens()
            => new CollectionResourceQueryable<IAccessToken>(this.AccessTokens.Href, this.GetInternalAsyncDataStore());

        IAsyncQueryable<IRefreshToken> IAccount.GetRefreshTokens()
            => new CollectionResourceQueryable<IRefreshToken>(this.RefreshTokens.Href, this.GetInternalAsyncDataStore());

        IAsyncQueryable<IApiKey> IAccount.GetApiKeys()
            => new CollectionResourceQueryable<IApiKey>(this.ApiKeys.Href, this.GetInternalAsyncDataStore());

        public IFactorCollection Factors
            => new DefaultFactorCollection(GetLinkProperty(FactorsPropertyName).Href, GetInternalAsyncDataStore());

        public IPhoneCollection Phones
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
