using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.UI.Controllers
{
    // handy stuff here https://github.com/cefsharp/CefSharp/wiki/Frequently-asked-questions#CallJS
    public class Controller
    {
        protected string Json(object data)
        {
            var serialized = JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            // because we're now parsing JSON in JS executed from C#, all backslashes need to be double-escaped
            return serialized.Replace("\\", "\\\\");
        }
    }
}
