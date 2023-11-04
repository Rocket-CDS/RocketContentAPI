# Razor Tokens - RocketContent

```
@AssigDataModel(SimplisityRazor sModel)
```
Assigns the data model for razor, this makes the template easier to build.

```
@RowKey(SimplisityInfo rowData)
```
A row MUST have a rowkey to be saved to the DB.  This generates the rowkey.

```
@ListUrl(int listpageid)
```
For list and detail this token builds the List URL.

```
@DetailUrl(int detailpageid, string title, string rowkey)
```
For list and detail this token builds the Detail URL.

```
@TextBoxRowTitle(SimplisityInfo rowData)
```
Creates a textbox for the title, using a standard xpath.

```
@CheckBoxRowIsHidden(SimplisityInfo rowData)
```
Creates a checkbox for the IsHidden property of a row.
