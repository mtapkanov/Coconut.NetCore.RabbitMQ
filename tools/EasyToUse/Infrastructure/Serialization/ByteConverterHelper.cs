using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyToUse.Infrastructure.Serialization
{
    public static class ByteConverterHelper
    {
        public static T FromByteArray<T>(byte[] data)
        {
            if(data == null)
                return default(T);
            var bf = new BinaryFormatter();
            using var ms = new MemoryStream(data);
            var obj = bf.Deserialize(ms);
            return (T)obj;
        }
        
        public static byte[] ObjectToByteArray(object obj)
        {
            if(obj == null)
                return null;
            var bf = new BinaryFormatter();
            using MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }
}