﻿videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.portal = videre.widgets.base.extend(
{
    get_portals: function() { return this._portals; },
    set_portals: function(v)
    {
        this._portals = v;
        this._portalDict = this._portals.toDictionary(function(d) { return d.Id; });
    },
    get_selectedPortalId: function() { return this._selectedPortalId; },
    set_selectedPortalId: function(v) { this._selectedPortalId = v; },
    get_installedThemes: function() { return this._installedThemes; },
    set_installedThemes: function(v) { this._installedThemes = v; },
    get_themeAPIUrl: function() { return this._themeAPIUrl; },
    set_themeAPIUrl: function(v) { this._themeAPIUrl = v; },
    get_attributeDefs: function() { return this._attributeDefs; },
    set_attributeDefs: function(v)
    {
        this._attributeDefs = v;
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._portals = null;
        this._selectedPortal = null;
        this._selectedPortalId = null;
        this._installedThemes = null;
        this._themeWidget = null;
        this._attributeDefs = {};
        this._createPortalDialog = null;
        this._themeAPIUrl = null;

        this._delegates = {
            onAttributeItemBind: videre.createDelegate(this, this._onAttributeItemBind),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._createPortalDialog = this.getControl('CreatePortalDialog').hide().modal('hide');

        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.getControl('btnNew').click(videre.createDelegate(this, this._onNewClicked));
        this.getControl('btnCreate').click(videre.createDelegate(this, this._onCreateClicked));
        this.getControl('ddlPortals').change(videre.createDelegate(this, this._onPortalChanged));
        this.getControl('btnRecycle').click(videre.createDelegate(this, this._onRecycleClicked));
        this._selectedPortal = this._portalDict[this._selectedPortalId];

        //todo: fix theme for multi-portals!
        this._themeWidget = videre.widgets.register(this._id, videre.widgets.admin.theme, { id: this._id, ns: this._ns, installedThemes: this._installedThemes, themeAPIUrl: this._themeAPIUrl });

        //this.refreshThemes();
        this.bind();

    },

    bind: function()
    {
        videre.UI.bindDropdown(this.getControl('ddlPortals'), this._portals, 'Id', 'Name', null, this._selectedPortalId);
        this._themeWidget.changeTheme(this._selectedPortal.ThemeName);

        this.bindData(this._selectedPortal);
        for(var key in this._attributeDefs)
            this.bindAttributes(key, this._attributeDefs[key]);

    },

    bindAttributes: function(groupName, defs)
    {
        var ctr = this.getControl('AttributeList-' + this._getSafeGroupName(groupName));
        ctr.html(this.getControl('AttributeListTemplate').render(defs.orderBy(function(d) { return d.Label; }), { groupName: groupName, roleDataDict: this._roleDataDict, attributes: this._selectedPortal.Attributes }));
        ctr.find('[title]').tooltip({ html: true });
        videre.UI.initializeControlTypes(ctr);;
    },

    save: function()
    {
        //todo: validation!
        var portal = this.persistData(this._selectedPortal, true, this.getControl('GeneralTab'));
        portal.ThemeName = this._themeWidget.get_selectedTheme() != null ? this._themeWidget.get_selectedTheme().Name : '';
        //this._themeWidget.persistData(portal);
        //portal.ThemeName = this._selectedTheme != null ? this._selectedTheme.Name : '';
        for(var key in this._attributeDefs)
            this.persistData(portal.Attributes, false, this.getControl('tab' + this._getSafeGroupName(key)));
        this.ajax('~/core/Portal/SavePortal', { portal: portal }, this._delegates.onSaveReturn);
    },

    recycle: function()
    {
        if (confirm('Are you sure you wish to recycle this application?'))
        {
            this.ajax('~/core/Portal/RecycleApp', null, function(result)
            {
                if (!result.HasError && result.Data)
                {
                    alert('Application Recycle command sent.  Press Ok to Refresh page');
                    window.location.href = window.location.href;
                }
            });
        }
    },

    newPortal: function()
    {
        this.bindData({}, this._createPortalDialog);
        this._createPortalDialog.modal('show');
    },

    createPortal: function()
    {
        if (this.validControls(this._createPortalDialog))
        {
            var user = this.persistData({}, true, this.getControl('AdminUser'));
            var portal = {
                Name: this._createPortalDialog.find('[data-column="PortalName"]').val()
            };
            var packages = [];
            this.getControl('Packages').find(':checked').each(function() { packages.push($(this).val()) });

            this.ajax('~/core/Portal/CreatePortal', { adminUser: user, portal: portal, packages: packages }, this._delegates.onSaveReturn, null, this._createPortalDialog);
        }
    },

    _getSafeGroupName: function(name)
    {
        return name.replace(new RegExp(' ', 'g'), '-').replace(new RegExp('\\.', 'g'), '-');
    },

    _onSaveReturn: function(result)
    {
        if(!result.HasError && result.Data)
        {
            this._selectedPortalId = result.Data.selectedId;
            this.set_portals(result.Data.portals);
            this._selectedPortal = this._portalDict[this._selectedPortalId];
            this.bind();
            this._createPortalDialog.modal('hide');
        }
    },

    _onSaveClicked: function(e)
    {
        this.save();
    },

    _onRecycleClicked: function(e)
    {
        this.recycle();
    },

    _onNewClicked: function(e)
    {
        this.newPortal();
    },

    _onCreateClicked: function(e)
    {
        this.createPortal();
    },

    _onPortalChanged: function(e)
    {
        this._selectedPortalId = this.getControl('ddlPortals').val();
        this._selectedPortal = this._portalDict[this._selectedPortalId];
        this.bind();
    },

    _onFileSubmit: function(id, fileName)
    {
        this.lock();
    },

    _onFileUploadReturn: function(id, fileName, result)
    {
        this.unlock();
        if(!result.HasError)
        {

        }
        this.addMsgs(result.Messages);
    },

    _onFileMessage: function(message)
    {
        this.addMsg('FileMessage', message, true, this.getControl('Dialog'));   //todo: test!
    }

});

