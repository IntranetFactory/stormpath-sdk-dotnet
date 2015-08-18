﻿// <copyright file="DefaultResourceFactory.cs" company="Stormpath, Inc.">
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
using System.Collections;
using System.Collections.Generic;
using Stormpath.SDK.Account;
using Stormpath.SDK.Impl.Account;
using Stormpath.SDK.Impl.Tenant;
using Stormpath.SDK.Tenant;

namespace Stormpath.SDK.Impl.DataStore
{
    internal sealed class DefaultResourceFactory : IResourceFactory
    {
        private readonly Dictionary<Type, Type> typeMap = new Dictionary<Type, Type>()
        {
            { typeof(IAccount), typeof(DefaultAccount) },
            { typeof(ITenant), typeof(DefaultTenant) }
        };

        T IResourceFactory.CreateDefault<T>()
        {
            Type targetType;
            if (!typeMap.TryGetValue(typeof(T), out targetType))
                throw new ApplicationException($"Unknown resource type {typeof(T).Name}");

            var targetObject = Activator.CreateInstance(targetType) as T;
            if (targetObject == null)
                throw new ApplicationException($"Unable to create resource type {targetType.Name}");

            return targetObject;
        }

        T IResourceFactory.Deserialize<T>(Hashtable properties)
        {
            throw new NotImplementedException();
        }
    }
}