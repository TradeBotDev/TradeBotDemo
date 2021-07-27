using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Rest.Requests
{
    /// <summary>
    /// 
    /// Конвертер для записи json строки в http запросы.
    /// TODO
    /// Не должне использования для десериализации
    /// Конвертер игнорирует все поля, значение которых = null
    /// TODO
    /// Конвертер сериализирует перечисления в имена значений 
    /// (например OrderType Type = OrderType.Sell он сериадизирует в Type : Sell)
    /// написан по гайду : https://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm
    /// </summary>
    public class HttpContextConverter : JsonConverter
    {
        private readonly Type[] _types;

        public HttpContextConverter(params Type[] types)
        {
            
            _types = types;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                JObject o = t as JObject;
                //взяли все поля объекта которые равны null
                List<JProperty> properties = o.Properties().Where(prop => prop.Value is null).ToList();
                
                //удаляем такие поля
                foreach(var prop in properties)
                {
                    var res = o.Remove(prop.Name);
                    Console.WriteLine($"Property{prop.Name} was removed with result {res} from {o.GetType()}");
                }

                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return _types.Any(t => t == objectType);
        }
    }
}
