videre.registerNamespace('videre.controls');
videre.registerNamespace('videre.controls.texteditor');

videre.controls.texteditor.ck = videre.widgets.base.extend(
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
        //this._editor = this.getControl('txtHtml').cleditor()[0];

        //var pic = mySettings.markupSet.where(function(s) { return s.name == 'Picture'; });
        //pic.beforeInsert = videre.createDelegate(this, this._onPictureInsert);
        //this._editor = this.getControl('txtHtml').markItUp(mySettings);
        this._editor = this.getControl(this._id).ckeditor().ckeditorGet();
        var self = this;
        this._editor.on('blur', videre.createDelegate(self, self._onBlur));
        //        this._editor.ckeditor(function()
        //        {
        //            this.on('blur', videre.createDelegate(self, self._onBlur));
        //        });
    },

    bind: function(data)
    {
        //this._editor.updateFrame();
    },

    persist: function()
    {
        //this.getControl('txtHtml').cleditor('updateTextArea');
    },

    _onBlur: function(e)
    {
        if (this._editor.checkDirty())
            this.getControl(this._id).trigger('change');
    }

});

