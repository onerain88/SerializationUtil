using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

namespace SerializationUtil
{
    [TestFixture()]
    public class TestList
    {
        [Test()]
        public void TestIntList() {
            List<int> iList = new List<int>() { 1, 1, 2, 3, 5, 8 };
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, iList);
            stream.Position = 0;
            List<int> niList = SerializationUtil.Deserialize(stream) as List<int>;
            for (int i = 0; i < niList.Count; i++)
            {
                Assert.AreEqual(niList[i], iList[i]);
            }
        }

        [Test()]
        public void TestStringList()
        {
            List<string> sList = new List<string>() { "hello", "world", "code" };
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, sList);
            stream.Position = 0;
            List<string> nsList = SerializationUtil.Deserialize(stream) as List<string>;
            for (int i = 0; i < nsList.Count; i++)
            {
                Assert.AreEqual(nsList[i], sList[i]);
            }
        }

        [Test()]
        public void TestCustomTypeList()
        {
            SerializationUtil.RegisterCustomType(typeof(Hero), (byte)'h', Hero.Serialize, Hero.Deserialize);
            List<Hero> hList = new List<Hero>();
            for (int i = 0; i < 3; i++)
            {
                var hero = new Hero()
                {
                    Name = string.Format("Name: {0}", i),
                    Gold = i * 100
                };
                hList.Add(hero);
            }
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, hList);
            stream.Position = 0;
            List<Hero> nhList = SerializationUtil.Deserialize(stream) as List<Hero>;
            for (int i = 0; i < nhList.Count; i++)
            {
                Hero h = hList[i];
                Hero nh = nhList[i];
                Assert.AreEqual(h.Name, nh.Name);
                Assert.AreEqual(h.Gold, nh.Gold);
            }
        }

        [Test()]
        public void TestObjectArray()
        {
            List<object> oList = new List<object>(3);
            oList.Add(123);
            oList.Add("hello, world");
            oList.Add(true);
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, oList);
            stream.Position = 0;
            List<object> noList = SerializationUtil.Deserialize(stream) as List<object>;
            Assert.AreEqual(noList[0], 123);
            Assert.AreEqual(noList[1], "hello, world");
            Assert.AreEqual(noList[2], true);
        }
    }
}
