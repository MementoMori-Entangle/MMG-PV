using System;

namespace MMG.Dto
{
    [Serializable]
    public class Dto
    {
        public Dto() { }
    }

    public static class CopyHelper
    {
        public static T DeepCopy<T>(this T src)
        {
            /* セキュリティ向上のためMemoryStreamからJsonSerializerに変更
            using (MemoryStream stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, src);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
            */
            ReadOnlySpan<byte> b = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes<T>(src);
            return System.Text.Json.JsonSerializer.Deserialize<T>(b);
        }
    }
}
