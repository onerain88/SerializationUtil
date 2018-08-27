using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Serializer;
using SerializationUtil;

namespace Test
{
    [TestFixture]
    public class TestSerializer
    {
        [Test]
        public void TestInt() {
            int i = 23;
            var stream = new MemoryStream();
            Serializer.Serializer.Encode(stream, i);
            stream.Position = 0;
            int ni = (int)Serializer.Serializer.Decode(stream);
            Assert.AreEqual(ni, i);
        }

        [Test]
        public void TestString() {
            string s = "hello, world";
            var stream = new MemoryStream();
            Serializer.Serializer.Encode(stream, s);
            stream.Position = 0;
            string ns = Serializer.Serializer.Decode(stream) as string;
            Assert.AreEqual(s, ns);
        }

        [Test]
        public void TestIntList() {
            var list = new List<int>() { 1, 1, 2, 3, 5, 8 };
            var stream = new MemoryStream();
            Serializer.Serializer.Encode(stream, list);
            stream.Position = 0;
            var nList = Serializer.Serializer.Decode(stream) as List<object>;
            for (int i = 0; i < list.Count; i++) {
                Assert.AreEqual(list[i], nList[i]);
            }
        }

        [Test]
        public void TestStringList()
        {
            var list = new List<string>() { "hello", "world", "code" };
            var stream = new MemoryStream();
            Serializer.Serializer.Encode(stream, list);
            stream.Position = 0;
            var nList = Serializer.Serializer.Decode(stream) as List<object>;
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], nList[i]);
            }
        }

        [Test]
        public void TestIntStringDictionary() {
            var dict = new Dictionary<int, string>();
            dict.Add(1, "hello");
            dict.Add(2, "world");
            dict.Add(3, "code");
            var stream = new MemoryStream();
            Serializer.Serializer.Encode(stream, dict);
            stream.Position = 0;
            var nDict = Serializer.Serializer.Decode(stream) as Dictionary<object, object>;
            foreach (var entry in dict) {
                Assert.AreEqual(entry.Value, nDict[entry.Key]);
            }
        }

        [Test]
        public void TestNestedIntList() {
            var list = new List<List<int>>();
            for (int i = 0; i < 3; i++) {
                var sList = new List<int>() { 1 * i, 2 * i, 3 * i };
                list.Add(sList);
            }
            var stream = new MemoryStream();
            Serializer.Serializer.Encode(stream, list);
            stream.Position = 0;
            var nList = Serializer.Serializer.Decode(stream) as List<object>;
            for (int i = 0; i < nList.Count; i++) {
                var snList = nList[i] as List<object>;
                var sList = list[i];
                for (int j = 0; j < sList.Count; j++) {
                    Assert.AreEqual(sList[i], snList[i]);
                }
            }
        }

        [Test]
        public void TestCustomClass() {
            Serializer.Serializer.RegisterCustomType(typeof(Hero), (byte)'h', Hero.Encode, Hero.Decode);
            Hero hero = new Hero()
            {
                Name = "Li Lei",
                Gold = 100,
                ItemIdList = new List<int>() { 1, 1, 2, 3, 5, 8 }
            };
            var stream = new MemoryStream();
            Serializer.Serializer.Encode(stream, hero);
            stream.Position = 0;
            var nHero = Serializer.Serializer.Decode(stream) as Hero;
            Assert.AreEqual(hero.Name, nHero.Name);
            Assert.AreEqual(hero.Gold, nHero.Gold);
            for (int i = 0; i < hero.ItemIdList.Count; i++) {
                Assert.AreEqual(hero.ItemIdList[i], nHero.ItemIdList[i]);
            }
        }

        [Test]
        public void TestObjectArray() {
            var list = new List<object>() {
                123,
                "abc",
                (byte)'c',
            };
            var stream = new MemoryStream();
            Serializer.Serializer.Encode(stream, list);
            stream.Position = 0;
            var nList = Serializer.Serializer.Decode(stream) as List<object>;
            for (int i = 0; i < list.Count; i++) {
                Assert.AreEqual(list[i], nList[i]);
            }
        }
    }
}
