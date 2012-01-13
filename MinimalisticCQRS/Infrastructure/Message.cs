using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace MinimalisticCQRS.Infrastructure
{
    public class Message
    {
        public IEnumerable<KeyValuePair<string, object>> Parameters;
        public String MethodName;

        public Message() { }

        public Message(string MethodName, IEnumerable<KeyValuePair<string, object>> Parameters)
        {
            this.MethodName = MethodName;
            this.Parameters = Parameters;
        }

        public Message(InvokeMemberBinder binder, object[] args)
        {
            MethodName = binder.Name;
            var names = new List<string>();
            while (binder.CallInfo.ArgumentNames.Count + names.Count < binder.CallInfo.ArgumentCount)
                names.Add(names.Count.ToString());
            names.AddRange(binder.CallInfo.ArgumentNames);
            Parameters = names.Select((x, i) => new KeyValuePair<string, object>(x, args[i]));
        }

        public void InvokeOnInstanceIfPossible(object instance, string prefix = "")
        {
            var mi = instance.GetType().GetMethod(prefix + MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi == null && prefix == "")
            {
                // maybe it is an event (i.e. prefixed with "On")
                InvokeOnInstanceIfPossible(instance, "On");
                return;
            }
            if (mi == null)
                return;
            var pars = mi.GetParameters()
                .Select((x, i) => Parameters
                    .Where(y => y.Key == i.ToString() || y.Key == x.Name)
                    .Select(y => y.Value).FirstOrDefault())
                .ToArray();
            mi.Invoke(instance, pars);
        }
    }
}