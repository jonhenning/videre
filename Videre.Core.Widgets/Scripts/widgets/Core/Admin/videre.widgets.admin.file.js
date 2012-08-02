videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.file = videre.widgets.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method
        this._tree = null;
        this._data = null;
        this._dataDict = null;
        this._treeData = null;
        this._itemData = null;
        this._itemDataDict = null;

        this._selectedKey = null;
        this._selectedItem = null;

        this._itemListCtr = null;
        this._dialog = null;
        this._uniqueName = null;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn),
            onNodeSelected: videre.createDelegate(this, this._onNodeSelected),
            onActionClicked: videre.createDelegate(this, this._onActionClicked)
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base

        this._tree = this.getControl('Tree').dynatree(
        {
            minExpandLevel: 2,
            onActivate: this._delegates.onNodeSelected
        });
        this._itemListCtr = this.getControl('ItemListCtr').hide();
        this._dialog = this.getControl('Dialog').modal('hide');
        this.getControl('btnNew').click(videre.createDelegate(this, this._onNewClicked));
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));

        var uploader = new qq.FileUploaderBasic({
            button: this.getControl('btnUpload')[0],
            params: {},
            action: videre.resolveUrl('~/core/file/upload'),
            onSubmit: videre.createDelegate(this, this._onFileSubmit),
            onComplete: videre.createDelegate(this, this._onFileUploadReturn),
            showMessage: videre.createDelegate(this, this._onFileMessage),
            debug: true
        });

        this.refresh();
    },

    refresh: function()
    {
        this.ajax('~/core/File/Get', {}, this._delegates.onDataReturn);
    },

    bindTree: function()
    {
        this._tree.dynatree("getRoot").removeChildren();
        this._tree.dynatree("getRoot").append(this._treeData);
        this._tree.find('.dynatree-container').height(this._tree.innerHeight() - 8).css('margin', 0);   //todo: better way?

        if (this._selectedKey != null)
        {
            var node = (this._tree.dynatree('getTree').activateKey(this._selectedKey));
            if (node == null)
            {
                this._itemData = null;
                this.bindItems(this._selectedKey);
            }
        }
    },

    bindItems: function(key)
    {
        var items = this._data.where(function(d) { return d.treeKey.indexOf(key) == 0; }).orderBy(function(d) { return d.Key; }); 
        if (items.length > 0)
        {
            this._itemDataDict = items.toDictionary(function(d) { return d.Id; });

            this._itemListCtr.show();
            this.getControl('ItemTable').dataTable().fnDestroy();
            this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(items));
            this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);
            this.getControl('ItemTable').dataTable({ sPaginationType: 'full_numbers', sPaginationType: 'bootstrap', aoColumns: [{ bSortable: false }, null, null, null] }); //http://datatables.net/blog/Twitter_Bootstrap_2
        }
        else
            this._itemListCtr.hide();
    },

    newItem: function()
    {
        this._selectedItem = { Id: '', PortalId: '', MimeType: '', Url: '', Size: null };
        this.edit();
    },

    edit: function()
    {
        if (this._selectedItem != null)
        {
            this.getControl('txtFileName').val('');
            this.bindData(this._selectedItem);
            this._uniqueName = null;

            this._dialog.modal('show');
            this.clearMsgs(this._dialog);
        }
        else
            this._dialog.modal('hide');
    },

    save: function()
    {
        if (this._selectedItem != null)
        {
            var file = this.persistData(this._selectedItem, null, null, true);
            //file.Urls = this.getControl('txtUrls').val().split('\r\n');
            this.ajax('~/core/File/Save', { file: file, uniqueName: this._uniqueName }, this._delegates.onSaveReturn, null, this._dialog);
        }
    },

    deleteItem: function(id)
    {
        if (confirm('Are you sure you wish to remove this entry?'))
            this.ajax('~/core/File/Delete', { id: id }, this._delegates.onSaveReturn);
    },

    handleAction: function(action, id)
    {
        this._selectedItem = this._itemDataDict[id];
        if (this._selectedItem != null)
        {
            if (action == 'edit')
                this.edit();
            else if (action == 'delete')
                this.deleteItem(id);
        }
    },

    _onNodeSelected: function(node)
    {
        this._selectedKey = node.data.key;
        this.bindItems(this._selectedKey);
    },

    _onDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._data = result.Data.select(function(d)
            {
                d.treeKey = d.Url;
                d.imageUrl = (d.MimeType.indexOf('image/') > -1 ? videre.resolveUrl('~/Core/f/' + d.Url) : videre.resolveUrl('~/content/images/document.png'));
                return d; 
            });
            this._treeData = videre.tree.getTreeData('root', this._data, function(d) { return d.treeKey; });
            this.bindTree();
        }
    },

    _onSaveReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this._dialog.modal('hide');
            this.refresh();
        }
    },

    _onActionClicked: function(e)
    {
        var ctl = $(e.target);
        if (e.target.tagName.toLowerCase() != 'a')  //if clicked in i tag, need a
            ctl = ctl.parent();
        this.handleAction(ctl.data('action'), ctl.data('id'));
    },

    _onNewClicked: function(e)
    {
        this.newItem();
    },

    _onSaveClicked: function(e)
    {
        this.save();
    },

    _onTypeChanged: function(e)
    {
        this.refresh();
    },

    _onFileSubmit: function(id, fileName)
    {
        this.lock(this.getControl('Dialog'));
    },

    _onFileUploadReturn: function(id, fileName, result)
    {
        this.unlock(this.getControl('Dialog'));
        if (!result.HasError)
        {
            this._uniqueName = result.Data.uniqueName;
            this.getControl('txtMimeType').val(result.Data.mimeType);
            this.getControl('txtFileName').val(result.Data.fileName);
            this.getControl('txtFileSize').val(result.Data.fileSize);
        }
        this.addMsgs(result.Messages, this.getControl('Dialog'));
    },

    _onFileMessage: function(message)
    {
        this.addMsg('FileMessage', message, true, this.getControl('Dialog'));   //todo: test!
    }


});

