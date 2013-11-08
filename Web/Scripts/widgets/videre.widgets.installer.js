videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.install');

videre.widgets.installer = videre.widgets.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method
        this._delegates = {
            onInstallReturn: videre.createDelegate(this, this._onInstallReturn)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnInstall').click(videre.createDelegate(this, this._onInstallClicked));
        videre.UI.handleEnter(this._widget, videre.createDelegate(this, this._onInstallClicked));

        //this._widget.find('input,select,textarea').jqBootstrapValidation();
        this.getControl('txtUserName').focus();
    },


    install: function(e)
    {
        if (this.validControls(this._widget))
        {
            var user = this.persistData({}, true, this.getControl('AdminUser'));
            var portal = {
                Name: 'Default',
                Default: true
            };
            var packages = [];
            this.getControl('Packages').find(':checked').each(function () { packages.push($(this).val()) });
            this.getControl('CorePackages').find(':checked').each(function () { packages.push($(this).val()) });
            this.getControl('WebReferencePackages').find(':checked').each(function() { packages.push($(this).val()) });

            this.ajax('~/Installer/Install', { adminUser: user, portal: portal, packages: packages }, this._delegates.onInstallReturn);
        }
    },

    _onInstallClicked: function(e)
    {
        this.install();
    },

    _onInstallReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this.addMsg('wait', 'Please Wait...', false);
            setTimeout(function()
            {
                window.location.href = window.ROOT_URL;
            }, 3000);
        }
    }




});

