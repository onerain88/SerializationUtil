using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;

namespace SerializationUtil
{
    [TestFixture()]
    public class TestDictionary
    {
        [Test()]
        public void TestDict() {
            Dictionary<string, int> dict = new Dictionary<string, int>() {
                { "aaa", 111 },
                { "bbb", 222 },
                { "ccc", 333 }
            };
            MemoryStream stream = new MemoryStream();
            SerializationUtil.Serialize(stream, dict);
            stream.Position = 0;
            Dictionary<string, int> nDict = SerializationUtil.Deserialize(stream) as Dictionary<string, int>;
            foreach (var entry in nDict) {
                Assert.AreEqual(entry.Value, dict[entry.Key]);
            }
        }
    }
}
