videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.editor');

videre.widgets.editor.blog = videre.widgets.editor.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method

        this._blogData = null;
        this._blogDataDict = null;
        this._newBlogData = null;
        this._newBlogDialog = null;

        this._delegates = {
            onBlogDataReceived: videre.createDelegate(this, this._onBlogDataReceived)
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base

        this._newBlogDialog = this.getControl('NewBlogDialog').modal('hide');
        this.getControl('btnNewBlog').click(videre.createDelegate(this, this._onNewBlogClicked));
        this.getControl('btnDeleteBlog').click(videre.createDelegate(this, this._onDeleteBlogClicked));
        this.getControl('btnOkNewBlog').click(videre.createDelegate(this, this._onNewBlogOkClicked));
        this.getControl('ddlName').change(videre.createDelegate(this, this._onBlogChanged));
    },

    show: function(widget, manifest)
    {
        this._base(widget, manifest);
        this.reset();

        this._newBlogData = widget.Content != null && widget.Content.Id == '-1' ? widget.Content : null; //store reference to original blog data (really only for new blog that was appended to list)
        this.refreshBlogs();
    },

    bind: function()
    {
        this.bindBlogs();
        this.bindData(this._widgetData.Content, this.getControl('GeneralTab'));
    },

    bindBlogs: function()
    {
        var ddl = this.getControl('ddlName');
        var origValue = ddl.val();
        videre.UI.bindDropdown(ddl, this._blogData, 'Id', 'Name', '');

        if (this._widgetData.Content != null)// && !String.isNullOrEmpty(this._widgetData.Content.Name))   //if name is not blank, we lost our blog due to a delete
        {
            if (this._newBlogData != null && this._blogDataDict[this._newBlogData.Id] == null)    //if blog doesn't exist in data, its a new one, use data from widget to populate
                ddl.append($('<option></option>').val(this._newBlogData.Id).html(this._newBlogData.Name));  //Id will be empty ''
            ddl.val(this._widgetData.Content.Id);
        }

        if (ddl.val() != origValue)
            this._handleBlogChanged();
    },

    showNewBlogDialog: function()
    {
        this._widget.find('[data-column="Name"]').val('');
        this._widget.find('[data-column="Description"]').val('');
        this._newBlogDialog.modal('show');
    },

    refreshBlogs: function()
    {
        this.ajax('~/blogapi/admin/get', {}, this._delegates.onBlogDataReceived);
    },

    deleteBlog: function(id)
    {
        this.ajax('~/blogapi/admin/delete', { id: id }, this._delegates.onBlogDataReceived);
    },

    validate: function ()
    {
        //add custom validation here
        return this._base() && this.validControls(this.getControl('GeneralTab'), this._widget);
    },

    save: function()
    {
        if (this._widgetData.Content != null)   //if blog is set...
        {
            this.persistData(this._widgetData.Content, false, this.getControl('GeneralTab'));
            this._widgetData.Content.Name = $('option:selected', this.getControl('ddlName')).text();   //todo:  have ability to persist text using attributes??
        }

        this.reset();
        this._base();
    },

    reset: function()
    {
        this._widget.find('.nav-tabs a:first').tab('show');
    },

    _getNewBlog: function(name, desc)
    {
        return { Id: '-1', Name: name, Description: desc, Entries: [] };
    },

    _handleBlogChanged: function()
    {
        var id = this.getControl('ddlName').val();
        var data = this._blogData.where(function(m) { return m.Id == id; }).singleOrDefault();
        if (data == null)   //if blog not found, it is a new entry
            data = this._newBlogData; 

        this._widgetData.Content = data;
        this.bindData(this._widgetData.Content, this.getControl('GeneralTab'));
    },

    _handleDeleteBlog: function(id)
    {
        if (!String.isNullOrEmpty(id))
            this.deleteBlog(id);
        else
            this._newBlogData = null;
    },

    _handleNewBlog: function()
    {
        var name = this._newBlogDialog.find('[data-column="Name"]').val();
        if (!String.isNullOrEmpty(name))    //todo: verify not exist in _blogData as well
        {
            this._newBlogData = this._getNewBlog(name, this._newBlogDialog.find('[data-column="Description"]').val());
            this._widgetData.Content = this._newBlogData;
            this.bindBlogs();
            this._handleBlogChanged();
            this._newBlogDialog.modal('hide');
        }
    },

    _onNewBlogClicked: function(e)
    {
        this.showNewBlogDialog();
    },

    _onNewBlogOkClicked: function(e)
    {
        this._handleNewBlog();
    },

    _onDeleteBlogClicked: function(e)
    {
        this._handleDeleteBlog(this.getControl('ddlName').val());
    },

    _onBlogDataReceived: function(result)
    {
        if (!result.HasError)
        {
            this._blogData = result.Data;
            this._blogDataDict = this._blogData.toDictionary(function(d) { return d.Id });
            this.bind();
        }
    },

    _onBlogChanged: function(e)
    {
        this._handleBlogChanged();
    }

});

