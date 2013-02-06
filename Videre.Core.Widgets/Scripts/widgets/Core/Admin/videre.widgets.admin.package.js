videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.package = videre.widgets.base.extend(
{
    get_installedPackages: function() { return this._installedPackages; },
    set_installedPackages: function(v)
    {
        this._installedPackages = v;
        //this._dataDict = v.toDictionary(function(d) { return d.Id; });
    },
    get_availablePackages: function() { return this._availablePackages; },
    set_availablePackages: function(v)
    {
        this._availablePackages = v;
        //this._dataDict = v.toDictionary(function(d) { return d.Id; });
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._installedPackages = null;
        this._availablePackages = null;

        this._delegates = {
            onInstalledPackageDataReturn: videre.createDelegate(this, this._onInstalledPackageDataReturn),
            onActionClicked: videre.createDelegate(this, this._onActionClicked)
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.bind();

        var uploader = new qq.FileUploaderBasic({
            button: this.getControl('btnImport')[0],
            params: {},
            action: videre.resolveUrl('~/core/portal/importportal'),
            onSubmit: videre.createDelegate(this, this._onFileSubmit),
            onComplete: videre.createDelegate(this, this._onFileUploadReturn),
            showMessage: videre.createDelegate(this, this._onFileMessage),
            debug: true
        });
    },

    refresh: function()
    {
        this.ajax('~/core/Package/GetInstalledPackages', {}, this._delegates.onInstalledPackageDataReturn);
    },

    bind: function()
    {
        videre.dataTables.clear(this.getControl('ItemTable'));
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._installedPackages));
        this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);
        videre.dataTables.bind(this.getControl('ItemTable'), [{ bSortable: false, sWidth: '62px' }]);
    },

    reset: function()
    {
        this.clearMsgs();
        //this.clearMsgs(this._dialog);
    },

    _handleAction: function(action, id)
    {
        this._selectedItem = this._dataDict[id];
        if (this._selectedItem != null)
        {
            //if (action == 'edit')
            //    this.edit();
            //else if (action == 'delete')
            //    this.deleteItem(id);
        }
    },

    _onInstalledPackageDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._installedPackages = result.Data;
            this.bind();
        }
    },

    _onActionClicked: function(e)
    {
        var ctl = $(e.target);
        if (e.target.tagName.toLowerCase() != 'a')  //if clicked in i tag, need a
            ctl = ctl.parent();
        this._handleAction(ctl.data('action'), ctl.data('id'));
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
            this.refresh();
        }
        this.addMsgs(result.Messages);
    },

    _onFileMessage: function(message)
    {
        this.addMsg('FileMessage', message, true, this.getControl('Dialog'));   //todo: test!
        this.refresh();
    }


});

