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

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn)
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.bind();

    },

    refresh: function()
    {
        this.ajax('~/core/Account/GetUserProfile', {}, this._delegates.onDataReturn);
    },

    bind: function()
    {
        this.bindData(this._data);
    },

    save: function()
    {
        //todo: validation!
        var user = this.persistData(this._data);    //todo: clone?
        this.ajax('~/core/Account/SaveUserProfile', { User: user }, this._delegates.onSaveReturn);
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

