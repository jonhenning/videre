videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.search = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v)
    {
        this._data = v;
        this._dataDict = v.toDictionary(function(d) { return d.Id; });
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._dataDict = null;
        this._selectedItem = null;

        this._dialog = null;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn),
            onGenerateReturn: videre.createDelegate(this, this._onGenerateReturn),
            onActionClicked: videre.createDelegate(this, this._onActionClicked)
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._dialog = this.getControl('Dialog').modal('hide');
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.getControl('btnNew').click(videre.createDelegate(this, this._onNewClicked));
        this.getControl('btnClearIndex').click(videre.createDelegate(this, this._onClearIndexClicked));
        this.bind();
    },

    refresh: function()
    {
        this.ajax('~/core/Search/GetProviders', {}, this._delegates.onDataReturn);
    },

    bind: function()
    {
        //this.getControl('ItemTable').dataTable().fnDestroy();
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._data));
        this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);

        //http://datatables.net/blog/Twitter_Bootstrap_2
        //this.getControl('ItemTable').dataTable({ sPaginationType: 'full_numbers', sPaginationType: 'bootstrap',
        //    aoColumns: [{ bSortable: false }, null, null, null]
        //});
    },

    newItem: function()
    {
        this._selectedItem = { Name: '', ProviderType: '', Connection: '' };
        this.edit();
    },

    reset: function()
    {
        this.clearMsgs();
        this.clearMsgs(this._dialog);
    },

    edit: function()
    {
        if (this._selectedItem != null)
        {
            this.reset();
            this._dialog.modal('show');
            this.bindData(this._selectedItem, this._dialog);
        }
    },

    save: function()
    {
        //todo: validation!
        var provider = this.persistData(this._selectedItem, true, this._dialog);
        this.ajax('~/core/Search/SaveProvider', { provider: provider }, this._delegates.onSaveReturn, null, this._dialog);
    },

    deleteItem: function(id)
    {
        if (confirm('Are you sure you wish to remove this entry?')) //todo: localize?
            this.ajax('~/core/Search/DeleteProvider', { id: id }, this._delegates.onSaveReturn);
    },

    generate: function(id)
    {
        this.ajax('~/core/Search/Generate', { id: id }, this._delegates.onGenerateReturn);
    },

    clearIndex: function()
    {
        this.ajax('~/core/Search/ClearIndex', { }, this._delegates.onGenerateReturn);
    },

    _handleAction: function(action, id)
    {
        this._selectedItem = this._dataDict[id];
        if (this._selectedItem != null)
        {
            if (action == 'edit')
                this.edit();
            else if (action == 'delete')
                this.deleteItem(id);
            else if (action == 'generate')
                this.generate(id);
        }
    },

    _onDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this.set_data(result.Data);
            this.bind();
        }
    },

    _onSaveReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this.refresh();
            this._dialog.modal('hide');
        }
    },

    _onGenerateReturn: function(result)
    {
        this.refresh();
    },

    _onActionClicked: function(e)
    {
        var ctl = $(e.target);
        if (e.target.tagName.toLowerCase() != 'a')  //if clicked in i tag, need a
            ctl = ctl.parent();
        this._handleAction(ctl.data('action'), ctl.data('id'));
    },

    _onSaveClicked: function(e)
    {
        this.save();
    },

    _onNewClicked: function(e)
    {
        this.newItem();
    },

    _onClearIndexClicked: function(e)
    {
        this.clearIndex();
    }

});

