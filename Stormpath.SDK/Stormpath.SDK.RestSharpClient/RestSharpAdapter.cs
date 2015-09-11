﻿// <copyright file="RestSharpAdapter.cs" company="Stormpath, Inc.">
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

namespace Stormpath.SDK.Extensions.Http
{
    internal class RestSharpAdapter
    {
        public RestSharp.IRestRequest ToRestRequest(SDK.Http.IHttpRequest request)
        {
            var method = this.ConvertMethod(request.Method);
            var restRequest = new RestSharp.RestRequest(request.CanonicalUri.ToUri(), method);
            restRequest.RequestFormat = RestSharp.DataFormat.Json;

            if (request.HasBody)
            {
                restRequest.AddParameter(new RestSharp.Parameter()
                {
                    Type = RestSharp.ParameterType.RequestBody,
                    Value = request.Body
                });
            }

            this.CopyHeaders(request.Headers, restRequest);

            return restRequest;
        }

        public SDK.Http.IHttpResponse ToHttpResponse(RestSharp.IRestResponse response)
        {
            var responseErrorType = SDK.Http.ResponseErrorType.None;
            var responseMessages = new List<string>();

            if (response.ResponseStatus == RestSharp.ResponseStatus.TimedOut ||
                response.ResponseStatus == RestSharp.ResponseStatus.Aborted ||
                response.ResponseStatus == RestSharp.ResponseStatus.Error)
                responseErrorType = SDK.Http.ResponseErrorType.Recoverable;

            if (response.ErrorException != null)
                responseMessages.Add(response.ErrorException.Message);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
                responseMessages.Add(response.ErrorMessage);

            if (!string.IsNullOrEmpty(response.StatusDescription))
                responseMessages.Add(response.StatusDescription);

            var headers = this.ToHttpHeaders(response.Headers);

            return new Impl.Http.DefaultHttpResponse(
                (int)response.StatusCode,
                string.Join(Environment.NewLine, responseMessages),
                headers,
                response.Content,
                response.ContentType,
                responseErrorType);
        }

        private void CopyHeaders(SDK.Http.HttpHeaders httpHeaders, RestSharp.IRestRequest restRequest)
        {
            throw new NotImplementedException();
        }

        private SDK.Http.HttpHeaders ToHttpHeaders(IList<RestSharp.Parameter> restSharpHeaders)
        {
            var result = new SDK.Http.HttpHeaders();

            foreach (var header in restSharpHeaders.Where(x => x.Type == RestSharp.ParameterType.HttpHeader))
            {
                result.Add(header.Name, header.Value);
            }

            return result;
        }

        private RestSharp.Method ConvertMethod(SDK.Http.HttpMethod httpMethod)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}