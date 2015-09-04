﻿// <copyright file="NullCache.cs" company="Stormpath, Inc.">
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

using System.Threading;
using System.Threading.Tasks;
using Stormpath.SDK.Cache;

namespace Stormpath.SDK.Impl.Cache
{
    internal sealed class NullCache<K, V> : ISynchronousCache<K, V>, IAsynchronousCache<K, V>
    {
        string ICache<K, V>.Name => "NullCache";

        V ISynchronousCache<K, V>.Get(K key)
        {
            return default(V);
        }

        V ISynchronousCache<K, V>.Put(K key, V value)
        {
            return default(V);
        }

        V ISynchronousCache<K, V>.Remove(K key)
        {
            return default(V);
        }

        Task<V> IAsynchronousCache<K, V>.GetAsync(K key, CancellationToken cancellation)
        {
            return Task.FromResult(default(V));
        }

        Task<V> IAsynchronousCache<K, V>.PutAsync(K key, V value, CancellationToken cancellationToken)
        {
            return Task.FromResult(default(V));
        }

        Task<V> IAsynchronousCache<K, V>.RemoveAsync(K key, CancellationToken cancellationToken)
        {
            return Task.FromResult(default(V));
        }

        public override string ToString()
        {
            return "<null cache>";
        }
    }
}