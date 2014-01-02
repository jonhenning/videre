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
        this.getControl('btnLogon').button().click(videre.createDelegate(this, this._onLogonClick));

    },

    login: function()
    {
        this.clearMsgs();
        this.lock();
        videre.ajax('~/core/Account/LogIn', { UserName: this.getControl('txtName').val(), Password: this.getControl('txtPassword').val(), RememberMe: true }, this._delegates.onLogonResult, this._baseDelegates.onAjaxFail);
    },

    _onWidgetKeyDown: function(e)
    {
        if (e.keyCode == 13)
            this.login();
    },

    _onLogonClick: function(e)
    {
        this.login();
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

