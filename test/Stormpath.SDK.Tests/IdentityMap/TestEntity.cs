﻿// <copyright file="TestEntity.cs" company="Stormpath, Inc.">
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

namespace Stormpath.SDK.Tests.IdentityMap
{
    public class TestEntity
    {
        public string Id { get; }

        public int Count { get; private set; }

        public TestEntity(string id)
        {
            this.Id = id;
        }

        public void SetCount(int count)
        {
            this.Count = count;
        }
    }
}
