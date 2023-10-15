using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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
                var articleData = new ArticleLimpet(_dataObject.PortalId, _moduleRef, cultureCode, _moduleId);

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

            var articleData2 = new ArticleLimpet(_dataObject.PortalId, _moduleRef, _sessionParams.CultureCodeEdit, _moduleId);
            _dataObject.SetDataObject("articledata", articleData2);
            return AdminDetailDisplay();
        }
        public string RemoveRow()
        {
            var articleData = _dataObject.ArticleData;
            articleData.RemoveRow(_rowKey);

            // reload so we always have 1 row.
            var articleData2 = new ArticleLimpet(_dataObject.PortalId, _moduleRef, _sessionParams.CultureCodeEdit, _moduleId);
            _dataObject.SetDataObject("articledata", articleData2);

            _rowKey = articleData.GetRow(0).Info.GetXmlProperty("genxml/config/rowkey");
            return AdminDetailDisplay();
        }
        public string SaveArticleRow()
        {
            var articleData = _dataObject.ArticleData;
            articleData.ModuleId = _dataObject.ModuleSettings.ModuleId;
            articleData.UpdateRow(_rowKey, _postInfo, _dataObject.ModuleSettings.SecureSave);
            articleData.ClearCache();
            DNNrocketUtils.SynchronizeModule(articleData.ModuleId); // module search
            _dataObject.SetDataObject("articledata", articleData);
            return AdminDetailDisplay();
        }
        public void DeleteArticle()
        {
            CacheUtils.ClearAllCache("article");
            _dataObject.ArticleData.Delete();
        }
        public string AddArticleImage(bool singleImage = false)
        {
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

            return AdminDetailDisplay();
        }
        public string RemoveArticleImage()
        {
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);
            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                articleRow.Info.RemoveList(articleData.ImageListName);
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }

            return AdminDetailDisplay();
        }
        public string AddArticleDoc(bool singleDoc = false)
        {
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
                        if (singleDoc) articleRow.Info.RemoveList(articleData.ImageListName);
                        articleRow.AddDoc(Path.GetFileName(docFileMapPath), articleData.ModuleId);
                        articleData.UpdateRow(_rowKey, articleRow.Info);
                    }
                }
            }

            return AdminDetailDisplay();
        }
        public string RemoveArticleDoc()
        {
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);
            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                articleRow.Info.RemoveList(articleData.DocumentListName);
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
            return AdminDetailDisplay();
        }

        /// <summary>
        /// Add a listitem to a row item.
        /// </summary>
        /// <returns></returns>
        public string AddArticleListItem()
        {
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);

            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                var listName = _paramInfo.GetXmlProperty("genxml/hidden/listname");
                articleRow.Info.AddListItem(listName, new SimplisityInfo());
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
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
            var articleData = _dataObject.ArticleData;
            articleData.UpdateRow(_rowKey, _postInfo);

            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                articleRow.AddLink();
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
            return AdminDetailDisplay();
        }
        public String GetAdminDeleteArticle()
        {
            var articleData = _dataObject.ArticleData;
            articleData.Delete();
            var articleData2 = new ArticleLimpet(_dataObject.PortalId, _moduleRef, _sessionParams.CultureCodeEdit, _moduleId);
            _dataObject.SetDataObject("articledata", articleData2);
            return AdminDetailDisplay();
        }
        public Dictionary<string, object> ArticleSearch()
        {
            var bodyXpathList = _dataObject.ModuleSettings.GetSetting("searchbody").Split(',');
            var descriptionXpathList = _dataObject.ModuleSettings.GetSetting("searchdescription").Split(',');
            var titleXpathList = _dataObject.ModuleSettings.GetSetting("searchtitle").Split(',');
            var rtn = new Dictionary<string, object>();

            var infoDict = _dataObject.ArticleData.Info.ToDictionary();
            foreach (var articleRowInfo in _dataObject.ArticleData.GetRowList())
            {
                var rowDict = articleRowInfo.ToDictionary();
                var bodydata = "";
                var descriptiondata = "";
                var titledata = "";
                foreach (var fname in bodyXpathList)
                {
                    if (infoDict.ContainsKey(fname))
                        bodydata += infoDict[fname] + " ";
                    else
                        if (rowDict.ContainsKey(fname)) bodydata += rowDict[fname] + " ";
                }
                foreach (var fname in descriptionXpathList)
                {
                    if (infoDict.ContainsKey(fname))
                        descriptiondata += infoDict[fname] + " ";
                    else
                        if (rowDict.ContainsKey(fname)) descriptiondata += rowDict[fname] + " ";
                }
                foreach (var fname in titleXpathList)
                {
                    if (infoDict.ContainsKey(fname))
                        titledata += infoDict[fname] + " ";
                    else
                        if (rowDict.ContainsKey(fname)) titledata += rowDict[fname] + " ";
                }
                rtn.Add("body", bodydata.TrimEnd(' '));
                rtn.Add("description", descriptiondata);
                rtn.Add("modifieddate", _dataObject.ArticleData.Info.ModifiedDate.ToString("O"));
                rtn.Add("title", titledata);                
            }
            return rtn;
        }
        public String AdminDetailDisplay()
        {
            // rowKey can come from the sessionParams or paramInfo.  (Because on no rowkey on the language change)
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

