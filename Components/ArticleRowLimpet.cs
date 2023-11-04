using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DNNrocketAPI.Components;
using System.Globalization;
using System.Text.RegularExpressions;
using RocketContentAPI.Components;

namespace RocketContentAPI.Components
{
    public class ArticleRowLimpet
    {
        /// <summary>
        /// Should be used to create an article, the portalId is required on creation
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="dataRef"></param>
        /// <param name="langRequired"></param>
        public ArticleRowLimpet(int articleId, string XMLData, string guidKey)
        {
            ArticleId = articleId;
            Info = new SimplisityInfo();
            Info.XMLData = XMLData;
            Info.ItemID = ArticleId;
            Info.SetXmlProperty("genxml/column/itemid", ArticleId.ToString());
            Info.SetXmlProperty("genxml/column/guidkey", guidKey);
        }
        public SimplisityInfo Copy()
        {
            return Info;
        }

        #region "images"

        public void UpdateImages(List<SimplisityInfo> imageList)
        {
            Info.RemoveList(ImageListName);
            foreach (var sInfo in imageList)
            {
                var imgData = new ArticleImage(sInfo, "articleimage");
                UpdateImage(imgData);
            }
        }
        public List<SimplisityInfo> GetImageList()
        {
            return Info.GetList(ImageListName);
        }
        public ArticleImage AddImage(string uniqueName, int moduleId)
        {
            var articleImage = new ArticleImage(new SimplisityInfo(), "articleimage");
            var portalContent = new PortalContentLimpet(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture());
            articleImage.RelPath = portalContent.ImageFolderRel.TrimEnd('/') + "/" + moduleId + "/" + uniqueName;
            Info.AddListItem(ImageListName, articleImage.Info);
            return articleImage;
        }
        public void UpdateImage(ArticleImage articleImage)
        {
            Info.RemoveListItem(ImageListName, "genxml/hidden/imagekey", articleImage.ImageKey);
            Info.AddListItem(ImageListName, articleImage.Info);
        }
        public ArticleImage GetImage(int idx)
        {
            return new ArticleImage(Info.GetListItem(ImageListName, idx), "articleimage");
        }
        public List<ArticleImage> GetImages()
        {
            var rtn = new List<ArticleImage>();
            foreach (var i in Info.GetList(ImageListName))
            {
                rtn.Add(new ArticleImage(i, "articleimage"));
            }
            return rtn;
        }
        #endregion

        #region "docs"
        public void UpdateDocs(List<SimplisityInfo> docList)
        {
            Info.RemoveList(DocumentListName);
            foreach (var sInfo in docList)
            {
                var docData = new ArticleDoc(sInfo, "articledoc");
                UpdateDoc(docData);
            }
        }
        public List<SimplisityInfo> GetDocList()
        {
            return Info.GetList(DocumentListName);
        }
        public ArticleDoc AddDoc(string uniqueName, int moduleId)
        {
            var articleDoc = new ArticleDoc(new SimplisityInfo(), "articledoc");
            var portalContent = new PortalContentLimpet(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture());
            articleDoc.RelPath = portalContent.DocFolderRel.TrimEnd('/') + "/" + moduleId + "/" + uniqueName;
            articleDoc.FileName = uniqueName;
            Info.AddListItem(DocumentListName, articleDoc.Info);
            return articleDoc;
        }
        public void UpdateDoc(ArticleDoc articleDoc)
        {
            Info.RemoveListItem(DocumentListName, "genxml/hidden/dockey", articleDoc.DocKey);
            Info.AddListItem(DocumentListName, articleDoc.Info);
        }
        public ArticleDoc GetDoc(int idx)
        {
            return new ArticleDoc(Info.GetListItem(DocumentListName, idx), "articledoc");
        }
        public List<ArticleDoc> GetDocs()
        {
            var rtn = new List<ArticleDoc>();
            foreach (var i in Info.GetList(DocumentListName))
            {
                rtn.Add(new ArticleDoc(i, "articledoc"));
            }
            return rtn;
        }
        #endregion

        #region "links"
        public void UpdateLinks(List<SimplisityInfo> linkList)
        {
            Info.RemoveList(LinkListName);
            foreach (var sInfo in linkList)
            {
                var linkData = new ArticleLink(sInfo, "articlelink");
                UpdateLink(linkData);
            }
        }
        public List<SimplisityInfo> GetLinkList()
        {
            return Info.GetList(LinkListName);
        }
        public ArticleLink AddLink()
        {
            var articleLink = new ArticleLink(new SimplisityInfo(), "articlelink");
            Info.AddListItem(LinkListName, articleLink.Info);
            return articleLink;
        }
        public void UpdateLink(ArticleLink articleLink)
        {
            Info.RemoveListItem(LinkListName, "genxml/hidden/linkkey", articleLink.LinkKey);
            Info.AddListItem(LinkListName, articleLink.Info);
        }
        public ArticleLink Getlink(int idx)
        {
            return new ArticleLink(Info.GetListItem(LinkListName, idx), "articlelink");
        }
        public List<ArticleLink> Getlinks()
        {
            var rtn = new List<ArticleLink>();
            foreach (var i in Info.GetList(LinkListName))
            {
                rtn.Add(new ArticleLink(i, "articlelink"));
            }
            return rtn;
        }
        #endregion

        #region "properties"
        public string Get(string xpath, string defaultvalue = "")
        {
            var val = Info.GetXmlProperty(xpath);
            if (val == "") val = defaultvalue;
            return val;
        }
        public int GetInt(string xpath, int defaultvalue = 0)
        {
            var val = Info.GetXmlPropertyInt(xpath);
            if (val == 0 && defaultvalue != 0) val = defaultvalue;
            return val;
        }
        public DateTime GetDate(string xpath)
        {
            return GetDate(xpath, DateTime.MinValue);
        }
        public DateTime GetDate(string xpath, DateTime defaultvalue)
        {
            var val = Info.GetXmlPropertyDate(xpath);
            if (val == DateTime.MinValue) val = defaultvalue;
            return val;
        }
        public bool GetBool(string xpath)
        {
            return Info.GetXmlPropertyBool(xpath);
        }
        public SimplisityInfo Info { get; set; }
        public int ArticleId { get; set; }
        public string LinkListName { get { return "linklist"; } }
        public string DocumentListName { get { return "documentlist"; } }
        public string ImageListName { get { return "imagelist"; } }
        public string RowKey { get { return Info.GetXmlProperty("genxml/config/rowkey"); } }
        public bool IsHidden { get { return Info.GetXmlPropertyBool("genxml/checkbox/hiderow"); } }
        
        #endregion

    }

}
