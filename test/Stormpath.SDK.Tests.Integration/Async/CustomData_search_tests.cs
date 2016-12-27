﻿// <copyright file="CustomData_search_tests.cs" company="Stormpath, Inc.">
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
using System.Threading.Tasks;
using Polly;
using Shouldly;
using Stormpath.SDK.Account;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Group;
using Stormpath.SDK.Tests.Common;
using Stormpath.SDK.Tests.Common.Integration;
using Stormpath.SDK.Tests.Common.RandomData;
using Xunit;

namespace Stormpath.SDK.Tests.Integration.Async
{
    [Collection(nameof(IntegrationTestCollection))]
    public class CustomData_search_tests : IClassFixture<CreateSingleDirectoryFixture>
    {
        private readonly TestFixture fixture;
        private readonly string directoryHref;

        public CustomData_search_tests(TestFixture fixture, CreateSingleDirectoryFixture directoryFixture)
        {
            this.fixture = fixture;
            this.directoryHref = directoryFixture.DirectoryHref;
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_by_string(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            // Create an account with some custom data
            var tk421 = client.Instantiate<IAccount>()
                .SetEmail(new RandomEmail("testmail.stormpath.com"))
                .SetPassword(new RandomPassword(12))
                .SetGivenName("TK")
                .SetSurname("421");
            tk421.CustomData["post"] = "Cell Block 1138";

            var directory = await client.GetDirectoryAsync(this.directoryHref);
            await directory.CreateAccountAsync(tk421);
            this.fixture.CreatedAccountHrefs.Add(tk421.Href);

#pragma warning disable CS0252 // Unintended reference comparison
            // Retry up to 3 times if CDS infrastructure isn't ready yet
            var result = await Policy.Handle<ShouldAssertException>()
                .WaitAndRetryAsync(Delay.CustomDataRetry)
                .ExecuteAndCaptureAsync(async () =>
                {
                    var foundAccount1 = await directory.GetAccounts()
                        .Where(account => account.CustomData["post"] == "Cell Block 1138").SingleOrDefaultAsync();

                    var foundAccount2 = await directory.GetAccounts()
                        .Where(account => (string)account.CustomData["post"] == "Cell Block 1138").SingleOrDefaultAsync();

                    var foundAccount3 = await directory.GetAccounts()
                        .Where(account => (account.CustomData["post"] as string).Equals("Cell Block 1138")).SingleOrDefaultAsync();

                    foundAccount1.ShouldNotBeNull();
                    foundAccount2.ShouldNotBeNull();
                    foundAccount3.ShouldNotBeNull();

                    foundAccount1.Href.ShouldBe(tk421.Href);
                    foundAccount2.Href.ShouldBe(tk421.Href);
                    foundAccount3.Href.ShouldBe(tk421.Href);
                });
#pragma warning restore CS0252

            // Cleanup
            (await tk421.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(tk421.Href);

            // Report
            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_by_partial_string(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            // Create an account with some custom data
            var tester = client.Instantiate<IAccount>()
                .SetEmail(new RandomEmail("testmail.stormpath.com"))
                .SetPassword(new RandomPassword(12))
                .SetGivenName("Test")
                .SetSurname("TestermanPartial");
            tester.CustomData["stuff"] = "foobar";

            var directory = await client.GetDirectoryAsync(this.directoryHref);
            await directory.CreateAccountAsync(tester);
            this.fixture.CreatedAccountHrefs.Add(tester.Href);

            // Retry up to 3 times if CDS infrastructure isn't ready yet
            var result = await Policy.Handle<ShouldAssertException>()
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(Delay.CustomDataRetry)
                .ExecuteAndCaptureAsync(async () =>
                {
                    var foundAccount1 = await directory.GetAccounts()
                        .Where(account => (account.CustomData["stuff"] as string).StartsWith("foo")).SingleOrDefaultAsync();

                    var foundAccount2 = await directory.GetAccounts()
                        .Where(account => (account.CustomData["stuff"] as string).EndsWith("bar")).SingleOrDefaultAsync();

                    foundAccount1.ShouldNotBeNull();
                    foundAccount2.ShouldNotBeNull();

                    foundAccount1.Href.ShouldBe(tester.Href);
                    foundAccount2.Href.ShouldBe(tester.Href);
                });

            // Cleanup
            (await tester.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(tester.Href);

            // Report
            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_by_date_range(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            // Create an account with some custom data
            var tester = client.Instantiate<IAccount>()
                .SetEmail(new RandomEmail("testmail.stormpath.com"))
                .SetPassword(new RandomPassword(12))
                .SetGivenName("Test")
                .SetSurname("TestermanDate");
            tester.CustomData["birthday"] = "1983-05-02T13:00:00Z"; // store as ISO 8601 date

            var directory = await client.GetDirectoryAsync(this.directoryHref);
            await directory.CreateAccountAsync(tester);
            this.fixture.CreatedAccountHrefs.Add(tester.Href);

            // Retry up to 3 times if CDS infrastructure isn't ready yet
            var result = await Policy.Handle<ShouldAssertException>()
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(Delay.CustomDataRetry)
                .ExecuteAndCaptureAsync(async () =>
                {
                    var foundAccount1 = await directory.GetAccounts()
                        .Where(account => (DateTimeOffset)account.CustomData["birthday"] < new DateTimeOffset(1990, 01, 01, 00, 00, 00, TimeSpan.Zero)).SingleOrDefaultAsync();

                    var foundAccount2 = await directory.GetAccounts()
                        .Where(account => (DateTimeOffset)account.CustomData["birthday"] > new DateTimeOffset(1983, 05, 01, 00, 00, 00, TimeSpan.Zero)).SingleOrDefaultAsync();

                    foundAccount1.ShouldNotBeNull();
                    foundAccount2.ShouldNotBeNull();

                    foundAccount1.Href.ShouldBe(tester.Href);
                    foundAccount2.Href.ShouldBe(tester.Href);
                });

            // Cleanup
            (await tester.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(tester.Href);

            // Report
            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_by_date_range_hack(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            // Create an account with some custom data
            var tester = client.Instantiate<IAccount>()
                .SetEmail(new RandomEmail("testmail.stormpath.com"))
                .SetPassword(new RandomPassword(12))
                .SetGivenName("Test")
                .SetSurname("TestermanDate");
            tester.CustomData["birthday"] = new DateTimeOffset(1982, 05, 02, 13, 00, 00, TimeSpan.Zero); // store as ISO 8601 date

            var directory = await client.GetDirectoryAsync(this.directoryHref);
            await directory.CreateAccountAsync(tester);
            this.fixture.CreatedAccountHrefs.Add(tester.Href);

            // Retry up to 3 times if CDS infrastructure isn't ready yet
            var result = await Policy.Handle<ShouldAssertException>()
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(Delay.CustomDataRetry)
                .ExecuteAndCaptureAsync(async () =>
                {
                    var foundAccount = await directory.GetAccounts()
                        .Where(account => (int)account.CustomData["birthday"] >= 1982 && (int)account.CustomData["birthday"] <= 1983).SingleOrDefaultAsync();

                    foundAccount.ShouldNotBeNull();

                    foundAccount.Href.ShouldBe(tester.Href);
                });

            // Cleanup
            (await tester.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(tester.Href);

            // Report
            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_by_int_range(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            // Create an account with some custom data
            var tester = client.Instantiate<IAccount>()
                .SetEmail(new RandomEmail("testmail.stormpath.com"))
                .SetPassword(new RandomPassword(12))
                .SetGivenName("Test")
                .SetSurname("TestermanDate");
            tester.CustomData["score"] = 75;

            var directory = await client.GetDirectoryAsync(this.directoryHref);
            await directory.CreateAccountAsync(tester);
            this.fixture.CreatedAccountHrefs.Add(tester.Href);

            // Retry up to 3 times if CDS infrastructure isn't ready yet
            var result = await Policy.Handle<ShouldAssertException>()
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(Delay.CustomDataRetry)
                .ExecuteAndCaptureAsync(async () =>
                {
                    var foundAccount1 = await directory.GetAccounts()
                        .Where(account => (int)account.CustomData["score"] < 100).SingleOrDefaultAsync();

                    var foundAccount2 = await directory.GetAccounts()
                        .Where(account => (int)account.CustomData["score"] > 50 && (int)account.CustomData["score"] <= 75).SingleOrDefaultAsync();

                    foundAccount1.ShouldNotBeNull();
                    foundAccount2.ShouldNotBeNull();

                    foundAccount1.Href.ShouldBe(tester.Href);
                    foundAccount2.Href.ShouldBe(tester.Href);
                });

            // Cleanup
            (await tester.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(tester.Href);

            // Report
            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_by_float_range(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            // Create an account with some custom data
            var tester = client.Instantiate<IAccount>()
                .SetEmail(new RandomEmail("testmail.stormpath.com"))
                .SetPassword(new RandomPassword(12))
                .SetGivenName("Test")
                .SetSurname("TestermanDate");
            tester.CustomData["stdDev"] = 2.02;

            var directory = await client.GetDirectoryAsync(this.directoryHref);
            await directory.CreateAccountAsync(tester);
            this.fixture.CreatedAccountHrefs.Add(tester.Href);

            // Retry up to 3 times if CDS infrastructure isn't ready yet
            var result = await Policy.Handle<ShouldAssertException>()
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(Delay.CustomDataRetry)
                .ExecuteAndCaptureAsync(async () =>
                {
                    var foundAccount1 = await directory.GetAccounts()
                        .Where(account => (float)account.CustomData["stdDev"] < 3.WithPlaces(5)).SingleOrDefaultAsync();

                    var foundAccount2 = await directory.GetAccounts()
                        .Where(account => (float)account.CustomData["stdDev"] > 0.01 && (int)account.CustomData["stdDev"] <= 2.02).SingleOrDefaultAsync();

                    foundAccount1.ShouldNotBeNull();
                    foundAccount2.ShouldNotBeNull();

                    foundAccount1.Href.ShouldBe(tester.Href);
                    foundAccount2.Href.ShouldBe(tester.Href);
                });

            // Cleanup
            (await tester.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(tester.Href);

            // Report
            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Ordering_by_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            // Create an account with some custom data
            var tester1 = client.Instantiate<IAccount>()
                .SetEmail(new RandomEmail("testmail.stormpath.com"))
                .SetPassword(new RandomPassword(12))
                .SetGivenName("Test")
                .SetSurname("TestermanOrdering1");
            tester1.CustomData["score"] = 10;

            // Create another
            var tester2 = client.Instantiate<IAccount>()
                .SetEmail(new RandomEmail("testmail.stormpath.com"))
                .SetPassword(new RandomPassword(12))
                .SetGivenName("Test")
                .SetSurname("TestermanOrdering2");
            tester2.CustomData["score"] = 50;

            var directory = await client.GetDirectoryAsync(this.directoryHref);

            await directory.CreateAccountAsync(tester1);
            this.fixture.CreatedAccountHrefs.Add(tester1.Href);
            await directory.CreateAccountAsync(tester2);
            this.fixture.CreatedAccountHrefs.Add(tester2.Href);

            // Retry up to 3 times if CDS infrastructure isn't ready yet
            var result = await Policy.Handle<ShouldAssertException>()
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(Delay.CustomDataRetry)
                .ExecuteAndCaptureAsync(async () =>
                {
                    var foundAccount1 = await directory.GetAccounts()
                        .OrderBy(x => x.CustomData["score"])
                        .FirstAsync();

                    var foundAccount2 = await directory.GetAccounts()
                        .OrderByDescending(x => x.CustomData["score"])
                        .FirstAsync();

                    foundAccount1.ShouldNotBeNull();
                    foundAccount2.ShouldNotBeNull();

                    foundAccount1.Href.ShouldBe(tester1.Href);
                    foundAccount2.Href.ShouldBe(tester2.Href);
                });

            // Cleanup
            (await tester1.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(tester1.Href);
            (await tester2.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(tester2.Href);

            // Report
            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_for_group_by_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            // Create an account with some custom data
            var testGroup = client.Instantiate<IGroup>()
                .SetName($"CDS Group {fixture.TestRunIdentifier}");
            testGroup.CustomData["on_duty"] = "TK421";

            var directory = await client.GetDirectoryAsync(this.directoryHref);
            await directory.CreateGroupAsync(testGroup);
            this.fixture.CreatedGroupHrefs.Add(testGroup.Href);

#pragma warning disable CS0252 // Unintended reference comparison
            // Retry up to 3 times if CDS infrastructure isn't ready yet
            var result = await Policy.Handle<ShouldAssertException>()
                .WaitAndRetryAsync(Delay.CustomDataRetry)
                .ExecuteAndCaptureAsync(async () =>
                {
                    var foundGroup = await directory.GetGroups()
                        .Where(g => g.CustomData["on_duty"] == "TK421").SingleOrDefaultAsync();

                    foundGroup.ShouldNotBeNull();
                    foundGroup.Href.ShouldBe(testGroup.Href);
                });
#pragma warning restore CS0252

            // Cleanup
            (await testGroup.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(testGroup.Href);

            // Report
            if (result.Outcome == OutcomeType.Failure)
            {
                throw result.FinalException;
            }
        }
    }
}
