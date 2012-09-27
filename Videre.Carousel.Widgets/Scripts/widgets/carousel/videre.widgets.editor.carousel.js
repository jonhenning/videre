videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.editor');

videre.widgets.editor.carousel = videre.widgets.editor.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method
        this._itemDict = null;
        this._selectedItem = null;
        this._carouselList = null;
        //this._fileBrowser = null;
        this._genId = 0;

        this._delegates = {
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._dialog = this.getControl('Dialog').modal('hide');

        this.getControl('btnApply').click(videre.createDelegate(this, this._onApplyClicked));
        this._carouselList = this.getControl('CarouselList').click(videre.createDelegate(this, this._onCarouselClick));
        this.getControl('ActionButtons').find('a').click(videre.createDelegate(this, this._onActionClick));
        this._widget.find('[data-column]').change(videre.createDelegate(this, this._onDataChange));

        //        this.getControl('btnBrowseImage').click(videre.createDelegate(this, this._onBrowseImageClick));
        //        this._fileBrowser = videre.widgets.findFirstByType('videre.widgets.admin.filebrowser');
        //        if (this._fileBrowser != null)
        //        {
        //            this._fileBrowser.add_onSelection(videre.createDelegate(this, this._onFileSelected));
        //        }

    },

    show: function (widget, manifest)
    {
        this._base(widget, manifest);
        this._widget.find('.cke_contents').css('height', '100px');  //todo: this is a hack as it relies on ck editor being used...  not really good idea
        this.reset();
        if (widget.Content == null || widget.Content.length == 0)
            widget.Content = { Name: '', Items: [] };

        this.bindData(this._widgetData.Content, this.getControl('GeneralTab'));
        this.bindCarousel();
    },

    bindCarousel: function()
    {
        this._carouselList.html('');
        this._itemDict = {};
        this._bindItems(this._carouselList, this._widgetData.Content.Items);
    },

    editItem: function()
    {
        this.getControl('EditCtr').show();
        this.bindData(this._selectedItem, this.getControl('EditCtr'));  //CarouselTab
        this._setDirty(false);
    },

    applyItem: function()
    {
        if (this._selectedItem != null && this.validControls(this.getControl('EditCtr'), this._widget))
        {
            this.persistData(this._selectedItem, false, this.getControl('EditCtr'));    //CarouselTab
            this._selectedItem = null;
            this.bindCarousel();
            this.getControl('EditCtr').hide();
            this._setDirty(false);
        }
    },

    addItem: function()
    {
        this._genId += 1;

        var item = { genId: this._genId, Text: '<h4>Image Description</h4><p>Cras justo odio, dapibus ac facilisis in, egestas eget quam. Donec id elit non mi porta gravida at eget metus. Nullam id dolor id nibh ultricies vehicula ut id elit.</p>', ImageUrl: '' };
        this._widgetData.Content.Items.push(item);
        this._selectedItem = item;
        this.bindCarousel();
        this.editItem();
    },

    removeItem: function()
    {
        if (this._selectedItem != null)
        {
            var items = this._widgetData.Content.Items; //todo:  mini-hack
            var itemPos = this._getItemPos(this._selectedItem, items);
            items.remove(itemPos);
            this.bindCarousel();
            this.getControl('EditCtr').hide();
        }
    },

    validate: function ()
    {
        return this._base() && this.validControls(this.getControl('GeneralTab'), this._widget);
    },

    save: function ()
    {
        //persist before calling base
        if (this._dirty)
            this.applyItem();

        this.persistData(this._widgetData.Content, false, this.getControl('GeneralTab'));
        this._base();
    },

    reset: function()
    {
        this._widget.find('.nav-tabs a:first').tab('show');
        this._selectedItem = null;
        this.getControl('EditCtr').hide();
        this._dirty = false;
    },

    //close: function()
    //{
    //    this.reset();
    //    this._dialog.modal('hide');
    //},

    _bindItems: function(parent, items)
    {
        for (var i = 0; i < items.length; i++)
        {
            var item = items[i];

            if (item.genId == null)
            {
                this._genId += 1;
                item.genId = 'gen' + this._genId;
            }

            this._itemDict[item.genId] = item;

            var li = $('<li><a style="cursor: pointer;">' + (String.isNullOrEmpty(item.Text) ? '(No Caption)' : item.Text) + '</a></li>').appendTo(parent).data('item', item);  //todo: localize?
            if (this._selectedItem != null && item.genId == this._selectedItem.genId)
                li.addClass('active');
        }
    },

    _move: function(type, dir)
    {
        var items = this._widgetData.Content.Items;
        var itemPos = this._getItemPos(this._selectedItem, items);

        if (type == 'y')
        {
            var swapWith = itemPos + dir;
            if (swapWith >= 0 && swapWith < items.length)
            {
                items.swap(itemPos, swapWith);
                this.bindCarousel();
            }
        }
    },

    _getItemPos: function(item, items)
    {
        for (var i = 0; i < items.length; i++)
        {
            if (items[i].genId == item.genId)
                return i;
        }
    },

    _setDirty: function(dirty)
    {
        this._dirty = dirty;
        this.getControl('btnApply').attr("disabled", !dirty);
    },

    _onCarouselClick: function(e)
    {
        var li = e.target.tagName == 'LI' ? $(e.target) : $(e.target).parents('li');
        if (li.data('item') != null)
        {
            if (!this._dirty || confirm('Carousel information has changed, are you sure you wish to continue and lose your changes?'))  //todo: localize
            {
                this._carouselList.find('li').removeClass('active');
                li.addClass('active');
                this._selectedItem = li.data('item');
                this.editItem();
            }
        }
    },

    _onActionClick: function(e)
    {
        var a = e.target.tagName == 'A' ? $(e.target) : $(e.target).parent('a');
        var action = a.data('action');

        if (action == 'add')
            this.addItem();
        if (action == 'remove')
            this.removeItem();
        else if (action == 'up')
            this._move('y', -1);
        else if (action == 'down')
            this._move('y', 1);

    },

    _onApplyClicked: function(e)
    {
        this.applyItem();
    },

    _onDataChange: function(e)  //override dirty change/// todo: hack?
    {
        if (this._selectedItem != null) //if click apply when focus on changed control, we apply, dirty = false, then change...  so we stop that by checking to ensure we still are working on an item
            this._setDirty(true);
    }

});

