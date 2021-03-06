﻿// <copyright file="Error_tests.cs" company="Stormpath, Inc.">
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

using System.Threading.Tasks;
using Shouldly;
using Stormpath.SDK.Account;
using Stormpath.SDK.Error;
using Stormpath.SDK.Tests.Common.Integration;
using Xunit;

namespace Stormpath.SDK.Tests.Integration.Async
{
    public class Error_tests
    {
        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task When_resource_does_not_exist(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            try
            {
                var bad = await client.GetResourceAsync<IAccount>(tenant.Href + "/foobar");
            }
            catch (ResourceException rex)
            {
                rex.Code.ShouldBe(404);
                rex.DeveloperMessage.ShouldBe("The requested resource does not exist.");
                rex.Message.ShouldNotBe(null);
                rex.MoreInfo.ShouldNotBe(null);
                rex.HttpStatus.ShouldBe(404);
            }
        }
    }
}