using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace RocketContentAPI.API
{
    public partial class StartConnect
    {
        private string SelectAppThemeProject()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.ProjectName = _paramInfo.GetXmlProperty("genxml/hidden/projectname");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RenderSystemTemplate("SelectAppTheme.cshtml");
        }
        private string SelectAppTheme(string cmdappendix)
        {
            _dataObject.SetSetting("cmdappendix", cmdappendix);
            var moduleData = _dataObject.ModuleSettings;
            if (cmdappendix == "")
                moduleData.AppThemeAdminFolder = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolder");
            else
                moduleData.AppThemeViewFolder = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolder");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RenderSystemTemplate("SelectAppThemeVersion.cshtml");
        }
        private string SelectAppThemeVersion(string cmdappendix)
        {
            _dataObject.SetSetting("cmdappendix", cmdappendix);
            var moduleData = _dataObject.ModuleSettings;
            if (cmdappendix == "")
                moduleData.AppThemeAdminVersion = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolderversion");
            else
                moduleData.AppThemeViewVersion = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolderversion");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RenderSystemTemplate("ModuleSettings.cshtml");
        }
        private string ResetAppTheme()
        {
            _dataObject.SetSetting("cmdappendix", "");
            var moduleData = _dataObject.ModuleSettings;
            moduleData.ProjectName = "";
            moduleData.AppThemeAdminFolder = "";
            moduleData.AppThemeAdminVersion = "";
            moduleData.Update();
            _dataObject.SetDataObject("modulesettings", moduleData);
            return RenderSystemTemplate("SelectProject.cshtml");
        }
        private string ResetAppThemeView()
        {
            _dataObject.SetSetting("cmdappendix", "view");
            var moduleData = _dataObject.ModuleSettings;
            moduleData.AppThemeViewFolder = "";
            moduleData.AppThemeViewVersion = "";
            moduleData.Update();
            _dataObject.SetDataObject("modulesettings", moduleData);
            return RenderSystemTemplate("SelectAppTheme.cshtml");
        }
        private string SaveSettings()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.Save(_postInfo);
            moduleData.Update();
            _dataObject.SetDataObject("modulesettings", moduleData);
            return RenderSystemTemplate("ModuleSettings.cshtml");
        }
        private string DisplaySettings()
        {
            var moduleData = _dataObject.ModuleSettings;
            if (!moduleData.HasProject) return RenderSystemTemplate("SelectProject.cshtml");
            if (!moduleData.HasAppThemeAdmin) return RenderSystemTemplate("SelectAppTheme.cshtml");
            if (!moduleData.HasAppThemeAdminVersion) return RenderSystemTemplate("SelectAppThemeVersion.cshtml");
            return RenderSystemTemplate("ModuleSettings.cshtml");
        }

    }
}

