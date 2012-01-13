using System;
using System.Collections.Generic;
using System.Linq;

namespace MinimalisticCQRS.Infrastructure
{
    public class MiniVanRegistry
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

        public IEnumerable<object> InstancesForMessage(Message msg)
        {
            foreach (var pn in msg.Parameters)
            {
                foreach (var mn in RegisteredARTypeIdNames.Where(x => x.Value == pn.Key))
                {
                    var ar = ResolveAR(mn.Key, pn.Value.ToString());
                    if (ar != null)
                        yield return ar;
                }
            }

            foreach (var c in NonArInstances)
            {
                yield return c;
            }
        }

        protected virtual AR ResolveAR(Type type, string id)
        {
            var ar = ARInstances.Where(x => x.Id == id && x.GetType() == type).FirstOrDefault();
            if (ar == null)
            {
                ar = (AR)Activator.CreateInstance(type);
                ar.Id = id;
                ARInstances.Add(ar);
            }
            return ar;
        }
    }

}