videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.webreference = videre.widgets.base.extend(
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
            onDataSaveReturn: videre.createDelegate(this, this._onDataSaveReturn),
            onActionClicked: videre.createDelegate(this, this._onActionClicked)
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._dialog = this.getControl('Dialog').modal('hide');
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.getControl('btnNew').click(videre.createDelegate(this, this._onNewClicked));
        this.bind();

    },

    refresh: function()
    {
        this.ajax('~/core/Portal/GetWebReferences', {}, this._delegates.onDataReturn);
    },

    bind: function()
    {
        videre.dataTables.clear(this.getControl('ItemTable'));
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._data));
        this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);
        videre.dataTables.bind(this.getControl('ItemTable'), { aoColumns: [{ bSortable: false }] });
    },

    reset: function()
    {
        this.clearMsgs();
        this.clearMsgs(this._dialog);
    },

    newItem: function()
    {
        this._selectedItem = { Name: '', DependencyGroups: [] };
        this.edit();
    },

    edit: function()
    {
        if (this._selectedItem != null)
        {
            this.reset();
            videre.UI.showModal(this._dialog);
            this.bindData(this._selectedItem, this._dialog);
        }
    },

    save: function()
    {
        if (this.validControls(this._dialog, this._dialog))
        {
            var item = this.persistData(this._selectedItem, true, this._dialog);

            if (item.Sequence == '')
                item.Sequence = null;

            item.Type = new Number(item.Type);
            item.LoadType = new Number(item.LoadType);

            this.ajax('~/core/Portal/SaveWebReference', { webReference: item }, this._delegates.onDataSaveReturn, null, this._dialog);
        }
    },

    deleteItem: function(id)
    {
        //if (confirm('Are you sure you wish to remove this entry?')) //todo: localize?
        var self = this;
        videre.UI.confirm('Delete Entry', 'Are you sure you wish to remove this entry?', function ()
        {
            self.ajax('~/core/Portal/DeleteWebReference', { id: id }, self._delegates.onDataSaveReturn);
        });
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

    _onDataSaveReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this.refresh();
            this._dialog.modal('hide');
        }
    },

    _onActionClicked: function(e)
    {
        var ctl = $(e.target).closest('[data-action]');
        this._handleAction(ctl.data('action'), ctl.data('id'));
    },

    _onSaveClicked: function(e)
    {
        this.save();
    },

    _onNewClicked: function(e)
    {
        this.newItem();
    }


});

