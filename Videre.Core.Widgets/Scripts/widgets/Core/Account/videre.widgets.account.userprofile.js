videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.account');

videre.widgets.account.userprofile = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v)
    {
        this._data = v;
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
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
    }

});

