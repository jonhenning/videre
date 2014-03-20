videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.account');

videre.widgets.account.userprofile = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v) { this._data = v; },
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
        this._customElements = null;
        this._authProviders = [];
        this._userAuthProviders = [];
        this._hasCustomAttributes = false;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn),
            onDisassociateReturn: videre.createDelegate(this, this._onDisassociateReturn)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.getControl('AssociatedAuthCtr').find('[data-authprovider]').click(videre.createDelegate(this, this._onUnassocateProviderClicked));
        this._hasCustomAttributes = this.getControl('CustomElementsCtr').length > 0;
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

        var self = this;
        this.getControl('UnassociatedAuthCtr').toggle(this._authProviders.length != this._userAuthProviders.length).find('[data-authprovider]').each(function(idx, item)
        {
            //$(item).toggle(!self._userAuthProviders.contains($(item).data('authprovider')));
            $(item).toggle(!self._userAuthProviders.where(function(d) { return d.toLowerCase() == $(item).data('authprovider').toLowerCase(); }).length > 0);
        });
        this.getControl('AssociatedAuthCtr').toggle(this._userAuthProviders.length > 0).find('[data-authprovider]').each(function(idx, item)
        {
            //$(item).toggle(self._userAuthProviders.contains($(item).data('authprovider')));
            $(item).toggle(self._userAuthProviders.where(function(d) { return d.toLowerCase() == $(item).data('authprovider').toLowerCase(); }).length > 0);
        });
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
            this.ajax('~/core/Account/SaveUserProfile', { user: user }, this._delegates.onSaveReturn);
        }
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
        this.ajax('~/core/Account/DisassociateExternalLogin', { provider: provider }, this._delegates.onDisassociateReturn);
    },

    _onDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this.set_data(result.Data);
            this.bind();
        }
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
    },

    _onSaveReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this.refresh();
        }
    },

    _onSaveClicked: function(e)
    {
        this.save();
    },

    _onUnassocateProviderClicked: function(e)
    {
        this._disassociateProvider($(e.target).closest('[data-authprovider]').data('authprovider'));
    }


});

