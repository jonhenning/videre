videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.oauth');

videre.widgets.oauth.userprofile = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v)
    {
        this._data = v;
    },

    get_OAuthProviders: function () { return this._oAuthProviders; },
    set_OAuthProviders: function (v) { this._oAuthProviders = v; },
    get_userAuthProviders: function () { return this._userAuthProviders; },
    set_userAuthProviders: function (v) { this._userAuthProviders = v; },
    
    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._oAuthProviders = [];
        this._userAuthProviders = [];
        this._hasCustomAttributes = false;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.getControl('AssociatedOAuthCtr').find('[data-oauthprovider]').click(videre.createDelegate(this, this._onUnassocateProviderClicked));
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
            this.bindData(this._data.Attributes, this.getControl('CustomElementsCtr'));
        var self = this;

        this.getControl('UnassociatedOAuthCtr').toggle(this._oAuthProviders.length != this._userAuthProviders.length).find('[data-oauthprovider]').each(function (idx, item)
        {
            //$(item).toggle(!self._data.OAuthProviders.contains($(item).data('oauthprovider')));
            $(item).toggle(!self._userAuthProviders.contains($(item).data('oauthprovider')));
        });
        this.getControl('AssociatedOAuthCtr').toggle(this._userAuthProviders.length > 0).find('[data-oauthprovider]').each(function (idx, item)
        {
            //$(item).toggle(self._data.OAuthProviders.contains($(item).data('oauthprovider')));
            $(item).toggle(self._userAuthProviders.contains($(item).data('oauthprovider')));
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

    _disassociateProvider: function(provider)
    {
        this.ajax('~/OAuth/Authentication/DisassociateExternalLogin', { provider: provider }, this._delegates.onSaveReturn);
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
        }
    },

    _onSaveClicked: function(e)
    {
        this.save();
    },

    _onUnassocateProviderClicked: function (e)
    {
        this._disassociateProvider($(e.target).closest('[data-oauthprovider]').data('oauthprovider'));
    }

});

