using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;

namespace MinimalisticCQRS.Infrastructure
{

    public class MiniBus: DynamicObject
    {
        Dictionary<Type, string> RegisteredARTypeIdNames = new Dictionary<Type, string>();

        List<object> NonArInstances = new List<object>();

        List<AR> ARInstances = new List<AR>();

        public void RegisterArType<T>() where T : AR
        {
            RegisteredARTypeIdNames[typeof(T)] = typeof(T).Name + "Id";
        }

        public void RegisterNonArInstance(params object[] instances)
        {
            NonArInstances.AddRange(instances);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var msg = new Message(binder, args);
            ApplyInternal(msg,"Can");
            ApplyInternal(msg);
            result = null;
            return true;
        }

        protected virtual void ApplyInternal(Message msg,string prefix="")
        {
            foreach (var pn in msg.Parameters)
            {
                foreach (var mn in RegisteredARTypeIdNames.Where(x=>x.Value == pn.Key))
                {
                    var ar =ResolveAR(mn.Key,pn.Value.ToString());
                    msg.InvokeOnInstanceIfPossible(ar,prefix);
                }
            }
            foreach (var c in NonArInstances)
            {
                msg.InvokeOnInstanceIfPossible(c,prefix);
            }
        }

        private AR ResolveAR(Type type, string id)
        {
            var ar = ARInstances.Where(x => x.Id == id && x.GetType() == type).FirstOrDefault();
            if (ar == null)
            {
                ar = (AR)Activator.CreateInstance(type);
                ar.Apply = this;
                ar.Id = id;
                ARInstances.Add(ar);
            }
            return ar;
        }

        public class Message
        {
            public IEnumerable<KeyValuePair<string, object>> Parameters;
            public String MethodName;

            public Message(InvokeMemberBinder binder, object[] args)
            {
                MethodName = binder.Name;
                var names = new List<string>();
                while (binder.CallInfo.ArgumentNames.Count + names.Count < binder.CallInfo.ArgumentCount)
                    names.Add(names.Count.ToString());
                names.AddRange(binder.CallInfo.ArgumentNames);
                Parameters = names.Select((x, i) => new KeyValuePair<string,object>(x, args[i]));
            }

            public void InvokeOnInstanceIfPossible(object instance,string prefix="")
            {
                var mi = instance.GetType().GetMethod(prefix+MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi == null && prefix == "")
                {
                    // maybe it is an event (i.e. prefixed with "On")
                    InvokeOnInstanceIfPossible(instance, "On");
                    return;
                }
                if (mi == null)
                    return ;
                var pars = mi.GetParameters()
                    .Select((x, i) => Parameters
                        .Where(y => y.Key == i.ToString() || y.Key == x.Name)
                        .Select(y=>y.Value).FirstOrDefault())
                    .ToArray();
                mi.Invoke(instance, pars);
            }
        }
    }
}