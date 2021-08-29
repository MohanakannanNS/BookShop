using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class SessionExtension
    {
        public static void SetValue(this ISession session,string key,object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T GetValue<T>(this ISession session,string key)
        {
            var value=session.GetString(key);
            return (value == null) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
        //default set available for integer
    }
}
