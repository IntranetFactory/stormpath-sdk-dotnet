﻿// <copyright file="DefaultResourceFactory.cs" company="Stormpath, Inc.">
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stormpath.SDK.Shared.Extensions;
using Stormpath.SDK.Impl.IdentityMap;
using Stormpath.SDK.Impl.Resource;
using Map = System.Collections.Generic.IDictionary<string, object>;

namespace Stormpath.SDK.Impl.DataStore
{
    internal sealed class DefaultResourceFactory : IResourceFactory
    {
        private readonly IInternalDataStore dataStore;
        private readonly IIdentityMap<ResourceData> identityMap;
        private readonly ResourceTypeLookup typeLookup;

        private bool isDisposed = false; // To detect redundant calls

        public DefaultResourceFactory(IInternalDataStore dataStore, IIdentityMap<ResourceData> identityMap)
        {
            this.dataStore = dataStore;
            this.identityMap = identityMap;
            this.typeLookup = new ResourceTypeLookup();
        }

        private IResourceFactory AsInterface => this;

        T IResourceFactory.Create<T>(ILinkable original)
            => (T)this.AsInterface.Create(typeof(T), null, original);

        private object Create(Type type, ILinkable original)
            => this.AsInterface.Create(type, null, original);

        T IResourceFactory.Create<T>(Map properties, ILinkable original)
            => (T)this.AsInterface.Create(typeof(T), properties, original);

        object IResourceFactory.Create(Type type, Map properties, ILinkable original)
        {
            if (this.typeLookup.IsCollectionResponse(type))
            {
                return this.InstantiateCollection(type, properties);
            }

            return this.InstantiateSingle(type, properties, original);
        }

        private object InstantiateSingle(Type type, Map properties, ILinkable original)
        {
            var resolvedType = new TypeResolver().Resolve(type, properties);

            var typeName = resolvedType.Name;

            var targetType = this.typeLookup.GetConcrete(resolvedType);
            if (targetType == null)
            {
                throw new Exception($"Unknown resource type {typeName}");
            }

            var identityMapOptions = new IdentityMapOptionsResolver().GetOptions(resolvedType);

            AbstractResource targetObject;
            try
            {
                string id = RandomResourceId(typeName);

                if (properties == null)
                {
                    properties = new Dictionary<string, object>();
                }

                object href = null;
                bool propertiesContainsHref =
                    properties.TryGetValue("href", out href) &&
                    href != null;
                if (propertiesContainsHref)
                {
                    id = $"{typeName}/{href.ToString()}";
                }

                if (!propertiesContainsHref)
                {
                    properties["href"] = id;
                }

                var resourceData = identityMapOptions.SkipIdentityMap
                    ? new ResourceData(this.dataStore)
                    : this.identityMap.GetOrAdd(id, () => new ResourceData(this.dataStore), identityMapOptions.StoreWithInfiniteExpiration);

                if (properties != null)
                {
                    resourceData.Update(properties);
                }

                targetObject = Activator.CreateInstance(targetType, new object[] { resourceData }) as AbstractResource;

                var notifyableTarget = targetObject as INotifiable;
                if (notifyableTarget != null)
                {
                    notifyableTarget.OnUpdate(properties, this.dataStore);
                }

                if (original != null)
                {
                    original.Link(resourceData);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error creating resource type {targetType.Name}", e);
            }

            return targetObject;
        }

        private object InstantiateCollection(Type collectionType, Map properties)
        {
            Type innerType = this.typeLookup.GetInnerCollectionInterface(collectionType);
            var targetType = this.typeLookup.GetConcrete(innerType);
            if (innerType == null || targetType == null)
            {
                throw new Exception($"Error creating collection resource: unknown inner type '{innerType?.Name}'.");
            }

            if (properties == null)
            {
                throw new Exception($"Unable to create collection resource of type {innerType.Name}: no properties to materialize with.");
            }

            long offset, limit, size;
            try
            {
                offset = Convert.ToInt64(properties["offset"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create collection resource of type {innerType.Name}: invalid 'offset' value.", ex);
            }

            try
            {
                limit = Convert.ToInt64(properties["limit"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create collection resource of type {innerType.Name}: invalid 'limit' value.", ex);
            }

            try
            {
                size = Convert.ToInt64(properties["size"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create collection resource of type {innerType.Name}: invalid 'size' value.", ex);
            }

            var href = properties["href"]?.ToString();
            if (string.IsNullOrEmpty(href))
            {
                throw new Exception($"Unable to create collection resource of type {innerType.Name}: invalid 'href' value.");
            }

            var items = properties["items"] as IEnumerable<object>;
            if (items == null)
            {
                throw new Exception($"Unable to create collection resource of type {innerType.Name}: 'items' sub-collection is missing.");
            }

            if (!items.Any())
            {
                // Empty list of innerType
                return this.CreateCollection(collectionType, innerType, offset, limit, size, href, Enumerable.Empty<object>());
            }

            var itemMaps = items as IEnumerable<Map>;
            if (itemMaps == null)
            {
                throw new Exception($"Unable to create collection resource of type {innerType.Name}: 'items' sub-collection is nonempty but not a collection of maps.");
            }

            try
            {
                var materializedItems = itemMaps
                    .Select(item => this.InstantiateSingle(innerType, item, original: null));

                return this.CreateCollection(collectionType, innerType, offset, limit, size, href, materializedItems);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to create collection resource of type {innerType.Name}: failed to add items to collection.", e);
            }
        }

        private object CreateCollection(Type collectionType, Type innerType, long offset, long limit, long size, string href, IEnumerable<object> items)
        {
            var typeOfListType = typeof(List<>).MakeGenericType(innerType);
            var listOfMaterializedItems = typeOfListType.GetTypeInfo()
                .GetDefaultConstructor()
                .Invoke(Type.EmptyTypes);
            var addMethod = typeOfListType.GetTypeInfo()
                .GetDeclaredMethod("Add");

            foreach (var item in items)
            {
                addMethod.Invoke(listOfMaterializedItems, new object[] { item });
            }

            var targetObject = Activator.CreateInstance(collectionType, new object[] { this.dataStore.Client, href, offset, limit, size, listOfMaterializedItems });
            return targetObject;
        }

        private static string RandomResourceId(string typeName)
            => $"autogen://{typeName}/{Guid.NewGuid().ToString().ToLowerInvariant().Replace("-", string.Empty)}";

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.identityMap.Dispose();
                }

                this.isDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}
