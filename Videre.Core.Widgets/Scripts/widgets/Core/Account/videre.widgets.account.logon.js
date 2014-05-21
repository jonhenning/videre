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
        this._resetDialog = null;
        this._changePasswordDialog = null;
        this._loginResult = null;

        this._delegates = {
            onLoginResult: videre.createDelegate(this, this._onLoginResult),
            onSendResetResult: videre.createDelegate(this, this._onSendResetResult),
            onChangePasswordResult: videre.createDelegate(this, this._onChangePasswordResult),
            onForgeryTokenRefresh: videre.createDelegate(this, this._onForgeryTokenRefresh)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._widget.find('[data-standardprovider]').button().click(videre.createDelegate(this, this._onLoginClick));
        this._widget.find('[data-multipleproviders]').button().click(videre.createDelegate(this, this._onMultiLoginClick));
        this._resetDialog = this.getControl('ResetPasswordDialog').modal('hide');
        this._changePasswordDialog = this.getControl('ChangePasswordDialog').modal('hide');
        this.getControl('lnkForgotPassword').click(videre.createDelegate(this, this._onForgotPasswordClick));
        this.getControl('btnSendReset').click(videre.createDelegate(this, this._onSendResetClick));
        this.getControl('btnChangePassword').click(videre.createDelegate(this, this._onChangePasswordClick));
    },

    login: function(provider)
    {
        this.ajax('~/core/Account/LogIn', { userName: this.getControl('txtName').val(), password: this.getControl('txtPassword').val(), rememberMe: this.getControl('chkRememberMe').is(':checked'), provider: provider }, this._delegates.onLoginResult);
    },

    sendReset: function()
    {
        this.ajax('~/core/Account/SendResetTicket', { userName: this.getControl('txtResetUserName').val(), url: location.href }, this._delegates.onSendResetResult, null, this._resetDialog);
    },

    showResetDialog: function()
    {
        this._resetDialog.modal('show');
        this.getControl('txtResetUserName').val('').focus();
    },

    refreshForgeryToken: function()
    {
        this.ajax('~/core/Account/GetAntiForgeryToken', { }, this._delegates.onForgeryTokenRefresh, null, this._resetDialog);
    },

    showChangePasswordDialog: function()
    {
        this._changePasswordDialog.modal('show');
        this.getControl('txtChangePassword1').val('').focus();
        this.getControl('txtChangePassword2').val('');
    },

    changePassword: function()
    {
        if (this.validControls(this._changePasswordDialog, this._changePasswordDialog))
            this.ajax('~/core/Account/ChangePassword', { userId: this._loginResult.UserId, password: this.getControl('txtChangePassword1').val() }, this._delegates.onChangePasswordResult, null, this._changePasswordDialog);
    },

    _handleLoginSuccess: function()
    {
        location.href = this._loginResult.RedirectUrl != null ? this._loginResult.RedirectUrl : this._returnUrl;
    },

    _onWidgetKeyDown: function(e)
    {
        if (e.keyCode == 13)
        {
            if (this._resetDialog.data('bs.modal').isShown)
                this.sendReset();
            else if (this._changePasswordDialog.data('bs.modal').isShown)
                this.changePassword();
            else
            {
                var provider = this._widget.find('[data-standardprovider]').data('standardprovider');
                provider = !String.isNullOrEmpty(provider) ? provider : this.getControl('ddlAuthenticationAgainst').val();
                this.login(provider);
            }
        }
    },

    _onLoginClick: function(e)
    {
        var provider = $(e.target).closest('[data-standardprovider]').data('standardprovider');
        this.login(provider);
    },

    _onMultiLoginClick: function(e)
    {
        var provider = this.getControl('ddlAuthenticationAgainst').val();
        this.login(provider);
    },

    _onSendResetClick: function(e)
    {
        this.sendReset();
    },

    _onForgotPasswordClick: function(e)
    {
        this.showResetDialog();
    },

    _onChangePasswordClick: function(e)
    {
        this.changePassword();
    },

    _onLoginResult: function(result)
    {
        if (!result.HasError)
        {
            this._loginResult = result.Data;
            if (this._loginResult.MustChangePassword)
            {
                this.refreshForgeryToken();
                this.showChangePasswordDialog();
            }
            else if (this._loginResult.UserId != null)
                this._handleLoginSuccess();
        }
    },

    _onSendResetResult: function(result)
    {
        if (!result.HasError)
        {
            this._resetDialog.modal('hide');
            this.addMsg('ResetCode', 'Reset code sent.'); //todo: localize?
        }
    },

    _onForgeryTokenRefresh: function(result)
    {
        videre._csrfToken = result.Data.token;
    },

    _onChangePasswordResult: function(result)
    {
        if (!result.HasError)
        {
            this._changePasswordDialog.modal('hide');
            this._handleLoginSuccess();
        }
    }


});

