using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS.Specs
{
    public class SpecBus : MiniVan
    {
        public readonly List<Message> ResultingEvents = new List<Message>();
        private Verifier _then;

        public override void Apply(Message msg)
        {
            ResultingEvents.Add(msg);
            base.Apply(msg);
        }

        public SpecBus(MiniVanRegistry mvr)
            : base(mvr)
        {
            _then = new Verifier(ResultingEvents);
        }

        public void Given(Action<dynamic> a)
        {
            a(this as dynamic);
            ResultingEvents.Clear();
        }

        public void When(Action<dynamic> a)
        {
            ResultingEvents.Clear();
            a(this as dynamic);
            ResultingEvents.RemoveAt(0);
        }

        public void Then(Action<dynamic> a)
        {
            _then.Then(a);
        }

        protected class Verifier : DynamicObject
        {
            public Verifier(List<Message> input)
            {
                this.input = input;
            }

            Message msg = null;
            public Exception Except = null;
            private List<Message> input;

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                msg = new Message(binder, args);
                result = null;
                return true;
            }

            public void Then(Action<dynamic> a)
            {
                a(this);
                if (!input.Any(x => x.MethodName == msg.MethodName &&
                    msg.Parameters.All(y => x.Parameters.Any(z => y.Key == z.Key && z.Value == y.Value))))
                {
                    AssertFail("Expected " + MessageToText(msg) + " but could not find it");
                }
            }

            public void ThenNot(Action<dynamic> a)
            {
                a(this);
                if (input.Any(x => x.MethodName == msg.MethodName &&
                    msg.Parameters.All(y => x.Parameters.Any(z => y.Key == z.Key && z.Value == y.Value))))
                {
                    AssertFail("Should not contain" + MessageToText(msg));
                }
            }

            public void ThenException<T>(Predicate<T> assertion = null) where T : Exception
            {
                if ((this.Except as T) == null)
                    AssertFail("Expected an exception of type " + typeof(T) + " but found none");
                if (assertion != null && !assertion(Except as T))
                {
                    AssertFail("Received an exception of type " + typeof(T) + " but the assertion did not match");
                }
            }

            private void AssertFail(string message)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(message);
            }
        }

        public static string MessageToText(Message msg)
        {
            var sb = new StringBuilder();
            sb.Append(msg.MethodName);
            sb.Append("(");
            foreach (var kv in msg.Parameters)
            {
                sb.Append(kv.Key);
                sb.Append("=");
                sb.Append(kv.Value.ToString());
                if (kv.Key != msg.Parameters.Last().Key)
                    sb.Append(",");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}