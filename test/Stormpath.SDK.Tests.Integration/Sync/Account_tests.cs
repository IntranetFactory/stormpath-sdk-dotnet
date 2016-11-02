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
using Shouldly;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Auth;
using Stormpath.SDK.Error;
using Stormpath.SDK.Sync;
using Stormpath.SDK.Tests.Common.Integration;
using Stormpath.SDK.Tests.Common.RandomData;
using Xunit;

namespace Stormpath.SDK.Tests.Integration.Sync
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
        public void Getting_tenant_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();

            var accounts = tenant.GetAccounts().Synchronously().ToList();

            accounts.Any().ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Getting_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var accounts = application.GetAccounts().Synchronously().ToList();

            // Verify data from IntegrationTestData
            accounts.Count.ShouldBeGreaterThanOrEqualTo(8);

            var luke = accounts.Where(x => x.GivenName == "Luke").Single();
            luke.FullName.ShouldBe("Luke Skywalker");
            luke.Email.ShouldBe("lskywalker@testmail.stormpath.com");
            luke.Username.ShouldStartWith("sonofthesuns");
            luke.Status.ShouldBe(AccountStatus.Enabled);

            var vader = accounts.Where(x => x.Surname == "Vader").Single();
            vader.FullName.ShouldBe("Darth Vader");
            vader.Email.ShouldStartWith("vader@testmail.stormpath.com");
            vader.Username.ShouldStartWith("lordvader");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Getting_account_provider_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var luke = application
                .GetAccounts()
                .Synchronously()
                .Where(x => x.Email.StartsWith("lskywalker"))
                .Single();

            var providerData = luke.GetProviderData();
            providerData.Href.ShouldNotBeNullOrEmpty();
            providerData.ProviderId.ShouldBe("stormpath");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Updating_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var leia = application
                .GetAccounts()
                .Synchronously()
                .Where(a => a.Email == "leia.organa@testmail.stormpath.com")
                .Single();

            leia.SetMiddleName("Organa");
            leia.SetSurname("Solo");
            var saveResult = leia.Save();

            // In 8 ABY of course
            saveResult.FullName.ShouldBe("Leia Organa Solo");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Saving_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var chewie = application
                .GetAccounts()
                .Synchronously()
                .Where(a => a.Email == "chewie@testmail.stormpath.com")
                .Single();

            chewie.SetUsername($"rwaaargh-{this.fixture.TestRunIdentifier}");
            chewie.Save(response => response.Expand(x => x.GetCustomData()));
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Getting_account_applications(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();

            var luke = client.GetAccount(this.fixture.PrimaryAccountHref);

            var apps = luke.GetApplications().Synchronously().ToList();
            apps.Where(x => x.Href == this.fixture.PrimaryApplicationHref).Any().ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Getting_account_directory(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var luke = application
                .GetAccounts()
                .Synchronously()
                .Filter("Luke")
                .Single();

            // Verify data from IntegrationTestData
            var directoryHref = luke.GetDirectory().Href;
            directoryHref.ShouldBe(this.fixture.PrimaryDirectoryHref);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Getting_account_tenant(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var leia = application
                .GetAccounts()
                .Synchronously()
                .Filter("Leia")
                .Single();

            // Verify data from IntegrationTestData
            var tenantHref = leia.GetTenant().Href;
            tenantHref.ShouldBe(this.fixture.TenantHref);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_by_email(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var testAccounts = application
                .GetAccounts()
                .Synchronously()
                .Where(acct => acct.Email.EndsWith("testmail.stormpath.com"))
                .ToList();

            // Verify data from IntegrationTestData
            var han = testAccounts.Where(x => x.GivenName == "Han").Single();
            han.Email.ShouldBe("han.solo@testmail.stormpath.com");
            han.Username.ShouldStartWith("cptsolo");

            var leia = testAccounts.Where(x => x.GivenName == "Leia").Single();
            leia.Email.ShouldStartWith("leia.organa@testmail.stormpath.com");
            leia.Username.ShouldStartWith("princessleia");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_by_firstname(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var chewie = application
                .GetAccounts()
                .Synchronously()
                .Where(a => a.GivenName == "Chewbacca")
                .Single();

            // Verify data from IntegrationTestData
            chewie.FullName.ShouldBe("Chewbacca the Wookiee");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_by_lastname(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var palpatine = application
                .GetAccounts()
                .Synchronously()
                .Where(a => a.Surname == "Palpatine")
                .Single();

            // Verify data from IntegrationTestData
            palpatine.FullName.ShouldBe("Emperor Palpatine");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_by_middle_name(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var chewie = application
                .GetAccounts()
                .Synchronously()
                .Where(a => a.MiddleName == "the")
                .Single();

            // Verify data from IntegrationTestData
            chewie.FullName.ShouldBe("Chewbacca the Wookiee");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_by_username(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var vader = application
                .GetAccounts()
                .Synchronously()
                .Where(a => a.Username.Equals($"lordvader-{this.fixture.TestRunIdentifier}"))
                .Single();

            // Verify data from IntegrationTestData
            vader.Email.ShouldBe("vader@testmail.stormpath.com");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_by_status(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var tarkin = application
                .GetAccounts()
                .Synchronously()
                .Where(x => x.Status == AccountStatus.Disabled)
                .Single();

            // Verify data from IntegrationTestData
            tarkin.FullName.ShouldBe("Wilhuff Tarkin");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_by_creation_date(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var longTimeAgo = application
                .GetAccounts()
                .Synchronously()
                .Where(x => x.CreatedAt < DateTime.Now.Subtract(TimeSpan.FromHours(1)))
                .ToList();
            longTimeAgo.ShouldBeEmpty();

            var createdRecently = application
                .GetAccounts()
                .Synchronously()
                .Where(x => x.CreatedAt >= DateTime.Now.Subtract(TimeSpan.FromHours(1)))
                .ToList();
            createdRecently.ShouldNotBeEmpty();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_by_creation_date_within_shorthand(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var firstAccount = application.GetAccounts().Synchronously().First();
            var created = firstAccount.CreatedAt;

            var createdToday = application
                .GetAccounts()
                .Synchronously()
                .Where(x => x.CreatedAt.Within(created.Year, created.Month, created.Day))
                .Count();
            createdToday.ShouldNotBe(0);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_using_filter(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var filtered = application
                .GetAccounts()
                .Synchronously()
                .Filter("lo")
                .ToList();

            filtered.Count.ShouldBeGreaterThanOrEqualTo(3);
            filtered.ShouldContain(acct => acct.FullName == "Han Solo");
            filtered.ShouldContain(acct => acct.Username.StartsWith("lottanerve"));
            filtered.ShouldContain(acct => acct.Username.StartsWith("lordvader"));
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Sorting_accounts_by_lastname(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var accountsSortedByLastName = application
                .GetAccounts()
                .Synchronously()
                .OrderBy(x => x.Surname)
                .ToList();

            var lando = accountsSortedByLastName.First();
            lando.FullName.ShouldBe("Lando Calrissian");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Sorting_accounts_by_username_and_lastname(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var accountsSortedByMultiple = application
                .GetAccounts()
                .Synchronously()
                .OrderByDescending(x => x.Username)
                .OrderByDescending(x => x.Surname)
                .ToList();

            var tarkin = accountsSortedByMultiple.First();
            tarkin.FullName.ShouldBe("Wilhuff Tarkin");
            var luke = accountsSortedByMultiple.ElementAt(1);
            luke.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Taking_only_two_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var firstTwo = application
                .GetAccounts()
                .Synchronously()
                .Take(2)
                .ToList();

            firstTwo.Count.ShouldBe(2);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Counting_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var count = application.GetAccounts().Synchronously().Count();
            count.ShouldBeGreaterThanOrEqualTo(8);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Any_returns_false_for_empty_filtered_set(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var anyDroids = application
                .GetAccounts()
                .Synchronously()
                .Where(x => x.Email.EndsWith("droids.co"))
                .Any();

            anyDroids.ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Any_returns_true_for_nonempty_filtered_set(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var anyWookiees = application
                .GetAccounts()
                .Synchronously()
                .Where(x => x.Email.EndsWith("testmail.stormpath.com"))
                .Any();

            anyWookiees.ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Creating_and_deleting_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = application.CreateAccount("Gial", "Ackbar", "admiralackbar@testmail.stormpath.com", new RandomPassword(12));

            account.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedAccountHrefs.Add(account.Href);

            account.FullName.ShouldBe("Gial Ackbar");
            account.Email.ShouldBe("admiralackbar@testmail.stormpath.com");
            account.Username.ShouldBe("admiralackbar@testmail.stormpath.com");
            account.Status.ShouldBe(AccountStatus.Enabled);
            account.CreatedAt.ShouldBe(DateTimeOffset.Now, TimeSpan.FromSeconds(10));
            account.ModifiedAt.ShouldBe(DateTimeOffset.Now, TimeSpan.FromSeconds(10));

            var deleted = account.Delete(); // It's a trap! :(
            deleted.ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Creating_account_with_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = application.CreateAccount(
                "Mara", "Jade", new RandomEmail("testmail.stormpath.com"), new RandomPassword(12), new { quote = "I'm a fighter. I've always been a fighter.", birth = -17, death = 40 });

            account.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedAccountHrefs.Add(account.Href);
            var customData = account.GetCustomData();

            account.FullName.ShouldBe("Mara Jade");
            customData["quote"].ShouldBe("I'm a fighter. I've always been a fighter.");
            customData["birth"].ShouldBe(-17);
            customData["death"].ShouldBe(40);

            // Clean up
            account.Delete().ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Creating_account_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = client
                .Instantiate<IAccount>()
                .SetGivenName("Galen")
                .SetSurname("Marek")
                .SetEmail("gmarek@testmail.stormpath.com")
                .SetPassword(new RandomPassword(12));
            application.CreateAccount(account, opt =>
            {
                opt.ResponseOptions.Expand(x => x.GetCustomData());
            });

            account.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedAccountHrefs.Add(account.Href);

            // Clean up
            account.Delete().ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Authenticating_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var username = $"sonofthesuns-{this.fixture.TestRunIdentifier}";
            var result = application.AuthenticateAccount(username, "whataPieceofjunk$1138");
            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();

            var account = result.GetAccount();
            account.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Authenticating_account_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var request = new UsernamePasswordRequestBuilder();
            request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}");
            request.SetPassword("whataPieceofjunk$1138");

            var result = application.AuthenticateAccount(request.Build(), response => response.Expand(x => x.GetAccount()));

            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Authenticating_account_in_specified_account_store(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);
            var accountStore = application.GetDefaultAccountStore();

            var result = application.AuthenticateAccount(
                request => request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}").SetPassword("whataPieceofjunk$1138").SetAccountStore(accountStore));
            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();

            var account = result.GetAccount();
            account.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Authenticating_account_in_specified_account_store_by_href(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);
            var accountStore = application.GetDefaultAccountStore();

            var result = application.AuthenticateAccount(
                request =>
                {
                    request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}");
                    request.SetPassword("whataPieceofjunk$1138");
                    request.SetAccountStore(this.fixture.PrimaryOrganizationHref);
                });
            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();

            var account = result.GetAccount();
            account.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Authenticating_account_in_specified_organization_by_nameKey(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);
            var accountStore = application.GetDefaultAccountStore();

            var result = application.AuthenticateAccount(
                request =>
                {
                    request.SetUsernameOrEmail($"sonofthesuns-{this.fixture.TestRunIdentifier}");
                    request.SetPassword("whataPieceofjunk$1138");
                    request.SetAccountStore(this.fixture.PrimaryOrganizationNameKey);
                });
            result.ShouldBeAssignableTo<IAuthenticationResult>();
            result.Success.ShouldBeTrue();

            var account = result.GetAccount();
            account.FullName.ShouldBe("Luke Skywalker");
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Authenticating_account_in_specified_account_store_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);
            var accountStore = application.GetDefaultAccountStore();

            var result = application.AuthenticateAccount(
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
        public void TryAuthenticating_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var username = $"sonofthesuns-{this.fixture.TestRunIdentifier}";

            application
                .TryAuthenticateAccount(username, "whataPieceofjunk$1138")
                .ShouldBeTrue();

            application
                .TryAuthenticateAccount(username, "notLukesPassword?")
                .ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Authenticating_throws_for_invalid_credentials(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var username = $"sonofthesuns-{this.fixture.TestRunIdentifier}";
            var password = "notLukesPassword?";

            bool didFailCorrectly = false;
            try
            {
                var result = application.AuthenticateAccount(username, password);
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
        public void Resetting_password_updates_modified_date(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = application.GetAccounts()
                .Where(a => a.Email == "chewie@testmail.stormpath.com")
                .Synchronously()
                .Single();

            var oldModificationDate = account.PasswordModifiedAt.Value;

            System.Threading.Thread.Sleep(3000); // Wait for a bit because sometimes clocks are slow

            account.SetPassword(new RandomPassword(16));
            account.Save();

            account.PasswordModifiedAt.Value.ShouldBeGreaterThan(oldModificationDate);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Searching_accounts_with_modified_passwords(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var chewie = application.GetAccounts()
                .Where(a => a.Email == "chewie@testmail.stormpath.com")
                .Synchronously()
                .Single();
            chewie.SetPassword(new RandomPassword(16));
            chewie.Save();

            // Get all accounts that modified their passwords this year
            var accounts = application.GetAccounts()
                .Where(a => a.PasswordModifiedAt > new DateTimeOffset(DateTimeOffset.Now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .Synchronously()
                .ToList();

            accounts.ShouldContain(a => a.Email == "chewie@testmail.stormpath.com");
        }

        /// <summary>
        /// Regression test for https://github.com/stormpath/stormpath-sdk-dotnet/issues/175
        /// </summary>
        /// <param name="clientBuilder">The client to use.</param>
        /// <returns>A Task that represents the asynchronous test.</returns>
        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Creating_account_with_special_chars(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var trickyEmail = "admiral.ackbar+itsatrap@testmail.stormpath.com";
            var password = new RandomPassword(12);
            var account = application.CreateAccount("Gial", "Ackbar", trickyEmail, password);

            account.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedAccountHrefs.Add(account.Href);

            var searchResult = application.GetAccounts().Synchronously().Where(x => x.Username == trickyEmail).Single();
            searchResult.Username.ShouldBe(trickyEmail);
            searchResult.Email.ShouldBe("admiral.ackbar+itsatrap@testmail.stormpath.com");
            searchResult.Status.ShouldBe(AccountStatus.Enabled);

            var loginResult = application.AuthenticateAccount(trickyEmail, password);
            loginResult.Success.ShouldBeTrue();

            var deleted = account.Delete();
            deleted.ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public void Creating_account_with_UTF8_properties(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var account = application.CreateAccount("四", "李", "utf8@testmail.stormpath.com", "Supersecret!123");
            this.fixture.CreatedAccountHrefs.Add(account.Href);

            var searched = application.GetAccounts().Where(x => x.Surname == "李").Synchronously().Single();
            searched.GivenName.ShouldBe("四");
            searched.Surname.ShouldBe("李");

            account.Delete().ShouldBeTrue();
            this.fixture.CreatedAccountHrefs.Remove(account.Href);
        }
    }
}
