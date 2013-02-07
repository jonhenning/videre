/// <reference path="../../../controls/core/CKTextEditor/ckeditor_3.6.4/ckeditor.js" />
videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.package = videre.widgets.base.extend(
{
    get_installedPackages: function() { return this._installedPackages; },
    set_installedPackages: function(v)
    {
        this._installedPackages = v;
        this._installedPackageDict = v.toDictionary(function(d) { return d.Name + '~' + d.Version; });
    },
    get_availablePackages: function() { return this._availablePackages; },
    set_availablePackages: function(v)
    {
        this._availablePackages = v;
        this._availablePackageDict = v.toDictionary(function(d) { return d.Name + '~' + d.Version; });
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._installedPackages = null;
        this._installedPackageDict = null;
        this._availablePackages = null;
        this._availablePackageDict = null;
        this._dialog = null;

        this._delegates = {
            onInstalledPackageDataReturn: videre.createDelegate(this, this._onInstalledPackageDataReturn),
            onInstalledPackageReturn: videre.createDelegate(this, this._onInstalledPackageReturn),
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

        this._dialog = this.getControl('Dialog').modal('hide');
        this.getControl('btnCreatePackage').click(videre.createDelegate(this, this._onCreatePackageClicked));

    },

    refresh: function()
    {
        this.ajax('~/core/Package/GetPackages', {}, this._delegates.onInstalledPackageDataReturn);
    },

    installPackage: function(pkg)
    {
        this.ajax('~/core/Package/InstallPackage', {name: pkg.Name, version: pkg.Version}, this._delegates.onInstalledPackageDataReturn);
    },

    bind: function()
    {
        videre.dataTables.clear(this.getControl('ItemTable'));
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._availablePackages, { installed: this._installedPackageDict }));

        this.getControl('ItemList').find('[data-action]').click(this._delegates.onActionClicked);
        videre.dataTables.bind(this.getControl('ItemTable'), [{ bSortable: false, sWidth: '62px' }]);
    },

    reset: function()
    {
        this.clearMsgs();
        //this.clearMsgs(this._dialog);
    },

    showCreatePackage: function()
    {
        this._dialog.modal('show');

    },

    _handleAction: function(action, id)
    {
        var pkg = this._availablePackageDict[id];

        if (pkg != null)
        {
            if (action == 'install')
                this.installPackage(pkg);
        }
    },

    _onInstalledPackageDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this.set_availablePackages(result.Data.availablePackages);
            this.set_installedPackages(result.Data.installedPackages);
            this.bind();
        }
    },

    _onInstalledPackageReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this.set_availablePackages(result.Data.availablePackages);
            this.set_installedPackages(result.Data.installedPackages);
            this.refresh();
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
    },

    _onCreatePackageClicked: function(e)
    {
        this.showCreatePackage();
    }



});

