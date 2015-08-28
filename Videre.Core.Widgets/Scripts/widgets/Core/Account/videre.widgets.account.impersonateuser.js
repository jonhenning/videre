videre.registerNamespace('videre.widgets.account');

videre.widgets.account.impersonateuser = videre.widgets.base.extend(
{

    //constructor
    init: function()
    {
        this._base();  //call base method

        this._dialog = null;

        this._delegates = {
            onImpersonateResult: videre.createDelegate(this, this._onImpersonateResult)
        };
    },

    _onLoad: function(src, args)
    {
        var self = this;
        this._base(); //call base
        this.getControl('btnChange').click(function() { self.showDialog(); });
        this.getControl('btnImpersonate').click(function() { self.impersonate(self.findControl('[data-column="ImpersonateUser"]').val()); });
        videre.UI.handleEnter(this._widget, function() { self.impersonate(self.findControl('[data-column="ImpersonateUser"]').val()); });
        this.getControl('btnReset').click(function() { self.impersonate(null); });

        this._dialog = this.getControl('Dialog').modal('hide');
    },

    showDialog: function()
    {
        this._dialog.modal('show');
        this.findControl('[data-column="ImpersonateUser"]').focus();
    },

    impersonate: function(userName)
    {
        this.ajax('~/core/Account/Impersonate', { userName: userName }, this._delegates.onImpersonateResult);
    },

    _onImpersonateResult: function(result)
    {
        if (!result.HasError)
        {
            if (result.Data)
            {
                this._dialog.modal('hide');
                window.location.href = window.location.href;    //reload
            }
        }
    }

});

