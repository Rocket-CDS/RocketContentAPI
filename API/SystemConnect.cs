using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RocketContentAPI.API
{
    public partial class StartConnect
    {

        private string RenderSystemTemplate(string templateName)
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate(templateName);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        private string RocketSystemSave()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                _dataObject.PortalContent.Save(_postInfo);
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey + "setup", "True");
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey, "True");
                _dataObject.PortalData.Update();
                return RocketSystem();
            }
            return "Invalid PortalId";
        }
        private String RocketSystem()
        {
            return RenderSystemTemplate("RocketSystem.cshtml");
        }
        private String RocketSystemInit()
        {
            var newportalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/newportalid");
            if (newportalId > 0)
            {
                var portalContent = new PortalContentLimpet(newportalId, _sessionParams.CultureCodeEdit);
                portalContent.Validate();
                portalContent.Active = true;
                portalContent.Update();
                _dataObject.SetDataObject("portalcontent", portalContent);
            }
            return "";
        }
        private String RocketSystemDelete()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalId > 0)
            {
                _dataObject.PortalContent.Delete();
            }
            return "";
        }
        private string ReloadPage()
        {
            // user does not have access, logoff
            UserUtils.SignOut();

            var portalAppThemeSystem = new AppThemeDNNrocketLimpet("rocketportal");
            var razorTempl = portalAppThemeSystem.GetTemplate("Reload.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string MessageDisplay(string msgKey)
        {
            _dataObject.SetSetting("msgkey", msgKey);
            return RenderSystemTemplate("MessageDisplay.cshtml");
        }

        private string ExportData()
        {
            // check the scheduler initiated the call.
            var rtn = "";
            var securityKey = DNNrocketUtils.GetTempStorage(_paramInfo.GetXmlProperty("genxml/hidden/securitykey"));
            if (securityKey != null) // if it exists in the temp table, it was created by the scheduler.
            {

                rtn = "<export>";

                rtn += "<systemkey>" + _dataObject.SystemKey + "</systemkey>";
                rtn += "<databasetable>RocketContentAPI</databasetable>";

                rtn += "<modulesettings>";
                rtn += _dataObject.ModuleSettings.Record.ToXmlItem();
                rtn += "</modulesettings>";

                rtn += "<appthemes>";
                rtn += "<admin>";

                var zipMapPath = _dataObject.AppThemeAdmin.ExportZipFile(_moduleRef);
                var systemByte = File.ReadAllBytes(zipMapPath);
                var systemBase64 = Convert.ToBase64String(systemByte, Base64FormattingOptions.None);
                rtn += "<systembase64 filetype='zip'><![CDATA[";
                rtn += systemBase64;
                rtn += "]]></systembase64>";
                File.Delete(zipMapPath);

                zipMapPath = _dataObject.AppThemeAdmin.ExportPortalZipFile(_moduleRef);
                systemByte = File.ReadAllBytes(zipMapPath);
                systemBase64 = Convert.ToBase64String(systemByte, Base64FormattingOptions.None);
                rtn += "<portalbase64 filetype='zip'><![CDATA[";
                rtn += systemBase64;
                rtn += "]]></portalbase64>";
                File.Delete(zipMapPath);

                rtn += "</admin>";

                if (_dataObject.ModuleSettings.HasAppThemeView)
                {
                    rtn += "<view>";

                    zipMapPath = _dataObject.AppThemeView.ExportZipFile(_moduleRef);
                    systemByte = File.ReadAllBytes(zipMapPath);
                    systemBase64 = Convert.ToBase64String(systemByte, Base64FormattingOptions.None);
                    rtn += "<systembase64 filetype='zip'><![CDATA[";
                    rtn += systemBase64;
                    rtn += "]]></systembase64>";
                    File.Delete(zipMapPath);

                    zipMapPath = _dataObject.AppThemeView.ExportPortalZipFile(_moduleRef);
                    systemByte = File.ReadAllBytes(zipMapPath);
                    systemBase64 = Convert.ToBase64String(systemByte, Base64FormattingOptions.None);
                    rtn += "<portalbase64 filetype='zip'><![CDATA[";
                    rtn += systemBase64;
                    rtn += "]]></portalbase64>";
                    File.Delete(zipMapPath);

                    rtn += "</view>";
                }
                rtn += "</appthemes>";

                rtn += "<art>";
                var artList = _dataObject.GetAllRecordART(_dataObject.ModuleSettings.ModuleId);
                foreach (var a in artList)
                {
                    rtn += a.ToXmlItem();
                }
                rtn += "</art>";
                rtn += "<artlang>";
                var artLangList = _dataObject.GetAllRecordARTLANG(_dataObject.ModuleSettings.ModuleId);
                foreach (var a in artLangList)
                {
                    rtn += a.ToXmlItem();
                }
                rtn += "</artlang>";
                rtn += "<images>";
                var destDir = _dataObject.PortalContent.ImageFolderMapPath + "\\" + _dataObject.ModuleSettings.ModuleId;
                if (Directory.Exists(destDir))
                {
                    foreach (var i in Directory.GetFiles(destDir))
                    {
                        var imgByte = File.ReadAllBytes(i);
                        var imgBase64 = Convert.ToBase64String(imgByte, Base64FormattingOptions.None);
                        rtn += "<imgbase64 filerelpath='" + i + "'><![CDATA[";
                        rtn += imgBase64;
                        rtn += "]]></imgbase64>";
                    }
                }
                rtn += "</images>";
                rtn += "<docs>";
                var destDir2 = _dataObject.PortalContent.DocFolderMapPath + "\\" + _dataObject.ModuleSettings.ModuleId;
                if (Directory.Exists(destDir2))
                {
                    foreach (var i in Directory.GetFiles(destDir2))
                    {
                        var imgByte = File.ReadAllBytes(i);
                        var imgBase64 = Convert.ToBase64String(imgByte, Base64FormattingOptions.None);
                        rtn += "<docbase64 filerelpath='" + i + "'><![CDATA[";
                        rtn += imgBase64;
                        rtn += "]]></docbase64>";
                    }
                }
                rtn += "</docs>";

                rtn += "</export>";
            }

            return rtn;
        }
        private void ImportAppTheme(AppThemeLimpet appTheme, XmlNode appThemeNod, string prefix)
        {
            if (appTheme != null &&  appThemeNod != null)
            {
                var base64String = appThemeNod.InnerText;
                if (base64String != "")
                {
                    var importZipMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + prefix + ".zip";
                    File.WriteAllBytes(importZipMapPath, Convert.FromBase64String(base64String));
                    appTheme.ImportZipFile(importZipMapPath);
                    //File.Delete(importZipMapPath);
                }
            }
        }
        private void ImportPortalAppTheme(AppThemeLimpet appTheme, XmlNode appThemeNod, string prefix, string oldModuleRef, string newModuleRef)
        {
            if (appTheme != null && appThemeNod != null)
            {
                var base64String = appThemeNod.InnerText;
                if (base64String != "")
                {
                    var importZipMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + prefix + ".zip";
                    File.WriteAllBytes(importZipMapPath, Convert.FromBase64String(base64String));
                    appTheme.ImportPortalZipFile(importZipMapPath, oldModuleRef, newModuleRef);
                    //File.Delete(importZipMapPath);
                }
            }
        }
        private void ImportData()
        {
            // check the scheduler initiated the call.
            var securityKey = DNNrocketUtils.GetTempStorage(_paramInfo.GetXmlProperty("genxml/hidden/securitykey"));
            if (securityKey != null) // if it exists in the temp table, it was created by the scheduler.
            {

                var moduleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
                var tabId = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid");
                var systemKey = _paramInfo.GetXmlProperty("genxml/hidden/systemkey");
                var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                var databasetable = _paramInfo.GetXmlProperty("genxml/hidden/databasetable");
                var moduleRef = portalId + "_ModuleID_" + moduleId;

                var objCtrl = new DNNrocketController();

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(_postInfo.XMLData);

                //import Settings (Saved in DNNrocket table)
                var settingsNod = xmlDoc.SelectSingleNode("export/modulesettings");
                if (settingsNod != null)
                {
                    var ms = new SimplisityRecord();
                    ms.FromXmlItem(settingsNod.InnerXml);
                    var rec = objCtrl.GetRecordByGuidKey(portalId, moduleId, "MODSETTINGS", moduleRef, "");
                    if (rec != null)
                    {
                        var storeId = rec.ItemID;
                        ms = rec;
                        ms.FromXmlItem(settingsNod.InnerXml);
                        ms.ItemID = storeId;
                    }
                    else
                        ms.ItemID = -1;

                    var legacymoduleref = ms.GUIDKey;
                    ms.SetXmlProperty("genxml/legacymoduleref", legacymoduleref); // used to link DataRef on Satellite modules.
                    var legacymoduleid = ms.ModuleId.ToString();
                    ms.SetXmlProperty("genxml/legacymoduleid", legacymoduleid);
                    ms.SetXmlProperty("genxml/settings/name", ms.GetXmlProperty("genxml/settings/name").Replace(legacymoduleid, moduleId.ToString()));
                    ms.PortalId = portalId;
                    ms.ModuleId = moduleId;
                    ms.GUIDKey = moduleRef;

                    objCtrl.Update(ms);

                    var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, systemKey, moduleId, tabId);

                    if (moduleSettings.HasProject)
                    {
                        AppThemeLimpet appThemeAdmin = null;
                        AppThemeLimpet appThemeView = null;
                        if (moduleSettings.HasAppThemeAdmin)
                        {
                            appThemeAdmin = new AppThemeLimpet(moduleSettings.PortalId, moduleSettings.AppThemeAdminFolder, moduleSettings.AppThemeAdminVersion, moduleSettings.ProjectName);
                        }
                        if (moduleSettings.HasAppThemeView)
                        { 
                            appThemeView = new AppThemeLimpet(moduleSettings.PortalId, moduleSettings.AppThemeViewFolder, moduleSettings.AppThemeViewVersion, moduleSettings.ProjectName);
                        }

                        // Import AppTheme
                        ImportAppTheme(appThemeAdmin, xmlDoc.SelectSingleNode("export/appthemes/admin/systembase64"), moduleRef + "adminsystem");
                        ImportAppTheme(appThemeView, xmlDoc.SelectSingleNode("export/appthemes/view/systembase64"), moduleRef + "viewsystem");
                        ImportPortalAppTheme(appThemeAdmin, xmlDoc.SelectSingleNode("export/appthemes/admin/portalbase64"), moduleRef + "adminportal", legacymoduleref, moduleRef);
                        ImportPortalAppTheme(appThemeView, xmlDoc.SelectSingleNode("export/appthemes/view/portalbase64"), moduleRef + "viewportal", legacymoduleref, moduleRef);
                    }
                }

                //import ART
                var parentItemId = -1;
                var artNod = xmlDoc.SelectSingleNode("export/art/item[1]");
                if (artNod != null)
                {
                    var ms = new SimplisityRecord();
                    ms.FromXmlItem(artNod.OuterXml);
                    var rec = objCtrl.GetRecordByGuidKey(portalId, moduleId, "ART", moduleRef, "", "RocketContentAPI");
                    if (rec != null)
                    {
                        var storeId = rec.ItemID;
                        ms = rec;
                        ms.FromXmlItem(artNod.OuterXml);
                        ms.ItemID = storeId;
                    }
                    else
                        ms.ItemID = -1;

                    var legacymoduleid = ms.ModuleId.ToString();
                    ms.SetXmlProperty("genxml/legacymoduleid", legacymoduleid);
                    ms.SetXmlProperty("genxml/legacyid", ms.ItemID.ToString());
                    ms.PortalId = portalId;
                    ms.ModuleId = moduleId;
                    ms.GUIDKey = moduleRef;

                    parentItemId = objCtrl.Update(ms, "RocketContentAPI");
                }

                //import ARTLANG
                var artLangNods = xmlDoc.SelectNodes("export/artlang/item");
                if (artLangNods != null && parentItemId > 0)
                {
                    foreach (XmlNode artLangNod in artLangNods)
                    {
                        var ms = new SimplisityRecord();
                        ms.FromXmlItem(artLangNod.OuterXml);

                        var rec = objCtrl.GetRecordLang(parentItemId, ms.Lang, "RocketContentAPI");
                        if (rec != null)
                        {
                            var storeId = rec.ItemID;
                            ms = rec;
                            ms.FromXmlItem(artLangNod.OuterXml);
                            ms.ItemID = storeId;
                        }
                        else
                            ms.ItemID = -1;

                        var legacymoduleid = ms.ModuleId.ToString();
                        ms.SetXmlProperty("genxml/legacymoduleid", legacymoduleid);
                        ms.SetXmlProperty("genxml/legacyid", ms.ItemID.ToString());
                        ms.PortalId = portalId;
                        ms.ModuleId = moduleId;
                        ms.GUIDKey = ms.GUIDKey.Split('_')[0] + "_" + moduleId;
                        ms.ParentItemId = parentItemId;

                        objCtrl.Update(ms, "RocketContentAPI");

                    }
                }

                objCtrl.RebuildLangIndex(portalId, parentItemId, "RocketContentAPI");


                //import IMAGES
                var imageDict = new Dictionary<string, string>();
                var destImgFolder = _dataObject.PortalContent.ImageFolderRel + "/" + moduleId;
                var destImgFolderMapPath = DNNrocketUtils.MapPath(destImgFolder);
                if (!Directory.Exists(destImgFolderMapPath)) Directory.CreateDirectory(destImgFolderMapPath);
                var articleData = new ArticleLimpet(portalId, moduleRef, _sessionParams.CultureCodeEdit, moduleId);
                foreach (var rowData in articleData.GetRows())
                {
                    foreach (var i in rowData.GetImages())
                    {
                        if (!imageDict.ContainsKey(i.RelPath)) imageDict.Add(i.RelPath, destImgFolder + "/" + Path.GetFileName(i.RelPath));
                    }
                }
                var xData = articleData.Info.XMLData;
                foreach (var i in imageDict)
                {
                    xData = xData.Replace(i.Key, i.Value);
                }
                articleData.Info.XMLData = xData;
                articleData.Update();

                var imgNods = xmlDoc.SelectNodes("export/images/*");
                if (imgNods != null)
                {
                    foreach (XmlNode base64Node in imgNods)
                    {
                        var oldImgPath = base64Node.SelectSingleNode("@filerelpath").InnerText;
                        var base64String = base64Node.InnerText;
                        if (base64String != "")
                        {
                            var imgpath = destImgFolderMapPath + "\\" + Path.GetFileName(oldImgPath);
                            File.WriteAllBytes(imgpath, Convert.FromBase64String(base64String));
                        }
                    }
                }

                //import DOCS

                var docsDict = new Dictionary<string, string>();
                var destDocFolder = _dataObject.PortalContent.DocFolderRel + "/" + moduleId;
                var destDocFolderMapPath = DNNrocketUtils.MapPath(destDocFolder);
                if (!Directory.Exists(destDocFolderMapPath)) Directory.CreateDirectory(destDocFolderMapPath);
                var articleData2 = new ArticleLimpet(portalId, moduleRef, _sessionParams.CultureCodeEdit, moduleId);
                foreach (var rowData in articleData2.GetRows())
                {
                    foreach (var i in rowData.GetDocs())
                    {
                        if (!docsDict.ContainsKey(i.RelPath)) docsDict.Add(i.RelPath, destDocFolder + "/" + Path.GetFileName(i.RelPath));
                    }
                }
                var xData2 = articleData2.Info.XMLData;
                foreach (var i in docsDict)
                {
                    xData2 = xData2.Replace(i.Key, i.Value);
                }
                articleData2.Info.XMLData = xData2;
                articleData2.Update();

                var DocNods = xmlDoc.SelectNodes("export/docs/*");
                if (DocNods != null)
                {
                    foreach (XmlNode base64Node in DocNods)
                    {
                        var oldDocPath = base64Node.SelectSingleNode("@filerelpath").InnerText;
                        var base64String = base64Node.InnerText;
                        if (base64String != "")
                        {
                            var Docpath = destDocFolderMapPath + "\\" + Path.GetFileName(oldDocPath);
                            File.WriteAllBytes(Docpath, Convert.FromBase64String(base64String));
                        }
                    }
                }

            }

        }
    }
}

