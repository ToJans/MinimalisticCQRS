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

        public Message(object msg)
        {
            this.MethodName = msg.GetType().Name;
            this.Parameters = msg.GetType().GetProperties().Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(msg, null)))
                .Union(msg.GetType().GetFields().Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(msg))));
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

        public override int GetHashCode()
        {
            return MethodName.GetHashCode() ^ Parameters.Aggregate(0, (feed, x) => feed ^ x.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return (obj as Message) == this;
        }

        public static bool operator ==(Message m1, Message m2)
        {
            if (ReferenceEquals(m1, m2)) return true;
            if (ReferenceEquals(null, m1) || ReferenceEquals(null, m2)) return false;
            if (m1.GetHashCode() != m2.GetHashCode()) return false;
            if (m1.MethodName != m2.MethodName) return false;
            if (m1.Parameters.Count() != m2.Parameters.Count()) return false;
            if (m1.Parameters.All(x => m2.Parameters.Contains(x))) return true;
            return false;
        }

        public static bool operator !=(Message m1, Message m2)
        {
            return !(m1 == m2);
        }
    }
}