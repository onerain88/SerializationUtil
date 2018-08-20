using System;
using System.IO;

namespace SerializationUtil
{
    public class Hero
    {
        public string Name {
            get; set;
        }

        public int Gold {
            get; set;
        }

        public static void Serialize(MemoryStream stream, object obj) {
            Hero hero = obj as Hero;
            SerializationUtil.SerializeString(stream, hero.Name);
            SerializationUtil.SerializeInt(stream, hero.Gold);
        }

        public static object Deserialize(MemoryStream stream) {
            Hero hero = new Hero();
            hero.Name = SerializationUtil.DeserializeString(stream);
            hero.Gold = SerializationUtil.DeserializeInt(stream);
            return hero;
        }
    }
}
