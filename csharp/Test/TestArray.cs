using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

namespace SerializationUtil
{
    [TestFixture()]
    public class TestArray
    {
        [Test()]
        public void TestIntArray()
        {
            int[] iArr = new int[] { 1, 1, 2, 3, 5, 8 };
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, iArr);
            stream.Position = 0;
            int[] niArr = SerializationUtil.Deserialize(stream) as int[];
            for (int i = 0; i < niArr.Length; i++)
            {
                Assert.AreEqual(niArr[i], iArr[i]);
            }
        }

        [Test()]
        public void TestStringArray()
        {
            string[] sArr = new string[] { "hello", "world", "code" };
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, sArr);
            stream.Position = 0;
            string[] nsArr = SerializationUtil.Deserialize(stream) as string[];
            for (int i = 0; i < nsArr.Length; i++)
            {
                Assert.AreEqual(nsArr[i], sArr[i]);
            }
        }

        [Test()]
        public void TestCustomTypeArray()
        {
            SerializationUtil.RegisterCustomType(typeof(Hero), (byte)'h', Hero.Serialize, Hero.Deserialize);
            Hero[] hArr = new Hero[3];
            for (int i = 0; i < 3; i++)
            {
                hArr[i] = new Hero()
                {
                    Name = string.Format("Name: {0}", i),
                    Gold = i * 100
                };
            }
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, hArr);
            stream.Position = 0;
            Hero[] nhArr = SerializationUtil.Deserialize(stream) as Hero[];
            for (int i = 0; i < nhArr.Length; i++)
            {
                Hero h = hArr[i];
                Hero nh = nhArr[i];
                Assert.AreEqual(h.Name, nh.Name);
                Assert.AreEqual(h.Gold, nh.Gold);
            }
        }
    }
}
