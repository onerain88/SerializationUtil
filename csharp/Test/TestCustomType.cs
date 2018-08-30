using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

namespace SerializationUtil
{
    [TestFixture()]
    public class TestCustomType
    {
        [Test]
        public void TestHero()
        {
            SerializationUtil.RegisterCustomType(typeof(Hero), (byte)'h', Hero.Serialize, Hero.Deserialize);
            Hero hero = new Hero()
            {
                Name = "Li Lei",
                Gold = 123,
                ItemIdList = new List<int>() { 123, 456, 789 },
            };
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, hero);
            stream.Position = 0;
            Hero nHero = SerializationUtil.Deserialize(stream) as Hero;
            Assert.AreEqual(nHero.Name, "Li Lei");
            Assert.AreEqual(nHero.Gold, 123);
            for (int i = 0; i < nHero.ItemIdList.Count; i++) {
                Assert.AreEqual(nHero.ItemIdList[i], hero.ItemIdList[i]);
            }
        }
    }
}
