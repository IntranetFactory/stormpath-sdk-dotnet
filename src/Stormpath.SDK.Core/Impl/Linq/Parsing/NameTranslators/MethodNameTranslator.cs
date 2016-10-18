﻿// <copyright file="MethodNameTranslator.cs" company="Stormpath, Inc.">
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

using System.Collections.Generic;

namespace Stormpath.SDK.Impl.Linq.Parsing.NameTranslators
{
    internal class MethodNameTranslator : AbstractNameTranslator
    {
        private static Dictionary<string, string> methodNameMap = new Dictionary<string, string>()
        {
            ["GetDirectory"] = "directory",
            ["GetTenant"] = "tenant",
            ["GetCustomData"] = "customData",
            ["GetProviderData"] = "providerData",
            ["GetDefaultAccountStore"] = "defaultAccountStoreMapping",
            ["GetDefaultGroupStore"] = "defaultGroupStoreMapping",
            ["GetAccountStore"] = "accountStore",
            ["GetProvider"] = "provider",
            ["GetAccount"] = "account",
            ["GetGroup"] = "group",
            ["GetApplication"] = "application",
            ["GetOrganization"] = "organization",
            ["GetOauthPolicy"] = "oAuthPolicy",

            ["GetGroups"] = "groups",
            ["GetGroupMemberships"] = "groupMemberships",
            ["GetAccountMemberships"] = "accountMemberships",
            ["GetAccounts"] = "accounts",
            ["GetAccountStoreMappings"] = "accountStoreMappings",
            ["GetApplications"] = "applications",
            ["GetDirectories"] = "directories",
            ["GetOrganizations"] = "organizations",
            ["GetFactors"] = "factors",
            ["GetPhones"] = "phones",
            ["GetChallenges"] = "challenges",

            // New syntax style (member access)
            ["Account"] = "account",
            ["MostRecentChallenge"] = "mostRecentChallenge",
        };

        public MethodNameTranslator()
            : base(methodNameMap)
        {
        }
    }
}
