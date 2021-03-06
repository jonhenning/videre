﻿videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.secureactivity = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v)
    {
        this._data = v;
        this._dataDict = v.toDictionary(function(d) { return d.Id; });
    },
    get_roleData: function() { return this._data; },
    set_roleData: function(v)
    {
        this._roleData = v;
        this._roleDataDict = this._roleData.toDictionary(function(d) { return d.Id; });
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._dataDict = null;
        this._roleData = null;
        this._roleDataDict = null;
        this._selectedItem = null;

        this._dialog = null;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onDataSaveReturn: videre.createDelegate(this, this._onDataSaveReturn),
            onActionClicked: videre.createDelegate(this, this._onActionClicked)//,
            //onItemRendered: videre.createDelegate(this, this._onItemRendered)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._dialog = this.getControl('Dialog').modal('hide');
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.bind();

    },

    refresh: function()
    {
        this.ajax('~/core/Portal/GetSecureActivities', {}, this._delegates.onDataReturn);
    },

    bind: function()
    {
        videre.dataTables.clear(this.getControl('ItemTable'));
        //this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._data));
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._data, { roleDataDict: this._roleDataDict }));
        this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);

        videre.dataTables.bind(this.getControl('ItemTable'), { aoColumns: [{ bSortable: false, sWidth: '31px' }] });
        this.getControl('ItemTable').width('500px');    //todo:  hack necessary for when its hidden in non-shown tab during bind
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
            videre.UI.showModal(this._dialog);
            this.bindData(this._selectedItem, this._dialog);
        }
    },

    save: function()
    {
        if (this.validControls(this._dialog, this._dialog))
        {
            var item = this.persistData(this._selectedItem, true, this._dialog);
            this.ajax('~/core/Portal/SaveSecureActivity', { activity: item }, this._delegates.onDataSaveReturn, null, this._dialog);
        }
    },

    _handleAction: function(action, id)
    {
        this._selectedItem = this._dataDict[id];
        if (this._selectedItem != null)
        {
            if (action == 'edit')
                this.edit();
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
    }//,

    //_onItemRendered: function(ctr, data)
    //{
    //    var td = ctr.find('td[data-container="role"]');
    //    var roles = this._roleData
    //        .where(function(r) { return data.Roles.contains(r.Id); })
    //        .select(function(r) { return r.Name; });
    //    //        for (var i = 0; i < data.Roles.length; i++)
    //    //        {
    //    //            if (this._roleDataDict[data.Roles[i]] != null)
    //    //                roles.push(this._roleDataDict[data.Roles[i]].Name);
    //    //        }
    //    td.html(roles.join(', '));
    //}

});

