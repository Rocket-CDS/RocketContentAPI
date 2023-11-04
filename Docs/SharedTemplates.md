To create high-performance, uncomplicated modules, there are shared templates.   
These are templates used for specific functionalities and stop duplication of common requirements.

To use a shared template you use a INJECT token (recommended) or the @RenderTemplate() token.

**\[INJECT:appthemesystem,ArticleRowHeader.cshtml\]**  
Standard input fields for a row heading.

**\[INJECT:appthemesystem,ArticleRowHeaderView.cshtml\]**  
Displays standard fields for a row heading.

**\[INJECT:appthemesystem,ArticleLinks.cshtml\]**  
Add multiple links 

**\[INJECT:appthemesystem,ArticleLink.cshtml\]**  
Add a single link

**\[INJECT:appthemesystem,ArticleImages.cshtml\]**  
Add multiple images 

**\[INJECT:appthemesystem,ArticleImage.cshtml\]**    
Add a single image

**\[INJECT:appthemesystem,ArticleImagesSize.cshtml\]**  
Add multiple images with fields to define size

**\[INJECT:appthemesystem,ArticleDocuments.cshtml\]**  
Add multiple documents 

**\[INJECT:appthemesystem,ArticleDocument.cshtml\]**  
Add a single document

**\[INJECT:appthemesystem,ArticleHeader.cshtml\]**  
Add a title, its size and alignment, for the headerData.

**\[INJECT:appthemesystem,ArticleHeaderView.cshtml\]**  
Display headerData.

**\[INJECT:appthemesystem,CKEditor4.cshtml\]**  
Adds a CDN for CKEditor. This token is to standardize across AppThemes.