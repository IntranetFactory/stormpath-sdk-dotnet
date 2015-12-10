﻿// <copyright file="DefaultOrganization.CreationActionsSync.cs" company="Stormpath, Inc.">
// Copyright (c) 2015 Stormpath, Inc.
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
using Stormpath.SDK.Group;
using Stormpath.SDK.Impl.Account;
using Stormpath.SDK.Impl.Group;

namespace Stormpath.SDK.Impl.Organization
{
    internal sealed partial class DefaultOrganization
    {
        IAccount IAccountCreationActionsSync.CreateAccount(IAccount account, Action<AccountCreationOptionsBuilder> creationOptionsAction)
            => AccountCreationActionsShared.CreateAccount(this.GetInternalSyncDataStore(), this.Accounts.Href, account, creationOptionsAction);

        IAccount IAccountCreationActionsSync.CreateAccount(IAccount account, IAccountCreationOptions creationOptions)
             => AccountCreationActionsShared.CreateAccount(this.GetInternalSyncDataStore(), this.Accounts.Href, account, creationOptions);

        IAccount IAccountCreationActionsSync.CreateAccount(IAccount account)
             => AccountCreationActionsShared.CreateAccount(this.GetInternalSyncDataStore(), this.Accounts.Href, account);

        IAccount IAccountCreationActionsSync.CreateAccount(string givenName, string surname, string email, string password, object customData)
            => AccountCreationActionsShared.CreateAccount(this.GetInternalSyncDataStore(), this.Accounts.Href, givenName, surname, email, password, customData);

        IGroup IGroupCreationActionsSync.CreateGroup(IGroup group)
            => GroupCreationActionsShared.CreateGroup(this.GetInternalSyncDataStore(), this.Groups.Href, group);

        IGroup IGroupCreationActionsSync.CreateGroup(IGroup group, Action<GroupCreationOptionsBuilder> creationOptionsAction)
            => GroupCreationActionsShared.CreateGroup(this.GetInternalSyncDataStore(), this.Groups.Href, group, creationOptionsAction);

        IGroup IGroupCreationActionsSync.CreateGroup(IGroup group, IGroupCreationOptions creationOptions)
            => GroupCreationActionsShared.CreateGroup(this.GetInternalSyncDataStore(), this.Groups.Href, group, creationOptions);

        IGroup IGroupCreationActionsSync.CreateGroup(string name, string description)
            => GroupCreationActionsShared.CreateGroup(this.GetInternalSyncDataStore(), this.Groups.Href, name, description);
    }
}
