using DNNrocketAPI.Components;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace RocketContentAPI.Components
{
    public class HandlebarsEngineRC : HandlebarsEngine
    {
        public string ExecuteRC(string source, object model)
        {
            try
            {
                var hbs = HandlebarsDotNet.Handlebars.Create();
                RegisterHelpers(hbs);
                RegisterRCHelpers(hbs);
                return CompileTemplate(hbs, source, model);
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                throw new TemplateException("Failed to render Handlebar template : " + ex.Message, ex, model, source);
            }
        }

        public static void RegisterRCHelpers(IHandlebars hbs)
        {
            RegisterArticle(hbs);
            RegisterRow(hbs);
            RegisterModuleRef(hbs);
            RegisterImageShow(hbs);
            RegisterImage(hbs);
            RegisterDocumentShow(hbs);
            RegisterDocument(hbs);
            RegisterLinkShow(hbs);
            RegisterLink(hbs);
            RegisterEngineUrl(hbs);
            RegisterCultureCodeEdit(hbs);
            RegisterCultureCode(hbs);
        }

        private static ArticleLimpet GetArticleData(JObject o)
        {
            var moduleref = (string)o.SelectToken("genxml.data.genxml.column.guidkey") ?? "";
            if (moduleref == "") moduleref = (string)o.SelectToken("genxml.sessionparams.r.moduleref") ?? "";
            var cultureCode = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";

            var cacheKey = moduleref + "*" + cultureCode;
            var articleData = (ArticleLimpet)CacheUtils.GetCache(cacheKey, "article");
            if (articleData == null)
            {
                articleData = new ArticleLimpet(-1, moduleref, cultureCode);
                CacheUtils.SetCache(cacheKey, articleData, "article");
            }
            return articleData;
        }

        // Get moduleRef
        private static void RegisterModuleRef(IHandlebars hbs)
        {
            hbs.RegisterHelper("moduleref", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.data.genxml.column.guidkey") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterEngineUrl(IHandlebars hbs)
        {
            hbs.RegisterHelper("engineurl", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterCultureCode(IHandlebars hbs)
        {
            hbs.RegisterHelper("culturecode", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterCultureCodeEdit(IHandlebars hbs)
        {
            hbs.RegisterHelper("culturecodeedit", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.culturecodeedit") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        // Get Image Data
        private static void RegisterImageShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("imagetest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetArticleData(o);

                var rowidx = 0;
                if (arguments.Length >= 3) rowidx = Convert.ToInt32(arguments[2]);
                var imgidx = 0;
                if (arguments.Length >= 4) imgidx = Convert.ToInt32(arguments[3]);
                var img = articleData.GetRow(rowidx).GetImage(imgidx);
                var cmd = arguments[1].ToString();

                if (cmd == "isshown")
                {
                    if (!img.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (img.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasheading")
                {
                    if (img.Alt != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasimage")
                {
                    if (img.RelPath != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "haslink")
                {
                    if (img.UrlText != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hassummary")
                {
                    if (!String.IsNullOrWhiteSpace(img.Summary))
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }


            });
        }
        private static void RegisterImage(IHandlebars hbs)
        {
            hbs.RegisterHelper("image", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetArticleData(o);

                    var rowidx = 0;
                    if (arguments.Length >= 3) rowidx = Convert.ToInt32(arguments[2]);
                    var imgidx = 0;
                    if (arguments.Length >= 4) imgidx = Convert.ToInt32(arguments[3]);
                    var img = articleData.GetRow(rowidx).GetImage(imgidx);
                    var cmd = arguments[1].ToString();

                    switch (cmd)
                    {
                        case "alt":
                            dataValue = img.Alt;
                            break;
                        case "relpath":
                            dataValue = img.RelPath;
                            break;
                        case "height":
                            dataValue = img.Height.ToString();
                            break;
                        case "width":
                            dataValue = img.Width.ToString();
                            break;
                        case "count":
                            dataValue = articleData.GetRow(rowidx).GetImageList().Count.ToString();
                            break;
                        case "summary":
                            dataValue = img.Summary;
                            break;
                        case "url":
                            dataValue = img.Url;
                            break;
                        case "urltext":
                            dataValue = img.UrlText;
                            break;
                        case "hidden":
                            dataValue = img.Hidden.ToString();
                            break;
                        case "thumburl":
                            var width = Convert.ToInt32(arguments[4]);
                            var height = Convert.ToInt32(arguments[5]);
                            var enginrUrl = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                            dataValue = enginrUrl.TrimEnd('/') + "/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=" + img.RelPath + "&w=" + width + "&h=" + height;
                            break;
                        case "fieldid":
                            dataValue = img.FieldId;
                            break;
                        default:
                            dataValue = img.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = img.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;

                    }
               }

                writer.WriteSafeString(dataValue);
            });
        }

        // Get row data
        private static void RegisterRow(IHandlebars hbs)
        {
            hbs.RegisterHelper("articlerow", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length == 3)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetArticleData(o);
                    var cmd = arguments[1].ToString();
                    var rowidx = 0;
                    if (arguments.Length >= 3) rowidx = (int)arguments[2];
                    var row = articleData.GetRow(rowidx);

                    switch (cmd)
                    {
                        case "rowkey":
                            dataValue = row.RowKey;
                            break;
                        default:
                            dataValue = row.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = row.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;

                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterArticle(IHandlebars hbs)
        {
            hbs.RegisterHelper("article", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Count() >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetArticleData(o);
                    var cmd = arguments[1].ToString();

                    dataValue = articleData.Info.GetXmlProperty(cmd);
                    if (dataValue == "") dataValue = articleData.Info.GetXmlProperty("genxml/lang/" + cmd);

                }
                writer.WriteSafeString(dataValue);
            });
        }

        private static void RegisterDocumentShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("doctest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetArticleData(o);

                var cmd = arguments[1].ToString();
                var rowidx = 0;
                if (arguments.Length >= 3) rowidx = (int)arguments[2];
                var idx = 0;
                if (arguments.Length >= 4) idx = (int)arguments[3];
                var doc = articleData.GetRow(rowidx).GetDoc(idx);

                if (cmd == "isshown")
                {
                    if (!doc.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (doc.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }

            });
        }
        private static void RegisterDocument(IHandlebars hbs)
        {
            hbs.RegisterHelper("document", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetArticleData(o);

                    var cmd = arguments[1].ToString();
                    var rowidx = 0;
                    if (arguments.Length >= 3) rowidx = (int)arguments[2];
                    var idx = 0;
                    if (arguments.Length >= 4) idx = (int)arguments[3];
                    var doc = articleData.GetRow(rowidx).GetDoc(idx);

                    switch (cmd)
                    {
                        case "key":
                            dataValue = doc.DocKey;
                            break;
                        case "name":
                            dataValue = doc.Name;
                            break;
                        case "hidden":
                            dataValue = doc.Hidden.ToString().ToLower();
                            break;
                        case "fieldid":
                            dataValue = doc.FieldId;
                            break;
                        case "relpath":
                            dataValue = doc.RelPath;
                            break;
                        case "count":
                            dataValue = articleData.GetRow(rowidx).GetDocList().Count.ToString();
                            break;
                        case "url":
                            var enginrUrl = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                            dataValue = enginrUrl.TrimEnd('/') + "/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=" + doc.RelPath;
                            break;
                        default:
                            dataValue = doc.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = doc.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;
                    }
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterLinkShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("linktest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetArticleData(o);
                var cmd = arguments[1].ToString();
                var rowidx = 0;
                if (arguments.Length >= 3) rowidx = (int)arguments[2];
                var idx = 0;
                if (arguments.Length >= 4) idx = (int)arguments[3];
                var lnk = articleData.GetRow(rowidx).Getlink(idx);

                if (cmd == "isshown")
                {
                    if (!lnk.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (lnk.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }

            });
        }
        private static void RegisterLink(IHandlebars hbs)
        {
            hbs.RegisterHelper("link", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetArticleData(o);
                    var cmd = arguments[1].ToString();
                    var rowidx = 0;
                    if (arguments.Length >= 3) rowidx = (int)arguments[2];
                    var idx = 0;
                    if (arguments.Length >= 4) idx = (int)arguments[3];
                    var lnk = articleData.GetRow(rowidx).Getlink(idx);

                    switch (cmd)
                    {
                        case "key":
                            dataValue = lnk.LinkKey;
                            break;
                        case "name":
                            dataValue =  lnk.Name;
                            break;
                        case "hidden":
                            dataValue = lnk.Hidden.ToString().ToLower();
                            break;
                        case "fieldid":
                            dataValue = lnk.FieldId;
                            break;
                        case "count":
                            dataValue = articleData.GetRow(rowidx).GetLinkList().Count.ToString();
                            break;
                        case "ref":
                            dataValue = lnk.Ref;
                            break;
                        case "type":
                            dataValue = lnk.LinkType.ToString();
                            break;
                        case "target":
                            dataValue = lnk.Target;
                            break;
                        case "anchor":
                            dataValue = lnk.Anchor;
                            break;
                        case "url":
                            dataValue = lnk.Url;
                            break;
                        default:
                            dataValue = lnk.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = lnk.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;
                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }

    }
}
