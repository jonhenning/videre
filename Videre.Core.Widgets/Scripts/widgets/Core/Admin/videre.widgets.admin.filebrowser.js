videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.filebrowser = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v)
    {
        var self = this;
        this._data = v.select(function(d)
        {
            d.pathParts = d.Url.split('/');

            for (var i = 0; i < d.pathParts.length - 1; i++)    //-1 to trim off last part (fileName)
            {
                var path = d.pathParts.slice(0, i).join('/');
                if (self._folderDict[path] == null)
                    self._folderDict[path] = [];
                
                var newPath = path + (path != '' ? '/' : '') + d.pathParts[i];
                if (self._folderDict[path].where(function(p) { return p.path == newPath; }).length == 0)
                    self._folderDict[path].push({ path: newPath, name: d.pathParts[i] });
            }
            d.folderPath = d.pathParts.slice(0, d.pathParts.length - 1).join('/');
            return d;
        });
        this._dataDict = this._data.toDictionary(function(d) { return d.Id; });
    },

    get_editorData: function() { return this._editorData; },
    set_editorData: function(v) { this._editorData = v; },
    get_filePath: function() { return this._filePath; },
    set_filePath: function(v) { this._filePath = v; },
    get_mimeType: function() { return this._mimeType; },
    set_mimeType: function(v) { this._mimeType = v; },
    get_isModal: function() { return this._isModal; },
    set_isModal: function(v) { this._isModal = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = [];
        this._editorData = null;
        this._dataDict = null;
        this._folderDict = {};
        this._isModal = true;
        this._dialog = null;
        this._dataControl = null;

        this._delegates = {
            onNavClick: videre.createDelegate(this, this._onNavClick)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        if (this._isModal)
        {
            this._dialog = this.getControl('Dialog').modal('hide');
            this.getControl('btnOk').click(videre.createDelegate(this, this._onOkClicked));
        }
        else
            this.bind('');

        $('[data-action="filebrowser"]').click(videre.createDelegate(this, this._onBrowseClick));

    },

    show: function()
    {
        this.bind('');
        videre.UI.showModal(this._dialog);
    },

    hide: function()
    {
        this._dialog.modal('hide');
    },

    bind: function(folder)
    {
        var images = this._data
            .where(function(d) { return d.folderPath == folder; })
            .select(function(d) { d.isFolder = false; return d; })
            .orderBy(function(d) { return d.Url; });

        var bcPath = '';
        var breadcrumbs = [{ name: '(root)', path: '', active: folder == ''}];
        breadcrumbs.addRange(folder.split('/').select(function(d)
        {
            bcPath += (bcPath != '' ? '/' : '') + d;    //only put prefix of / when we are one deep... todo: hacky?
            return { name: d, path: bcPath, active: bcPath == folder };
        }));

        var folders = [];
        if (this._folderDict[folder] != null)
        {
            folders.addRange(this._folderDict[folder]
                .select(function(d) { d.isFolder = true; return d; })
                .orderBy(function(d) { return d; }));
        }

        this.getControl('BreadcrumbList').html(this.getControl('BreadcrumbListTemplate').render(breadcrumbs));

        var items = [].addRange(folders).addRange(images);

        this.getControl('ImageList').html(this.getControl('ImageListTemplate').render(items));

        this.getControl('BreadcrumbList').find('a').click(this._delegates.onNavClick);
        this.getControl('ImageList').find('a').click(this._delegates.onNavClick);

    },

    select: function(id)
    {
        var item = this._dataDict[id];
        if (item != null)
        {
            if (this._editorData.CKEditor != null)
            {
                window.opener.CKEDITOR.tools.callFunction(this._editorData.CKEditorFuncNum, this._filePath + item.Url);
                window.close();
            }
            else if (this._dataControl != null)
            {
                this._dataControl.val(this._filePath + item.Url);
                this._dataControl.trigger('change');
            }
            //else
            //this.raiseOnSelection(item);

            if (this._isModal)
                this.hide();
        }
    },

    _onNavClick: function(e)
    {
        var ctl = $(e.target).closest('[data-action]');
        var action = ctl.data('action');
        if (action != null)
        {
            if (action == 'navigate')
                this.bind(ctl.data('path'));
            if (action == 'select')
                this.select(ctl.data('id'));
        }
    },

    //    _onOkClicked: function(e)
    //    {
    //        if (this._dirty)
    //            this.applyItem();
    //        this.save();
    //    },

    _onBrowseClick: function(e)
    {
        var ctl = $(e.target).closest('[data-action]');
        this._dataControl = ctl.closest('.input-group').find('[data-controltype="filebrowser-input"]'); //$('#' + ctl.data('control'));
        this.show();
    } //,

    //    add_onSelection: function(handler) { this.get_events().addHandler("OnSelection", handler); },
    //    remove_onSelection: function(handler) { this.get_events().removeHandler("OnSelection", handler); },
    //    raiseOnSelection: function(item)
    //    {
    //        var handler = this.get_events().getHandler("OnSelection");
    //        if (handler)
    //            return handler(this, { url: this._filePath + item.Url, item: item });
    //        return true;
    //    }

});

