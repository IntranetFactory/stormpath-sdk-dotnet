﻿// <copyright file="ITenantActions.cs" company="Stormpath, Inc.">
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

using System;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Linq;

namespace Stormpath.SDK.Tenant
{
    /// <summary>
    /// Represents common tenant actions that can be executed on a <see cref="ITenant"/> instance
    /// <i>or</i> a <see cref="Client.IClient"/> instance acting on behalf of its current tenant.
    /// </summary>
    public interface ITenantActions
    {
        /// <summary>
        /// Creates a new <see cref="Application.IApplication"/> resource in the current tenant.
        /// </summary>
        /// <param name="application">The <see cref="IApplication"/> to create.</param>
        /// <param name="creationOptionsAction">An inline builder for an instance of <see cref="ApplicationCreationOptionsBuilder"/>,
        /// which will be used when sending the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the created <see cref="IApplication"/>.</returns>
        /// <exception cref="Error.ResourceException">There was a problem creating the application.</exception>
        Task<IApplication> CreateApplicationAsync(IApplication application, Action<ApplicationCreationOptionsBuilder> creationOptionsAction, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a new <see cref="Application.IApplication"/> resource in the current tenant.
        /// </summary>
        /// <param name="application">The <see cref="IApplication"/> to create.</param>
        /// <param name="creationOptions">An <see cref="IApplicationCreationOptions"/> instance to use when sending the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the created <see cref="IApplication"/>.</returns>
        /// <exception cref="Error.ResourceException">There was a problem creating the application.</exception>
        Task<IApplication> CreateApplicationAsync(IApplication application, IApplicationCreationOptions creationOptions, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a new <see cref="Application.IApplication"/> resource in the current tenant, with the default creation options.
        /// </summary>
        /// <param name="application">The <see cref="IApplication"/> to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the created <see cref="IApplication"/>.</returns>
        /// <exception cref="Error.ResourceException">There was a problem creating the application.</exception>
        Task<IApplication> CreateApplicationAsync(IApplication application, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a new <see cref="Application.IApplication"/> resource in the current tenant.
        /// </summary>
        /// <param name="name">The name of the application.</param>
        /// <param name="createDirectory">Whether a default directory should be created automatically.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the created <see cref="IApplication"/>.</returns>
        /// <exception cref="Error.ResourceException">There was a problem creating the application.</exception>
        Task<IApplication> CreateApplicationAsync(string name, bool createDirectory, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets a queryable list of all accounts in this tenant.
        /// </summary>
        /// <returns>An <see cref="IAsyncQueryable{IAccount}"/> that may be used to asynchronously list or search accounts.</returns>
        /// <example>
        ///     var allAccounts = await myTenant.GetAccounts().ToListAsync();
        /// </example>
        IAsyncQueryable<IAccount> GetAccounts();

        /// <summary>
        /// Gets a queryable list of all applications in this tenant.
        /// </summary>
        /// <returns>An <see cref="IAsyncQueryable{IApplication}"/> that may be used to asynchronously list or search applications.</returns>
        /// <example>
        ///     var allApplications = await myTenant.GetApplications().ToListAsync();
        /// </example>
        IAsyncQueryable<IApplication> GetApplications();

        /// <summary>
        /// Gets a queryable list of all directories in this tenant.
        /// </summary>
        /// <returns>An <see cref="IAsyncQueryable{IDirectory}"/> that may be used to asynchronously list or search directories.</returns>
        /// <example>
        ///     var allDirectories = await myTenant.GetDirectories().ToListAsync();
        /// </example>
        IAsyncQueryable<IDirectory> GetDirectories();
    }
}
