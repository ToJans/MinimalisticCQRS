using System;

namespace MinimalisticCQRS.Infrastructure
{
    public abstract class AR
    {
        public string Id;
        public dynamic ApplyEvent;
        public Action<object> Apply;
    }
}