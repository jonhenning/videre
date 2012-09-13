videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.editor');

videre.widgets.editor.texthtml = videre.widgets.editor.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method
        this._editor = null;
        this._sharedContent = null;
        this._sharedContentDict = null;
        this._linkCountDict = null;
        //this._linked = false;
        //this._shared = false;

        this._delegates = {
            onDataReceived: videre.createDelegate(this, this._onDataReceived)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._editor = videre.widgets.find(this.getId('txtHtml'));
        //this.getControl('btnShare').click(videre.createDelegate(this, this._onShareClicked));
        //this.getControl('btnLink').click(videre.createDelegate(this, this._onLinkClicked));
        this.getControl('ddlLink').change(videre.createDelegate(this, this._onLinkChanged));
    },

    show: function(widget, manifest)
    {
        this._base(widget, manifest);
        if (widget.Content == null)
            widget.Content = [];
        if (widget.Content.length == 0)
            widget.Content[0] = this.newContent();

        this._widget.find('.nav-tabs a:first').tab('show');
        if (this._sharedContent == null)
            this.getSharedContent();
        else
            this.bind();
    },

    newContent: function()
    {
        return { Id: '', Key: 'Content.Text', Namespace: '', Text: '', Locale: '', EffectiveDate: null, ExpirationDate: null };  //Namespace: widget.ResourcePath
    },

    bind: function()
    {
        //this._shared = !String.isNullOrEmpty(this._widgetData.Content[0].Namespace);
        //this._linked = this._linkCountDict[this._widgetData.Content[0].Id] != null && this._linkCountDict[this._widgetData.Content[0].Id].Count > 1;

        //this.getControl('ShareGroup').toggle(!this._linked);

        //this.getControl('btnShare').removeClass('active').addClass(this._shared ? 'active' : '');
        //this.getControl('btnLink').removeClass('active').addClass(this._linked ? 'active' : '');

        //this.getControl('LinkGroup').toggle(!this._linked);
        var content = this._widgetData.Content[0];
        this.getControl('ddlLink').val(this._sharedContentDict[content.Id] != null ? this._widgetData.Content[0].Id : '');
        this.getControl('lblLinkCount').html(String.format("({0})", this._linkCountDict[content.Id] != null ? this._linkCountDict[content.Id].length : '0'));

        this.bindData(content, this.getControl('GeneralTab'));
        this.bindData(content, this.getControl('ContentProperties'));
        this._editor.bind(content);
    },

    bindLinks: function()
    {
        var ddl = this.getControl('ddlLink');
        ddl[0].options.length = 0;
        ddl.append($('<option></option>').val('').html(''));
        $.each(this._sharedContent, function(val, text)
        {
            ddl.append($('<option></option>').val(this.Id).html(this.Namespace));
        });
    },

    validate: function()
    {
        //add custom validation here
        return this._base();
    },

    save: function()
    {
        //persist before calling base
        this.persistData(this._widgetData.Content[0], false, this.getControl('GeneralTab'));
        this.persistData(this._widgetData.Content[0], false, this.getControl('ContentProperties'));
        this._editor.persist();
        this._base();
    },

    getSharedContent: function()
    {
        this.ajax('~/core/localization/GetSharedWidgetContent', {}, this._delegates.onDataReceived);
    },

    showImport: function()
    {
        this._shareDialog.modal('show');
    },

    _handleShareChanged: function(id)
    {
        this._removeWidgetLink();   //clear any links from this widget
        if (this._sharedContentDict[id] != null)
        {
            this.getControl('txtShareName').attr('readonly', 'readonly');
            this._widgetData.Content[0] = this._sharedContentDict[id];
            this._addWidgetLink(id);
        }
        else
        {
            this.getControl('txtShareName').attr('readonly', null);
            this._widgetData.Content[0] = this.newContent();
        }
        this.bind();
    },

    _addWidgetLink: function(contentId)
    {
        if (!String.isNullOrEmpty(contentId))
        {
            if (this._linkCountDict[contentId] == null)
                this._linkCountDict[contentId] = [];
            this._linkCountDict[contentId].push(this._widgetData.Id);  //add this widget to current count
        }

    },

    _removeWidgetLink: function()
    {
        for (var contentId in this._linkCountDict)
        {
            for (var i = 0; i < this._linkCountDict[contentId].length; i++)
            {
                if (this._linkCountDict[contentId][i] == this._widgetData.Id)
                {
                    this._linkCountDict[contentId].remove(i);
                    return; //get out
                }
            }
        }
    },

    _onDataReceived: function(result)
    {
        this._sharedContent = result.Data.localizations;
        this._linkCountDict = result.Data.idCounts; //already a dictionary (id, count)
        this._sharedContentDict = this._sharedContent.toDictionary(function(d) { return d.Id; });
        //this.showImport();
        this.bindLinks();
        this.bind();
    },

    _onShareClicked: function(e)
    {
        this.getSharedContent();
    },

    _onLinkClicked: function(e)
    {
        this.getSharedContent();
    },

    _onLinkChanged: function(e)
    {
        this._handleShareChanged(this.getControl('ddlLink').val());
    }


});

