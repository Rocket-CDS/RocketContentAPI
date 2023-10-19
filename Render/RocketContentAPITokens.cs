using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using RazorEngine.Text;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RocketContentAPI.Components
{
    public class RocketContentAPITokens<T> : DNNrocketAPI.render.DNNrocketTokens<T>
    {
        // Define data classes, so we can use intellisense in inject templates
        public ArticleLimpet articleData;
        public ArticleRowLimpet articleRowData;
        public AppThemeLimpet appThemeView;
        public AppThemeLimpet appThemeAdmin;
        public AppThemeSystemLimpet appThemeSystem;
        public ModuleContentLimpet moduleData;
        public PortalLimpet portalData;
        public SystemGlobalData globalSettings = new SystemGlobalData();
        public List<string> enabledlanguages = DNNrocketUtils.GetCultureCodeList();
        public SessionParams sessionParams;
        public UserParams userParams;
        public SimplisityInfo info;
        public SimplisityInfo infoArticle;
        public string AssigDataModel(SimplisityRazor sModel)
        {
            // use return of "string", so we don;t get error with converting void to object.
            articleData = (ArticleLimpet)sModel.GetDataObject("articledata");
            articleRowData = (ArticleRowLimpet)sModel.GetDataObject("articlerow");
            appThemeView = (AppThemeLimpet)sModel.GetDataObject("appthemeview");
            appThemeAdmin = (AppThemeLimpet)sModel.GetDataObject("appthemeadmin");
            appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");
            moduleData = (ModuleContentLimpet)sModel.GetDataObject("modulesettings");
            portalData = (PortalLimpet)sModel.GetDataObject("portaldata");
            sessionParams = sModel.SessionParamsData;
            userParams = (UserParams)sModel.GetDataObject("userparams");

            if (sessionParams == null) sessionParams = new SessionParams(new SimplisityInfo());
            infoArticle = new SimplisityInfo();
            if (articleData != null && articleData.Info != null) infoArticle = articleData.Info;
            info = infoArticle;
            if (articleRowData != null && articleRowData.Info != null) info = articleRowData.Info;
            return "";
        }

    }
}
