using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using RazorEngine.Text;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RocketContentAPI.Components
{
    public class RocketContentAPITokens<T> : DNNrocketAPI.render.DNNrocketTokens<T>
    {

        public IEncodedString RenderHandleBarsRC(Dictionary<string, SimplisityInfo> dataObjects, AppThemeLimpet appTheme, string templateName, string moduleref = "", string cacheKey = "")
        {
            var strOut = "";
            if (cacheKey != "") strOut = (string)CacheUtils.GetCache(moduleref + cacheKey, "hbs");
            if (String.IsNullOrEmpty(strOut))
            {
                string jsonString = SimplisityUtils.ConvertToJson(dataObjects);
                var template = appTheme.GetTemplate(templateName, moduleref);
                JObject model = JObject.Parse(jsonString);
                HandlebarsEngineRC hbEngine = new HandlebarsEngineRC();
                strOut = hbEngine.ExecuteRC(template, model);
                if (cacheKey != "") CacheUtils.SetCache(moduleref + cacheKey, strOut, "hbs");
            }
            return new RawString(strOut);
        }

    }
}
