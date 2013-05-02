/// <reference path="../../../controls/core/CKTextEditor/ckeditor_3.6.4/ckeditor.js" />
videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.packageadmin = videre.widgets.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._dataDict = null;

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

        var uploader = new qq.FileUploaderBasic({
            button: this.getControl('btnImport')[0],
            params: {},
            action: videre.resolveUrl('~/core/package/importpackage'),
            onSubmit: videre.createDelegate(this, this._onFileSubmit),
            onComplete: videre.createDelegate(this, this._onFileUploadReturn),
            showMessage: videre.createDelegate(this, this._onFileMessage),
            debug: true
        });

        this._dialog = this.getControl('Dialog').modal('hide');
        this.getControl('btnCreatePackage').click(videre.createDelegate(this, this._onCreatePackageClicked));

        this.refresh();

    },

    refresh: function()
    {
        this.ajax('~/core/Package/GetPackages', {}, this._delegates.onInstalledPackageDataReturn);
    },

    installPackage: function(pkg)
    {
        this.ajax('~/core/Package/InstallPackage', { name: pkg.name, version: pkg.version }, this._delegates.onInstalledPackageReturn);
    },

    downloadPackage: function(pkg)
    {
        this.ajax('~/core/Package/DownloadPackage', { name: pkg.name, version: pkg.version }, this._delegates.onInstalledPackageReturn);
    },

    togglePublishPackage: function(pkg, publish)
    {
        this.ajax('~/core/Package/TogglePublishPackage', { name: pkg.name, version: pkg.version, publish: !pkg.published }, this._delegates.onInstalledPackageReturn);
    },

    removePackage: function(pkg)
    {
        var self = this;
        //todo: localize?
        videre.UI.prompt(this.getId('RemovePackage'), 'Remove Package', String.format('Do you wish to remove the package {0}?', pkg.name), null,
            [{
                text: 'Ok', css: 'btn-primary', close: true, handler: function()
                {
                    self.ajax('~/core/Package/RemovePackage', { name: pkg.name, version: pkg.version }, self._delegates.onInstalledPackageReturn);
                    return true;
                }
            }, { text: 'Cancel', close: true }]);       
    },

    uninstallPackage: function(pkg)
    {
        var self = this;
        //todo: localize?
        videre.UI.prompt(this.getId('UninstallPackage'), 'Uninstall Package', String.format('Do you wish to uninstall this package {0}?  Note:  Currently this just removes the db entry, not the files/content itself.', pkg.name), null,
            [{
                text: 'Ok', css: 'btn-primary', close: true, handler: function()
                {
                    self.ajax('~/core/Package/UninstallPackage', { name: pkg.name, version: pkg.version }, self._delegates.onInstalledPackageReturn);
                    return true;
                }
            }, { text: 'Cancel', close: true }]);
    },

    bind: function()
    {
        videre.dataTables.clear(this.getControl('ItemTable'));
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._data));

        this.getControl('ItemList').find('[data-action]').click(this._delegates.onActionClicked);
        videre.dataTables.bind(this.getControl('ItemTable'));
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
        var pkg = this._dataDict[id];

        if (pkg != null)
        {
            if (action == 'install')
                this.installPackage(pkg);
            else if (action == 'download')
                this.downloadPackage(pkg);
            else if (action == 'remove')
                this.removePackage(pkg);
            else if (action == 'uninstall')
                this.uninstallPackage(pkg);
            else if (action == 'publish')
                this.togglePublishPackage(pkg);
}
    },

    _handleData: function(d)
    {
        var data = {};

        var publishedDict = d.publishedPackages.toDictionary(function(d) { return d.Name + '~' + d.Version; });

        d.availablePackages.forEach(function(d)
        {
            var id = d.Name + '~' + d.Version;
            data[id] = { id: id, name: d.Name, version: d.Version, type: d.Type, desc: d.Description, source: d.Source, availPackageDate: d.PackagedDate, installedPackageDate: null, installedDate: null, remoteDate: null, published: publishedDict[id] != null && publishedDict[id].PackagedDate == d.PackagedDate };
        });

        d.installedPackages.forEach(function(d)
        {
            var id = d.Name + '~' + d.Version;
            if (data[id] != null)
            {
                data[id].installedDate = d.InstallDate;
                data[id].installedPackageDate = d.PackagedDate;
            }
            else
                data[id] = { id: id, name: d.Name, version: d.Version, type: d.Type, desc: d.Description, source: d.Source, availPackageDate: null, installedPackageDate: d.PackagedDate, installedDate: d.InstallDate, remoteDate: null, published: false };
        });

        d.remotePackages.forEach(function(d)
        {
            var id = d.Name + '~' + d.Version;
            if (data[id] != null)
            {
                data[id].remoteDate = d.PackagedDate;
            }
            else
                data[id] = { id: id, name: d.Name, version: d.Version, type: d.Type, desc: d.Description, source: d.Source, availPackageDate: null, installed: null, remoteDate: d.PackagedDate, published: false };
        });
        this._dataDict = data;
        this._data = [];
        for (var key in data)
        {
            if (data.hasOwnProperty(key))   //todo: unsure if this will filter out methods
                this._data.push(data[key]);
        }
        this._data = this._data.orderBy(function(d) { return d.name; });
        //this._data = data.values();

    },

    _onInstalledPackageDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._handleData(result.Data);
            this.bind();
        }
    },

    _onInstalledPackageReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            //this._handleData(result.Data);    //not updated yet...
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

