using System.Dynamic;
using System.Linq;

namespace MinimalisticCQRS.Infrastructure
{
    public class MiniVan: DynamicObject
    {
        MiniVanRegistry Registry;

        public MiniVan(MiniVanRegistry Registry)
        {
            this.Registry = Registry;
        }

        public virtual void Apply(Message msg)
        {
            var instances = Registry.InstancesForMessage(msg).ToArray();
            foreach (var inst in instances.Where(x=> x is AR))
                (inst as AR).Apply = this;
            foreach (var inst in instances)
                msg.InvokeOnInstanceIfPossible(inst, "Can");
            foreach (var inst in instances)
                msg.InvokeOnInstanceIfPossible(inst);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var msg = new Message(binder, args);
            Apply(msg);
            result = null;
            return true;
        }

    }
}