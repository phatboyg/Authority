﻿// Copyright 2012-2016 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Authority
{
    using Microsoft.Extensions.Logging;
    using Rules;


    public interface IAuthorityConfigurator
    {
        void AddRule(IRule rule);

        /// <summary>
        /// Specifies the logger factory to use for logging
        /// </summary>
        /// <param name="loggerFactory"></param>
        void SetLoggerFactory(ILoggerFactory loggerFactory);
    }
}