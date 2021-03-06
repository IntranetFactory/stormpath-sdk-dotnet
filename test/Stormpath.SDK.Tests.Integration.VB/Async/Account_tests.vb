﻿' <copyright file="Account_tests.vb" company="Stormpath, Inc.">
' Copyright (c) 2016 Stormpath, Inc.
'
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
'
'      http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' </copyright>

Option Strict On
Option Explicit On
Option Infer On
Imports Shouldly
Imports Stormpath.SDK.Account
Imports Stormpath.SDK.Application
Imports Stormpath.SDK.Auth
Imports Stormpath.SDK.Error
Imports Stormpath.SDK.Tests.Common.Integration
Imports Stormpath.SDK.Tests.Common.RandomData
Imports Xunit

Namespace Async
    <Collection(NameOf(IntegrationTestCollection))>
    Public Class Account_tests
        Private ReadOnly fixture As TestFixture

        Public Sub New(fixture As TestFixture)
            Me.fixture = fixture
        End Sub

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Getting_tenant_accounts(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim tenant = Await client.GetCurrentTenantAsync()

            Dim accounts = Await tenant.GetAccounts().ToListAsync()

            accounts.Any().ShouldBeTrue()
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Getting_accounts(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim accounts = Await application.GetAccounts().ToListAsync()

            ' Verify data from IntegrationTestData
            accounts.Count.ShouldBeGreaterThanOrEqualTo(8)

            Dim luke = accounts.Where(Function(x) x.GivenName = "Luke").[Single]()
            luke.FullName.ShouldBe("Luke Skywalker")
            luke.Email.ShouldBe("lskywalker@tattooine.rim")
            luke.Username.ShouldStartWith("sonofthesuns")
            luke.Status.ShouldBe(AccountStatus.Enabled)

            Dim vader = accounts.Where(Function(x) x.Surname = "Vader").[Single]()
            vader.FullName.ShouldBe("Darth Vader")
            vader.Email.ShouldStartWith("vader@galacticempire.co")
            vader.Username.ShouldStartWith("lordvader")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Getting_account_provider_data(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim luke = Await application.GetAccounts() _
                .Where(Function(x) x.Email.StartsWith("lskywalker")) _
                .SingleAsync()

            Dim providerData = Await luke.GetProviderDataAsync()
            providerData.Href.ShouldNotBeNullOrEmpty()
            providerData.ProviderId.ShouldBe("stormpath")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Updating_account(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim leia = Await application.GetAccounts() _
                .Where(Function(a) a.Email = "leia.organa@alderaan.core") _
                .SingleAsync()

            leia.SetMiddleName("Organa")
            leia.SetSurname("Solo")
            Dim saveResult = Await leia.SaveAsync()

            ' In 8 ABY of course
            saveResult.FullName.ShouldBe("Leia Organa Solo")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Saving_with_response_options(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim chewie = Await application.GetAccounts() _
                .Where(Function(a) a.Email = "chewie@kashyyyk.rim") _
                .SingleAsync()

            chewie.SetUsername($"rwaaargh-{fixture.TestRunIdentifier}")
            Await chewie.SaveAsync(Function(response) response.Expand(Function(x) x.GetCustomData))
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Getting_account_applications(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()

            Dim luke = Await client.GetAccountAsync(Me.fixture.PrimaryAccountHref)

            Dim apps = Await luke.GetApplications().ToListAsync()
            apps.Where(Function(x) x.Href = Me.fixture.PrimaryApplicationHref).Any().ShouldBeTrue()
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Getting_account_directory(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim luke = Await application.GetAccounts() _
                .Filter("Luke") _
                .SingleAsync()

            ' Verify data from IntegrationTestData
            Dim directoryHref = (Await luke.GetDirectoryAsync()).Href
            directoryHref.ShouldBe(Me.fixture.PrimaryDirectoryHref)
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Getting_account_tenant(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim leia = Await application.GetAccounts() _
                .Filter("Leia") _
                .SingleAsync()

            ' Verify data from IntegrationTestData
            Dim tenantHref = (Await leia.GetTenantAsync()).Href
            tenantHref.ShouldBe(Me.fixture.TenantHref)
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_by_email(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim coreCitizens = Await application.GetAccounts() _
                .Where(Function(acct) acct.Email.EndsWith(".core")) _
                .ToListAsync()

            ' Verify data from IntegrationTestData
            coreCitizens.Count.ShouldBe(2)

            Dim han = coreCitizens.Where(Function(x) x.GivenName = "Han").[Single]()
            han.Email.ShouldBe("han.solo@corellia.core")
            han.Username.ShouldStartWith("cptsolo")

            Dim leia = coreCitizens.Where(Function(x) x.GivenName = "Leia").[Single]()
            leia.Email.ShouldStartWith("leia.organa@alderaan.core")
            leia.Username.ShouldStartWith("princessleia")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_by_firstname(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim chewie = Await application.GetAccounts() _
                .Where(Function(a) a.GivenName = "Chewbacca") _
                .SingleAsync()

            ' Verify data from IntegrationTestData
            chewie.FullName.ShouldBe("Chewbacca the Wookiee")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_by_lastname(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim palpatine = Await application.GetAccounts() _
                .Where(Function(a) a.Surname = "Palpatine") _
                .SingleAsync()

            ' Verify data from IntegrationTestData
            palpatine.FullName.ShouldBe("Emperor Palpatine")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_by_middle_name(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim chewie = Await application.GetAccounts() _
                .Where(Function(a) a.MiddleName = "the") _
                .SingleAsync()

            ' Verify data from IntegrationTestData
            chewie.FullName.ShouldBe("Chewbacca the Wookiee")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_by_username(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim vader = Await application.GetAccounts() _
                .Where(Function(a) a.Username.Equals($"lordvader-{fixture.TestRunIdentifier}")) _
                .SingleAsync()

            ' Verify data from IntegrationTestData
            vader.Email.ShouldBe("vader@galacticempire.co")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_by_status(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim tarkin = Await application.GetAccounts() _
                .Where(Function(x) x.Status = AccountStatus.Disabled) _
                .SingleAsync()

            ' Verify data from IntegrationTestData
            tarkin.FullName.ShouldBe("Wilhuff Tarkin")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_by_creation_date(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim longTimeAgo = Await application _
                .GetAccounts() _
                .Where(Function(x) x.CreatedAt < Date.Now.Subtract(TimeSpan.FromHours(1))) _
                .ToListAsync()
            longTimeAgo.ShouldBeEmpty()

            Dim createdRecently = Await application _
                .GetAccounts() _
                .Where(Function(x) x.CreatedAt >= Date.Now.Subtract(TimeSpan.FromHours(1))) _
                .ToListAsync()
            createdRecently.ShouldNotBeEmpty()
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_by_creation_date_within_shorthand(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim firstAccount = Await application.GetAccounts().FirstAsync()
            Dim created = firstAccount.CreatedAt

            Dim createdToday = Await application.GetAccounts().Where(Function(x) x.CreatedAt.Within(created.Year, created.Month, created.Day)).CountAsync()
            createdToday.ShouldNotBe(0)
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Searching_accounts_using_filter(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim filtered = Await application.GetAccounts() _
                .Filter("lo") _
                .ToListAsync()

            filtered.Count.ShouldBeGreaterThanOrEqualTo(3)
            filtered.ShouldContain(Function(acct) acct.FullName = "Han Solo")
            filtered.ShouldContain(Function(acct) acct.Username.StartsWith("lottanerve"))
            filtered.ShouldContain(Function(acct) acct.Username.StartsWith("lordvader"))
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Sorting_accounts_by_lastname(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim accountsSortedByLastName = Await application _
                .GetAccounts().OrderBy(Function(x) x.Surname) _
                .ToListAsync()

            Dim lando = accountsSortedByLastName.First()
            lando.FullName.ShouldBe("Lando Calrissian")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Sorting_accounts_by_username_and_lastname(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim accountsSortedByMultiple = Await application.GetAccounts() _
                .OrderByDescending(Function(x) x.Username) _
                .OrderByDescending(Function(x) x.Surname) _
                .ToListAsync()

            Dim tarkin = accountsSortedByMultiple.First()
            tarkin.FullName.ShouldBe("Wilhuff Tarkin")
            Dim luke = accountsSortedByMultiple.ElementAt(1)
            luke.FullName.ShouldBe("Luke Skywalker")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Taking_only_two_accounts(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim firstTwo = Await application.GetAccounts().Take(2).ToListAsync()

            firstTwo.Count.ShouldBe(2)
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Counting_accounts(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim count = Await application.GetAccounts().CountAsync()
            count.ShouldBeGreaterThanOrEqualTo(8)
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Any_returns_false_for_empty_filtered_set(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim anyDroids = Await application.GetAccounts() _
                .Where(Function(x) x.Email.EndsWith("droids.co")) _
                .AnyAsync()

            anyDroids.ShouldBeFalse()
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Any_returns_true_for_nonempty_filtered_set(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim anyWookiees = Await application.GetAccounts() _
                .Where(Function(x) x.Email.EndsWith("kashyyyk.rim")) _
                .AnyAsync()

            anyWookiees.ShouldBeTrue()
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Creating_and_deleting_account(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim account = Await application.CreateAccountAsync("Gial", "Ackbar", "admiralackbar@dac.rim", New RandomPassword(12))

            account.Href.ShouldNotBeNullOrEmpty()
            Me.fixture.CreatedAccountHrefs.Add(account.Href)

            account.FullName.ShouldBe("Gial Ackbar")
            account.Email.ShouldBe("admiralackbar@dac.rim")
            account.Username.ShouldBe("admiralackbar@dac.rim")
            account.Status.ShouldBe(AccountStatus.Enabled)
            account.CreatedAt.ShouldBe(DateTimeOffset.Now, TimeSpan.FromSeconds(10))
            account.ModifiedAt.ShouldBe(DateTimeOffset.Now, TimeSpan.FromSeconds(10))

            Dim deleted = Await account.DeleteAsync()
            ' It's a trap! :(
            deleted.ShouldBeTrue()
            Me.fixture.CreatedAccountHrefs.Remove(account.Href)
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Creating_account_with_custom_data(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim account = Await application.CreateAccountAsync("Mara", "Jade", New RandomEmail("empire.co"), New RandomPassword(12), New With {
                Key .quote = "I'm a fighter. I've always been a fighter.",
                Key .birth = -17,
                Key .death = 40
            })

            account.Href.ShouldNotBeNullOrEmpty()
            Me.fixture.CreatedAccountHrefs.Add(account.Href)
            Dim customData = Await account.GetCustomDataAsync()

            account.FullName.ShouldBe("Mara Jade")
            CStr(customData("quote")).ShouldBe("I'm a fighter. I've always been a fighter.")
            CInt(customData("birth")).ShouldBe(-17)
            CInt(customData("death")).ShouldBe(40)

            ' Clean up
            Dim result = Await account.DeleteAsync()
            result.ShouldBeTrue()
            Me.fixture.CreatedAccountHrefs.Remove(account.Href)
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Creating_account_with_response_options(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim account = client.Instantiate(Of IAccount)() _
                .SetGivenName("Galen") _
                .SetSurname("Marek") _
                .SetEmail("gmarek@kashyyk.rim") _
                .SetPassword(New RandomPassword(12))
            Await application.CreateAccountAsync(account, Sub(opt)
                                                              opt.ResponseOptions.Expand(Function(x) x.GetCustomData)
                                                          End Sub)

            account.Href.ShouldNotBeNullOrEmpty()
            Me.fixture.CreatedAccountHrefs.Add(account.Href)

            ' Clean up
            Dim result = Await account.DeleteAsync()
            result.ShouldBeTrue()
            Me.fixture.CreatedAccountHrefs.Remove(account.Href)
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Authenticating_account(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim username = $"sonofthesuns-{fixture.TestRunIdentifier}"
            Dim result = Await application.AuthenticateAccountAsync(username, "whataPieceofjunk$1138")
            result.ShouldBeAssignableTo(Of IAuthenticationResult)()
            result.Success.ShouldBeTrue()

            Dim account = Await result.GetAccountAsync()
            account.FullName.ShouldBe("Luke Skywalker")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Authenticating_account_with_response_options(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim request = New UsernamePasswordRequestBuilder()
            request.SetUsernameOrEmail($"sonofthesuns-{fixture.TestRunIdentifier}")
            request.SetPassword("whataPieceofjunk$1138")

            Dim result = Await application.AuthenticateAccountAsync(request.Build(), Function(response) response.Expand(Function(x) x.GetAccount))

            result.ShouldBeAssignableTo(Of IAuthenticationResult)()
            result.Success.ShouldBeTrue()
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Authenticating_account_in_specified_account_store(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)
            Dim accountStore = Await application.GetDefaultAccountStoreAsync()

            Dim result = Await application.AuthenticateAccountAsync(Sub(request)
                                                                        request.SetUsernameOrEmail($"sonofthesuns-{fixture.TestRunIdentifier}")
                                                                        request.SetPassword("whataPieceofjunk$1138")
                                                                        request.SetAccountStore(accountStore)
                                                                    End Sub)
            result.ShouldBeAssignableTo(Of IAuthenticationResult)()
            result.Success.ShouldBeTrue()

            Dim account = Await result.GetAccountAsync()
            account.FullName.ShouldBe("Luke Skywalker")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Authenticating_account_in_specified_account_store_by_href(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)
            Dim accountStore = Await application.GetDefaultAccountStoreAsync()

            Dim result = Await application.AuthenticateAccountAsync(Sub(request)
                                                                        request.SetUsernameOrEmail($"sonofthesuns-{fixture.TestRunIdentifier}")
                                                                        request.SetPassword("whataPieceofjunk$1138")
                                                                        request.SetAccountStore(Me.fixture.PrimaryOrganizationHref)
                                                                    End Sub)
            result.ShouldBeAssignableTo(Of IAuthenticationResult)()
            result.Success.ShouldBeTrue()

            Dim account = Await result.GetAccountAsync()
            account.FullName.ShouldBe("Luke Skywalker")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Authenticating_account_in_specified_organization_by_nameKey(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)
            Dim accountStore = Await application.GetDefaultAccountStoreAsync()

            Dim result = Await application.AuthenticateAccountAsync(Sub(request)
                                                                        request.SetUsernameOrEmail($"sonofthesuns-{fixture.TestRunIdentifier}")
                                                                        request.SetPassword("whataPieceofjunk$1138")
                                                                        request.SetAccountStore(Me.fixture.PrimaryOrganizationNameKey)
                                                                    End Sub)
            result.ShouldBeAssignableTo(Of IAuthenticationResult)()
            result.Success.ShouldBeTrue()

            Dim account = Await result.GetAccountAsync()
            account.FullName.ShouldBe("Luke Skywalker")
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Authenticating_account_in_specified_account_store_with_response_options(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)
            Dim accountStore = Await application.GetDefaultAccountStoreAsync()

            Dim result = Await application.AuthenticateAccountAsync(Sub(request)
                                                                        request.SetUsernameOrEmail($"sonofthesuns-{fixture.TestRunIdentifier}")
                                                                        request.SetPassword("whataPieceofjunk$1138")
                                                                        request.SetAccountStore(accountStore)
                                                                    End Sub,
                                                                    Function(response) response.Expand(Function(x) x.GetAccount))

            result.ShouldBeAssignableTo(Of IAuthenticationResult)()
            result.Success.ShouldBeTrue()
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function TryAuthenticating_account(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim username = $"sonofthesuns-{fixture.TestRunIdentifier}"

            Dim success = Await application.TryAuthenticateAccountAsync(username, "whataPieceofjunk$1138")
            success.ShouldBeTrue()

            Dim failure = Await application.TryAuthenticateAccountAsync(username, "notLukesPassword?")
            failure.ShouldBeFalse()
        End Function

        <Theory>
        <MemberData(NameOf(TestClients.GetClients), MemberType:=GetType(TestClients))>
        Public Async Function Authenticating_throws_for_invalid_credentials(clientBuilder As TestClientProvider) As Task
            Dim client = clientBuilder.GetClient()
            Dim application = Await client.GetResourceAsync(Of IApplication)(Me.fixture.PrimaryApplicationHref)

            Dim username = $"sonofthesuns-{fixture.TestRunIdentifier}"
            Dim password = "notLukesPassword?"

            Dim didFailCorrectly As Boolean = False
            Try
                Dim result = Await application.AuthenticateAccountAsync(username, password)
            Catch rex As ResourceException
                didFailCorrectly = rex.HttpStatus = 400
            Catch
                didFailCorrectly = False
            End Try

            Assert.[True](didFailCorrectly)
        End Function
    End Class
End Namespace