using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinimalisticCQRS.Infrastructure
{
    public static class Guard
    {
        public static void Against(bool assertion, string message)
        {
            if (assertion) throw new InvalidOperationException(message);
        }
    }

}