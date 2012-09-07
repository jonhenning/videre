videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.editor');

videre.widgets.editor.base = videre.widgets.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method
        this._widgetData = null;
        this._manifestData = null;
        this._dialog = null;
        this._dirty = false;

        this._baseEditorDelegates = {
            onItemBind: videre.createDelegate(this, this._onItemBind)
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._dialog = this.getControl('Dialog').modal('hide').hide();
        this.getControl('btnOk').click(videre.createDelegate(this, this._onOkClicked));
        this._widget.find('[data-column]').change(videre.createDelegate(this, this._onDataChange));
        //this._widget.find('input,select,textarea').jqBootstrapValidation();
    },

    show: function(widget, manifest)
    {
        this._widgetData = widget;
        this._manifestData = manifest;
        this.bindData(this._widgetData, this.getControl('CommonProperties'));
        this.bindAttributes();
        this._dialog.modal('show');
        this.clearMsgs(this._dialog);
        this._dirty = false;
    },

    bindAttributes: function()
    {
        this.getControl('AttributeList').html(this.getControl('AttributeListTemplate').render(this._manifestData.AttributeDefinitions, { attributes: this._widgetData.Attributes }));
        this.getControl('AttributeList').toggle(this._manifestData.AttributeDefinitions.length > 0);
    },

    validate: function()
    {
        return this.validControls(this.getControl('CommonProperties'), this._widget);
    },

    save: function()
    {
        this.persistData(this._widgetData, false, this.getControl('CommonProperties'));
        this.persistData(this._widgetData.Attributes, false, this.getControl('AttributeList'));
        this.raiseCustomEvent('Save', this._widgetData);
        this._dialog.modal('hide');
    },

    _onOkClicked: function(e)
    {
        if (this.validate())
            this.save();
    },

    _onDataChange: function (e)
    {
        this._dirty = true;
    }


});

