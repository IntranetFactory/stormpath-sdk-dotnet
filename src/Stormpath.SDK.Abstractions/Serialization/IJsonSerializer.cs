﻿// <copyright file="IJsonSerializer.cs" company="Stormpath, Inc.">
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

using Map = System.Collections.Generic.IDictionary<string, object>;

namespace Stormpath.SDK.Serialization
{
    /// <summary>
    /// A wrapper interface for JSON serialization plugins.
    /// </summary>
    /// <seealso cref="ISerializerFactory"/>
    public interface IJsonSerializer
    {
        /// <summary>
        /// Serializes the specified properties to JSON.
        /// </summary>
        /// <param name="map">The property data to serialize.</param>
        /// <returns>A JSON string representation of <paramref name="map"/></returns>
        string Serialize(Map map);

        /// <summary>
        /// Deserializes a JSON string to a property dictionary.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A tree of name-value pairs stored in an <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>.</returns>
        Map Deserialize(string json);
    }
}
