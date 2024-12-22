using DNNrocketAPI;
using DNNrocketAPI.Components;
using Newtonsoft.Json;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RocketContentAPI.API
{
    public partial class StartConnect
    {
        public string AddRow()
        {
            _rowKey = _dataObject.ArticleData.AddRow();
            return AdminDetailDisplay();
        }
        public string SortRows()
        {
            var selectkeylist = _paramInfo.GetXmlProperty("genxml/hidden/selectkeylist");
            var rowSortOrder = new List<string>();
            var l = selectkeylist.Split(',');
            foreach (var rKey in l)
            {
                if (rKey != "") rowSortOrder.Add(rKey);
            }

            var sortedArticles = new List<ArticleLimpet>();

            // we need to sort ALL langauges. 
            foreach (var cultureCode in DNNrocketUtils.GetCultureCodeList(_dataObject.PortalId))
            {
                var articleData = RocketContentAPIUtils.GetArticleData(_dataObject.ModuleSettings, cultureCode);

                // Build new sorted list
                var rowInfoDict = new Dictionary<string, SimplisityInfo>();
                foreach (var r in articleData.GetRowList())
                {
                    var key = r.GetXmlProperty("genxml/config/rowkey");
                    rowInfoDict.Add(key, r);
                }

                // Remove existing row and Add sorted rows.
                articleData.Info.RemoveList("rows");
                foreach (var k in rowSortOrder)
                {
                    if (rowInfoDict.ContainsKey(k)) articleData.Info.AddListItem("rows", rowInfoDict[k]);
                }

                sortedArticles.Add(articleData);

            }

            // cannot update in loop, it will change sortorder on first record, the rest of the language will then be wrong.
            foreach (var a in sortedArticles)
            {
                a.Update();
                //a.RebuildLangIndex(); // rebuild the index record [Essential to get the correct sort order]
            }

            var articleData2 = RocketContentAPIUtils.GetArticleData(_dataObject.ModuleSettings, _sessionParams.CultureCodeEdit);
            _dataObject.SetDataObject("articledata", articleData2);
            return AdminDetailDisplay();
        }
        public string RemoveRow()
        {
            var articleData = _dataObject.ArticleData;
            articleData.RemoveRow(_rowKey);

            // reload so we always have 1 row.
            var articleData2 = RocketContentAPIUtils.GetArticleData(_dataObject.ModuleSettings, _sessionParams.CultureCodeEdit);
            _dataObject.SetDataObject("articledata", articleData2);

            _rowKey = articleData2.GetRow(0).Info.GetXmlProperty("genxml/config/rowkey");
            return AdminDetailDisplay();
        }
        public string SaveArticleRow()
        {
            _dataObject.SetArticleDataObject(false);
            var articleData = _dataObject.ArticleData;
            articleData.ModuleId = _dataObject.ModuleSettings.ModuleId;
            articleData.UpdateRow(_rowKey, _postInfo, _dataObject.ModuleSettings.SecureSave);
            articleData.ClearCache();
            DNNrocketUtils.SynchronizeModule(articleData.ModuleId); // module search
            _dataObject.SetArticleDataObject(false);
            return AdminDetailDisplay();
        }
        public void DeleteArticle()
        {
            _dataObject.ArticleData.Delete();
        }
        public string AddArticleChatGptImageAsync(bool singleImage = false)
        {
            _dataObject.SetArticleDataObject(false);
            var articleData = _dataObject.ArticleData;
            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                if (singleImage) articleRow.Info.RemoveList(articleData.ImageListName);

                //call ChatGptimage
                var prompt = GeneralUtils.DeCode(_postInfo.GetXmlProperty("genxml/hidden/chatgptimagetext"));
                if (!String.IsNullOrEmpty(prompt))
                {
                    try
                    {
                        _dataObject.PortalData.AiImageCount();
                        var chatGpt = new ChatGPT();
                        var iUrl = chatGpt.GenerateImageAsync(prompt).Result;
                        if (GeneralUtils.IsUriValid(iUrl))
                        {
                            var moduleImgFolder = _dataObject.PortalContent.ImageFolderMapPath + "\\" + articleData.ModuleId;
                            if (!Directory.Exists(moduleImgFolder)) Directory.CreateDirectory(moduleImgFolder); 
                            var imgFileMapPath = moduleImgFolder + "\\" + GeneralUtils.GetGuidKey() + ".webp";
                            ImgUtils.DownloadAndSaveImage(iUrl, imgFileMapPath);

                            articleRow.AddImage(Path.GetFileName(imgFileMapPath), articleData.ModuleId);
                            articleData.UpdateRow(_rowKey, articleRow.Info);
                            _dataObject.SetArticleDataObject(false);
                        }
                        else
                        {
                            return iUrl;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtils.LogException(ex);
                        return ex.ToString();
                    }
                }
            }
            return AdminDetailDisplay();
        }

        public string AddArticleImage(bool singleImage = false)
        {
            _dataObject.SetArticleDataObject(false);
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);

            // Add new image if found in postInfo
            var fileuploadlist = _postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            var fileuploadbase64 = _postInfo.GetXmlProperty("genxml/hidden/fileuploadbase64");
            if (fileuploadbase64 != "")
            {
                var filenameList = fileuploadlist.Split('*');
                var filebase64List = fileuploadbase64.Split('*');
                var baseFileMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + GeneralUtils.GetGuidKey();
                var imgsize = _postInfo.GetXmlPropertyInt("genxml/hidden/imageresize");
                if (imgsize == 0) imgsize = _dataObject.ModuleSettings.Record.GetXmlPropertyInt("genxml/settings/imageresize");
                if (imgsize == 0) imgsize = 640;
                var destDir = _dataObject.PortalContent.ImageFolderMapPath + "\\" + _dataObject.ModuleSettings.ModuleId;
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                var imgList = ImgUtils.UploadBase64Image(filenameList, filebase64List, baseFileMapPath, destDir, imgsize);
                foreach (var imgFileMapPath in imgList)
                {
                    var articleRow = articleData.GetRow(_rowKey);
                    if (articleRow != null)
                    {
                        if (singleImage) articleRow.Info.RemoveList(articleData.ImageListName);
                        articleRow.AddImage(Path.GetFileName(imgFileMapPath), articleData.ModuleId);
                        articleData.UpdateRow(_rowKey, articleRow.Info);
                    }
                }
            }
            _dataObject.SetArticleDataObject(false);
            return AdminDetailDisplay();
        }
        public string RemoveArticleImage()
        {
            _dataObject.SetArticleDataObject(false);
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);
            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                articleRow.Info.RemoveList(articleData.ImageListName);
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
            _dataObject.SetArticleDataObject(false);
            return AdminDetailDisplay();
        }
        public string AddArticleDoc(bool singleDoc = false)
        {
            _dataObject.SetArticleDataObject(false);
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);

            // Add new image if found in postInfo
            var fileuploadlist = _postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            var fileuploadbase64 = _postInfo.GetXmlProperty("genxml/hidden/fileuploadbase64");
            if (fileuploadbase64 != "")
            {
                var filenameList = fileuploadlist.Split('*');
                var filebase64List = fileuploadbase64.Split('*');
                var destDir = _dataObject.PortalContent.DocFolderMapPath + "\\" + _dataObject.ModuleSettings.ModuleId;
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                var fileList = DocUtils.UploadBase64file(filenameList, filebase64List, destDir);
                if (fileList.Count == 0) return MessageDisplay("RCT.invalidfile");
                foreach (var docFileMapPath in fileList)
                {
                    var articleRow = articleData.GetRow(_rowKey);
                    if (articleRow != null)
                    {
                        if (singleDoc) articleRow.Info.RemoveList(articleData.DocumentListName);
                        articleRow.AddDoc(Path.GetFileName(docFileMapPath), articleData.ModuleId);
                        articleData.UpdateRow(_rowKey, articleRow.Info);
                    }
                }
            }
            _dataObject.SetArticleDataObject(false);
            return AdminDetailDisplay();
        }
        public string RemoveArticleDoc()
        {
            _dataObject.SetArticleDataObject(false);
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);
            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                articleRow.Info.RemoveList(articleData.DocumentListName);
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
            _dataObject.SetArticleDataObject(false);
            return AdminDetailDisplay();
        }

        /// <summary>
        /// Add a listitem to a row item.
        /// </summary>
        /// <returns></returns>
        public string AddArticleListItem()
        {
            _dataObject.SetArticleDataObject(false);
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);

            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                var listName = _paramInfo.GetXmlProperty("genxml/hidden/listname");
                articleRow.Info.AddListItem(listName, new SimplisityInfo());
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
            _dataObject.SetArticleDataObject(false);
            return AdminDetailDisplay();
        }
        public string RestoreArticle()
        {
            var recycleref = _paramInfo.GetXmlProperty("genxml/hidden/recycleref");
            _dataObject.ArticleData.RestoreArticle(recycleref);
            _dataObject.Settings.Add("restoredarticle", "true");
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("recyclebin.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.ArticleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string EmptyRecycleBin()
        {
            _dataObject.ArticleData.EmptyRecycleBin();
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("recyclebin.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.ArticleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string AddArticleLink()
        {
            _dataObject.SetArticleDataObject(false);
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);

            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                articleRow.AddLink();
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
            _dataObject.SetArticleDataObject(false);
            return AdminDetailDisplay();
        }
        public String GetAdminDeleteArticle()
        {
            var articleData = _dataObject.ArticleData;
            articleData.Delete();
            _dataObject.SetArticleDataObject(false);
            return AdminDetailDisplay();
        }
        public string ChatGptReturn()
        {
            try
            {
                var chatGPT = new DNNrocketAPI.Components.ChatGPT();
                var sQuestion = _postInfo.GetXmlProperty("genxml/textbox/chatgptquestion");
                var chatgpttext = chatGPT.SendMsg(sQuestion);
                _sessionParams.Set("chatgptreturn", chatgpttext);
                var razorTempl = AppThemeUtils.AppThemeRocketApi(_dataObject.PortalId).GetTemplate("ChatGptReturn.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "No responce from Chat GPT - please check you API key.";
            }
        }
        public string TranslateReturn()
        {
            var sQuestion = _postInfo.GetXmlProperty("genxml/textbox/deeplquestion");
            var globalData = new SystemGlobalData();
            var deepltext = DeepLUtils.TranslateText(globalData.DeepLurl,globalData.DeepLauthKey, sQuestion, _sessionParams.CultureCodeEdit).Result;
            _sessionParams.Set("deeplreturn", deepltext);
            var razorTempl = AppThemeUtils.AppThemeRocketApi(_dataObject.PortalId).GetTemplate("DeepLReturn.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public Dictionary<string, object> ArticleSearch()
        {
            var rtn = new Dictionary<string, object>();
            var rtnList = new List<Dictionary<string, object>>();

            if (_dataObject.AppThemeAdmin == null) return rtn; //Possible on website import 

            var searchIndexFields = _dataObject.AppThemeAdmin.GetSeachIndexFieldNames(_moduleRef);
            var bodyFieldNameList = searchIndexFields.GetXmlProperty("genxml/searchbody").Split(',');
            var descriptionFieldNameList = searchIndexFields.GetXmlProperty("genxml/searchdescription").Split(',');
            var titleFieldNameList = searchIndexFields.GetXmlProperty("genxml/searchtitle").Split(',');

            if (titleFieldNameList.Count() > 0 || descriptionFieldNameList.Count() > 0 || bodyFieldNameList.Count() > 0)
            {
                foreach (var l in DNNrocketUtils.GetCultureCodeList(_dataObject.PortalId))
                {
                    var articleData = new ArticleLimpet(_dataObject.PortalId, _dataObject.ArticleData.DataRef, l, _dataObject.ModuleSettings.ModuleId);
                    foreach (var articleRowInfo in articleData.GetRowList())
                    {
                        var rtn2 = new Dictionary<string, object>();
                        var bodydata = "";
                        var descriptiondata = "";
                        var titledata = "";
                        foreach (var fname in bodyFieldNameList)
                        {
                            bodydata += articleRowInfo.GetXmlProperty(fname) + " ";
                        }
                        foreach (var fname in descriptionFieldNameList)
                        {
                            descriptiondata += articleRowInfo.GetXmlProperty(fname) + " ";
                        }
                        foreach (var fname in titleFieldNameList)
                        {
                            titledata += articleRowInfo.GetXmlProperty(fname) + " ";
                        }

                        rtn2.Add("body", bodydata.Trim(' '));
                        rtn2.Add("description", descriptiondata.Trim(' '));
                        rtn2.Add("modifieddate", articleData.Info.ModifiedDate.ToString("O"));
                        if (String.IsNullOrWhiteSpace(titledata)) titledata = articleData.Info.GetXmlProperty("genxml/lang/genxml/header/headertitle");
                        rtn2.Add("title", titledata.Trim(' '));
                        rtn2.Add("culturecode", articleData.CultureCode);
                        rtn2.Add("querystring", "");
                        rtn2.Add("removesearchrecord", "false");
                        var uniquekey = articleData.ArticleId + "_" + articleRowInfo.GetXmlProperty("genxml/config/rowkey");
                        rtn2.Add("uniquekey", uniquekey);
                        rtnList.Add(rtn2);
                    }
                }
                rtn.Add("searchindex", rtnList);
            }

            return rtn;
        }
        public String AdminDetailDisplay()
        {
            // rowKey can come from the sessionParams or paramInfo.  (Because on no rowkey on the language change)
            if (_selectKey != "") _rowKey = _selectKey;
            if (_dataObject.ArticleData.GetRowList().Count == 0) AddRow(); // create first row automatically
            var articleRow = _dataObject.ArticleData.GetRow(0);
            if (_rowKey != "") articleRow = _dataObject.ArticleData.GetRow(_rowKey);
            if (articleRow == null) articleRow = _dataObject.ArticleData.GetRow(0);  // row removed and still in sessionparams

            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("admindetail.cshtml");
            _dataObject.SetDataObject("articlerow", articleRow);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.ArticleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String AdminCreateArticle()
        {
            var appTheme = _postInfo.GetXmlProperty("genxml/radio/apptheme");
            if (appTheme == "") return "No AppTheme Selected";

            _moduleRef = GeneralUtils.GetGuidKey();

            return AdminDetailDisplay();
        }
        public String AdminSelectAppThemeDisplay()
        {
            var appThemeDataList = new AppThemeDataList(_dataObject.PortalId, _dataObject.SystemKey);
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("SelectAppTheme.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, appThemeDataList, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string ArticleDocumentList()
        {
            var articleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var docList = new List<object>();
            foreach (var i in DNNrocketUtils.GetFiles(DNNrocketUtils.MapPath(_dataObject.PortalContent.DocFolderRel)))
            {
                var sInfo = new SimplisityInfo();
                sInfo.SetXmlProperty("genxml/name", i.Name);
                sInfo.SetXmlProperty("genxml/relname", _dataObject.PortalContent.DocFolderRel + "/" + i.Name);
                sInfo.SetXmlProperty("genxml/fullname", i.FullName);
                sInfo.SetXmlProperty("genxml/extension", i.Extension);
                sInfo.SetXmlProperty("genxml/directoryname", i.DirectoryName);
                sInfo.SetXmlProperty("genxml/lastwritetime", i.LastWriteTime.ToShortDateString());
                docList.Add(sInfo);
            }

            _dataObject.SetSetting("uploadcmd", "articleadmin_docupload");
            _dataObject.SetSetting("deletecmd", "articleadmin_docdelete");
            _dataObject.SetSetting("articleid", articleid.ToString());

            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("DocumentSelect.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, docList, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;

        }
        public void ArticleDocumentUploadToFolder()
        {
            var userid = UserUtils.GetCurrentUserId(); // prefix to filename on upload.
            if (!Directory.Exists(_dataObject.PortalContent.DocFolderMapPath)) Directory.CreateDirectory(_dataObject.PortalContent.DocFolderMapPath);
            var fileuploadlist = _paramInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var friendlyname = GeneralUtils.DeCode(f);
                        var userfilename = userid + "_" + friendlyname;
                        File.Copy(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename, _dataObject.PortalContent.DocFolderMapPath + "\\" + friendlyname, true);
                        File.Delete(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename);
                    }
                }

            }
        }
        public void ArticleDeleteDocument()
        {
            var docfolder = _postInfo.GetXmlProperty("genxml/hidden/documentfolder");
            if (docfolder == "") docfolder = "docs";
            var docDirectory = PortalUtils.HomeDNNrocketDirectoryMapPath() + "\\" + docfolder;
            var docList = _postInfo.GetXmlProperty("genxml/hidden/dnnrocket-documentlist").Split(';');
            foreach (var i in docList)
            {
                if (i != "")
                {
                    var documentname = GeneralUtils.DeCode(i);
                    var docFile = docDirectory + "\\" + documentname;
                    if (File.Exists(docFile))
                    {
                        File.Delete(docFile);
                    }
                }
            }

        }
        public String GetAppThemeList()
        {
            return "GetAppThemeList()";
        }
        public String GetPublicArticle()
        {
            return GetPublicView("View.cshtml");
        }
        public String GetPublicArticleHeader()
        {
            return GetPublicView("Viewlastheader.cshtml");
        }
        public String GetPublicArticleBeforeHeader()
        {
            return GetPublicView("Viewfirstheader.cshtml");
        }

        private string GetPublicView(string templateName)
        {
            var razorTempl = _dataObject.AppThemeView.GetTemplate(templateName, _moduleRef);
            if (razorTempl == "") return "";
            _dataObject.SetDataObject("paraminfo", _paramInfo);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.ArticleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

    }
}

