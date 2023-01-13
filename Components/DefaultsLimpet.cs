using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RocketContentAPI.Components
{
    public class DefaultsLimpet
    {
        private const string _defaultFileMapPath = "/DesktopModules/DNNrocketModules/RocketContentAPI/Installation/SystemDefaults.rules";
        public DefaultsLimpet()
        {
            try
            {
                Info = (SimplisityInfo)CacheUtils.GetCache(_defaultFileMapPath);
                if (Info == null)
                {
                    var filenamepath = DNNrocketUtils.MapPath(_defaultFileMapPath);
                    var xmlString = FileUtils.ReadFile(filenamepath);
                    Info = new SimplisityInfo();
                    Info.XMLData = xmlString;
                    CacheUtils.SetCache(_defaultFileMapPath, Info);
                }
            }
            catch (Exception)
            {
                CacheUtils.SetCache(_defaultFileMapPath, null);
            }
        }
        public SimplisityInfo Info { get; set; }

        public string Get(string xpath)
        {
            return Info.GetXmlProperty(xpath);
        }


    }
}
