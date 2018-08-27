using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Test
{
    [TestFixture()]
    public class TestCSharp
    {
        [Test()]
        public void TestObject()
        {
            List<object> list = new List<object>();
            list.Add(1);
            list.Add(2);
            Type type = list.GetType();
            Console.WriteLine(type.GetGenericArguments()[0]);
        }
    }
}
