using DNNrocketAPI;
using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketContentAPI.Components
{
    public class DataObjectLimpet
    {
        public const string _tableName = "RocketContentAPI";
        private Dictionary<string, object> _dataObjects;
        private Dictionary<string, string> _passSettings;
        private string _rowKey;
        private string _cultureCode;
        public DataObjectLimpet(int portalid, string moduleRef, string rowKey, SessionParams sessionParams, bool editMode = true)
        {
            var cultureCode = sessionParams.CultureCodeEdit;
            if (!editMode) cultureCode = sessionParams.CultureCode;
            Populate(portalid, moduleRef, rowKey, cultureCode, sessionParams.ModuleId, sessionParams.TabId);
        }
        public DataObjectLimpet(int portalid, string moduleRef, string rowKey, string cultureCode, int moduleId, int tabId)
        {
            Populate(portalid, moduleRef, rowKey, cultureCode, moduleId, tabId);
        }
        public void Populate(int portalid, string moduleRef, string rowKey, string cultureCode, int moduleId, int tabId)
        {
            _passSettings = new Dictionary<string, string>();
            _dataObjects = new Dictionary<string, object>();
            _rowKey = rowKey;
            _cultureCode = cultureCode;

            // could be scheduler with only the moduleid.
            if (moduleRef == "")  moduleRef = portalid + "_ModuleID_" + moduleId;

            var moduleSettings = new ModuleContentLimpet(portalid, moduleRef, SystemKey, moduleId, tabId);
            SetDataObject("modulesettings", moduleSettings);
            SetDataObject("appthemesystem", AppThemeUtils.AppThemeSystem(portalid, SystemKey));
            SetDataObject("portalcontent", new PortalContentLimpet(portalid, cultureCode));
            SetDataObject("portaldata", new PortalLimpet(portalid));
            SetDataObject("systemdata", SystemSingleton.Instance(SystemKey));
            SetDataObject("appthemeprojects", AppThemeUtils.AppThemeProjects());
            SetDataObject("userparams", new UserParams("ModuleID:" + moduleId, true));
            SetDataObject("appthemerocketapi", AppThemeUtils.AppThemeRocketApi(portalid));
            var appThemeShared = new AppThemeLimpet(ModuleSettings.PortalId, "rocketcontentapi.01shared", "1.0", ModuleSettings.ProjectName);
            SetDataObject("appthemeshared", appThemeShared);
            SetArticleDataObject(false);// this must be overwritten by any admin/update to not use cache.
        }
        public void SetArticleDataObject(bool useCache)
        {
            var articleData = RocketContentAPIUtils.GetArticleData(ModuleSettings, _cultureCode, useCache);
            SetDataObject("articledata", articleData);
            if (articleData != null)
            {
                LogUtils.LogSystem("rowKey:" + _rowKey);
                if (_rowKey == "")
                    SetDataObject("articlerow", articleData.GetRow(0));
                else
                {
                    var rowData = articleData.GetRow(_rowKey);
                    SetDataObject("articlerow", rowData);
                    if (rowData == null)
                        LogUtils.LogSystem("rowKey isValid:");
                    else
                        LogUtils.LogSystem("rowKey isValid:");
                }
            }
        }
        public void SetDataObject(String key, object value)
        {
            if (_dataObjects.ContainsKey(key)) _dataObjects.Remove(key);
            _dataObjects.Add(key, value);

            if (key == "modulesettings") // load appTheme if we have settings in ModuleSettings
            {
                if (ModuleSettings.HasProject)
                {
                    if (!_dataObjects.ContainsKey("appthemedatalist")) _dataObjects.Add("appthemedatalist", new AppThemeDataList(ModuleSettings.PortalId, ModuleSettings.ProjectName, SystemKey));
                    if (ModuleSettings.HasAppThemeAdmin && !_dataObjects.ContainsKey("apptheme") && !_dataObjects.ContainsKey("appthemeview"))
                    {
                        var appTheme = new AppThemeLimpet(ModuleSettings.PortalId, ModuleSettings.AppThemeAdminFolder, ModuleSettings.AppThemeAdminVersion, ModuleSettings.ProjectName);
                        _dataObjects.Add("apptheme", appTheme);
                        // the AppTheme were once split, now RocketContent uses the same "apptheme" AppTheme.
                        // "appthemeview" && "appthemeadmin" is legacy but still used in templates.
                        _dataObjects.Add("appthemeview", appTheme);
                        _dataObjects.Add("appthemeadmin", appTheme);
                    }
                }
            }
        }
        public object GetDataObject(String key)
        {
            if (_dataObjects != null && _dataObjects.ContainsKey(key)) return _dataObjects[key];
            return null;
        }
        public void SetSetting(string key, string value)
        {
            if (_passSettings.ContainsKey(key)) _passSettings.Remove(key);
            _passSettings.Add(key, value);
        }
        public string GetSetting(string key)
        {
            if (!_passSettings.ContainsKey(key)) return "";
            return _passSettings[key];
        }
        public List<SimplisityRecord> GetAppThemeProjects()
        {
            return AppThemeProjects.List;
        }
        public string SystemKey { get { return "rocketcontentapi"; } }
        public int PortalId { get { return PortalData.PortalId; } }
        public Dictionary<string, object> DataObjects { get { return _dataObjects; } }
        public ModuleContentLimpet ModuleSettings { get { return (ModuleContentLimpet)GetDataObject("modulesettings"); } }
        public AppThemeSystemLimpet AppThemeSystem { get { return (AppThemeSystemLimpet)GetDataObject("appthemesystem"); } }
        public PortalContentLimpet PortalContent { get { return (PortalContentLimpet)GetDataObject("portalcontent"); } }
        public AppThemeLimpet AppThemeShared { get { return (AppThemeLimpet)GetDataObject("appthemeshared"); } set { SetDataObject("appthemeshared", value); } }
        public AppThemeLimpet AppThemeView { get { return (AppThemeLimpet)GetDataObject("appthemeview"); } set { SetDataObject("appthemeview", value); } }
        public AppThemeLimpet AppTheme { get { return (AppThemeLimpet)GetDataObject("apptheme"); } set { SetDataObject("apptheme", value); } }
        public PortalLimpet PortalData { get { return (PortalLimpet)GetDataObject("portaldata"); } }
        public SystemLimpet SystemData { get { return (SystemLimpet)GetDataObject("systemdata"); } }
        public AppThemeProjectLimpet AppThemeProjects { get { return (AppThemeProjectLimpet)GetDataObject("appthemeprojects"); } }
        public ArticleLimpet ArticleData { get { return (ArticleLimpet)GetDataObject("articledata"); } }
        public Dictionary<string, string> Settings { get { return _passSettings; } }

    }
}
