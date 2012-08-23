videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.user = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v)
    {
        this._data = v;
        if (this._hasCustomAttributes && this._data.Attributes == null)
            this._data.Attributes = {};
        this._dataDict = v.toDictionary(function(d) { return d.Id; });
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._dataDict = null;
        this._selectedItem = null;
        this._hasCustomAttributes = false;

        this._dialog = null;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn),
            onActionClicked: videre.createDelegate(this, this._onActionClicked)
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._dialog = this.getControl('Dialog').modal('hide');
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.getControl('btnNew').click(videre.createDelegate(this, this._onNewClicked));
        this._hasCustomAttributes = this.getControl('CustomTab').length > 0;
        this.bind();

    },

    refresh: function()
    {
        this.ajax('~/core/Account/GetUsers', {}, this._delegates.onDataReturn);
    },

    bind: function()
    {
        this.getControl('ItemTable').dataTable().fnDestroy();
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._data));
        this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);

        //http://datatables.net/blog/Twitter_Bootstrap_2
        this.getControl('ItemTable').dataTable({ sPaginationType: 'full_numbers', sPaginationType: 'bootstrap',
            aoColumns: [{ bSortable: false }, null, null, null]});

    },

    newItem: function()
    {
        this._selectedItem = { Name: '', Description: '' };
        this.edit();
    },

    edit: function()
    {
        if (this._selectedItem != null)
        {
            this._dialog.modal('show');
            this.bindData(this._selectedItem, this.getControl('GeneralTab'));
            if (this._hasCustomAttributes)
                this.bindData(this._selectedItem.Attributes, this.getControl('CustomTab'));
        }
    },

    save: function()
    {
        //todo: validation!
        var user = this.persistData(this._selectedItem, true, this.getControl('GeneralTab'));
        if (this._hasCustomAttributes)
            this.persistData(user.Attributes, false, this.getControl('CustomTab'));

        this.ajax('~/core/Account/SaveUser', { user: user }, this._delegates.onSaveReturn, null, this._dialog);
    },

    deleteUser: function(id)
    {
        if (confirm('Are you sure you wish to remove this entry?')) //todo: localize?
            this.ajax('~/core/Account/DeleteUser', { id: id }, this._delegates.onSaveReturn, null);
    },

    _handleAction: function(action, id)
    {
        this._selectedItem = this._dataDict[id];
        if (this._selectedItem != null)
        {
            if (action == 'edit')
                this.edit();
            else if (action == 'delete')
                this.deleteUser(id);
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
    }

});

