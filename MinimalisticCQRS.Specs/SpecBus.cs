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

        protected Verifier verifier;

        public override void Apply(Message msg)
        {
            ResultingEvents.Add(msg);
            base.Apply(msg);
        }

        public SpecBus(MiniVanRegistry mvr)
            : base(mvr)
        {
            verifier = new Verifier(ResultingEvents);
        }

        public void GivenEvent(Action<dynamic> a)
        {
            a(this as dynamic);
            ResultingEvents.Clear();
        }

        public void WhenCommand(Action<dynamic> a)
        {
            ResultingEvents.Clear();
            try
            {
                verifier.Except = null;
                a(this as dynamic);
            }
            catch (Exception e)
            {
                if (e is System.Reflection.TargetInvocationException)
                    e = e.InnerException;
                verifier.Except = e;
            }
            ResultingEvents.RemoveAt(0);
        }

        public void ThenExpectEvent(Action<dynamic> a)
        {
            verifier.Then(a);
        }

        public void ThenDoNotExpectEvent(Action<dynamic> a)
        {
            verifier.ThenNot(a);
        }

        public void ThenExpectException<T>(Predicate<T> a = null) where T : Exception
        {
            verifier.ThenException<T>(a);
        }

        // support for message classes
        public void GivenEvent(object @event)
        {
            GivenEvent(x => x.Handle(@event));
        }

        public void WhenCommand(object command)
        {
            WhenCommand(x => x.Handle(command));
        }

        public void ThenExpectEvent(object @event)
        {
            verifier.Then(@event);
        }

        public void ThenDoNotExpectEvent(object @event)
        {
            verifier.ThenNot(@event);
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
                Then(msg);
            }

            public void Then(object o)
            {
                msg = (o as Message) ?? new Message(o);
                if (input.All(x => x != msg))
                {
                    var txt = MessageToText(msg);
                    var amsg = "Expected \n * " + txt + "\n but could not find it.";
                    var matches = input.Select(x => MessageToText(x))
                        .Select(x => new { distance = Levenshtein.Distance(txt, x), Event = x })
                        .OrderBy(x => x.distance)
                        .Take(5).Select(x => x.Event).ToArray();
                    amsg += "\nTop 5 of best possible matching events are:\n" + string.Join("\n * ", matches);
                    AssertFail(amsg);
                }
            }

            public void ThenNot(Action<dynamic> a)
            {
                a(this);
                ThenNot(msg);
            }

            public void ThenNot(object o)
            {
                msg = (o as Message) ?? new Message(o);
                if (input.Any(x => x == msg))
                {
                    AssertFail("Expected " + MessageToText(msg) + " but could not find it");
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
                sb.Append((kv.Value ?? "").ToString());
                if (kv.Key != msg.Parameters.Last().Key)
                    sb.Append(",");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}