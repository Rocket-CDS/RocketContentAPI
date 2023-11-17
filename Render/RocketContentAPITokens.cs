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
        public AppThemeLimpet appTheme;
        public AppThemeSystemLimpet appThemeSystem;
        public ModuleContentLimpet moduleData;
        public SimplisityInfo moduleDataInfo;
        public PortalLimpet portalData;
        public SystemGlobalData globalSettings = new SystemGlobalData();
        public List<string> enabledlanguages = DNNrocketUtils.GetCultureCodeList();
        public SessionParams sessionParams;
        public UserParams userParams;
        public SimplisityInfo info;
        public SimplisityInfo infoArticle;
        public SimplisityInfo rowData;
        public SimplisityInfo headerData;
        /// <summary>
        /// Assigns the data model for razor, this makes the template easier to build.
        /// </summary>
        /// <param name="sModel">The s model.</param>
        /// <returns></returns>
        public string AssigDataModel(SimplisityRazor sModel)
        {
            // use return of "string", so we don;t get error with converting void to object.
            articleData = (ArticleLimpet)sModel.GetDataObject("articledata");
            articleRowData = (ArticleRowLimpet)sModel.GetDataObject("articlerow");
            appThemeView = (AppThemeLimpet)sModel.GetDataObject("appthemeview");
            appThemeAdmin = (AppThemeLimpet)sModel.GetDataObject("appthemeadmin");
            appTheme = appThemeAdmin;
            appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");
            moduleData = (ModuleContentLimpet)sModel.GetDataObject("modulesettings");
            moduleDataInfo = new SimplisityInfo(moduleData.Record);
            portalData = (PortalLimpet)sModel.GetDataObject("portaldata");
            sessionParams = sModel.SessionParamsData;
            userParams = (UserParams)sModel.GetDataObject("userparams");

            if (sessionParams == null) sessionParams = new SessionParams(new SimplisityInfo());
            infoArticle = new SimplisityInfo();
            if (articleData != null && articleData.Info != null) infoArticle = articleData.Info;
            info = infoArticle;
            if (articleRowData != null && articleRowData.Info != null) info = articleRowData.Info;
            rowData = info;
            headerData = infoArticle;
            return "";
        }
        /// <summary>
        /// A row MUST have a rowkey to be saved to the DB.  This generates the rowkey.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public IEncodedString RowKey(SimplisityInfo info)
        {
            var strOut = HiddenField(info, "genxml/config/rowkey").ToString();
            strOut += HiddenField(info, "genxml/lang/genxml/config/rowkeylang", "", info.GetXmlProperty("genxml/config/rowkey")).ToString();
            strOut += HiddenField(info, "genxml/config/eid", "", GeneralUtils.Numeric(((int)(DateTime.Now.Ticks >> 23) + GeneralUtils.GetRandomKey(4,true)).ToString()) ).ToString();
            return new RawString(strOut);
        }
        /// <summary>
        /// For list and detail this token builds the List URL.
        /// </summary>
        /// <param name="listpageid">The listpageid.</param>
        /// <returns></returns>
        public IEncodedString ListUrl(int listpageid)
        {
            var listurl = DNNrocketUtils.NavigateURL(listpageid);
            return new RawString(listurl);
        }
        /// <summary>
        /// For list and detail this token builds the Detail URL.
        /// </summary>
        /// <param name="detailpageid">The detailpageid.</param>
        /// <param name="title">The title.</param>
        /// <param name="eId">The row eId.</param>
        /// <returns></returns>
        public IEncodedString DetailUrl(int detailpageid, string title, string eId)
        {
            var seotitle = DNNrocketUtils.UrlFriendly(title);
            string[] urlparams = { "eid", eId, seotitle };
            var detailurl = DNNrocketUtils.NavigateURL(detailpageid, "", urlparams);
            return new RawString(detailurl);
        }
        /// <summary>
        /// Creates a checkbox for the IsHidden property of a row.
        /// </summary>
        /// <param name="rowData">The row data.</param>
        /// <returns></returns>
        public IEncodedString CheckBoxRowIsHidden(SimplisityInfo rowData)
        {
            return CheckBox(rowData, "genxml/checkbox/hiderow", "", "class='w3-check' ", false, false, 0);
        }
        /// <summary>
        /// Creates a textbox for the title, using a standard xpath.
        /// </summary>
        /// <param name="rowData">The row data.</param>
        /// <returns></returns>
        public IEncodedString TextBoxRowTitle(SimplisityInfo rowData)
        {
            return TextBox(rowData, "genxml/lang/genxml/textbox/title", " id='title' class='w3-input w3-border' autocomplete='off' ", "", true, 0);
        }

    }
}
