videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.portal = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v) { this._data = v; },
    get_installedThemes: function() { return this._installedThemes; },
    set_installedThemes: function(v) { this._installedThemes = v; },
    get_attributeDefs: function() { return this._attributeDefs; },
    set_attributeDefs: function(v)
    {
        this._attributeDefs = v;
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._installedThemes = null;
        this._themeWidget = null;
        this._attributeDefs = {};

        this._delegates = {
            onAttributeItemBind: videre.createDelegate(this, this._onAttributeItemBind),
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));       

        var uploader = new qq.FileUploaderBasic({
            button: this.getControl('btnImport')[0],
            params: {},
            action: videre.resolveUrl('~/core/portal/importportal'),
            onSubmit: videre.createDelegate(this, this._onFileSubmit),
            onComplete: videre.createDelegate(this, this._onFileUploadReturn),
            showMessage: videre.createDelegate(this, this._onFileMessage),
            debug: true
        });

        this._themeWidget = videre.widgets.register(this._id, videre.widgets.admin.theme, {id:this._id, ns: this._ns, data: this._data, installedThemes: this._installedThemes});

        //this.refreshThemes();
        this.bind();

    },

    bind: function()
    {
        this._themeWidget.bind();

        this.bindData(this._data);
        for(var key in this._attributeDefs)
            this.bindAttributes(this._getSafeGroupName(key), this._attributeDefs[key]);

    },

    bindAttributes: function(groupName, defs)
    {
        videre.UI.parseTemplate(this.getControl('AttributeListTemplate-' + groupName), this.getControl('AttributeList-' + groupName), defs, this._delegates.onAttributeItemBind);
        //this.getControl('AttributeList-' + groupName).html(this.getControl('AttributeListTemplate-' + groupName).render(defs, { roleDataDict: this._roleDataDict }));
    },

    save: function()
    {
        //todo: validation!
        var portal = this.persistData(this._data, true, this.getControl('GeneralTab'));
        this._themeWidget.persistData(portal);
        //portal.ThemeName = this._selectedTheme != null ? this._selectedTheme.Name : '';
        for(var key in this._attributeDefs)
            this.persistData(portal.Attributes, false, this.getControl('tab' + this._getSafeGroupName(key)));
        this.ajax('~/core/Portal/SavePortal', { portal: portal }, this._delegates.onSaveReturn);
    },

    _getSafeGroupName: function(name)
    {
        return name.replace(new RegExp(' ', 'g'), '-').replace(new RegExp('\\.', 'g'), '-');
    },

    //this logic is shared from widget.editor.base - todo:  common location?
    _onAttributeItemBind: function(ctr, data)
    {
        var td = $(ctr).find('[data-type="input"]');
        var groupName = td.data('group');
        var keyName = groupName + '.' + data.Name;
        var ctl;
        if(data.Values.length > 0)
        {
            ctl = $('<select>').attr('data-column', keyName);
            $.each(data.Values, function(idx, item)
            {
                $('<option>').attr('value', item).html(item).appendTo(ctl);
            });
            ctl.val(data.DefaultValue);
        }
        else
            ctl = $('<input>').attr({ type: 'text', 'data-column': keyName }).val(data.DefaultValue);

        if(data.Required)
            ctl.attr('data-required', 'true');
        ctl.appendTo(td);
        if(this._data.Attributes[keyName] != null)
            ctl.val(this._data.Attributes[keyName]);

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
        if(!result.HasError && result.Data)
        {
            this._data = result.Data;
            this.bind();
        }
    },

    _onSaveClicked: function(e)
    {
        this.save();
    },

    _onFileSubmit: function(id, fileName)
    {
        this.lock();
    },

    _onFileUploadReturn: function(id, fileName, result)
    {
        this.unlock();
        if(!result.HasError)
        {

        }
        this.addMsgs(result.Messages);
    },

    _onFileMessage: function(message)
    {
        this.addMsg('FileMessage', message, true, this.getControl('Dialog'));   //todo: test!
    }

});

