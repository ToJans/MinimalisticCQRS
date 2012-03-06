using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinimalisticCQRS.Infrastructure
{
    public class MethodBodyComparer : IEqualityComparer<MethodInfo>
    {
        public bool Equals(MethodInfo x, MethodInfo y)
        {
            if (x == null) return (y == null);
            if (y == null) return false;

            if (x.MetadataToken == y.MetadataToken) return true;

            // compare return types
            if (x.ReturnType != y.ReturnType) return false;

            // compare arguments
            var p1 = x.GetParameters();
            var p2 = y.GetParameters();
            if (p1.Length != p2.Length) return false;

            for (int i = 0; i < p1.Length; i++)
            {
                if (p1[i].IsIn != p2[i].IsIn || p1[i].IsOut != p2[i].IsOut) return false;
                if (p1[i].ParameterType != p2[i].ParameterType) return false;
            }

            // compare implementations
            var b1 = x.GetMethodBody();
            var b2 = y.GetMethodBody();

            return b1.GetILAsByteArray().SequenceEqual(b2.GetILAsByteArray());
        }

        public int GetHashCode(MethodInfo obj)
        {
            if (obj == null) return 0;

            int r = obj.ReturnType.MetadataToken;
            foreach (var p in obj.GetParameters())
                r = (r << 2) | p.MetadataToken;
            return r;
        }
    }
}