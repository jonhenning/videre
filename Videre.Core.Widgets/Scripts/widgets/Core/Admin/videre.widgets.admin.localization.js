videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.localization = videre.widgets.base.extend(
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
        this.getControl('ddlFilterType').change(videre.createDelegate(this, this._onTypeChanged));

        this.refresh();
    },

    refresh: function()
    {
        this.ajax('~/core/Localization/Get', { Type: this.getControl('ddlFilterType').val() }, this._delegates.onDataReturn);
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
        //this._itemData = this._data[this._selectedKey];
        var items = this._data.where(function(d) { return d.treeKey.indexOf(key) == 0; }).orderBy(function(d) { return d.Key; });
        if (items.length > 0)
        {
            this._itemDataDict = items.toDictionary(function(d) { return d.Id; });

            this._itemListCtr.show();
            videre.dataTables.clear(this.getControl('ItemTable'));
            this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(items));
            this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);
            videre.dataTables.bind(this.getControl('ItemTable'), { aoColumns: [{ bSortable: false }] });


        }
        else
            this._itemListCtr.hide();
    },

    newItem: function()
    {
        this._selectedItem = { Id: '', PortalId: '', Type: '', Namespace: '', Key: '', Locale: '', Text: '' };
        this.edit();
    },

    edit: function()
    {
        if (this._selectedItem != null)
        {
            this.bindData(this._selectedItem);
            videre.UI.showModal(this._dialog);
            this.clearMsgs(this._dialog);
        }
        else
            videre.UI.hideModal(this._dialog);
    },

    save: function()
    {
        if (this._selectedItem != null)
        {
            var loc = this.persistData(this._selectedItem);
            this.ajax('~/core/Localization/Save', { Localization: loc }, this._delegates.onSaveReturn, null, this._dialog);
        }
    },

    deleteItem: function(id)
    {
        //if (confirm('Are you sure you wish to remove this entry?'))
        var self = this;
        videre.UI.confirm('Delete Entry', 'Are you sure you wish to remove this entry?', function ()
        {
            self.ajax('~/core/Localization/Delete', { id: id }, self._delegates.onSaveReturn);
        });
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
            this._data = result.Data.select(function(d) { d.treeKey = d.Namespace + '/' + d.Key; return d; });
            this._treeData = videre.tree.getTreeData(this.getControl('ddlFilterType').find('option:selected').text(), this._data, function(d) { return d.treeKey; });
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
        var ctl = $(e.target).closest('[data-action]');
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
    }

});

