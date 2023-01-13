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
        public static string DisplayView(int portalId, string moduleRef, string rowKey, SessionParams sessionParam, string template = "view.cshtml", string noAppThemeReturn= "")
        {
            var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, sessionParam.ModuleId, sessionParam.TabId);
            var pr = (RazorProcessResult)CacheUtils.GetCache(moduleRef + template, moduleRef);
            if (moduleSettings.DisableCache || pr == null)
            {
                var dataObject = new DataObjectLimpet(portalId, moduleRef, rowKey, sessionParam, false);
                if (!dataObject.ModuleSettings.HasAppThemeAdmin) return noAppThemeReturn;
                var razorTempl = dataObject.AppThemeView.GetTemplate(template, moduleRef);
                pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
                CacheUtils.SetCache(moduleRef + template, pr, moduleRef);
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
    }

}
