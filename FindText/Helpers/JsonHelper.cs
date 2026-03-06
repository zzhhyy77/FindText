using System.Text.Json;

namespace FindText.Helpers
{
    internal class JsonHelper
    {
        public static string ToJson<T>(T obj,bool isCompress = true)
        {
            JsonSerializerOptions opt = new JsonSerializerOptions() { WriteIndented= !isCompress };
            return JsonSerializer.Serialize(obj, opt);
        }

        public static T? Parse<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }


        public static T Clone<T>(T obj)
        {
            var json = ToJson(obj);
            return Parse<T>(json);
        }
        
    }
}
