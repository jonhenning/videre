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
    get_layoutName: function() { return this._layoutName; },
    set_layoutName: function(v) { this._layoutName = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._widgetModel = null;
        this._manifest = null;
        this._templateId = null;
        this._layoutName = null;
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
        $('#' + this._containerId).bind("contextmenu", videre.createDelegate(this, this._onContextMenu));
        this._widgetModel.Content = videre.deserialize(this._widgetModel.ContentJson);
    },

    edit: function()
    {
        if (this._editor == null)
        {
            var editorType = this._manifest.EditorType != null ? this._manifest.EditorType : 'videre.widgets.editor.common';
            this._editor = videre.widgets.findFirstByType(editorType);
            this._editor.add_onCustomEvent(this._delegates.onWidgetSave);
        }
        
        this._editor.show(this._widgetModel, this._manifest);
    },

    save: function(widget)
    {
        this.ajax('~/core/Portal/SaveWidget', { templateId: this._templateId, layoutName: this._layoutName, widget: widget }, this._delegates.onSaveReturn, null, this._dialog);
    },

    toggleMenu: function()
    {
        this._widget.find('.dropdown-toggle').dropdown('toggle');
    },

    _onWidgetSave: function(e, args)
    {
        if (args.type == 'Save')
        {
            args.data.ContentJson = videre.serialize(args.data.Content);
            this.save(args.data);
        }
    },

    _onSaveReturn: function(e)
    {
        if (e.Data)
            location.href = location.href;  //reload
    },

    _onAdminClick: function(e)
    {
        var action = $(e.target).data('action');
        if (action == 'edit')
            this.edit();
        this.toggleMenu();
    },

    _onContextMenu: function(e)
    {
        this._widget.css({ top: e.pageY, left: e.pageX });
        this.toggleMenu();
        return false;
    },

});

