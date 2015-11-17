﻿// <copyright file="ExpandedPropertyConverter.cs" company="Stormpath, Inc.">
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
using System.Collections.Generic;
using Stormpath.SDK.Impl.DataStore;
using Stormpath.SDK.Impl.Resource;

namespace Stormpath.SDK.Impl.Serialization.FieldConverters
{
    internal sealed class ExpandedPropertyConverter : AbstractFieldConverter
    {
        private readonly Func<IDictionary<string, object>, Type, IDictionary<string, object>> converter;

        public ExpandedPropertyConverter(Func<IDictionary<string, object>, Type, IDictionary<string, object>> converter)
            : base(nameof(ExpandedPropertyConverter), appliesToTargetType: AnyType)
        {
            this.converter = converter;
        }

        protected override FieldConverterResult ConvertImpl(KeyValuePair<string, object> token)
        {
            var asEmbeddedResource = token.Value as IDictionary<string, object>;
            if (asEmbeddedResource == null)
                return FieldConverterResult.Failed;

            if (asEmbeddedResource.Count <= 1)
                return FieldConverterResult.Failed;

            if (this.converter == null)
                throw new ApplicationException($"Could not parse expanded property data from attribute '{token.Key}'.");

            var embeddedType = new ResourceTypeLookup().GetInterface(token.Key);
            var convertedData = this.converter(asEmbeddedResource, embeddedType);

            return new FieldConverterResult(true, new ExpandedProperty(convertedData));
        }
    }
}
