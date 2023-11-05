# Razor View Data Objects
The razor templates can use a defined set of data objects.  Some are auomatically add on every call, some are only injected if required.  

## Standard Razor View Data Objects
```
    articleData = (ArticleLimpet)sModel.GetDataObject("articledata");
    articleRowData = (ArticleRowLimpet)sModel.GetDataObject("articlerow");
    appThemeView = (AppThemeLimpet)sModel.GetDataObject("appthemeview");
    appThemeAdmin = (AppThemeLimpet)sModel.GetDataObject("appthemeadmin");
    appTheme = appThemeAdmin;
    appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");
    moduleData = (ModuleContentLimpet)sModel.GetDataObject("modulesettings");
    moduleDataInfo = new SimplisityInfo(moduleData.Record);
    portalData = (PortalLimpet)sModel.GetDataObject("portaldata");
    sessionParams = sModel.SessionParamsData;
    userParams = (UserParams)sModel.GetDataObject("userparams");
```

## SimplsityInfo Data 
```
    rowData   (Can be used for single row AppThemes)  
    headerData  
```

#### Legacy SimplsityInfo Data 
```
    info
    infoArticle
```
