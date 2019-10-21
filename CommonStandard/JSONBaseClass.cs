using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonStandard
{
    public class JSONBaseClass
    {
        protected string ToJSON()
        {
            string result = "";
            try
            {
                result = JsonSerializer.Serialize(this, this.GetType(), new JsonSerializerOptions() { WriteIndented = true }); 
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public static T FromJSONString<T>(string jsonContent, T defValue = default(T))
        {
            T result = defValue;
            try
            {
                result = JsonSerializer.Deserialize<T>(jsonContent);
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
