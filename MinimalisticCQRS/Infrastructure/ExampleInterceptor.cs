using System;
using System.Collections.Generic;
using System.Linq;

namespace MinimalisticCQRS.Infrastructure
{

    // Example of a possible message interceptor to upgrade old messages
    // Currently the infrastructure to do this is not yet in place
    public class ExampleInterceptor
    {
        // this Interceptor would be found due to it's special signature
        public IEnumerable<Message> InterceptMessagesBefore_2012_01_16_20_45(Message msg)
        {
            switch (msg.MethodName) 
            {
                 case "SomeMessage":
                    // meters to kilometers means divide old msgs length by 1000
                    yield return new Message(msg.MethodName,msg.Parameters.Substitute("Length",x=>x.Value/1000)); 
                    break;
                 default:
                    yield return msg;
                    break;
            }
        }
    }
    public static class IEnumerableKVPExtensions
    {
        public static IEnumerable<KeyValuePair<string, object>>
            Substitute(this IEnumerable<KeyValuePair<string, object>> input, string key, Func<dynamic, dynamic> replacement)
        {
            return input.Select(x => x.Key == key ? new KeyValuePair<string, object>(key, replacement(x.Value)) : x);
        }
    }
}