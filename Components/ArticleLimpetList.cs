using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketContentAPI.Components;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace RocketContentAPI.Components
{

    public class ArticleLimpetList
    {
        private string _langRequired;
        private List<ArticleLimpet> _articleList;
        public const string _tableName = "RocketContentAPI";
        private const string _entityTypeCode = "ART";
        private DNNrocketController _objCtrl;
        private string _searchFilter;

        public ArticleLimpetList(PortalContentLimpet portalContent, string langRequired, bool populate)
        {
            PortalContent = portalContent;

            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            var paramInfo = new SimplisityInfo();
            SessionParamData = new SessionParams(paramInfo);
            SessionParamData.PageSize = 0;

            if (populate) Populate();
        }
        public ArticleLimpetList(SimplisityInfo paramInfo, PortalContentLimpet portalContent, string langRequired, bool populate, bool showHidden = true)
        {
            PortalContent = portalContent;

            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            SessionParamData = new SessionParams(paramInfo);
            if (SessionParamData.PageSize == 0) SessionParamData.PageSize = 32;
            if (SessionParamData.OrderByRef == "") SessionParamData.OrderByRef = "sqlorderby-article-name";

            if (populate) Populate();
        }
        public void Populate()
        {
            _searchFilter = "";
            if (SessionParamData.SearchText != "")
            {
                _searchFilter = "      and ( ";
                _searchFilter += "      isnull([XMLData].value('(genxml/lang/genxml/textbox/articlename)[1]','nvarchar(max)'),'') like '%" + SessionParamData.SearchText + "%' ";
                _searchFilter += "      or isnull([XMLData].value('(genxml/textbox/articleref)[1]','nvarchar(max)'),'') like '%" + SessionParamData.SearchText + "%' ";
                _searchFilter += "      or isnull([XMLData].value('(genxml/lang/genxml/textbox/articlekeywords)[1]','nvarchar(max)'),'') like '%" + SessionParamData.SearchText + "%' ";
                _searchFilter += "      ) ";
            }

            SessionParamData.RowCount = _objCtrl.GetListCount(PortalContent.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, _tableName);
            RecordCount = SessionParamData.RowCount;

            DataList = _objCtrl.GetList(PortalContent.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, "", 0, SessionParamData.Page, SessionParamData.PageSize, SessionParamData.RowCount, _tableName);
        }
        private string GetFilterSQL(string SqlFilterTemplate, SimplisityInfo paramInfo)
        {
            FastReplacer fr = new FastReplacer("{", "}", false);
            fr.Append(SqlFilterTemplate);
            var tokenList = fr.GetTokenStrings();
            foreach (var token in tokenList)
            {
                var tok = "r/" + token;
                fr.Replace("{" + token + "}", paramInfo.GetXmlProperty(tok));
            }
            var filtersql = " " + fr.ToString() + " ";
            return filtersql;
        }

        public void DeleteAll()
        {
            var l = GetAllArticles();
            foreach (var r in l)
            {
                _objCtrl.Delete(r.ItemID);
            }
        }

        public SessionParams SessionParamData { get; set; }
        public List<SimplisityInfo> DataList { get; private set; }
        public PortalContentLimpet PortalContent { get; set; }
        public int RecordCount { get; set; }        
        public List<ArticleLimpet> GetArticleList()
        {
            _articleList = new List<ArticleLimpet>();
            foreach (var o in DataList)
            {
                var articleData = new ArticleLimpet(PortalContent.PortalId, o.GUIDKey, _langRequired);
                _articleList.Add(articleData);
            }
            return _articleList;
        }
        public List<SimplisityInfo> GetAllArticles()
        {
            return _objCtrl.GetList(PortalContent.PortalId, -1, _entityTypeCode, "", _langRequired, "", 0, 0, 0, 0, _tableName);
        }
        public void Validate()
        {
            var list = GetAllArticles();
            foreach (var pInfo in list)
            {
                var articleData = new ArticleLimpet(PortalContent.PortalId, pInfo.GUIDKey, _langRequired);
                articleData.ValidateAndUpdate();
            }
        }
    }

}
