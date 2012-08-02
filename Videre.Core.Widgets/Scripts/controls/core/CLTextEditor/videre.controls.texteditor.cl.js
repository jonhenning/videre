videre.registerNamespace('videre.controls');
videre.registerNamespace('videre.controls.texteditor');

videre.controls.texteditor.cl = videre.widgets.base.extend(
{
    //constructor
    init: function()
    {
        this._base();  //call base method
        this._editor = null;
        this._delegates = {
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._editor = this.getControl(this._id).cleditor()[0];
    },

    bind: function(data)
    {
        this._editor.updateFrame();
    },

    persist: function()
    {
        //this._editor.updateTextArea();
    }

});

