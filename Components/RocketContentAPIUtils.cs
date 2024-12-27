using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace RocketContentAPI.Components
{
    public static class RocketContentAPIUtils
    {
        public const string ControlPath = "/DesktopModules/DNNrocketModules/RocketContentAPI";
        public const string ResourcePath = "/DesktopModules/DNNrocketModules/RocketContentAPI/App_LocalResources";

        public static ArticleLimpet GetArticleData(ModuleContentLimpet moduleSettings, string cultureCode, bool useCache = true)
        {
            var cacheKey = moduleSettings.ModuleRef + "_" + cultureCode;
            var articleData = (ArticleLimpet)CacheUtils.GetCache(cacheKey, moduleSettings.ModuleRef);
            if (articleData == null || !useCache)
            {
                articleData = new ArticleLimpet(moduleSettings.PortalId, moduleSettings.DataRef, cultureCode, moduleSettings.ModuleId);
                CacheUtils.SetCache(cacheKey, articleData, moduleSettings.ModuleRef);
                LogUtils.LogSystem("RocketContentAPIUtils.GetArticleData: " + cacheKey);
            }
            return articleData;
        }
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
        public static string DisplayView(int portalId, string systemkey, string moduleRef, string rowKey, SessionParams sessionParam, string template = "view.cshtml", string noAppThemeReturn= "", bool disableCache = false, bool useCache = true)
        {
            var cacheKey = moduleRef + sessionParam.CultureCode + template + rowKey;
            var rtnString = CacheFileUtils.GetCache(portalId, cacheKey, moduleRef);
            if (!useCache || disableCache || String.IsNullOrEmpty(rtnString))
            {
                var dataObject = new DataObjectLimpet(portalId, moduleRef, rowKey, sessionParam, false);
                if (!dataObject.ModuleSettings.HasAppThemeAdmin) return noAppThemeReturn; // test on Admin Theme.
                var razorTempl = dataObject.AppThemeView.GetTemplate(template, moduleRef);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
                if (pr.StatusCode == "00")
                {
                    if (useCache) CacheFileUtils.SetCache(portalId, cacheKey, pr.RenderedText, moduleRef);
                    rtnString = pr.RenderedText;
                }
                else
                    rtnString = pr.ErrorMsg;                
            }
            return rtnString;
        }
        public static string DisplayAdminView(int portalId, string moduleRef, string rowKey, SessionParams sessionParam, string template = "AdminDetail.cshtml", string noAppThemeReturn = "")
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, rowKey, sessionParam, true);
            if (!dataObject.ModuleSettings.HasAppThemeAdmin) return noAppThemeReturn;
            var razorTempl = dataObject.AppThemeAdmin.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, new Dictionary<string, string>(), sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string DisplaySystemView(int portalId, string moduleRef, SessionParams sessionParam, string template, bool editMode = true, bool useCache = true)
        {
            var cacheKey = moduleRef + sessionParam.CultureCode + template + "DisplaySystemView";
            var rtnString = CacheFileUtils.GetCache(portalId, cacheKey, moduleRef);
            if (!useCache || String.IsNullOrEmpty(rtnString))
            {
                var dataObject = new DataObjectLimpet(portalId, moduleRef, "", sessionParam, editMode);
                if (dataObject.AppThemeSystem == null) return "No System View";
                var razorTempl = dataObject.AppThemeSystem.GetTemplate(template, moduleRef);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
                if (pr.StatusCode == "00")
                {
                    if (useCache) CacheFileUtils.SetCache(portalId, cacheKey, pr.RenderedText, moduleRef);
                    rtnString = pr.RenderedText;
                }
                else
                    rtnString = pr.ErrorMsg;
            }
            return rtnString;
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
        public static List<SimplisityInfo> GetAllRecordMODSETTINGS(int portalId)
        {
            var objCtrl = new DNNrocketController();
            var sqlSearchFilter = " select * from [dbo].[DNNrocket]  where portalid = " + portalId + " and typecode = 'MODSETTINGS' and [XMLData].value('(genxml/systemkey)[1]','nvarchar(max)') = 'rocketcontentapi' ";
            return objCtrl.ExecSqlList(sqlSearchFilter);
        }

    }

}
