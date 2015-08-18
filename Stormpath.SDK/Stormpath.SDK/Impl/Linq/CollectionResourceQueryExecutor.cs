﻿// <copyright file="CollectionResourceQueryExecutor.cs" company="Stormpath, Inc.">
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.Linq;
using Stormpath.SDK.Impl.DataStore;
using Stormpath.SDK.Impl.Linq.RequestModel;
using Stormpath.SDK.Impl.Resource;

namespace Stormpath.SDK.Impl.Linq
{
    internal sealed class CollectionResourceQueryExecutor : IQueryExecutor
    {
        public CollectionResourceQueryExecutor(string url, string resource, IDataStore dataStore)
        {
            this.Url = url;
            this.Resource = resource;
            this.DataStore = dataStore;
        }

        public string Url { get; private set; }

        public string Resource { get; private set; }

        public IDataStore DataStore { get; private set; }

        public IEnumerable<T> ExecuteCollection<T>(CollectionResourceRequestModel requestModel)
        {
            var asyncCollection = new CollectionResourceQueryable<T>(this.Url, this.Resource, this.DataStore, requestModel);
            var adapter = new Sync.SyncCollectionEnumeratorAdapter<T>(asyncCollection);

            return adapter;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var model = GenerateRequestModel(queryModel);

            return ExecuteCollection<T>(model);
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            // TODO support a synchronous path for scalar result operators like Count, Any
            throw new NotSupportedException("This operation is not supported. Use async methods.");
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            var model = GenerateRequestModel(queryModel);

            return returnDefaultWhenEmpty
                ? ExecuteCollection<T>(queryModel).SingleOrDefault()
                : ExecuteCollection<T>(queryModel).Single();
        }

        private static MethodInfo GetGenericExecuteCollectionMethod(Type innerType)
        {
            var method = typeof(CollectionResourceQueryExecutor).GetMethod(nameof(ExecuteCollection), new Type[] { typeof(CollectionResourceRequestModel) });
            var generic = method.MakeGenericMethod(innerType);
            return generic;
        }

        private static CollectionResourceRequestModel GenerateRequestModel(QueryModel queryModel)
        {
            return CollectionResourceQueryModelVisitor.GenerateRequestModel(queryModel);
        }
    }
}