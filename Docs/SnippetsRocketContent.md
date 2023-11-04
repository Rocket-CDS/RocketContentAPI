# Snippets - RocketContent
These snippets and examples give help for building AppThemes.    

## Templates
### AdminDetail.cshtml - Single Row
```
@inherits RocketContentAPI.Components.RocketContentAPITokens<Simplisity.SimplisityRazor>
@AssigDataModel(Model)
@AddProcessDataResx(appThemeAdmin, true)
<!--inject-->

[INJECT:appthemeadmin,AdminRow.cshtml]

```
### AdminRow.cshtml - Single Row
Injected by token: **[INJECT:appthemeadmin,AdminRow.cshtml]**
```
@inherits RocketContentAPI.Components.RocketContentAPITokens<Simplisity.SimplisityRazor>
@AssigDataModel(Model)
@AddProcessDataResx(appThemeAdmin, true)
<!--inject-->
<!-- require Key for saving -->
@RowKey(rowData)

<div id="heading" class='w3-row w3-margin-top sectionname'>
    <div class='w3-col m12 w3-padding' style='min-width:200px;'>
        <label>@ResourceKey("DNNrocket.heading")</label> @EditFlag(sessionParams)
    @TextBox(rowData, "genxml/lang/genxml/textbox/title", " id='title' class='w3-input w3-border' autocomplete='off' ", "", true, 0)
    </div>
</div>
```

### AdminDetail.cshtml - Multiple Rows
```
@inherits RocketContentAPI.Components.RocketContentAPITokens<Simplisity.SimplisityRazor>
@AssigDataModel(Model)
@AddProcessDataResx(appThemeAdmin, true)
<!--inject-->

<div class="w3-row">
    [INJECT:appthemesystem,ArticleHeader.cshtml]
    <div id="articledetailpanel" class="w3-threequarter">
        [INJECT:appthemeadmin,AdminRow.cshtml]
    </div>
    <div class="w3-quarter">
        [INJECT:appthemesystem,AdminRowSelect.cshtml]
    </div>
</div>
```
### AdminRow.cshtml - Multiple Rows 
Injected by token: **[INJECT:appthemeadmin,AdminRow.cshtml]**
```
@inherits RocketContentAPI.Components.RocketContentAPITokens<Simplisity.SimplisityRazor>
@AssigDataModel(Model)
@AddProcessDataResx(appThemeAdmin, true)
<!--inject-->
<!-- require Key for saving -->
@RowKey(rowData)

<div id="heading" class='w3-row w3-margin-top sectionname'>
    <div class='w3-col m12 w3-padding' style='min-width:200px;'>
        <label>@ResourceKey("DNNrocket.heading")</label> @EditFlag(sessionParams)
    @TextBox(rowData, "genxml/lang/genxml/textbox/title", " id='title' class='w3-input w3-border' autocomplete='off' ", "", true, 0)
    </div>
</div>
```
**Row Heading**
```
<div class="w3-row">
    <div class='w3-col m10 w3-padding' style='min-width:200px;'>
        <label>@ResourceKey("DNNrocket.heading")</label>&nbsp;@EditFlag(sessionParams)
        @TextBoxRowTitle(rowData)
    </div>
    <div class='w3-col m2 w3-padding'>
        <label>@ResourceKey("DNNrocket.hiderow")</label><br />
        @CheckBoxRowIsHidden(rowData)
    </div>
</div>
```
### ThemeSettings.cshtml - Settings Header.
```
@inherits RocketContentAPI.Components.RocketContentAPITokens<Simplisity.SimplisityRazor>
@AssigDataModel(Model)
@AddProcessDataResx(appThemeAdmin, true)
<!--inject-->
@{
    var info = moduleDataInfo;
    //NOTE: xPath for module settings must use "genxml/settings/*"
}
```
**Checkbox for ECOMode:  This enables or disables ECOMode at module level**  
By default ECOMode is always on.  And the property "moduleData.ECOMode" can be used to test for it in the templates.
```
@CheckBox(info, "genxml/settings/ecomode", "&nbsp;" + ResourceKey("DNNrocket.ecomode").ToString(), " class='w3-check' ", true)
```
### View.cshtml - Template Row Loop.
```
@inherits RocketContentAPI.Components.RocketContentAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketContentAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appThemeView, true)
<!--inject-->

@foreach (var articleRowData in articleData.GetRows()) 
{

}


```

### CKEditor4 - Shared Inject
```
[INJECT:appthemesystem,CKEditor4.cshtml]
```
### ArticleHeader - Shared Inject
**Admin**
```
[INJECT:appthemesystem,ArticleHeader.cshtml]
```
**View**
```
[INJECT:appthemesystem,ArticleHeaderView.cshtml]
```


## Images
### Admin
**Single Image**  *INJECT or Render method.*
```
[INJECT:appthemesystem,ArticleImage.cshtml]
```
```
@RenderTemplate("ArticleImage.cshtml",appThemeSystem, Model, false)
```
**Multiple Images**  *INJECT or Render method.*
```
[INJECT:appthemesystem,ArticleImages.cshtml]
```
```
@RenderTemplate("ArticleImages.cshtml",appThemeSystem, Model, false)
```
**Multiple Images**  *With size*
```
[INJECT:appthemesystem,ArticleImagesSize.cshtml]
```

### View
**Single Image**
```
@{
    var articleImage = articleRowData.GetImage(0);
}
<img src="@ImageUrl(articleImage.RelPathWebp,200,200)" />
```
**Multiple Images**
```
@foreach (ArticleImage articleImage in articleRowData.GetImages())
{
    if (articleImage.RelPath != "")
    {
        <img src="@ImageUrl(articleImage.RelPathWebp, articleImage.Width, articleImage.Height)" alt="@(articleImage.Alt)" />
    }
}

```

## Documents
### Admin
**Single Document**
```
[INJECT:appthemesystem,ArticleDocument.cshtml]
```
**Multiple Documents**
```
[INJECT:appthemesystem,ArticleDocuments.cshtml]
```
### View
```
@foreach (ArticleDoc documentData in articleRowData.GetDocs())
{
    if (!documentData.Hidden)
    {
        <div>
            <a href="@(documentData.RelPath)" target="_blank" >@documentData.Name</a>
        </div>
    }
}
```
## Links
### Admin
**Single Link**
```
[INJECT:appthemesystem,ArticleLink.cshtml]
```
**Multiple Links**
```
[INJECT:appthemesystem,ArticleLinks.cshtml]
```
### View
```
@foreach (var linkData in articleRowData.Getlinks())
{    
    if (!linkData.Hidden)
    {
        <a href="@(linkData.Url)" target="@(linkData.Target)">@linkData.Name</a>
    }
}
```

