/*
Copyright (c) 2003-2011, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.editorConfig = function(config)
{
    // Define changes to default configuration here. For example:
    // config.language = 'fr';
    // config.uiColor = '#AADC6E';
    //config.height = '100%';
    //config.contentsCss = 'editorContents';
    //config.toolbar = 'Videre';
    //config.toolbar_Videre =
    //[
    //    ['Source', 'Bold', 'Italic', '-', 'NumberedList', 'BulletedList', '-', 'Image', 'Link', 'Unlink', '-', 'About']
    //];
    config.filebrowserBrowseUrl = videre.resolveUrl('~/Admin/FileBrowser?MimeType=image');
    config.skin = 'BootstrapCK-Skin';
    //config.filebrowserUploadUrl = videre.rootUrl() + '/Admin/File';

};
