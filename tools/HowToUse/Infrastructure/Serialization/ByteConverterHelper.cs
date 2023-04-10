using System.Text;
using System.Text.Json;

namespace HowToUse.Infrastructure.Serialization
{
    public static class ByteConverterHelper
    {
        public static T? FromByteArray<T>(byte[]? data) => 
            data == null 
                ? default 
                : JsonSerializer.Deserialize<T>(data);

        public static byte[]? ObjectToByteArray(object? obj) => 
            obj == null 
                ? null 
                : Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
    }
}