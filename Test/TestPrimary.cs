using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

namespace SerializationUtil
{
    [TestFixture()]
    public class TestPrimary
    {
        [Test()]
        public void TestByte() {
            byte b = 16;
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, b, true);
            stream.Position = 0;
            byte nb = (byte)SerializationUtil.Deserialize(stream);
            Assert.AreEqual(nb, 16);
        }

        [Test()]
        public void TestBool()
        {
            bool b = true;
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, b, true);
            stream.Position = 0;
            bool nb = (bool)SerializationUtil.Deserialize(stream);
            Assert.AreEqual(nb, true);
        }

        [Test()]
        public void TestShort() {
            short s = 23;
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, s, true);
            stream.Position = 0;
            short ns = (short)SerializationUtil.Deserialize(stream);
            Assert.AreEqual(ns, ns);
        }

        [Test()]
        public void TestInt() {
            int i = 128;
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, i, true);
            stream.Position = 0;
            int ni = (int)SerializationUtil.Deserialize(stream);
            Assert.AreEqual(ni, i);
        }

        [Test()]
        public void TestFloat() {
            float f = 3.1415926f;
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, f, true);
            stream.Position = 0;
            float nf = (float)SerializationUtil.Deserialize(stream);
            Assert.LessOrEqual(Math.Abs(nf - f), float.Epsilon);
        }

        [Test()]
        public void TestDouble() {
            double d = 3.1415926;
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, d, true);
            stream.Position = 0;
            double nd = (double)SerializationUtil.Deserialize(stream);
            Assert.LessOrEqual(Math.Abs(nd - d), double.Epsilon);
        }

        [Test()]
        public void TestString()
        {
            string str = "hello, world";
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, str, true);
            stream.Position = 0;
            string nStr = SerializationUtil.Deserialize(stream) as string;
            Console.WriteLine(nStr);
            Assert.AreEqual(nStr, str);
        }
    }
}
