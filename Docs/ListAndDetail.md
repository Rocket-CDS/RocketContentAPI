# List and Detail AppThemes
Because we have multiple rows in an Apptheme we can choose to have a list and detail view.cshtml.  

To get a URL for the detail we use a url param "eid".  This is auto-generated on row save, by the "@RowKey(rowData)" token.  

To create the correct URL for details use the DetailUrl token:
```
var detailurl = DetailUrl(moduleData.GetSettingInt("listtabid"), title, articleRowData.eId).ToString();
```
## URL param of "eid"
The URL param name is always called "eid".  
*The "rowkey" param should not be used in the detail URL, it may cause a problem across modules.*

## Usage limitations
This method of list and detail is limited to the administration of the RocketContentMod.  While this does not limit the amount of detail pages it is recommanded not be used for lists of more than 100 records.  

**If bigger lists are required the RocketDirectory system should be used.**

## view.cshtml
The view.cshtml tests to see if we have a valid rowdetail for the "eid" in the URL.  
The "eid" can be found in the "sessionParams" data class.  
```
@inherits RocketContentAPI.Components.RocketContentAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketContentAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appThemeView, true)
@AddProcessData("resourcepath", "/DesktopModules/RocketThemes/AppTheme-AgenceSesame/rocketcontentapi.Articles/1.0/resx")
<!--inject-->
@{
    var rowDetail = articleData.GetRowById(sessionParams.Get("eid"));
}

@if (rowDetail != null)
{
    <!-- DETAIL VIEW --> 
    var rtnUrl = ListUrl(moduleData.GetSettingInt("listtabid"));

    <!-- Required Design here........... -->

}
else
{
    <!-- LIST VIEW --> 

    @foreach (var articleRowData in articleData.GetRows())
    {
        var detailurl = DetailUrl(moduleData.GetSettingInt("listtabid"), title, articleRowData.eId).ToString();

        <!-- Required Design here........... -->
    }
}
```

