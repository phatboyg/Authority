// Copyright 2012-2016 Chris Patterson
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
namespace Authority.Runtime
{
    using System;
    using System.Linq;
    using GreenPipes.Internals.Extensions;
    using Microsoft.Extensions.Logging;


    public static class LoggerExtensions
    {
        public static IDisposable BeginScope<T>(this ILogger logger)
        {
            
            var message = typeof(T).IsGenericType
                ? $"{typeof(T).GetGenericTypeDefinition().Name.TrimEnd('1').TrimEnd('`')}<{string.Join(",", typeof(T).GetClosingArguments(typeof(T).GetGenericTypeDefinition()).Select(x => x.Name))}>"
                : $"{typeof(T).Name}>";

            logger.LogDebug(message);
            return logger.BeginScope(message);
        }
    }
}