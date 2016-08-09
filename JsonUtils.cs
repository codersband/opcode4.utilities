using Newtonsoft.Json;

namespace opcode4.utilities
{
    public static class JsonUtils
    {      
        public static string ToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T Desserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T Desserialize<T>(dynamic json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        
        //public static dynamic Deserialize(string json)
        //{
        //    dynamic response = String.Empty;
        //    try
        //    {
        //        //var serializer = new JavaScriptSerializer();
        //        //response = serializer.Deserialize<dynamic>(json);

        //        var jss = new JavaScriptSerializer();
        //        jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

        //        response = jss.Deserialize(json, typeof(object)) as dynamic;
        //    }
        //    catch { }
        //    return response;
        //}
    }

    //public class DynamicJsonConverter : JavaScriptConverter
    //{
    //    public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
    //    {
    //        if (dictionary == null)
    //            throw new ArgumentNullException("dictionary");

    //        if (type == typeof(object))
    //        {
    //            return new DynamicJsonObject(dictionary);
    //        }

    //        return null;
    //    }

    //    public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override IEnumerable<Type> SupportedTypes
    //    {
    //        get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(object) })); }
    //    }
    //}

    //public class DynamicJsonObject : DynamicObject
    //{
    //    public IDictionary<string, object> Dictionary { get; set; }

    //    public DynamicJsonObject(IDictionary<string, object> dictionary)
    //    {
    //        this.Dictionary = dictionary;
    //    }

    //    public override bool TryGetMember(GetMemberBinder binder, out object result)
    //    {
    //        result = this.Dictionary[binder.Name];

    //        if (result is IDictionary<string, object>)
    //        {
    //            result = new DynamicJsonObject(result as IDictionary<string, object>);
    //        }
    //        else if (result is ArrayList && (result as ArrayList) is IDictionary<string, object>)
    //        {
    //            result = new List<DynamicJsonObject>((result as ArrayList).ToArray().Select(x => new DynamicJsonObject(x as IDictionary<string, object>)));
    //        }
    //        else if (result is ArrayList)
    //        {
    //            result = new List<object>((result as ArrayList).ToArray());
    //        }

    //        return this.Dictionary.ContainsKey(binder.Name);
    //    }

    //    public int Count { get { return Dictionary.Count; } }
    //}
}