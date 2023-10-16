using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketContentAPI.Components
{
    public static class RocketContentAPIUtils
    {
        public const string ControlPath = "/DesktopModules/DNNrocketModules/RocketContentAPI";
        public const string ResourcePath = "/DesktopModules/DNNrocketModules/RocketContentAPI/App_LocalResources";
        public static List<SimplisityRecord> DependanciesList(int portalId, string moduleRef, SessionParams sessionParam)
        {
            var rtn = new List<SimplisityRecord>();
            var dataObject = new DataObjectLimpet(portalId, moduleRef, "", sessionParam, false);
            if (dataObject.AppThemeView != null && dataObject.AppThemeView.Exists)
            {
                foreach (var depfile in dataObject.AppThemeView.GetTemplatesDep())
                {
                    var dep = dataObject.AppThemeView.GetDep(depfile.Key, moduleRef);
                    foreach (var r in dep.GetRecordList("deps"))
                    {
                        var urlstr = r.GetXmlProperty("genxml/url");
                        if (urlstr.Contains("{"))
                        {
                            if (dataObject.PortalData != null) urlstr = urlstr.Replace("{domainurl}", dataObject.PortalData.EngineUrlWithProtocol);
                            if (dataObject.AppThemeView != null) urlstr = urlstr.Replace("{appthemefolder}", dataObject.AppThemeView.AppThemeVersionFolderRel);
                            if (dataObject.AppThemeSystem != null) urlstr = urlstr.Replace("{appthemesystemfolder}", dataObject.AppThemeSystem.AppThemeVersionFolderRel);
                        }
                        r.SetXmlProperty("genxml/id", CacheUtils.Md5HashCalc(urlstr));
                        r.SetXmlProperty("genxml/url", urlstr);
                        rtn.Add(r);
                    }
                }
            }
            return rtn;
        }
        public static string DisplayView(int portalId, string systemKey, string moduleRef, string rowKey, SessionParams sessionParam, string template = "view.cshtml", string noAppThemeReturn= "")
        {
            var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, systemKey, sessionParam.ModuleId, sessionParam.TabId);
            var cacheKey = moduleRef + sessionParam.CultureCode + template;
            var pr = (RazorProcessResult)CacheUtils.GetCache(cacheKey, moduleRef);
            if (moduleSettings.DisableCache || pr == null)
            {
                var dataObject = new DataObjectLimpet(portalId, moduleRef, rowKey, sessionParam, false);
                if (!dataObject.ModuleSettings.HasAppThemeAdmin) return noAppThemeReturn; // test on Admin Theme.
                var razorTempl = dataObject.AppThemeView.GetTemplate(template, moduleRef);
                pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
                CacheUtils.SetCache(cacheKey, pr, moduleRef);
            }
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string DisplayAdminView(int portalId, string moduleRef, string rowKey, SessionParams sessionParam, string template = "AdminDetail.cshtml", string noAppThemeReturn = "")
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, rowKey, sessionParam, true);
            if (!dataObject.ModuleSettings.HasAppThemeAdmin) return noAppThemeReturn;

            var razorTempl = dataObject.AppThemeAdmin.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string DisplaySystemView(int portalId, string moduleRef, SessionParams sessionParam, string template, bool editMode = true)
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, "", sessionParam, editMode);
            if (dataObject.AppThemeSystem == null) return "No System View";

            var razorTempl = dataObject.AppThemeSystem.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string ResourceKey(string resourceKey, string resourceExt = "Text", string cultureCode = "")
        {
            return DNNrocketUtils.GetResourceString(ResourcePath, resourceKey, resourceExt, cultureCode);
        }        
        public static string TokenReplacementCultureCode(string str, string CultureCode)
        {
            if (CultureCode == "") return str;
            str = str.Replace("{culturecode}", CultureCode);
            var s = CultureCode.Split('-');
            if (s.Count() == 2)
            {
                str = str.Replace("{language}", s[0]);
                str = str.Replace("{country}", s[1]);
            }
            return str;
        }
        public static List<SimplisityRecord> GetAllRecordART(int portalId, int moduleid = -1, string tableName = "RocketContentAPI")
        {
            var objCtrl = new DNNrocketController();
            var rtn = new List<SimplisityRecord>();
            var l = objCtrl.GetList(portalId, moduleid, "ART", "", "", "", 0, 0, 0, 0, tableName);
            foreach (var a in l)
            {
                a.RemoveLangRecord();
                rtn.Add(a);
            }
            return rtn;
        }
        public static List<SimplisityRecord> GetAllRecordARTLANG(int portalId, int moduleid = -1, string tableName = "RocketContentAPI")
        {
            var objCtrl = new DNNrocketController();
            var rtn = new List<SimplisityRecord>();
            var l = objCtrl.GetList(portalId, moduleid, "ARTLANG", "", "", "", 0, 0, 0, 0, tableName);
            foreach (var a in l)
            {
                a.RemoveLangRecord();
                rtn.Add(a);
            }
            return rtn;
        }

    }

}
