videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.account');

videre.widgets.account.userprofile = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v) { this._data = v; },
    get_newUser: function() { return this._newUser; },
    set_newUser: function(v) { this._newUser = v; },
    get_loginUrl: function() { return this._loginUrl; },
    set_loginUrl: function(v) { this._loginUrl = v; },
    get_customElements: function() { return this._data; },
    set_customElements: function(v) { this._customElements = v; },
    get_authProviders: function() { return this._authProviders; },
    set_authProviders: function(v) { this._authProviders = v; },
    get_userAuthProviders: function() { return this._userAuthProviders; },
    set_userAuthProviders: function(v) { this._userAuthProviders = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._newUser = false;
        this._customElements = null;
        this._authProviders = [];
        this._userAuthProviders = [];
        this._hasCustomAttributes = false;
        this._loginUrl = '';

        this._externalLoginDialog = null;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn),
            onAssociateReturn: videre.createDelegate(this, this._onAssociateReturn),
            onDisassociateReturn: videre.createDelegate(this, this._onDisassociateReturn)
    };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.getControl('AssociatedAuthCtr').toggle(!this._newUser).find('[data-authprovider]').click(videre.createDelegate(this, this._onUnassocateProviderClicked));
        this.getControl('UnassociatedAuthCtr').toggle(!this._newUser).find('[data-authprovidertype="external"]').click(videre.createDelegate(this, this._onAssocateProviderClicked));
        this._hasCustomAttributes = this.getControl('CustomElementsCtr').length > 0;

        this._externalLoginDialog = this.getControl('ExternalLoginDialog').modal('hide');
        this.getControl('btnExternalLogin').click(videre.createDelegate(this, this._onExternalLoginClicked));

        this.bind();

    },

    refresh: function()
    {
        this.ajax('~/core/Account/GetUserProfile', {}, this._delegates.onDataReturn);
    },

    bind: function()
    {
        this.bindData(this._data, this.getControl('StandardElementsCtr'));
        if (this._hasCustomAttributes)
        {
            this._assignDefaultValues();
            this.bindData(this._data.Attributes, this.getControl('CustomElementsCtr'));
        }

        if (!this._newUser)
        {
            var externalAuthProviderNames = this._authProviders.select(function(d) { return d.Name.toLowerCase(); });
            var userAuthProviderNames = this._userAuthProviders.select(function(d) { return d.toLowerCase(); });
            this.getControl('UnassociatedAuthCtr').toggle(externalAuthProviderNames.where(function(d) { return userAuthProviderNames.contains(d) == false }).length > 0).find('[data-authprovider]').each(function(idx, item)
            {
                $(item).toggle(!userAuthProviderNames.contains($(item).data('authprovider').toLowerCase()));
            });
            this.getControl('AssociatedAuthCtr').toggle(externalAuthProviderNames.where(function(d) { return userAuthProviderNames.contains(d) == false }).length != externalAuthProviderNames.length).find('[data-authprovider]').each(function(idx, item)
            {
                $(item).toggle(userAuthProviderNames.contains($(item).data('authprovider').toLowerCase()));
            });
        }
    },

    save: function()
    {
        if (this.validControls())
        {
            var user = this.persistData(this._data, true, this.getControl('StandardElementsCtr'));
            if (this._hasCustomAttributes)
            {
                if (user.Attributes == null)
                    user.Attributes = {};
                this.persistData(user.Attributes, false, this.getControl('CustomElementsCtr'));
            }
            if (this._newUser)
                this.ajax('~/core/Account/CreateAccount', { user: user }, this._delegates.onSaveReturn);
            else
                this.ajax('~/core/Account/SaveUserProfile', { user: user }, this._delegates.onSaveReturn);
        }
    },


    _externalLogin: function()
    {
        var provider = this._externalLoginDialog.find('[data-column="AuthenticationProvider"]').text();
        var user = this._externalLoginDialog.find('[data-column="ExternalUser"]').val();
        var password = this._externalLoginDialog.find('[data-column="ExternalPassword"]').val();
        this.ajax('~/core/Account/AssociateExternalLogin', { provider: provider, userName: user, password: password }, this._delegates.onAssociateReturn, null, this._externalLoginDialog);
    },

    _assignDefaultValues: function()
    {
        if (this._data.Attributes == null)
            this._data.Attributes = {};
        if (this._customElements != null)
        {
            for (var i = 0; i < this._customElements.length; i++)
            {
                if (this._data.Attributes[this._customElements[i].Name] == null)
                    this._data.Attributes[this._customElements[i].Name] = this._customElements[i].DefaultValue;
            }
        }
    },

    _disassociateProvider: function(provider)
    {
        this.ajax('~/core/Account/DisassociateOAuthLogin', { provider: provider }, this._delegates.onDisassociateReturn);
    },

    _associateProvider: function(provider)
    {
        this._externalLoginDialog.find('[data-column="AuthenticationProvider"]').text(provider);
        this._externalLoginDialog.find('[data-column="ExternalUser"]').val('');
        this._externalLoginDialog.find('[data-column="ExternalPassword"]').val('');
        this._externalLoginDialog.modal('show');
    },

    _onDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this.set_data(result.Data);
            this.bind();
        }
        this.addMsgs(result.Messages);
    },

    _onDisassociateReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this.set_data(result.Data.profile);
            this._userAuthProviders = result.Data.userAuthProviders;
            this.bind();
            //this.refresh();
        }
        this.addMsgs(result.Messages);
    },

    _onAssociateReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            if (result.Data.associated)
            {
                this.set_data(result.Data.profile);
                this._userAuthProviders = result.Data.userAuthProviders;
                this.bind();
                this._externalLoginDialog.modal('hide');
            }
        }
        this.addMsgs(result.Messages, this._externalLoginDialog);
    },

    _onSaveReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            if (this._newUser)
                location.href = this._loginUrl;
            else
                this.refresh();
        }
        this.addMsgs(result.Messages);
    },

    _onSaveClicked: function(e)
    {
        this.save();
    },

    _onUnassocateProviderClicked: function(e)
    {
        this._disassociateProvider($(e.target).closest('[data-authprovider]').data('authprovider'));
    },

    _onAssocateProviderClicked: function(e)
    {
        this._associateProvider($(e.target).closest('[data-authprovider]').data('authprovider'));
    },

    _onExternalLoginClicked: function(e)
    {
        this._externalLogin();
    }


});

