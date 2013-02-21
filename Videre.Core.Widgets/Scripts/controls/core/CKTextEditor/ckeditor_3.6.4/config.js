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
    config.toolbar_Videre =
    [
        { name: 'document',    items : [ 'Source','-','Save','NewPage','DocProps','Preview','Print','-','Templates' ] },
        { name: 'clipboard',   items : [ 'Cut','Copy','Paste','PasteText','PasteFromWord','-','Undo','Redo' ] },
        { name: 'editing',     items : [ 'Find','Replace','-','SelectAll','-', 'Scayt' ] },
        //{ name: 'forms',       items : [ 'Form', 'Checkbox', 'Radio', 'TextField', 'Textarea', 'Select', 'Button', 'ImageButton', 'HiddenField' ] },
        '/',
        { name: 'basicstyles', items : [ 'Bold','Italic','Underline','Strike','Subscript','Superscript','-','RemoveFormat' ] },
        { name: 'paragraph',   items : [ 'NumberedList','BulletedList','-','Outdent','Indent','-','Blockquote','CreateDiv','-','JustifyLeft','JustifyCenter','JustifyRight','JustifyBlock','-','BidiLtr','BidiRtl' ] },
        { name: 'links',       items : [ 'Link','Unlink','Anchor' ] },
        { name: 'insert',      items : [ 'Image','Flash','Table','HorizontalRule','Smiley','SpecialChar','PageBreak' ] },
        '/',
        { name: 'styles',      items : [ 'Styles','Format','Font','FontSize' ] },
        { name: 'colors',      items : [ 'TextColor','BGColor' ] },
        { name: 'tools',       items : [ 'Maximize', 'ShowBlocks','-','About' ] }
    ];

    config.toolbar = 'Videre';
    config.language = 'en'; //force to english for now...  eventually include language files
    config.filebrowserBrowseUrl = videre.resolveUrl('~/Admin/FileBrowser?MimeType=image');
    config.skin = 'BootstrapCK-Skin';
    config.contentsCss = $('link[data-type="theme"]').toArray().toList(function(d) { return d.href; });

    config.stylesSet =
    [
        // Block Styles
        { name: 'Well', element: 'div', attributes: { 'class': 'well' } },
        { name: 'Address', element: 'address', attributes: { } },
        { name: 'Blockquote', element: 'blockquote', attributes: { } },
        { name: 'Inline Code', element: 'code', attributes: {} },
        { name: 'Code', element: 'pre', attributes: { 'class': 'prettyprint linenums'} },
        { name: 'Scrollable Code', element: 'pre', attributes: { 'class': 'prettyprint linenums pre-scrollable' } },
        { name: 'Label', element: 'span', attributes: { 'class': 'label' } },
        { name: 'Success Label', element: 'span', attributes: { 'class': 'label label-success' } },
        { name: 'Warning Label', element: 'span', attributes: { 'class': 'label label-warning' } },
        { name: 'Important Label', element: 'span', attributes: { 'class': 'label label-important' } },
        { name: 'Info Label', element: 'span', attributes: { 'class': 'label label-info' } },
        { name: 'Inverse Label', element: 'span', attributes: { 'class': 'label label-inverse' } },
        { name: 'Badge', element: 'span', attributes: { 'class': 'badge' } },
        { name: 'Success Badge', element: 'span', attributes: { 'class': 'badge badge-success' } },
        { name: 'Warning Badge', element: 'span', attributes: { 'class': 'badge badge-warning' } },
        { name: 'Important Badge', element: 'span', attributes: { 'class': 'badge badge-important' } },
        { name: 'Info Badge', element: 'span', attributes: { 'class': 'badge badge-info' } },
        { name: 'Inverse Badge', element: 'span', attributes: { 'class': 'badge badge-inverse' } },
        { name: 'Hero', element: 'div', attributes: { 'class': 'hero-unit' } },
        { name: 'Alert', element: 'div', attributes: { 'class': 'alert' } },
        { name: 'Alert Error', element: 'div', attributes: { 'class': 'alert alert-error' } },
        { name: 'Alert Success', element: 'div', attributes: { 'class': 'alert alert-success' } },
        { name: 'Alert Info', element: 'div', attributes: { 'class': 'alert alert-info' } }
    ];

    //CKEDITOR.plugins.addExternal('bootstrap', CKEDITOR.basePath, 'mybootstrap.js');
    //config.extraPlugins = 'bootstrap';

};
