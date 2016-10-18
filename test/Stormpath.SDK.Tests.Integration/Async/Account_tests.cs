﻿// <copyright file="Account_tests.cs" company="Stormpath, Inc.">
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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Shouldly;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Auth;
using Stormpath.SDK.Error;
using Stormpath.SDK.Tests.Common.Integration;
using Stormpath.SDK.Tests.Common.RandomData;
using Xunit;

namespace Stormpath.SDK.Tests.Integration.Async
{
    [Collection(nameof(IntegrationTestCollection))]
    public class Account_tests
    {
        private readonly TestFixture fixture;

        public Account_tests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_tenant_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            var accounts = await tenant.GetAccounts().ToListAsync();

            accounts.Any().ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var accounts = await application.GetAccounts().ToListAsync();

            // Verify data from IntegrationTestData
            accounts.Count.ShouldBeGreaterThanOrEqualTo(8);

            var luke = accounts.Where(x => x.GivenName == "Luke").Single();
            luke.FullName.ShouldBe("Luke Skywalker");
            luke.Email.ShouldBe("lskywalker@tattooine.rim");
            luke.Username.ShouldStartWith("sonofthesuns");
            luke.Status.ShouldBe(AccountStatus.Enabled);

            var vader = accounts.Where(x => x.Surname == "Vader").Single();
            vader.FullName.ShouldBe("Darth Vader");
            vader.Email.ShouldStartWith("vader@galacticempire.co");
            vader.Username.ShouldStartWith("lordvader");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_account_provider_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var luke = await application
                .GetAccounts()
                .Where(x => x.Email.StartsWith("lskywalker"))
                .SingleAsync();

            var providerData = await luke.GetProviderDataAsync();
            providerData.Href.ShouldNotBeNullOrEmpty();
            providerData.ProviderId.ShouldBe("stormpath");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Updating_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var leia = await application
                .GetAccounts()
                .Where(a => a.Email == "leia.organa@alderaan.core")
                .SingleAsync();

            leia.SetMiddleName("Organa");
            leia.SetSurname("Solo");
            var saveResult = await leia.SaveAsync();

            // In 8 ABY of course
            saveResult.FullName.ShouldBe("Leia Organa Solo");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Saving_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var chewie = await application
                .GetAccounts()
                .Where(a => a.Email == "chewie@kashyyyk.rim")
                .SingleAsync();

