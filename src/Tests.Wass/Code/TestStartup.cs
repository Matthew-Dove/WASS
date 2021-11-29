using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Wass.Code.Infrastructure;

namespace Tests.Wass.Code
{
    [TestClass]
    public class TestStartup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext _)
        {
            if (Debugger.IsAttached) Log.SetLogger(trace => Debug.WriteLine(trace), error => Debug.WriteLine(error));
            else Log.SetLogger(trace => Console.Out.WriteLine(trace), error => Console.Error.WriteLine(error));
        }
    }
}
