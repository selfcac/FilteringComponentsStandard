using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;



namespace CommonStandard
{
    public class JSONBaseClass
    {
        protected string ToJSON()
        {
            string result = "";
            try
            {
                result = JsonConvert.SerializeObject(this,Formatting.Indented); 
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
                result = JsonConvert.DeserializeObject<T>(jsonContent);
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
