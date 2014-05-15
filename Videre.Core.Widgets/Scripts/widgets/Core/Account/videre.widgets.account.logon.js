videre.registerNamespace('videre.widgets.account');

videre.widgets.account.logon = videre.widgets.base.extend(
{
    get_returnUrl: function() { return this._returnUrl; },
    set_returnUrl: function(v) { this._returnUrl = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._returnUrl = '';

        this._delegates = {
            onLogonResult: videre.createDelegate(this, this._onLogonResult)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._widget.find('[data-standardprovider]').button().click(videre.createDelegate(this, this._onLogonClick));
        this._widget.find('[data-multipleproviders]').button().click(videre.createDelegate(this, this._onMultiLogonClick));
        

    },

    login: function(provider)
    {
        this.clearMsgs();
        this.lock();
        videre.ajax('~/core/Account/LogIn', { userName: this.getControl('txtName').val(), password: this.getControl('txtPassword').val(), rememberMe: true, provider: provider }, this._delegates.onLogonResult, this._baseDelegates.onAjaxFail);
    },

    _onWidgetKeyDown: function(e)
    {
        if (e.keyCode == 13)
        {
            var provider = this._widget.find('[data-standardprovider]').data('standardprovider');
            provider = !String.isNullOrEmpty(provider) ? provider : this.getControl('ddlAuthenticationAgainst').val();
            this.login(provider);
        }
    },

    _onLogonClick: function(e)
    {
        var provider = $(e.target).closest('[data-standardprovider]').data('standardprovider');
        this.login(provider);
    },

    _onMultiLogonClick: function(e)
    {
        var provider = this.getControl('ddlAuthenticationAgainst').val();
        this.login(provider);
    },

    _onLogonResult: function(result)
    {
        this.unlock();
        if (!result.HasError)
        {
            var url = result.redirectUrl != null ? result.redirectUrl : this._returnUrl;
            location.href = url;
        }
        this.addMsgs(result.Messages);
    }


});

