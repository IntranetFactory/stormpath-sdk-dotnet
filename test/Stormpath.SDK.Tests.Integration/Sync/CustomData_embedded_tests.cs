﻿// <copyright file="CustomData_embedded_tests.cs" company="Stormpath, Inc.">
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

using System.Threading;
using Shouldly;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Sync;
using Stormpath.SDK.Tests.Common;
using Stormpath.SDK.Tests.Common.Integration;
using Stormpath.SDK.Tests.Common.RandomData;
using Xunit;

namespace Stormpath.SDK.Tests.Integration.Sync
{
    [Collection(nameof(IntegrationTestCollection))]
    public class CustomData_embedded_tests
    {
        private readonly TestFixture fixture;

        public CustomData_embedded_tests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        private IAccount CreateRandomAccountInstance(IClient client)
        {
            var accountObject = client.Instantiate<IAccount>();
            accountObject.SetEmail(new RandomEmail("testmail.stormpath.com"));
            accountObject.SetGivenName("Test");
            accountObject.SetSurname("Testerman");
            accountObject.SetPassword(new RandomPassword(12));

            return accountObject;
        }

        private IAccount SaveAccount(IClient client, IAccount instance)
        {
            var app = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);
            var created = app.CreateAccount(instance, x => x.RegistrationWorkflowEnabled = false);
            this.fixture.CreatedAccountHrefs.Add(created.Href);

            return created;
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Creating_new_account_with_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            var newAccount = this.CreateRandomAccountInstance(client);
            newAccount.CustomData.Put("status", 1337);
            newAccount.CustomData.Put("isAwesome", true);

            var created = this.SaveAccount(client, newAccount);
            var customData = created.GetCustomData();

            customData["status"].ShouldBe(1337);
            customData["isAwesome"].ShouldBe(true);

            created.Delete().ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(created.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Creating_new_application_with_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            var newApp = client.Instantiate<IApplication>();
            newApp.SetName(".NET IT App with CustomData Test " + RandomString.Create());
            newApp.CustomData.Put("isCool", true);
            newApp.CustomData.Put("my-custom-data", 1234);

            var created = client.CreateApplication(newApp, options => options.CreateDirectory = false);
            this.fixture.CreatedApplicationHrefs.Add(created.Href);
            var customData = created.GetCustomData();

            customData["isCool"].ShouldBe(true);
            customData["my-custom-data"].ShouldBe(1234);

            created.Delete().ShouldBeTrue();
            this.fixture.CreatedApplicationHrefs.Remove(created.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Editing_embedded_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            var newAccount = this.CreateRandomAccountInstance(client);
            newAccount.CustomData.Put("status", 1337);
            newAccount.CustomData.Put("isAwesome", true);

            var created = this.SaveAccount(client, newAccount);
            created.CustomData.Remove("isAwesome");
            created.CustomData.Put("phrase", "testing is neet");
            created.Save();

            Thread.Sleep(Delay.UpdatePropogation);

            var customData = created.GetCustomData();
            customData["status"].ShouldBe(1337);
            customData["isAwesome"].ShouldBe(null);
            customData["phrase"].ShouldBe("testing is neet");

            created.Delete().ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(created.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Clearing_embedded_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            var newAccount = this.CreateRandomAccountInstance(client);
            newAccount.CustomData.Put("foo", "bar");
            newAccount.CustomData.Put("admin", true);

            var created = this.SaveAccount(client, newAccount);
            var customData = created.GetCustomData();
            customData.IsEmptyOrDefault().ShouldBeFalse();

            created.CustomData.Clear();
            created.Save();

            Thread.Sleep(Delay.UpdatePropogation);

            customData = created.GetCustomData();
            customData.IsEmptyOrDefault().ShouldBeTrue();

            created.Delete().ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(created.Href);
        }
    }
}
