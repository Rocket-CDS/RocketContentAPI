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
        public AppThemeRocketApiLimpet appThemeRocketApi;

        /// <summary>
        /// Assigns the data model for razor, this makes the template easier to build.
        /// </summary>
        /// <param name="sModel">The s model.</param>
        /// <returns></returns>
        public string AssignDataModel(SimplisityRazor sModel)
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
            appThemeRocketApi = (AppThemeRocketApiLimpet)sModel.GetDataObject("appthemerocketapi");

            if (sessionParams == null) sessionParams = new SessionParams(new SimplisityInfo());
            infoArticle = new SimplisityInfo();
            if (articleData != null && articleData.Info != null) infoArticle = articleData.Info;
            info = infoArticle;
            if (articleRowData != null && articleRowData.Info != null) info = articleRowData.Info;
            rowData = info;
            headerData = infoArticle;

            AddProcessDataResx(appThemeView, true);
            AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketContentAPI/App_LocalResources/");

            return "";
        }
        [Obsolete]
        public string AssigDataModel(SimplisityRazor sModel)
        {
            AssignDataModel(sModel);
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
        public IEncodedString ChatGPT(string textId, string sourceTextId = "")
        {
            var globalData = new SystemGlobalData();
            if (String.IsNullOrEmpty(globalData.ChatGptKey)) return new RawString("");
            var apiResx = "/DesktopModules/DNNrocket/api/App_LocalResources/";
            return new RawString("<span class=\"w3-button w3-text-theme\" style=\"width:40px;height:40px;padding:8px 0;\"><span class=\"material-icons\" title=\"" + DNNrocketUtils.GetResourceString(apiResx, "DNNrocket.chatgpt", "Text") + "\" style=\"cursor:pointer;\" onclick=\"$('#chatgptmodal').show();simplisity_setSessionField('chatgpttextid','" + textId + "');simplisity_setSessionField('chatgptcmd','article_chatgpt');$('#chatgptquestion').val($('#" + sourceTextId + "').val());\">sms</span></span>");
        }
        public IEncodedString DeepL(string textId, string sourceTextId = "", string cultureCode = "")
        {
            if (DNNrocketUtils.GetPortalLanguageList().Count <= 1) return new RawString("");
            var globalData = new SystemGlobalData();
            if (String.IsNullOrEmpty(globalData.DeepLauthKey )) return new RawString("");
            var apiResx = "/DesktopModules/DNNrocket/api/App_LocalResources/";
            return new RawString("<span class=\"w3-button w3-text-theme\" style=\"width:40px;height:40px;padding:8px 0;\"><span class=\"material-icons\" title=\"" + DNNrocketUtils.GetResourceString(apiResx, "DNNrocket.translate", "Text", cultureCode) + "\" style=\"cursor:pointer;\" onclick=\"$('#deeplmodal').show();simplisity_setSessionField('deepltextid','" + textId + "');simplisity_setSessionField('deeplcmd','article_deepl');$('#deeplquestion').val(stripHTML($('#" + sourceTextId + "').val()));\">translate</span></span>");
        }
        /// <summary>
        /// Standardized method and names to craete top,bottom,left,right padding on an element.
        /// Allows potion adjustment from module settings without change CSS files.
        /// field Id: leftpadding,rightpadding,toppadding,bottompadding
        /// </summary>
        /// <param name="sModel"></param>
        /// <returns>The padding CSS for an inline style on an element.</returns>
        public string StylePadding()
        {
            var strOut = "";
            if (moduleData.GetSettingInt("leftpadding") > 0)
            {
                strOut += "padding-left:" + moduleData.GetSettingInt("leftpadding") + "px;" ;
            }
            if (moduleData.GetSettingInt("rightpadding") > 0)
            {
                strOut += "padding-right:" + moduleData.GetSettingInt("rightpadding") + "px;";
            }
            if (moduleData.GetSettingInt("toppadding") > 0)
            {
                strOut += "padding-top:" + moduleData.GetSettingInt("toppadding") + "px;";
            }
            if (moduleData.GetSettingInt("bottompadding") > 0)
            {
                strOut += "padding-bottom:" + moduleData.GetSettingInt("bottompadding") + "px;";
            }
            return strOut;
        }

    }
}
