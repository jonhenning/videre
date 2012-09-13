videre.registerNamespace('videre.widgets');

videre.widgets.blog = videre.widgets.base.extend(
{
    get_blogUrl: function() { return this._blogUrl; },
    set_blogUrl: function(v) { this._blogUrl = v; },
    get_blogId: function() { return this._blogId; },
    set_blogId: function(v) { this._blogId = v; },
    get_entryId: function() { return this._entryId; },
    set_entryId: function(v) { this._entryId = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._blogUrl = '';
        this._blogId = null;
        this._entryId = null;
        this._dialog = null;
        this._blogEntry = null;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn),
            onDeleteReturn: videre.createDelegate(this, this._onDeleteReturn)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._dialog = this.getControl('Dialog').modal('hide');
        if (this.getControl('txtPostDate').length > 0)  //date time picker plugin only registered if in admin mode
            this.getControl('txtPostDate').datetimepicker({
                dateFormat: videre.localization.dateFormats.date,
                timeFormat: 'h:mm TT', //videre.localization.dateFormats.time,
                ampm: true
            });
        this.getControl('AdminMenu').find('a').click(videre.createDelegate(this, this._onAdminMenuAction));
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
    },

    showEntryEdit: function()
    {
        this.bindData(this._blogEntry, this._dialog);
        this._dialog.modal('show');

    },

    newEntry: function()
    {
        this._blogEntry = { Id: '', Url: '', Title: '', Summary: '', Body: '', PostDate: null, Tags: [] };
        this.showEntryEdit();
    },

    saveEntry: function()
    {
        var entry = this.persistData(this._blogEntry, this._dialog);
        this.ajax('~/blogapi/admin/SaveEntry', { blogId: this._blogId, entry: entry }, this._delegates.onSaveReturn, null, this._dialog);
    },

    deleteEntry: function()
    {
        if (confirm('Are you sure you wish to remove this entry?'))    //todo: localize
            this.ajax('~/blogapi/admin/DeleteEntry', { blogId: this._blogId, entryId: this._entryId }, this._delegates.onDeleteReturn, null);
    },

    getEntry: function()
    {
        this.ajax('~/blogapi/admin/GetEntry', { blogId: this._blogId, entryId: this._entryId }, this._delegates.onDataReturn, null, this._dialog);
    },

    reload: function()
    {
        window.location.href = videre.resolveUrl(this._blogUrl);
    },

    _handleAdminAction: function(action)
    {
        if (action == 'new')
            this.newEntry();
        if (action == 'edit')
            this.getEntry();
        if (action == 'delete')
            this.deleteEntry();
    },

    _onDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._blogEntry = result.Data;
            this.showEntryEdit();
        }
    },

    _onSaveReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this._dialog.modal('hide');
            this.reload();
        }
    },

    _onDeleteReturn: function(result)
    {
        if (!result.HasError && result.Data)
            this.reload();
    },

    _onAdminMenuAction: function(e)
    {
        var ctl = $($(e.target).parents('li'));
        this._handleAdminAction(ctl.data('action'));
    },

    _onSaveClicked: function(e)
    {
        this.saveEntry();
    }


});

