nnr.registerNamespace('nnr.controls');
nnr.registerNamespace('nnr.controls.texteditor');

nnr.controls.texteditor.cl = nnr.component.presenterBase.extend(
{
    //constructor
    init: function()
    {
        this._super();  //call base method
        this._editor = null;
        this._delegates = {
        }
    },

    _onLoad: function(src, args)
    {
        this._super(); //call base
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

