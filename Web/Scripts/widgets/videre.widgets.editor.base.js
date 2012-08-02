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
        videre.UI.parseTemplate(this.getControl('AttributeListTemplate'), this.getControl('AttributeList'), this._manifestData.AttributeDefinitions, this._baseEditorDelegates.onItemBind);
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

    _onItemBind: function(ctr, data)
    {
        var td = $(ctr).find('[data-type="input"]');
        var ctl;
        if (data.Values.length > 0)
        {
            ctl = $('<select>').attr('data-column', data.Name);
            $.each(data.Values, function (idx, item)
            {
                $('<option>').attr('value', item).html(item).appendTo(ctl);
            });
            ctl.val(data.DefaultValue);
        }
        else
        {
            ctl = $('<input>').attr({ type: 'text', 'data-column': data.Name }).val(data.DefaultValue);
        }

        if (data.Required)
            ctl.attr('data-required', 'true');
        ctl.appendTo(td);
        if (this._widgetData.Attributes[data.Name] != null)
            ctl.val(this._widgetData.Attributes[data.Name]);

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

