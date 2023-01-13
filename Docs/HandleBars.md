# Handlebars Reference

RocketCDS uses Razor and handlebars.js to display data.  This document defines the handlerbars helpers (functions) to help display data.

https://handlebarsjs.com/  

RocketCDS uses XML to store data, Handlebars.js uses Json.  A conversion from XML to json is done for handlebars.  You can therefore use the standard json syntax to get data.  However, we have also introduced a number of helpers to make displaying data more friendly.

### general session

**{{displayData this}}** - This helper is used to display json string, this is helpful in understanding what data you are using.  
```
{{moduleref @root}}
{{engineurl @root}}
{{culturecode @root}}
{{culturecodeedit @root}}
```

### article

```
{{article @root "name" @index}}
{{article @root "genxml/textbox/mytextbox" @index}}

```

### articlerow

```
{{#each genxml.data.genxml.rows.genxml}}

    {{articlerow @root "rowkey" @index}}
    {{articlerow @root "genxml/textbox/mytextbox" @index}}

{{/each}}
```


### image
```
<img src='{{image @root "thumburl" 0 0 640 200}}' style="width:100%">

{{#each genxml.data.genxml.rows.genxml}}
    {{#each imagelist.genxml}}

        {{#imagetest @root "ishidden" @../index @index}}
            <h1>Is Hidden</h1>
        {{/imagetest}}
        {{#imagetest @root "isshown" @../index @index}}
            <h1>Is Shown</h1>
        {{/imagetest}}
        {{#imagetest @root "hasheading" @../index @index}}
            <h1>hasheading</h1>
        {{/imagetest}}
        {{#imagetest @root "hasimage" @../index @index}}
            <h1>hasimage</h1>
        {{/imagetest}}
        {{#imagetest @root "haslink" @../index @index}}
            <h1>haslink</h1>
        {{/imagetest}}
        {{#imagetest @root "hassummary" @../index @index}}
            <h1>hassummary</h1>
        {{/imagetest}}


        {{image @root "thumburl" @../index @index 640 200}}
        {{image @root "alt" @../index @index}}
        {{image @root "summary" @../index @index}}
        {{image @root "relpath" @../index @index}}
        {{image @root "height" @../index @index}}
        {{image @root "width" @../index @index}}
        {{image @root "url" @../index @index}}
        {{image @root "urltext" @../index @index}}
        {{image @root "fieldid" @../index @index}}
        {{image @root "count"}}
        {{image @root "genxml/textbox/mytextbox" @../index @index}}

    {{/each}}
{{/each}}
```
NOTE: There is a bug in handlebars.js.  If the "@../index" is used within a loop it only works for the @first, after that the @index seems to be passed.

```
<div class="w3-col m3 l2 w3-padding-small">
    {{#each genxml.data.genxml.imagelist.genxml}}
    {{#if @first}}
        <img src="{{thumbnailimageurl @root.genxml.sessionparams.r.engineurl hidden.imagepatharticleimage 400 -1  }}" style="width:100%">
    {{/if}}
    {{/each}}
</div>
```

### doc
```
{{#each genxml.data.genxml.rows.genxml}}
    {{#each documentlist.genxml}}

        {{#doctest @root "ishidden" @../index @index}}
            <h1>Is Hidden</h1>
        {{/doctest}}
        {{#doctest @root "isshown" @../index @index}}
            <h1>Is Shown</h1>
        {{/doctest}}

        {{document @root "key" @../index @index}}
        {{document @root "name" @../index @index}}
        {{document @root "hidden" @../index @index}}
        {{document @root "url" @../index @index}}
        {{document @root "relpath" @../index @index}}
        {{document @root "fieldid" @../index @index}}
        {{document @root "count"}}
        {{document @root "genxml/textbox/mytextbox" @../index @index}}

    {{/each}}
{{/each}}
```
### link
```
{{#each genxml.data.genxml.rows.genxml}}
    {{#each linklist.genxml}}

        {{#linktest @root "ishidden" @../index @index}}
            <h1>Is Hidden</h1>
        {{/linktest}}
        {{#linktest @root "isshown" @../index @index}}
            <h1>Is Shown</h1>
        {{/linktest}}

        {{link @root "key" @../index @index 640 200}}
        {{link @root "name" @../index @index 640 200}}
        {{link @root "hidden" @../index @index 640 200}}
        {{link @root "fieldid" @../index @index}}
        {{link @root "count"}}
        {{link @root "ref" @../index @index}}
        {{link @root "type" @../index @index}}
        {{link @root "target" @../index @index}}
        {{link @root "anchor" @../index @index}}
        {{link @root "url" @../index @index}}
        {{link @root "genxml/textbox/mytextbox" @../index @index}}

    {{/each}}
{{/each}}
```

## Settings

Settings that are added to the module via the "RemoteSettings.cshtml" can be accessed by using root helper.
```
<div class="w3-third w3-padding">
    <label>@ResourceKey("DNNrocket.height") (px)</label>
    @TextBox(info, "genxml/settings/height", " class='w3-input w3-border'", "200")
</div>
```
Is accesed in handlebars by:
```
{{@root.genxml.remotemodule.genxml.settings.height}}
```

```
<img class="sliderind{{moduleref @root}}" src="{{image @root "thumburl" @../index @index @root.genxml.remotemodule.genxml.settings.width @root.genxml.remotemodule.genxml.settings.height}}" style="width: 100%;">
```

#### imageresize
The resize of any image is a setting.  This can be in the razor template as a s-field called "imageresize".
```
<input id="imagefileupload" class="simplisity_base64upload" s-reload="false" s-cmd="remote_addimage" s-post="#a-articledata" s-list=".@articleData.ImageListName,.@articleData.DocumentListName,.@articleData.LinkListName" s-fields='{"imageresize":"640","dataref":"@(articleData.DataRef)","moduleref":"@remoteModule.ModuleRef"}' type="file" name="file[]" multiple style="display:none;">
```
If no imageresize value exists on the upload filed the module settings field called "imageresize" is used.
```
<div class="w3-third w3-padding">
    <label>@ResourceKey("DNNrocket.imageresize") (px)</label>
    @TextBox(info, "genxml/settings/imageresize", " class='w3-input w3-border'", "1400")
</div>
```

The default resize is 640px.

## when (test) helper

```
Test 2 values with select operator.

{{#when <operand1> 'eq' <operand2>}}
    do something here
{{/when}}


eq:     ==
noteq:  !=
gt:     >
gteq:   >=
lt:     <
lteq:   <=
```

## resourcekey helper
```
{{resourcekey this, file.key, language = '', extension = 'Text'}}
```
In the razor template that calls the handlebars templates (Usually view.cshtml) you must have defined the rexlist.  The resxlist passes the paths for resx files to handlebars. 
```
AddProcessData("resourcepath", "/DesktopModules/DNNrocket/api/App_LocalResources/");
AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketContentAPI/App_LocalResources/");
AddProcessData("resourcepath", "/DesktopModules/RocketThemes/" + articleData.Organisation + "/" + articleData.AppThemeFolder+ "/" + articleData.AdminAppThemeFolderVersion  + "/resx");
    
hbsDict.Add("resxlist", RenderRazorUtils.GetResxPaths(Processdata));
```

Example:
```
{{resourcekey @root "Theme.tel" "" "Text"}}:{{textbox.tel}}
```