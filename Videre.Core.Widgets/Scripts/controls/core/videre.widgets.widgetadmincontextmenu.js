videre.registerNamespace('videre.widgets');

videre.widgets.widgetadmincontextmenu = videre.widgets.base.extend(
{
    get_containerId: function() { return this._containerId; },
    set_containerId: function(v) { this._containerId = v; },
    get_widget: function() { return this._widgetModel; },
    set_widget: function(v) { this._widgetModel = v; },
    get_manifest: function() { return this._manifest; },
    set_manifest: function(v) { this._manifest = v; },
    get_templateId: function() { return this._templateId; },
    set_templateId: function(v) { this._templateId = v; },
    get_layoutId: function() { return this._layoutId; },
    set_layoutId: function(v) { this._layoutId = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._widgetModel = null;
        this._manifest = null;
        this._templateId = null;
        this._layoutId = null;
        this._editor = null;

        this._delegates = {
            onWidgetSave: videre.createDelegate(this, this._onWidgetSave),
            onSaveReturn: videre.createDelegate(this, this._onSaveReturn)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._widget.find('a[data-action]').click(videre.createDelegate(this, this._onAdminClick));
        $('#' + this._containerId).bind("contextmenu", videre.createDelegate(this, this._onContextMenu)).blur(videre.createDelegate(this, this._onInlineBlur));
        this._widgetModel.Content = videre.deserialize(this._widgetModel.ContentJson);
    },

    edit: function(editWidget)
    {
        if (this._editor == null)
        {
            var editorType = this._manifest.EditorType != null ? this._manifest.EditorType : 'videre.widgets.editor.common';
            this._editor = videre.widgets.findFirstByType(editorType);
            this._editor.add_onCustomEvent(this._delegates.onWidgetSave);
        }

        this._editor.show(this._widgetModel, this._manifest, editWidget);
    },

    inlineEdit: function()
    {
        var ctl = $('#' + this._containerId);
        ctl.attr('contenteditable', 'true');
        ctl.focus();
    },

    save: function(widget, ctr)
    {
        this.ajax('~/core/Portal/SaveWidget', { templateId: this._templateId, layoutId: this._layoutId, widget: widget }, this._delegates.onSaveReturn, null, ctr);    //todo: minihack accessing _dialog
    },

    toggleMenu: function()
    {
        this._widget.find('.dropdown-toggle').dropdown('toggle');
    },

    _saveInline: function()
    {
        var ctl = $('#' + this._containerId);
        if (ctl.attr('contenteditable') == 'true')
        {
            var clone = videre.jsonClone(this._widgetModel);
            var html = ctl.html();
            clone.Content[0].EditText = html;
            clone.ContentJson = videre.serialize(clone.Content);
            this.save(clone);
            ctl.attr('contenteditable', null);
        }
    },

    _onInlineBlur: function(e, args)
    {
        this._saveInline();
    },

    _onWidgetSave: function(e, args)
    {
        if (args.type == 'Save')
        {
            args.data.ContentJson = videre.serialize(args.data.Content);
            this.save(args.data, this._editor._dialog);
        }
    },

    _onSaveReturn: function(e)
    {
        if (e.Data)
        {
            if (this._editor != null)
                this._editor.hide();
            location.href = location.href;  //reload
        }
    },

    _onAdminClick: function(e)
    {
        var ctl = $(e.target).closest('[data-action]');
        var action = ctl.data('action');
        if (action == 'editwidget')
            this.edit(true);
        if (action == 'edit')
            this.edit(false);
        if (action == 'inline-edit')
            this.inlineEdit();
        this.toggleMenu();
    },

    _onContextMenu: function(e)
    {
        this._widget.css({ top: e.pageY, left: e.pageX });
        this.toggleMenu();
        return false;
    }

});
