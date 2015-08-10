videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.editor');

videre.widgets.editor.common = videre.widgets.editor.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
    },

    show: function(widget, manifest, contentAdmin)
    {
        this._base(widget, manifest, contentAdmin); //call base
    },

    validate: function ()
    {
        //add custom validation here
        return this._base();
    },

    save: function()
    {
        //persist before calling base
        this._base();
    }

});

