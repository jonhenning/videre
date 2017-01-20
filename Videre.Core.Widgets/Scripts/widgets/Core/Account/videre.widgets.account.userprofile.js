videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.account');

videre.widgets.account.userprofile = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v) { this._data = v; },
    get_newUser: function() { return this._newUser; },
    set_newUser: function(v) { this._newUser = v; },
    get_afterCreateUrl: function() { return this._afterCreateUrl; },
    set_afterCreateUrl: function(v) { this._afterCreateUrl = v; },
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
        this._userAuthProviders = {};
        this._hasCustomAttributes = false;
        this._afterCreateUrl = '';

        this._externalLoginDialog = null;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn),
            onAssociateReturn: videre.createDelegate(this, this._onAssociateReturn),
            onDisassociateReturn: videre.createDelegate(this, this._onDisassociateReturn)
        };

        //from console window at https://momentjs.com/ run this to generate...  hacky, but effective ;)
        //var s = ''; $.each($('.locales div'), function(idx, ctl) { s += '"' + $(ctl).attr('data-locale') + '": "' + $(ctl).text().trim().replace(/[\n\r]+/g, '') + '",'; }); console.log('{' + s + '}');
        this._localeNameDict = { "af": "Afrikaans", "sq": "Albanian", "ar": "Arabic", "ar-dz": "Arabic (Algeria)", "ar-ly": "Arabic (Lybia)", "ar-ma": "Arabic (Morocco)", "ar-sa": "Arabic (Saudi Arabia)", "ar-tn": "Arabic (Tunisia)", "hy-am": "Armenian", "az": "Azerbaijani", "eu": "Basque", "be": "Belarusian", "bn": "Bengali", "bs": "Bosnian", "br": "Breton", "bg": "Bulgarian", "my": "Burmese", "km": "Cambodian", "ca": "Catalan", "tzm": "Central Atlas Tamazight", "tzm-latn": "Central Atlas Tamazight Latin", "zh-cn": "Chinese (China)", "zh-hk": "Chinese (Hong Kong)", "zh-tw": "Chinese (Taiwan)", "cv": "Chuvash", "hr": "Croatian", "cs": "Czech", "da": "Danish", "nl": "Dutch", "nl-be": "Dutch (Belgium)", "en-au": "English (Australia)", "en-ca": "English (Canada)", "en-ie": "English (Ireland)", "en-nz": "English (New Zealand)", "en-gb": "English (United Kingdom)", "en": "English (United States)", "eo": "Esperanto", "et": "Estonian", "fo": "Faroese", "fi": "Finnish", "fr": "French", "fr-ca": "French (Canada)", "fr-ch": "French (Switzerland)", "fy": "Frisian", "gl": "Galician", "ka": "Georgian", "de": "German", "de-at": "German (Austria)", "el": "Greek", "he": "Hebrew", "hi": "Hindi", "hu": "Hungarian", "is": "Icelandic", "id": "Indonesian", "it": "Italian", "ja": "Japanese", "jv": "Javanese", "kk": "Kazakh", "tlh": "Klingon", "ko": "Korean", "ky": "Kyrgyz", "lo": "Lao", "lv": "Latvian", "lt": "Lithuanian", "lb": "Luxembourgish", "mk": "Macedonian", "ms-my": "Malay", "ms": "Malay", "ml": "Malayalam", "dv": "Maldivian", "mi": "Maori", "mr": "Marathi", "me": "Montenegrin", "ne": "Nepalese", "se": "Northern Sami", "nb": "Norwegian Bokmål", "nn": "Nynorsk", "fa": "Persian", "pl": "Polish", "pt": "Portuguese", "pt-br": "Portuguese (Brazil)", "x-pseudo": "Pseudo", "pa-in": "Punjabi (India)", "ro": "Romanian", "ru": "Russian", "gd": "Scottish Gaelic", "sr": "Serbian", "sr-cyrl": "Serbian Cyrillic", "si": "Sinhalese", "sk": "Slovak", "sl": "Slovenian", "es": "Spanish", "es-do": "Spanish (Dominican Republic)", "sw": "Swahili", "sv": "Swedish", "tl-ph": "Tagalog (Philippines)", "tzl": "Talossan", "ta": "Tamil", "te": "Telugu", "tet": "Tetun Dili (East Timor)", "th": "Thai", "bo": "Tibetan", "tr": "Turkish", "uk": "Ukrainian", "uz": "Uzbek", "vi": "Vietnamese", "cy": "Welsh", "yo": "Yoruba Nigeria", "ss": "siSwati" };

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

        if (typeof moment != 'undefined' && moment.locales != null)  //if moment defined and supports locales method (v2.12+) swap out text box for dropdown
        {
            var ddl = $('<select data-column="Locale" class="form-control"></select>');
            var locales = moment.locales();
            for (var i = 0; i < locales.length; i++)
            {
                var name = locales[i];
                if (this._localeNameDict[locales[i]] != null)
                    name = this._localeNameDict[locales[i]] + ' [' + locales[i] + ']';
                ddl.append($('<option></option>').text(name).attr('value', locales[i]));
            }
            this.findControl('[data-column="Locale"]').replaceWith(ddl);
        }

        this.bind();

    },

    refresh: function()
    {
        this.ajax('~/core/Account/GetUserProfile', {}, this._delegates.onDataReturn);
    },

    bind: function()
    {
        var self = this;
        if (this._data.Locale != null)
            this._data.Locale = this._data.Locale.toLowerCase();

        this.bindData(this._data, this.getControl('StandardElementsCtr'));
        if (this._hasCustomAttributes)
        {
            this._assignDefaultValues();
            this.bindData(this._data.Attributes, this.getControl('CustomElementsCtr'));
        }

        if (!this._newUser)
        {
            var externalAuthProviderNames = this._authProviders.select(function(d) { return d.Name.toLowerCase(); });
            this.getControl('UnassociatedAuthCtr').toggle(externalAuthProviderNames.where(function(d) { return self._userAuthProviders[d] == null }).length > 0).find('[data-authprovider]').each(function(idx, item)
            {
                $(item).toggle(self._userAuthProviders[$(item).data('authprovider').toLowerCase()] == null);
            });
            this.getControl('AssociatedAuthCtr').toggle(externalAuthProviderNames.where(function(d) { return self._userAuthProviders[d] == null }).length != externalAuthProviderNames.length).find('[data-authprovider]').each(function(idx, item)
            {
                var userProvider = self._userAuthProviders[$(item).data('authprovider').toLowerCase()];
                $(item).toggle(userProvider != null).attr('title', userProvider != null ? userProvider : null);
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

    _onWidgetKeyDown: function(e)
    {
        if (e.keyCode == 13)
        {
            if (this._externalLoginDialog.data('bs.modal').isShown)
                this._externalLogin();
        }
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
                location.href = this._afterCreateUrl;
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