            chewie.SetUsername($"rwaaargh-{this.fixture.TestRunIdentifier}");
            await chewie.SaveAsync(response => response.Expand(x => x.GetCustomData()));
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_account_applications(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            var luke = await client.GetAccountAsync(this.fixture.PrimaryAccountHref);

            var apps = await luke.GetApplications().ToListAsync();
            apps.Where(x => x.Href == this.fixture.PrimaryApplicationHref).Any().ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_account_directory(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var luke = await application
                .GetAccounts()
                .Filter("Luke")
                .SingleAsync();

            // Verify data from IntegrationTestData
            var directoryHref = (await luke.GetDirectoryAsync()).Href;
            directoryHref.ShouldBe(this.fixture.PrimaryDirectoryHref);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_account_tenant(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var leia = await application
                .GetAccounts()
                .Filter("Leia")
                .SingleAsync();

            // Verify data from IntegrationTestData
            var tenantHref = (await leia.GetTenantAsync()).Href;
            tenantHref.ShouldBe(this.fixture.TenantHref);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_by_email(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var coreCitizens = await application
                .GetAccounts()
                .Where(acct => acct.Email.EndsWith(".core"))
                .ToListAsync();

            // Verify data from IntegrationTestData
            coreCitizens.Count.ShouldBe(2);

            var han = coreCitizens.Where(x => x.GivenName == "Han").Single();
            han.Email.ShouldBe("han.solo@corellia.core");
            han.Username.ShouldStartWith("cptsolo");

            var leia = coreCitizens.Where(x => x.GivenName == "Leia").Single();
            leia.Email.ShouldStartWith("leia.organa@alderaan.core");
            leia.Username.ShouldStartWith("princessleia");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_by_firstname(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var chewie = await application
                .GetAccounts()
                .Where(a => a.GivenName == "Chewbacca")
                .SingleAsync();

            // Verify data from IntegrationTestData
            chewie.FullName.ShouldBe("Chewbacca the Wookiee");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_by_lastname(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var palpatine = await application
                .GetAccounts()
                .Where(a => a.Surname == "Palpatine")
                .SingleAsync();

            // Verify data from IntegrationTestData
            palpatine.FullName.ShouldBe("Emperor Palpatine");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_by_middle_name(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var chewie = await application
                .GetAccounts()
                .Where(a => a.MiddleName == "the")
                .SingleAsync();

            // Verify data from IntegrationTestData
            chewie.FullName.ShouldBe("Chewbacca the Wookiee");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_by_username(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var vader = await application
                .GetAccounts()
                .Where(a => a.Username.Equals($"lordvader-{this.fixture.TestRunIdentifier}"))
                .SingleAsync();

            // Verify data from IntegrationTestData
            vader.Email.ShouldBe("vader@galacticempire.co");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_by_status(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var tarkin = await application
                .GetAccounts()
                .Where(x => x.Status == AccountStatus.Disabled)
                .SingleAsync();

            // Verify data from IntegrationTestData
            tarkin.FullName.ShouldBe("Wilhuff Tarkin");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_by_creation_date(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var longTimeAgo = await application
                .GetAccounts()
                .Where(x => x.CreatedAt < DateTime.Now.Subtract(TimeSpan.FromHours(1)))
                .ToListAsync();
            longTimeAgo.ShouldBeEmpty();

            var createdRecently = await application
                .GetAccounts()
                .Where(x => x.CreatedAt >= DateTime.Now.Subtract(TimeSpan.FromHours(1)))
                .ToListAsync();
            createdRecently.ShouldNotBeEmpty();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_by_creation_date_within_shorthand(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var firstAccount = await application.GetAccounts().FirstAsync();
            var created = firstAccount.CreatedAt;

            var createdToday = await application
                .GetAccounts()
                .Where(x => x.CreatedAt.Within(created.Year, created.Month, created.Day))
                .CountAsync();
            createdToday.ShouldNotBe(0);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_using_filter(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var filtered = await application
                .GetAccounts()
                .Filter("lo")
                .ToListAsync();

            filtered.Count.ShouldBeGreaterThanOrEqualTo(3);
            filtered.ShouldContain(acct => acct.FullName == "Han Solo");
            filtered.ShouldContain(acct => acct.Username.StartsWith("lottanerve"));
            filtered.ShouldContain(acct => acct.Username.StartsWith("lordvader"));
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Sorting_accounts_by_lastname(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var accountsSortedByLastName = await application
                .GetAccounts()
                .OrderBy(x => x.Surname)
                .ToListAsync();

            var lando = accountsSortedByLastName.First();
            lando.FullName.ShouldBe("Lando Calrissian");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Sorting_accounts_by_username_and_lastname(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var accountsSortedByMultiple = await application
                .GetAccounts()
                .OrderByDescending(x => x.Username)
                .OrderByDescending(x => x.Surname)
                .ToListAsync();

            var tarkin = accountsSortedByMultiple.First();
            tarkin.FullName.ShouldBe("Wilhuff Tarkin");
            var luke = accountsSortedByMultiple.ElementAt(1);
            luke.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Taking_only_two_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var firstTwo = await application
                .GetAccounts()
                .Take(2)
                .ToListAsync();

            firstTwo.Count.ShouldBe(2);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Counting_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var count = await application.GetAccounts().CountAsync();
            count.ShouldBeGreaterThanOrEqualTo(8);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Any_returns_false_for_empty_filtered_set(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var anyDroids = await application
                .GetAccounts()
                .Where(x => x.Email.EndsWith("droids.co"))
                .AnyAsync();

            anyDroids.ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Any_returns_true_for_nonempty_filtered_set(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var anyWookiees = await application
                .GetAccounts()
                .Where(x => x.Email.EndsWith("kashyyyk.rim"))
                .AnyAsync();

            anyWookiees.ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_and_deleting_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = await application.CreateAccountAsync("Gial", "Ackbar", "admiralackbar@dac.rim", new RandomPassword(12));

            account.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedAccountHrefs.Add(account.Href);

            account.FullName.ShouldBe("Gial Ackbar");
            account.Email.ShouldBe("admiralackbar@dac.rim");
            account.Username.ShouldBe("admiralackbar@dac.rim");
            account.Status.ShouldBe(AccountStatus.Enabled);
            account.CreatedAt.ShouldBe(DateTimeOffset.Now, TimeSpan.FromSeconds(10));
            account.ModifiedAt.ShouldBe(DateTimeOffset.Now, TimeSpan.FromSeconds(10));

            var deleted = await account.DeleteAsync(); // It's a trap! :(
            deleted.ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_account_with_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = await application.CreateAccountAsync(
                "Mara", "Jade", new RandomEmail("empire.co"), new RandomPassword(12), new { quote = "I'm a fighter. I've always been a fighter.", birth = -17, death = 40 });

            account.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedAccountHrefs.Add(account.Href);
            var customData = await account.GetCustomDataAsync();

            account.FullName.ShouldBe("Mara Jade");
            customData["quote"].ShouldBe("I'm a fighter. I've always been a fighter.");
            customData["birth"].ShouldBe(-17);
            customData["death"].ShouldBe(40);

            // Clean up
            (await account.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_account_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = client
                .Instantiate<IAccount>()
                .SetGivenName("Galen")
                .SetSurname("Marek")
                .SetEmail("gmarek@kashyyk.rim")
                .SetPassword(new RandomPassword(12));
            await application.CreateAccountAsync(account, opt =>
            {
                opt.ResponseOptions.Expand(x => x.GetCustomData());
            });

            account.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedAccountHrefs.Add(account.Href);

            // Clean up
            (await account.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Authenticating_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var username = $"sonofthesuns-{this.fixture.TestRunIdentifier}";
            var result = await application.AuthenticateAccountAsync(username, "whataPieceofjunk$1138");
            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();

            var account = await result.GetAccountAsync();
            account.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Authenticating_account_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var request = new UsernamePasswordRequestBuilder();
            request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}");
            request.SetPassword("whataPieceofjunk$1138");

            var result = await application.AuthenticateAccountAsync(request.Build(), response => response.Expand(x => x.GetAccount()));

            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Authenticating_account_in_specified_account_store(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var accountStore = await application.GetDefaultAccountStoreAsync();

            var result = await application.AuthenticateAccountAsync(
                request => request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}").SetPassword("whataPieceofjunk$1138").SetAccountStore(accountStore));
            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();

            var account = await result.GetAccountAsync();
            account.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Authenticating_account_in_specified_account_store_by_href(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var accountStore = await application.GetDefaultAccountStoreAsync();

            var result = await application.AuthenticateAccountAsync(
                request =>
            {
                request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}");
                request.SetPassword("whataPieceofjunk$1138");
                request.SetAccountStore(this.fixture.PrimaryOrganizationHref);
            });
            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();

            var account = await result.GetAccountAsync();
            account.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Authenticating_account_in_specified_organization_by_nameKey(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var accountStore = await application.GetDefaultAccountStoreAsync();

            var result = await application.AuthenticateAccountAsync(
                request =>
                {
                    request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}");
                    request.SetPassword("whataPieceofjunk$1138");
                    request.SetAccountStore(this.fixture.PrimaryOrganizationNameKey);
                });
            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();

            var account = await result.GetAccountAsync();
            account.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Authenticating_account_in_specified_account_store_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var accountStore = await application.GetDefaultAccountStoreAsync();

            var result = await application.AuthenticateAccountAsync(
                request =>
            {
                request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}");
                request.SetPassword("whataPieceofjunk$1138");
                request.SetAccountStore(accountStore);
            },
            response => response.Expand(x => x.GetAccount()));

            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task TryAuthenticating_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var username = $"sonofthesuns-{this.fixture.TestRunIdentifier}";

            (await application.TryAuthenticateAccountAsync(username, "whataPieceofjunk$1138"))
                .ShouldBeTrue();

            (await application.TryAuthenticateAccountAsync(username, "notLukesPassword?"))
                .ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Authenticating_throws_for_invalid_credentials(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var username = $"sonofthesuns-{this.fixture.TestRunIdentifier}";
            var password = "notLukesPassword?";

            bool didFailCorrectly = false;
            try
            {
                var result = await application.AuthenticateAccountAsync(username, password);
            }
            catch (ResourceException rex)
            {
                didFailCorrectly = rex.HttpStatus == 400;
            }
            catch
            {
                didFailCorrectly = false;
            }

            Assert.True(didFailCorrectly);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Resetting_password_updates_modified_date(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = await application.GetAccounts()
                .Where(a => a.Email == "chewie@kashyyyk.rim")
                .SingleAsync();

            var oldModificationDate = account.PasswordModifiedAt.Value;

            await Task.Delay(3000); // Wait for a bit because sometimes clocks are slow
            
            account.SetPassword(new RandomPassword(16));
            await account.SaveAsync();

            account.PasswordModifiedAt.Value.ShouldBeGreaterThan(oldModificationDate);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Searching_accounts_with_modified_passwords(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var chewie = await application.GetAccounts()
                .Where(a => a.Email == "chewie@kashyyyk.rim")
                .SingleAsync();
            chewie.SetPassword(new RandomPassword(16));
            await chewie.SaveAsync();

            // Get all accounts that modified their passwords this year
            var accounts = await application.GetAccounts()
                .Where(a => a.PasswordModifiedAt > new DateTimeOffset(DateTimeOffset.Now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .ToListAsync();

            accounts.ShouldContain(a => a.Email == "chewie@kashyyyk.rim");
        }

        /// <summary>
        /// Regression test for https://github.com/stormpath/stormpath-sdk-dotnet/issues/175
        /// </summary>
        /// <param name="clientBuilder">The client to use.</param>
        /// <returns>A Task that represents the asynchronous test.</returns>
        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_account_with_special_chars(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var trickyEmail = "admiral.ackbar+itsatrap@dac.rim";
            var password = new RandomPassword(12);
            var account = await application.CreateAccountAsync("Gial", "Ackbar", trickyEmail, password);

            account.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedAccountHrefs.Add(account.Href);

            var searchResult = await application.GetAccounts().Where(x => x.Username == trickyEmail).SingleAsync();
            searchResult.Username.ShouldBe(trickyEmail);
            searchResult.Email.ShouldBe("admiral.ackbar+itsatrap@dac.rim");
            searchResult.Status.ShouldBe(AccountStatus.Enabled);

            var loginResult = await application.AuthenticateAccountAsync(trickyEmail, password);
            loginResult.Success.ShouldBeTrue();

            var deleted = await account.DeleteAsync();
            deleted.ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_account_with_UTF8_properties(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = await application.CreateAccountAsync("四", "李", "utf8@test.foo", "Supersecret!123");
            this.fixture.CreatedAccountHrefs.Add(account.Href);

            var searched = await application.GetAccounts().Where(x => x.Surname == "李").SingleAsync();
            searched.GivenName.ShouldBe("四");
            searched.Surname.ShouldBe("李");

            (await account.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Create_sms_factor_for_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var luke = await client.GetAccountAsync(fixture.PrimaryAccountHref);

            var factor = await luke.Factors.AddAsync(new SmsFactorCreationOptions
            {
                Number = "+1 818-555-2593" // Jack is back
            });

            (await factor.GetAccountAsync()).Href.Should().Be(luke.Href);

            factor.Href.Should().NotBeNullOrEmpty();
            factor.Type.Should().Be(FactorType.Sms);
            factor.Status.Should().Be(FactorStatus.Enabled);
            factor.VerificationStatus.Should().Be(FactorVerificationStatus.Unverified);

            // No challenges yet
            (await factor.GetMostRecentChallengeAsync()).Should().BeNull();
            (await factor.Challenges.AnyAsync()).ShouldBeFalse();

            // Get the phone associated with this factor
            var phone = await factor.GetPhoneAsync();
            phone.Number.Should().Be("+18185552593");

            (await factor.DeleteAsync()).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Create_totp_factor_for_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var luke = await client.GetAccountAsync(fixture.PrimaryAccountHref);

            var factor = await luke.Factors.AddAsync(new GoogleAuthenticatorFactorCreationOptions
            {
                Issuer = "MyApp"
            });

            (await factor.GetAccountAsync()).Href.Should().Be(luke.Href);

            factor.Href.Should().NotBeNullOrEmpty();
            factor.Type.Should().Be(FactorType.GoogleAuthenticator);
            factor.Status.Should().Be(FactorStatus.Enabled);
            factor.VerificationStatus.Should().Be(FactorVerificationStatus.Unverified);
            factor.AccountName.Should().NotBeNullOrEmpty();
            factor.Base64QrImage.Should().NotBeNullOrEmpty();
            factor.Issuer.Should().Be("MyApp");
            factor.KeyUri.Should().NotBeNullOrEmpty();
            factor.Secret.Should().NotBeNullOrEmpty();

            // No challenges yet
            (await factor.GetMostRecentChallengeAsync()).Should().BeNull();
            (await factor.Challenges.AnyAsync()).ShouldBeFalse();

            (await factor.DeleteAsync()).ShouldBeTrue();
        }

        /// <summary>
        /// Tests that polymorphism is handled correctly when retrieving this resource.
        /// </summary>
        /// <remarks>
        /// An SMS factor should be retrieved as an ISmsFactor, which implements IFactor.
        /// </remarks>
        /// <param name="clientBuilder">The client to use.</param>
        /// <returns>A Task that represents the asynchronous test.</returns>
        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Retrieving_sms_factor_should_create_sms_class(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var luke = await client.GetAccountAsync(fixture.PrimaryAccountHref);

            var factor = await luke.Factors.AddAsync(new SmsFactorCreationOptions
            {
                Number = "+1 818-555-2593"
            });

            factor.Should().BeAssignableTo<ISmsFactor>();
            factor.Should().BeAssignableTo<IFactor>();

            var retrievedGenericFactor = await client.GetResourceAsync<IFactor>(factor.Href);
            retrievedGenericFactor.Should().BeAssignableTo<ISmsFactor>();
            retrievedGenericFactor.Should().BeAssignableTo<IFactor>();
            retrievedGenericFactor.Href.Should().Be(factor.Href);

            var retrievedSpecificFactor = await client.GetResourceAsync<ISmsFactor>(factor.Href);
            retrievedSpecificFactor.Should().BeAssignableTo<ISmsFactor>();
            retrievedSpecificFactor.Should().BeAssignableTo<IFactor>();
            retrievedSpecificFactor.Href.Should().Be(factor.Href);

            (await factor.DeleteAsync()).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Create_sms_factor_for_account_and_challenge_immediately(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var luke = await client.GetAccountAsync(fixture.PrimaryAccountHref);

            var factor = await luke.Factors.AddAsync(new SmsFactorCreationOptions
            {
                Number = "+1 818-555-2593",
                Challenge = true
            });

            var challenge = await factor.GetMostRecentChallengeAsync();
            challenge.Href.Should().NotBeNullOrEmpty();
            challenge.Message.Should().NotBeNullOrEmpty();
            challenge.Status.Should().BeOfType<ChallengeStatus>();

            (await challenge.GetAccountAsync()).Href.Should().Be(luke.Href);
            (await challenge.GetFactorAsync()).Href.Should().Be(factor.Href);

            // Should get the same object when accessing the Challenges collection
            var challenge2 = await factor.Challenges.SingleAsync();
            challenge2.Href.Should().Be(challenge.Href);

            (await factor.DeleteAsync()).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Challenge_existing_factor(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var luke = await client.GetAccountAsync(fixture.PrimaryAccountHref);

            var factor = await luke.Factors.AddAsync(new SmsFactorCreationOptions
            {
                Number = "+1 818-555-2593"
            });

            var challenge = await factor.Challenges.AddAsync(new ChallengeCreationOptions
            {
                Message = "Dammit Chloe! The code is ${code}"
            });

            var retrievedChallenge = await factor.Challenges.SingleAsync();
            retrievedChallenge.Href.Should().Be(retrievedChallenge.Href);

            challenge.Message.Should().StartWith("Dammit Chloe!");
            challenge.Status.Should().BeOfType<ChallengeStatus>();

            (await challenge.GetAccountAsync()).Href.Should().Be(luke.Href);
            (await challenge.GetFactorAsync()).Href.Should().Be(factor.Href);

            (await factor.DeleteAsync()).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Update_sms_factor_and_save(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var luke = await client.GetAccountAsync(fixture.PrimaryAccountHref);

            var factor = await luke.Factors.AddAsync(new SmsFactorCreationOptions
            {
                Number = "+1 818-555-2593",
                Status = FactorStatus.Disabled
            });

            factor.Status.Should().Be(FactorStatus.Disabled);

            factor.Status = FactorStatus.Enabled;
            await factor.SaveAsync();

            factor.Status.Should().Be(FactorStatus.Enabled);

            (await factor.DeleteAsync()).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Update_totp_factor_and_save(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var luke = await client.GetAccountAsync(fixture.PrimaryAccountHref);

            var factor = await luke.Factors.AddAsync(new GoogleAuthenticatorFactorCreationOptions
            {
                Issuer = "MyApp"
            });

            factor.Issuer.Should().Be("MyApp");
            factor.AccountName.Should().Be(luke.Username);

            factor.Issuer = "MyBetterApp";
            factor.AccountName = "luke@hoth.rim";
            await factor.SaveAsync();

            factor.Issuer.Should().Be("MyBetterApp");
            factor.AccountName.Should().Be("luke@hoth.rim");

            (await factor.DeleteAsync()).ShouldBeTrue();
        }
    }
}