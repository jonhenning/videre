videre.registerNamespace('videre.controls');
videre.registerNamespace('videre.controls.texteditor');

videre.controls.comments = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v) { this._data = v; },
    get_isAdmin: function() { return this._isAdmin; },
    set_isAdmin: function(v) { this._isAdmin = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._delegates = {
            onAddCommentReturn: videre.createDelegate(this, this._onAddCommentReturn),
            onCommentActionReturn: videre.createDelegate(this, this._onCommentActionReturn)
    }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnAddComment').click(videre.createDelegate(this, this._onAddCommentClicked));
        if (this._isAdmin)
            this.bind();
    },

    bind: function()
    {
        this.getControl('Comments').html(this.getControl('CommentTemplate').render(this._data.Comments));
        this.getControl('Comments').find('[data-action]').click(videre.createDelegate(this, this._onCommentActionClicked));
    },

    addComment: function()
    {
        if (this.validControls(this._widget))
        {
            var comment = this.persistData({}, true, this._widget);
            this.ajax('~/core/Comment/AddComment', { comment: comment, containerType: this._data.ContainerType, containerId: this._data.ContainerId }, this._delegates.onAddCommentReturn);
        }
    },

    approveComment: function(id)
    {
        this.ajax('~/core/Comment/ApproveComment', { id: id, containerType: this._data.ContainerType, containerId: this._data.ContainerId }, this._delegates.onCommentActionReturn);
    },

    removeComment: function(id)
    {
        this.ajax('~/core/Comment/RemoveComment', { id: id, containerType: this._data.ContainerType, containerId: this._data.ContainerId }, this._delegates.onCommentActionReturn);
    },

    handleAction: function(action, id)
    {
        if (action == 'approve')
            this.approveComment(id);
        else if (action == 'remove')
            this.removeComment(id);
    },

    _onAddCommentReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this._data = result.Data;
            this.getControl('CommentContainer').hide();
        }
    },

    _onCommentActionReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this._data = result.Data;
            this.bind();
        }
    },

    _onAddCommentClicked: function(e)
    {
        this.addComment();
    },

    _onCommentActionClicked: function(e)
    {
        var ctl = $(e.target);
        if (e.target.tagName.toLowerCase() != 'a')  //if clicked in i tag, need a
            ctl = ctl.parent();
        this.handleAction(ctl.data('action'), ctl.data('id'));
    }

});

