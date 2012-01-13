using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinimalisticCQRS.Infrastructure
{
    public abstract class AR
    {
        public string Id;
        public dynamic Apply;
    }
}