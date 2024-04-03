using DNNrocketAPI.Components;
using DNNrocketAPI.Interfaces;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketContentAPI.API
{
    public partial class StartConnect : IProcessCommand
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private RocketInterface _rocketInterface;
        private SessionParams _sessionParams;
        private string _moduleRef;
        private string _rowKey;
        private string _selectKey;
        private DataObjectLimpet _dataObject;
        private int _moduleId;
        private int _tabId;

        public Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return nothing if not matching commands.
            var storeParamCmd = paramCmd;

            paramCmd = InitCmd(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);

            var rtnDic = new Dictionary<string, object>();

            switch (paramCmd)
            {

                case "rocketsystem_edit":
                    strOut = RocketSystem();
                    break;
                case "rocketsystem_init":
                    strOut = RocketSystemInit();
                    break;
                case "rocketsystem_delete":
                    strOut = RocketSystemDelete();
                    break;
                case "rocketsystem_save":
                    strOut = RocketSystemSave();
                    break;
                case "rocketsystem_login":
                    strOut = ReloadPage();
                    break;


                case "rocketcontentapi_activate":
                    strOut = RocketSystemSave();
                    break;


                case "article_admindetail":
                    strOut = AdminDetailDisplay();
                    break;
                case "article_admincreate":
                    strOut = AdminCreateArticle();
                    break;
                case "article_selectapptheme":
                    strOut = AdminSelectAppThemeDisplay();
                    break;
                case "article_adminsave":
                    strOut = SaveArticleRow();
                    break;
                case "article_admindelete":
                    strOut = GetAdminDeleteArticle();
                    break;
                case "article_addimage":
                    strOut = AddArticleImage();
                    break;
                case "article_addimage1":
                    strOut = AddArticleImage(true);
                    break;
                case "article_removeimage":
                    strOut = RemoveArticleImage();
                    break;                    
                case "article_adddoc":
                    strOut = AddArticleDoc();
                    break;
                case "article_adddoc1":
                    strOut = AddArticleDoc(true);
                    break;
                case "article_removedoc":
                    strOut = RemoveArticleDoc();
                    break;
                case "article_addlink":
                    strOut = AddArticleLink();
                    break;
                case "article_addrow":
                    strOut = AddRow();
                    break;
                case "article_editrow":
                    strOut = SaveArticleRow();
                    //strOut = AdminDetailDisplay();
                    break;
                case "article_removerow":
                    strOut = RemoveRow();
                    break;
                case "article_sortrows":
                    strOut = SortRows();
                    break;
                case "article_addlistitem":
                    strOut = AddArticleListItem();
                    break;
                case "article_restore":
                    strOut = RestoreArticle();
                    break;
                case "article_emptyrecyclebin":
                    strOut = EmptyRecycleBin();
                    break;
                case "article_chatgpt":
                    strOut = ChatGptReturn();
                    break;
                case "article_search":
                    rtnDic = ArticleSearch();
                    break;


                case "rocketcontentapi_settings":
                    strOut = DisplaySettings();
                    break;
                case "rocketcontentapi_savesettings":
                    strOut = SaveSettings();
                    break;
                case "rocketcontentapi_selectappthemeproject":
                    strOut = SelectAppThemeProject();
                    break;
                case "rocketcontentapi_selectapptheme":
                    strOut = SelectAppTheme("");
                    break;
                case "rocketcontentapi_selectappthemeview":
                    strOut = SelectAppTheme("view");
                    break;
                case "rocketcontentapi_selectappthemeversion":
                    strOut = SelectAppThemeVersion("");
                    break;
                case "rocketcontentapi_selectappthemeversionview":
                    strOut = SelectAppThemeVersion("view");
                    break;
                case "rocketcontentapi_resetapptheme":
                    strOut = ResetAppTheme();
                    break;
                case "rocketcontentapi_exportmodule":
                    strOut = ExportData();
                    break;
                case "rocketcontentapi_importmodule":
                     ImportData();
                    strOut = "";
                    break;
                case "rocketcontentapi_copylanguage":
                    strOut = CopyLanguage();
                    break;
                case "rocketcontentapi_validate":
                    strOut = ValidateContent();
                    break;




                case "remote_publiclist":
                    strOut = ""; // not used for rocketcontentapi
                    break;
                case "remote_publicview":
                    strOut = GetPublicArticle();
                    break;
                case "remote_publicviewlastheader":
                    strOut = GetPublicArticleHeader();
                    break;
                case "remote_publicviewfirstheader":
                    strOut = GetPublicArticleBeforeHeader();
                    break;
                case "remote_publicseo":
                    strOut = ""; // not used for rocketcontentapi
                    break;

                case "invalidcommand":
                    strOut = "INVALID COMMAND: " + storeParamCmd;
                    break;

            }

            if (!rtnDic.ContainsKey("remote-settingsxml")) rtnDic.Add("remote-settingsxml", _dataObject.ModuleSettings.Record.ToXmlItem());            
            if (!rtnDic.ContainsKey("outputjson")) rtnDic.Add("outputhtml", strOut);

            return rtnDic;

        }
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            var portalid = PortalUtils.GetCurrentPortalId();
            if (portalid < 0 && systemInfo.PortalId >= 0) portalid = systemInfo.PortalId;

            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);

            _moduleRef = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            _tabId = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid");
            _moduleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
            _rowKey = _postInfo.GetXmlProperty("genxml/config/rowkey");
            if (_rowKey == "") _rowKey = _paramInfo.GetXmlProperty("genxml/hidden/rowkey");
            if (_rowKey == "") _rowKey = _paramInfo.GetXmlProperty("genxml/hidden/selectkey");            
            _sessionParams.ModuleRef = _moduleRef; // we need this on the module view template, to stop clashes in modules that use the same dataref. 
            _sessionParams.TabId = _tabId;
            _sessionParams.ModuleId = _moduleId;

            // use a selectkeyfor editing.  the selectkey is the rowkey of the next row.
            _selectKey = _paramInfo.GetXmlProperty("genxml/hidden/selectkey");

            // Assign Langauge
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();
            DNNrocketUtils.SetCurrentCulture(_sessionParams.CultureCode);
            DNNrocketUtils.SetEditCulture(_sessionParams.CultureCodeEdit);

            _dataObject = new DataObjectLimpet(portalid, _sessionParams.ModuleRef, _rowKey, _sessionParams);

            if (paramCmd.StartsWith("remote_public")) return paramCmd;
            
            if (!_dataObject.ModuleSettings.HasAppThemeAdmin) // Check if we have an AppTheme
            {
                if (paramCmd != "article_search" && !paramCmd.StartsWith("rocketcontentapi_") && !paramCmd.StartsWith("rocketsystem_")) return "rocketcontentapi_settings";
            }

            var securityData = new SecurityLimpet(_dataObject.PortalId, _dataObject.SystemKey, _rocketInterface, _sessionParams.TabId, _sessionParams.ModuleId);
            return securityData.HasSecurityAccess(paramCmd, "rocketsystem_login");
        }
    }

}
