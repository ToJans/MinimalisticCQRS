using System;
using System.Collections.Generic;
using System.Linq;

namespace MinimalisticCQRS.Infrastructure
{

    // Example of a possible message interceptors to upgrade old messages
    // Currently the infrastructure to do this is not yet in place
    // These interceptor would be found due to it's type signatures and method names
    public class ExampleInterceptor
    {
        // this would intercept all messages before 2012/01/16 20:45
        public IEnumerable<Message> InterceptMessagesBefore_2012_01_16_20_45(Message msg)
        {
            switch (msg.MethodName) 
            {
                 case "SomeMessage":
                    // meters to kilometers means divide old msgs length by 1000
                    yield return new Message(msg.MethodName,msg.Parameters.Substitute("Length",x=>x.Value/1000)); 
                    break;
                case "AnotherMessage":
                    yield return new Message("AnotherBlah",msg.Parameters.Where(x=>x.Key == "TargetId"));
                    yield return new Message("AnotherBlah2",msg.Parameters.Where(x=>x.Key !="TargetId"));
                    break;
                 default:
                    yield return msg;
                    break;
            }
        }

        // this would intercept all "Foo" messages before 2011/12/12 00:00
        public IEnumerable<Message> InterceptFooMessagesBefore_2011_12_12_00_00(Message msg)
        {
            yield return new Message("Bar", msg.Parameters);
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