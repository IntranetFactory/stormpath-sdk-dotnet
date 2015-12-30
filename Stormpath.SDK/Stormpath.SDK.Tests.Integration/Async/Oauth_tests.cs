﻿// <copyright file="Oauth_tests.cs" company="Stormpath, Inc.">
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
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Stormpath.SDK.Error;
using Stormpath.SDK.Oauth;
using Stormpath.SDK.Tests.Common.Integration;
using Xunit;

namespace Stormpath.SDK.Tests.Integration.Async
{
    [Collection(nameof(IntegrationTestCollection))]
    public class Oauth_tests
    {
        private readonly TestFixture fixture;

        public Oauth_tests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_token_with_password_grant(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            // Create a dummy application
            var createdApplication = await tenant.CreateApplicationAsync(
                $".NET IT {this.fixture.TestRunIdentifier}-{clientBuilder.Name} Creating Token With Password Grant Flow",
                createDirectory: false);
            createdApplication.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(createdApplication.Href);

            // Add the test accounts
            await createdApplication.AddAccountStoreAsync(this.fixture.PrimaryDirectoryHref);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin("lskywalker@tattooine.rim")
                .SetPassword("whataPieceofjunk$1138")
                .SetAccountStore(this.fixture.PrimaryDirectoryHref)
                .Build();
            var authenticateResult = await createdApplication.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(passwordGrantRequest);

            // Verify authentication response
            authenticateResult.AccessTokenHref.ShouldContain("/accessTokens/");
            authenticateResult.AccessTokenString.ShouldNotBeNullOrEmpty();
            authenticateResult.ExpiresIn.ShouldBeGreaterThanOrEqualTo(3600);
            authenticateResult.TokenType.ShouldBe("Bearer");
            authenticateResult.RefreshTokenString.ShouldNotBeNullOrEmpty();

            // Verify generated access token
            var accessToken = await authenticateResult.GetAccessTokenAsync();
            accessToken.CreatedAt.ShouldNotBe(default(DateTimeOffset));
            accessToken.Href.ShouldBe(authenticateResult.AccessTokenHref);
            accessToken.Jwt.ShouldBe(authenticateResult.AccessTokenString);
            accessToken.ApplicationHref.ShouldBe(createdApplication.Href);

            // Clean up
            (await accessToken.DeleteAsync()).ShouldBeTrue();

            (await createdApplication.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedApplicationHrefs.Remove(createdApplication.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Failed_password_grant_throws_ResourceException(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            // Create a dummy application
            var createdApplication = await tenant.CreateApplicationAsync(
                $".NET IT {this.fixture.TestRunIdentifier}-{clientBuilder.Name} Failed Password Grant Throws",
                createDirectory: false);
            createdApplication.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(createdApplication.Href);

            // Add the test accounts
            await createdApplication.AddAccountStoreAsync(this.fixture.PrimaryDirectoryHref);

            var badPasswordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin("lskywalker@tattooine.rim")
                .SetPassword("notLukesPassword")
                .Build();

            await Should.ThrowAsync<ResourceException>(async () => await createdApplication.NewPasswordGrantAuthenticator().AuthenticateAsync(badPasswordGrantRequest));

            (await createdApplication.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedApplicationHrefs.Remove(createdApplication.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Listing_account_tokens(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            // Create a dummy application
            var createdApplication = await tenant.CreateApplicationAsync(
                $".NET IT {this.fixture.TestRunIdentifier}-{clientBuilder.Name} Listing Tokens",
                createDirectory: false);
            createdApplication.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(createdApplication.Href);

            // Add the test accounts
            await createdApplication.AddAccountStoreAsync(this.fixture.PrimaryDirectoryHref);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin("lskywalker@tattooine.rim")
                .SetPassword("whataPieceofjunk$1138")
                .SetAccountStore(this.fixture.PrimaryDirectoryHref)
                .Build();
            var authenticateResult = await createdApplication.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(passwordGrantRequest);

            var account = await tenant.GetAccountAsync(this.fixture.PrimaryAccountHref);
            var accessTokens = await account.GetAccessTokens().ToListAsync();
            var refreshTokens = await account.GetRefreshTokens().ToListAsync();

            var accessToken = accessTokens.Where(x => x.Jwt == authenticateResult.AccessTokenString).SingleOrDefault();
            var refreshToken = refreshTokens.Where(x => x.Jwt == authenticateResult.RefreshTokenString).SingleOrDefault();
            accessToken.ShouldNotBeNull();
            refreshToken.ShouldNotBeNull();

            // Clean up
            (await accessToken.DeleteAsync()).ShouldBeTrue();
            (await refreshToken.DeleteAsync()).ShouldBeTrue();

            (await createdApplication.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedApplicationHrefs.Remove(createdApplication.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_access_token_for_application(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            // Create a dummy application
            var createdApplication = await tenant.CreateApplicationAsync(
                $".NET IT {this.fixture.TestRunIdentifier}-{clientBuilder.Name} Getting Access Token for Application",
                createDirectory: false);
            createdApplication.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(createdApplication.Href);

            // Add the test accounts
            await createdApplication.AddAccountStoreAsync(this.fixture.PrimaryDirectoryHref);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin("lskywalker@tattooine.rim")
                .SetPassword("whataPieceofjunk$1138")
                .SetAccountStore(this.fixture.PrimaryDirectoryHref)
                .Build();
            var authenticateResult = await createdApplication.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(passwordGrantRequest);

            var account = await tenant.GetAccountAsync(this.fixture.PrimaryAccountHref);
            var accessTokenForApplication = await account
                .GetAccessTokens()
                .Where(x => x.ApplicationHref == createdApplication.Href)
                .SingleOrDefaultAsync();

            accessTokenForApplication.ShouldNotBeNull();

            (await accessTokenForApplication.GetAccountAsync()).Href.ShouldBe(this.fixture.PrimaryAccountHref);
            (await accessTokenForApplication.GetApplicationAsync()).Href.ShouldBe(createdApplication.Href);
            (await accessTokenForApplication.GetTenantAsync()).Href.ShouldBe(this.fixture.TenantHref);

            // Clean up
            (await accessTokenForApplication.DeleteAsync()).ShouldBeTrue();

            (await createdApplication.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedApplicationHrefs.Remove(createdApplication.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_refresh_token_for_application(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            // Create a dummy application
            var createdApplication = await tenant.CreateApplicationAsync(
                $".NET IT {this.fixture.TestRunIdentifier}-{clientBuilder.Name} Getting Refresh Token for Application",
                createDirectory: false);
            createdApplication.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(createdApplication.Href);

            // Add the test accounts
            await createdApplication.AddAccountStoreAsync(this.fixture.PrimaryDirectoryHref);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin("lskywalker@tattooine.rim")
                .SetPassword("whataPieceofjunk$1138")
                .SetAccountStore(this.fixture.PrimaryDirectoryHref)
                .Build();
            var authenticateResult = await createdApplication.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(passwordGrantRequest);

            var account = await tenant.GetAccountAsync(this.fixture.PrimaryAccountHref);
            var refreshTokenForApplication = await account
                .GetRefreshTokens()
                .Where(x => x.ApplicationHref == createdApplication.Href)
                .SingleOrDefaultAsync();

            refreshTokenForApplication.ShouldNotBeNull();

            (await refreshTokenForApplication.GetAccountAsync()).Href.ShouldBe(this.fixture.PrimaryAccountHref);
            (await refreshTokenForApplication.GetApplicationAsync()).Href.ShouldBe(createdApplication.Href);
            (await refreshTokenForApplication.GetTenantAsync()).Href.ShouldBe(this.fixture.TenantHref);

            // Clean up
            (await refreshTokenForApplication.DeleteAsync()).ShouldBeTrue();

            (await createdApplication.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedApplicationHrefs.Remove(createdApplication.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Validating_jwt(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            // Create a dummy application
            var createdApplication = await tenant.CreateApplicationAsync(
                $".NET IT {this.fixture.TestRunIdentifier}-{clientBuilder.Name} Validating JWT",
                createDirectory: false);
            createdApplication.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(createdApplication.Href);

            // Add the test accounts
            await createdApplication.AddAccountStoreAsync(this.fixture.PrimaryDirectoryHref);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin("lskywalker@tattooine.rim")
                .SetPassword("whataPieceofjunk$1138")
                .SetAccountStore(this.fixture.PrimaryDirectoryHref)
                .Build();
            var authenticateResult = await createdApplication.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(passwordGrantRequest);
            var accessTokenJwt = authenticateResult.AccessTokenString;

            var jwtAuthenticationRequest = OauthRequests.NewJwtAuthenticationRequest()
                .SetJwt(accessTokenJwt)
                .Build();
            var validAccessToken = await createdApplication.NewJwtAuthenticator()
                .AuthenticateAsync(jwtAuthenticationRequest);

            validAccessToken.ShouldNotBeNull();
            validAccessToken.Href.ShouldBe(authenticateResult.AccessTokenHref);

            // Clean up
            (await validAccessToken.DeleteAsync()).ShouldBeTrue();

            (await createdApplication.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedApplicationHrefs.Remove(createdApplication.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Validating_jwt_locally(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();

            // Create a dummy application
            var createdApplication = await tenant.CreateApplicationAsync(
                $".NET IT {this.fixture.TestRunIdentifier}-{clientBuilder.Name} Validating JWT Locally",
                createDirectory: false);
            createdApplication.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(createdApplication.Href);

            // Add the test accounts
            await createdApplication.AddAccountStoreAsync(this.fixture.PrimaryDirectoryHref);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin("lskywalker@tattooine.rim")
                .SetPassword("whataPieceofjunk$1138")
                .SetAccountStore(this.fixture.PrimaryDirectoryHref)
                .Build();
            var authenticateResult = await createdApplication.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(passwordGrantRequest);
            var accessTokenJwt = authenticateResult.AccessTokenString;

            var jwtAuthenticationRequest = OauthRequests.NewJwtAuthenticationRequest()
                .SetJwt(accessTokenJwt)
                .Build();
            var validAccessToken = await createdApplication.NewJwtAuthenticator()
                .WithLocalValidation()
                .AuthenticateAsync(jwtAuthenticationRequest);

            validAccessToken.ShouldNotBeNull();
            validAccessToken.Href.ShouldBe(authenticateResult.AccessTokenHref);

            // Clean up
            (await validAccessToken.DeleteAsync()).ShouldBeTrue();

            (await createdApplication.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedApplicationHrefs.Remove(createdApplication.Href);
        }

        //TODO: ID Site Token Authentication exchange

        //TODO: Refresh token grant
    }
}
