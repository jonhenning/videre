videre.registerNamespace('videre.widgets.account');

videre.widgets.account.verify = videre.widgets.base.extend(
{
    get_returnUrl: function() { return this._returnUrl; },
    set_returnUrl: function(v) { this._returnUrl = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._returnUrl = '';

        this._delegates = {
            onVerifyResult: videre.createDelegate(this, this._onVerifyResult),
            onResendResult: videre.createDelegate(this, this._onResendResult)
    };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnVerify').click(videre.createDelegate(this, this._onVerifyClick));
        this.getControl('btnResendCode').click(videre.createDelegate(this, this._onResendCodeClick));
        this.getControl('txtCode').focus();
    },

    verify: function()
    {
        if (this.validControls())
            this.ajax('~/core/Account/VerifyAccount', { code: this.getControl('txtCode').val() }, this._delegates.onVerifyResult);
    },

    resendCode: function()
    {
        this.ajax('~/core/Account/SendVerificationCode', {}, this._delegates.onResendResult);
    },

    _onWidgetKeyDown: function(e)
    {
        if (e.keyCode == 13)
            this.verify();
    },

    _onVerifyClick: function(e)
    {
        this.verify();
    },

    _onResendCodeClick: function(e)
    {
        this.resendCode();
    },

    _onVerifyResult: function(result)
    {
        if (!result.HasError && result.Data)
        {
            if (!String.isNullOrEmpty(this._returnUrl))
                location.href = this._returnUrl;
            else 
                location.href = videre.resolveUrl('~/');
        }
    },

    _onResendResult: function(result)
    {
        //if (!result.HasError)
        //    this.addMsg('ResendCodeSent', 'Reset code sent.'); //todo: localize?
    }

});

