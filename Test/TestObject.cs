﻿using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

namespace SerializationUtil
{
    [TestFixture()]
    public class TestObject
    {
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

        [Test]
        public void TestCustomType()
        {
            SerializationUtil.RegisterCustomType(typeof(Hero), (byte)'h', Hero.Serialize, Hero.Deserialize);
            Hero hero = new Hero()
            {
                Name = "Li Lei",
                Gold = 123,
            };
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, hero);
            stream.Position = 0;
            Hero nHero = SerializationUtil.Deserialize(stream) as Hero;
            Assert.AreEqual(nHero.Name, "Li Lei");
            Assert.AreEqual(nHero.Gold, 123);
        }
    }
}