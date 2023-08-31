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
using System.Xml.XPath;
using System.Xml.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace RocketContentAPI.Components
{
    public class ArticleLimpet
    {
        public const string _tableName = "RocketContentAPI";
        public const string _entityTypeCode = "ART";
        private DNNrocketController _objCtrl;
        private int _articleId;
        private SimplisityInfo _info;
        private string _cacheKey;

        /// <summary>
        /// Should be used to create an article, the portalId is required on creation
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="dataRef"></param>
        /// <param name="langRequired"></param>
        public ArticleLimpet(int portalId, string dataRef, string langRequired, int moduleid)
        {
            PortalId = portalId;
            SecureSave = true;
            _cacheKey = dataRef;
            Populate(langRequired);
        }
        /// <summary>
        /// When we populate with a child article row.
        /// </summary>
        /// <param name="articleData"></param>
        public ArticleLimpet(ArticleLimpet articleData)
        {
            _info = articleData.Info;
            _articleId = articleData.ArticleId;
            CultureCode = articleData.CultureCode;
            PortalId = _info.PortalId;
        }
        private void Populate(string cultureCode)
        {
            _objCtrl = new DNNrocketController();
            CultureCode = cultureCode;
            if (_cacheKey == "") _cacheKey = PortalId + "_ModuleId_" + ModuleId;

            _info = (SimplisityInfo)CacheUtils.GetCache(_cacheKey);
            if (_info == null)
            {
                _info = _objCtrl.GetByGuidKey(PortalId, -1, _entityTypeCode, _cacheKey, "", _tableName, cultureCode);
                if (_info == null)
                {
                    _info = new SimplisityInfo();
                    _info.ItemID = -1;
                    _info.TypeCode = _entityTypeCode;
                    _info.ModuleId = ModuleId;
                    _info.UserId = -1;
                    _info.GUIDKey = _cacheKey;
                    _info.PortalId = PortalId;
                }
                else
                {
                    CacheUtils.SetCache(_cacheKey, _info);
                }
            }
            _info.Lang = CultureCode;
        }
        public void Delete()
        {
            _objCtrl.Delete(_info.ItemID, _tableName);
            ClearCache();
        }
        public void ClearCache()
        {
            CacheUtils.RemoveCache(_info.GUIDKey);
            // clear cache for satilite modules.
            var filter = " and [XMLdata].value('(genxml/settings/dataref)[1]','nvarchar(max)') = '" + _info.GUIDKey + "' ";
            var l = _objCtrl.GetList(_info.PortalId, -1, "MODSETTINGS", filter);
            foreach (var m in l)
            {
                CacheUtils.RemoveCache(m.GUIDKey);
            }
        }

        private SimplisityInfo ReplaceInfoFields(SimplisityInfo newInfo, SimplisityInfo postInfo, string xpathListSelect)
        {
            var textList = postInfo.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    if (UserUtils.IsSuperUser() || !SecureSave)
                        newInfo.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, nod.InnerText);
                    else
                        newInfo.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, SecurityInput.RemoveScripts(nod.InnerText));
                }
            }
            return newInfo;
        }
        public int Update()
        {
            _info = _objCtrl.SaveData(_info, _tableName);
            if (_info.GUIDKey == "")
            {
                _info.GUIDKey = PortalId + "_ModuleId_" + ModuleId;
                _info = _objCtrl.SaveData(_info, _tableName);
            }
            ClearCache();
            return _info.ItemID;
        }
        public void RebuildLangIndex()
        {
            _objCtrl.RebuildLangIndex(PortalId, ArticleId, _tableName);
        }
        public int ValidateAndUpdate()
        {
            Validate();
            return Update();
        }
        public void AddListItem(string listname)
        {
            if (_info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            _info.AddListItem(listname);         
            Update();
        }
        public void Validate()
        {
        }

        #region "rows"
        public void UpdateRow(string rowKey, SimplisityInfo postInfo, bool secureSave = true)
        {
            SecureSave = secureSave;
            var newArticleRows = new List<SimplisityInfo>();
            var articleRows = GetRowList();

            //_info = ReplaceInfoFields(_info, postInfo, "genxml/data/*");
            //_info = ReplaceInfoFields(_info, postInfo, "genxml/lang/genxml/data/*");

            foreach (var sInfo in articleRows)
            {                
                if (sInfo.GetXmlProperty("genxml/config/rowkey") == rowKey)
                {
                    var newInfo = ReplaceInfoFields(new SimplisityInfo(), postInfo, "genxml/textbox/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/lang/genxml/textbox/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/checkbox/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/lang/genxml/checkbox/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/select/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/lang/genxml/select/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/radio/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/lang/genxml/radio/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/config/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/lang/genxml/config/*");

                    var postLists = postInfo.GetLists();
                    foreach (var listname in postLists)
                    {
                        var listData = postInfo.GetList(listname);
                        foreach (var listItem in listData)
                        {
                            newInfo.AddListItem(listname, listItem);
                        }
                        newInfo.SetXmlProperty("genxml/" + listname + "/@list", "true");
                        newInfo.SetXmlProperty("genxml/lang/genxml/" + listname + "/@list", "true");
                    }

                    newArticleRows.Add(newInfo);
                }
                else
                {
                    newArticleRows.Add(sInfo);
                }
            }
            _info.RemoveList("rows");
            foreach (var sInfo in newArticleRows)
            {
                _info.AddListItem("rows", sInfo);
            }

            // Set header Data
            _info = ReplaceInfoFields(_info, postInfo, "genxml/header/*");
            _info = ReplaceInfoFields(_info, postInfo, "genxml/lang/genxml/header/*");

            Update();
        }
        public string AddRow()
        {
            var newInfo = new SimplisityInfo();
            var rowKey = GeneralUtils.GetGuidKey();
            newInfo.SetXmlProperty("genxml/config/rowkey", rowKey);
            newInfo.SetXmlProperty("genxml/lang/genxml/config/rowkeylang", rowKey);
            _info.AddListItem("rows", newInfo);
            Update();
            return rowKey;
        }
        public void RemoveRow(string rowKey)
        {
            var rowLangList = _objCtrl.GetList(PortalId, -1, _entityTypeCode + "LANG", " and R1.ParentItemId = " + ArticleId + " ", "", "", 0, 0, 0, 0, _tableName);
            foreach (var r in rowLangList)
            {
                var rRec = new SimplisityRecord(r);
                rRec.RemoveRecordListItem("rows", "genxml/config/rowkeylang", rowKey);
                _objCtrl.Update(rRec, _tableName);
            }
            _info.RemoveListItem("rows", "genxml/config/rowkey", rowKey);
            Update();
        }
        public ArticleRowLimpet GetRow(string rowKey)
        {
            var articleRow = _info.GetListItem("rows", "genxml/config/rowkey", rowKey);
            if (articleRow == null) return null;
            return new ArticleRowLimpet(ArticleId, articleRow.XMLData, _info.GUIDKey);
        }
        public ArticleRowLimpet GetRow(int idx)
        {
            var articleRow = _info.GetListItem("rows", idx);
            if (articleRow == null) return null;
            return new ArticleRowLimpet(ArticleId, articleRow.XMLData, _info.GUIDKey);
        }
        public List<SimplisityInfo> GetRowList()
        {
            return _info.GetList("rows");
        }
        public List<ArticleRowLimpet> GetRows()
        {
            var rtn = new List<ArticleRowLimpet>();
            foreach (var i in _info.GetList("rows"))
            {
                rtn.Add(new ArticleRowLimpet(ArticleId, i.XMLData, _info.GUIDKey));
            }
            return rtn;
        }
        #endregion

        #region "properties"

        public string CultureCode { get; private set; }
        public bool SecureSave { get; private set; }
        public string EntityTypeCode { get { return _entityTypeCode; } }
        public SimplisityInfo Info { get { return _info; } }
        public int ModuleId { get { return _info.ModuleId; } set { _info.ModuleId = value; } }
        public int XrefItemId { get { return _info.XrefItemId; } set { _info.XrefItemId = value; } }
        public int ParentItemId { get { return _info.ParentItemId; } set { _info.ParentItemId = value; } }
        public int ArticleId { get { return _info.ItemID; } set { _info.ItemID = value; } }
        public string DataRef { get { return _info.GUIDKey; } set { _info.GUIDKey = value; } }
        public string GUIDKey { get { return _info.GUIDKey; } set { _info.GUIDKey = value; } }
        public int SortOrder { get { return _info.SortOrder; } set { _info.SortOrder = value; } }
        public bool DebugMode { get; set; }
        public int PortalId { get; set; }
        public bool Exists { get {if (_info.ItemID  <= 0) { return false; } else { return true; }; } }
        public string LinkListName { get { return "linklist"; } }
        public string DocumentListName { get { return "documentlist"; } }
        public string ImageListName { get { return "imagelist"; } }

        #endregion

    }

}
