videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.packageexport = videre.widgets.base.extend(
{
    get_newManifest: function() { return this._newManifest; },
    set_newManifest: function(v) { this._newManifest = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._contentItems = null;
        this._itemListCtr = null;
        this._packageDialog = null;
        this._newManifest = null;
        this._editManifest = null;

        this._exportPackage = null;

        this._delegates = {
            onContentItemsReturn: videre.createDelegate(this, this._onContentItemsReturn),
            onContentItemExportReturn: videre.createDelegate(this, this._onContentItemExportReturn),
            onActionClicked: videre.createDelegate(this, this._onActionClicked)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._packageDialog = this.getControl('PackageDialog').modal('hide');

        this.getControl('ExportTypeList').find('[data-filter]').click(videre.createDelegate(this, this._onFilterClicked));
        this.getControl('PackageCtr').find('[data-action]').click(this._delegates.onActionClicked);
        this.getControl('btnPackageOk').click(videre.createDelegate(this, this._onPackageOkClicked));
        
        var uploader = new qq.FileUploaderBasic({
            button: this.getControl('PackageCtr').find('[data-action="load-package"]')[0],
            params: {},
            action: videre.resolveUrl('~/core/package/GetPackageContents'),
            onSubmit: videre.createDelegate(this, this._onFileSubmit),
            onComplete: videre.createDelegate(this, this._onFileUploadReturn),
            showMessage: videre.createDelegate(this, this._onFileMessage),
            debug: true
        });

        this._itemListCtr = this.getControl('ItemListCtr').hide();
        this.reset();
        //videre.UI.handleEnter(this._text, videre.createDelegate(this, this.search));
    },

    reset: function()
    {
        this._exportPackage = null;
        this._editManifest = videre.jsonClone(this._newManifest);
        this.refreshActions();
        this.clearMsgs();
        this.clearMsgs(this._packageDialog);
    },

    showPackageDialog: function()
    {
        this._packageDialog.modal('show');
        this._bindPackage();
    },


    refreshActions: function()
    {
        //this.getControl('PackageCtr').find('[data-action="edit-package"]').toggle(this._exportPackage != null);
        this.getControl('PackageCtr').find('[data-action="new-package"]').toggle(this._exportPackage != null || !String.isNullOrEmpty(this._editManifest.Name));
        this.getControl('PackageCtr').find('[data-action="download-package"]').toggle(this._exportPackage != null || !String.isNullOrEmpty(this._editManifest.Name));
        this.bindData(this._editManifest, this.getControl('PackageCtr'));
    },

    getContent: function()
    {
        this.ajax('~/core/Package/GetExportContentItems', { type: this._selectedType, exportPackage: this._exportPackage }, this._delegates.onContentItemsReturn);
    },

    exportContentItem: function(type, id)
    {
        this.ajax('~/core/Package/ExportContentItem', { type: type, id: id, exportPackage: this._exportPackage }, this._delegates.onContentItemExportReturn);
    },

    exportPackage: function()
    {
        //this.ajax('~/core/Package/ExportPackage', { manifest: this._editManifest, export: this._exportPackage });
        videre.download('~/core/Package/ExportPackage', { manifest: this._editManifest, exportPackage: this._exportPackage });
    },

    bindItems: function()
    {
        //this._itemData = this._data[this._selectedKey];
        if (this._contentItems.length > 0)
        {
            this._itemListCtr.show();
            videre.dataTables.clear(this.getControl('ItemTable'));
            this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._contentItems));
            this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);
            videre.dataTables.bind(this.getControl('ItemTable'), { aoColumns: [{ bSortable: false, sWidth: '31px' }] });
        }
        else
            this._itemListCtr.hide();
    },

    _handleAction: function(action, id)
    {
        if (action == 'add')
        {
            this.exportContentItem(this._selectedType, id);
        }
        else if (action == 'new-package')
            this.reset();
        else if (action == 'edit-package')
            this.showPackageDialog();
        else if (action == 'download-package')
            this.exportPackage();
        this.refreshActions();
    },

    _bindPackage: function()
    {
        this.bindData(this._editManifest, this._packageDialog);
        this.getControl('txtPackageContent').val(this._exportPackage != null ? videre.serialize(this._exportPackage, null, '   ') : '');
    },

    _persistPackage: function()
    {
        try 
        {
            if (this.validControls(this._packageDialog, this._packageDialog))
            {
                if (this.getControl('txtPackageContent').val().length > 0)
                    this._exportPackage = videre.deserialize(this.getControl('txtPackageContent').val());
                this.persistData(this._editManifest, false, this._packageDialog);
                return true;
            }
            return false;
        }
        catch (e)
        {
            this.addMsg('PersistPackage', e.message, true, this._packageDialog);
            return false;
        }
    },

    _onFilterClicked: function(e)
    {
        var ctl = $(e.target).closest('[data-filter]'); 
        this.getControl('ExportTypeList').find('a').removeClass('active');
        ctl.addClass('active');

        this._selectedType = ctl.data('filter');
        this.getContent();
    },

    _onActionClicked: function(e)
    {
        var ctl = $(e.target).closest('[data-action]');
        this._handleAction(ctl.data('action'), ctl.data('id'));
    },

    _onPackageOkClicked: function(e)
    {
        if (this._persistPackage())
        {
            this._packageDialog.modal('hide');
            this.refreshActions();
        }
    },

    _onContentItemsReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._contentItems = result.Data;
            this._itemDataDict = this._contentItems.toDictionary(function(d) { return d.Id; });
            this.bindItems();
        }
    },

    _onContentItemExportReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._exportPackage = result.Data;
            this.getContent();
            this.refreshActions();
        }
    },

    _onFileSubmit: function(id, fileName)
    {
        this.lock();
    },

    _onFileUploadReturn: function(id, fileName, result)
    {
        this.unlock();
        if (!result.HasError)
        {
            this._editManifest = result.Data.manifest;
            this._exportPackage = result.Data.content;
            this.refreshActions();
        }
        this.addMsgs(result.Messages);
    },

    _onFileMessage: function(message)
    {
        this.addMsg('FileMessage', message, true);
    }

});

