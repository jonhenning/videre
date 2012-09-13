videre.registerNamespace('videre.controls');
videre.registerNamespace('videre.controls.texteditor');

videre.controls.texteditor.wysihtml5 = videre.widgets.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method

        this._delegates = {
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._editor = this.getControl(this._id).wysihtml5().data("wysihtml5").editor;
    },

    bind: function(data)
    {
        this._editor.setValue(data.Text);
    },

    persist: function()
    {
    },

    _onBlur: function(e)
    {
        if (this._editor.checkDirty())
            this.getControl(this._id).trigger('change');
    }

});

