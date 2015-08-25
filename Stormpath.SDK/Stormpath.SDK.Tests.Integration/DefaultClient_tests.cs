﻿// <copyright file="DefaultClient_tests.cs" company="Stormpath, Inc.">
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

using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Stormpath.SDK.Tests.Integration
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single class", Justification = "Reviewed.")]
    public class DefaultClient_tests
    {
        private static async Task Impl_Getting_current_tenant(IntegrationHarness harness)
        {
            var tenant = await harness.Client.GetCurrentTenantAsync();

            tenant.ShouldNotBe(null);
            tenant.Href.ShouldNotBe(null);
            tenant.Name.ShouldNotBe(null);

            // TODO - verify actual tenant data?
        }

        public class DefaultClient_Basic_tests : BasicAuth_integration_tests
        {
            [Fact]
            public async Task Getting_current_tenant() => await Impl_Getting_current_tenant(harness);
        }

        public class DefaultClient_SAuthc1_tests : SAuthc1_integration_tests
        {
            [Fact]
            public async Task Getting_current_tenant() => await Impl_Getting_current_tenant(harness);
        }
    }
}