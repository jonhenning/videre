videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.editor');

videre.widgets.editor.menu = videre.widgets.editor.base.extend(
{

    init: function()
    {
        this._base();  //call base method
        this._itemDict = null;
        this._selectedItem = null;
        this._menuList = null;
        this._genId = 0;
        this._menuData = null;
        this._menuDataDict = null;
        this._newMenuData = null;
        this._newMenuDialog = null;
        //this._pendingMenuDict = {};

        this._delegates = {
            onMenuDataReceived: videre.createDelegate(this, this._onMenuDataReceived)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnApply').click(videre.createDelegate(this, this._onApplyClicked));
        this._menuList = this.getControl('MenuList').click(videre.createDelegate(this, this._onMenuClicked));
        this.getControl('ActionButtons').find('a').click(videre.createDelegate(this, this._onActionClicked));
        this._widget.find('[data-column]').change(videre.createDelegate(this, this._onDataChange));

        this._newMenuDialog = this.getControl('NewMenuDialog').modal('hide');
        this.getControl('btnNewMenu').click(videre.createDelegate(this, this._onNewMenuClicked));
        this.getControl('btnDeleteMenu').click(videre.createDelegate(this, this._onDeleteMenuClicked));
        this.getControl('btnOkNewMenu').click(videre.createDelegate(this, this._onNewMenuOkClicked));
        this.getControl('ddlName').change(videre.createDelegate(this, this._onMenuChanged));
    },

    show: function(widget, manifest)
    {
        this._base(widget, manifest);
        this.reset();

        this._newMenuData = widget.Content != null && String.isNullOrEmpty(widget.Content.Id) ? widget.Content : null; //store reference to original menu data (really only for new menu that was appended to list)

        if (widget.Content == null)
            widget.Content = this._getNewMenu('');

        //if (this._menuData == null)
            this.refreshMenus();
        //else
        //    this.bind();
    },

    showNewMenuDialog: function()
    {
        this.getControl('txtNewMenu').val('');
        this._newMenuDialog.modal('show');
    },

    refreshMenus: function()
    {
        this.ajax('~/core/menu/get', {}, this._delegates.onMenuDataReceived);
    },

    deleteMenu: function(id)
    {
        this.ajax('~/core/menu/delete', { id: id }, this._delegates.onMenuDataReceived);
    },

    bind: function()
    {
        this.bindMenus();
        this.bindData(this._widgetData.Content, this.getControl('GeneralTab'));
        this.bindMenu();
    },

    bindMenus: function()
    {
        var ddl = this.getControl('ddlName');
        var origValue = ddl.val();
        ddl[0].options.length = 0;
        $.each(this._menuData, function(val, text)
        {
            ddl.append($('<option></option>').val(this.Id).html(this.Name));
        });

        if (!String.isNullOrEmpty(this._widgetData.Content.Name))   //if name is blank, we lost our menu due to a delete
        {
            if (this._newMenuData != null && this._menuDataDict[this._newMenuData.Id] == null)    //if menu doesn't exist in data, its a new one, use data from widget to populate
                ddl.append($('<option></option>').val(this._newMenuData.Id).html(this._newMenuData.Name));  //Id will be empty ''
            ddl.val(this._widgetData.Content.Id);
            if (ddl.val() != origValue)
                this._handleMenuChanged();
        }
        else
            this._handleMenuChanged();  //choose first available menu - ours was deleted
    },

    bindMenu: function()
    {
        this._menuList.html('');
        this._itemDict = {};
        this._bindItems(this._menuList, this._widgetData.Content.Items, null);
    },

    editItem: function()
    {
        this.getControl('EditCtr').show();
        this.getControl('EditCtr').find('[required]').attr('bypassvalidation', null);   //todo:  we need a way to allow for panes to opt out of validation of their controls...  this is ok, but still feels a bit dirty
        this.bindData(this._selectedItem, this.getControl('EditCtr'));  //MenuTab
        this._setDirty(false);
    },

    applyItem: function()
    {
        if (this._selectedItem != null && this.validControls(this.getControl('EditCtr'), this.getControl('Dialog')))
        {
            this.persistData(this._selectedItem, false, this.getControl('EditCtr'));    //MenuTab
            this.bindMenu();
            this._setDirty(false);
            this.getControl('EditCtr').hide();
        }
    },

    addItem: function()
    {
        var item = { genId: this._nextId(), Text: 'New Item', Items: [], Url: '', Roles: [], Authenticated: null };
        this._widgetData.Content.Items.push(item);
        this._selectedItem = item;
        this.bindMenu();
        this.editItem();
    },

    removeItem: function()
    {
        if (this._selectedItem != null)
        {
            var items = this._getParentItems(this._selectedItem);
            var itemPos = this._getItemPos(this._selectedItem, items);
            items.remove(itemPos);
            this.bindMenu();
            this.getControl('EditCtr').hide();
        }
    },

    _getParentItems: function(item)
    {
        return item.parentId != null ? this._itemDict[item.parentId].Items : this._widgetData.Content.Items;
    },

    validate: function ()
    {
        //add custom validation here
        return this._base() && this.validControls(this.getControl('GeneralTab'), this._widget);
    },

    save: function()
    {
        //persist before calling base
        if (this._dirty)
            this.applyItem();

        this.persistData(this._widgetData.Content, false, this.getControl('GeneralTab'));
        this._widgetData.Content.Name = $('option:selected', this.getControl('ddlName')).text();   //todo:  have ability to persist text using attributes??

        this._base();
        //this.reset();
    },

    reset: function()
    {
        this._widget.find('.nav-tabs a:first').tab('show');
        this._selectedItem = null;
        this.getControl('EditCtr').hide();
        this.getControl('EditCtr').find('[required]').attr('bypassvalidation', 'true'); //todo:  we need a way to allow for panes to opt out of validation of their controls...  this is ok, but still feels a bit dirty
        this._dirty = false;
    },

    _nextId: function()
    {
        this._genId += 1;
        return 'genID_' + this._genId;
    },

    _getNewMenu: function(name)
    {
        return { Id: '', Text: '', Name: name, MenuType: 0, Items: [] };
    },

    _handleMenuChanged: function()
    {
        var id = this.getControl('ddlName').val();
        var data = this._menuData.where(function(m) { return m.Id == id; }).singleOrDefault();
        if (data == null)   //if menu not found, it is a new entry
            data = this._newMenuData;

        this._widgetData.Content = data;
        this.bindData(this._widgetData.Content, this.getControl('GeneralTab'));
        this.bindMenu();
    },

    _handleDeleteMenu: function(id)
    {
        if (!String.isNullOrEmpty(id))
            this.deleteMenu(id);
        else   
            this._newMenuData = null;
    },

    _handleNewMenu: function()
    {
        var name = this.getControl('txtNewMenu').val();
        if (!String.isNullOrEmpty(name))    //todo: verify not exist in _menuData as well
        {
            this._newMenuData = this._getNewMenu(name);
            this._widgetData.Content = this._newMenuData;
            this.bindMenus();
            this._handleMenuChanged();
            this._newMenuDialog.modal('hide');
        }
    },

    _bindItems: function(parent, items, parentItem)
    {
        for (var i = 0; i < items.length; i++)
        {
            var item = items[i];

            if (item.genId == null)
                item.genId = this._nextId();

            item.parentId = parentItem != null ? parentItem.genId : null;
            this._itemDict[item.genId] = item;

            var li = $('<li><a style="cursor: pointer;">' + item.Text + '</a></li>').appendTo(parent).data('item', item);
            if (this._selectedItem != null && item.genId == this._selectedItem.genId)
                li.addClass('active');
            if (item.Items.length > 0)
                this._bindItems($('<ul class="nav nav-list"></ul>').appendTo(li), item.Items, item);
        }
    },

    _move: function(type, dir)
    {
        var items = this._getParentItems(this._selectedItem);
        var itemPos = this._getItemPos(this._selectedItem, items);

        if (type == 'y')
        {
            var swapWith = itemPos + dir;
            if (swapWith >= 0 && swapWith < items.length)
            {
                items.swap(itemPos, swapWith);
                this.bindMenu();
            }
        }
        else if (type == 'x')
        {
            if (dir == -1 && this._selectedItem.parentId != null)
            {
                items.remove(itemPos);
                var parentItems = this._getParentItems(this._itemDict[this._selectedItem.parentId]);
                parentItems.push(this._selectedItem);
                this.bindMenu();
            }

            if (dir == 1 && itemPos > 0)
            {
                items[itemPos - 1].Items.push(this._selectedItem);
                items.remove(itemPos);
                this.bindMenu();
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

    _onMenuClicked: function(e)
    {
        var li = e.target.tagName == 'LI' ? $(e.target) : $(e.target).parent('li');
        if (li.data('item') != null)
        {
            if (!this._dirty || confirm('Menu information has changed, are you sure you wish to continue and lose your changes?'))  //todo: localize
            {
                this._menuList.find('li').removeClass('active');
                li.addClass('active');
                this._selectedItem = li.data('item');
                this.editItem();
            }
        }
    },

    _onActionClicked: function(e)
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
        else if (action == 'left')
            this._move('x', -1);
        else if (action == 'right')
            this._move('x', 1);

    },

    _onApplyClicked: function(e)
    {
        this.applyItem();
    },

    _onNewMenuClicked: function(e)
    {
        this.showNewMenuDialog();
    },

    _onNewMenuOkClicked: function(e)
    {
        this._handleNewMenu();
    },

    _onDeleteMenuClicked: function(e)
    {
        this._handleDeleteMenu(this.getControl('ddlName').val());
    },

    _onDataChange: function(e) //override dirty change/// todo: hack?
    {
        this._setDirty(true);
    },

    _onMenuDataReceived: function(result)
    {
        if (!result.HasError)
        {
            this._menuData = result.Data;
            this._menuDataDict = this._menuData.toDictionary(function(d) { return d.Id });
            this.bind();
        }
    },

    _onMenuChanged: function(e)
    {
        this._handleMenuChanged();
    }

});

