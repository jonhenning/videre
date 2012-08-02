videre.registerNamespace('videre.widgets');

videre.widgets.searchresults = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v) { this._data = v; },
    get_term: function() { return this._term; },
    set_term: function(v) { this._term = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._term = '';
        this._text = null;
        this._pendingLookup = false;
        this._timerId = null;
        this._lastLookup = null;

        this._delegates = {
            onDataReturn: videre.createDelegate(this, this._onDataReturn),
            onTermDelay: videre.createDelegate(this, this._onTermDelay)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this.getControl('btnSearch').click(videre.createDelegate(this, this._onSearchClicked));
        this._text = this.getControl('Term').on('keyup', videre.createDelegate(this, this._onTermKeyUp)).focus();
        this.getControl('FilterList').find('li').click(videre.createDelegate(this, this._onFilterClicked));
        //videre.UI.handleEnter(this._text, videre.createDelegate(this, this.search));

        this.bind();
    },

    search: function()
    {
        this.getControl('NoResults').hide();
        var term = this.getControl('Term').val();
        this._lastLookup = term;
        if (!String.isNullOrEmpty(term))
        {
            this._pendingLookup = true;

            var filter = this.getControl('FilterList').find('li.active').data('filter');
            if (!String.isNullOrEmpty(filter))
                term = filter + ' AND ' + term;
            this.ajax('~/core/Search/Query', { term: term, max: 100 }, this._delegates.onDataReturn);
        }
    },

    bind: function()
    {
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._data));
        this.getControl('ItemList').show().find('.btn').click(this._delegates.onActionClicked);
        this.getControl('NoResults').toggle(this._data.length == 0 && this.getControl('Term').val().length > 0);
    },

    clear: function()
    {
        this._data = [];
        this.bind();
    },

    _onDataReturn: function(result, ctx)
    {
        this._pendingLookup = false;
        if (!result.HasError)
        {
            this._data = result.Data;
            this.bind();
        }
    },

    _onSearchClicked: function(e)
    {
        this.search();
    },

    //https://gist.github.com/1848558
    _onTermKeyUp: function(e)
    {

        e.stopPropagation();
        e.preventDefault();

        //filter out up/down, tab, enter, and escape keys
        //if ($.inArray(e.keyCode, [40, 38, 9, 13, 27]) === -1)
        if (this._text.val() != this._lastLookup)
        {
            if (this._text.val().length > 0)
                this._delaySuggest();
            else
                this.clear();
        }
    },

    _onTermDelay: function()
    {
        if (!this._pendingLookup)
            this.search();
        else
            this._delaySuggest();
    },

    _delaySuggest: function()
    {
        if (this._timerId != null)
            clearTimeout(this._timerId);
        this._timerId = setTimeout(this._delegates.onTermDelay, 500);
        this.getControl('ItemList').hide();
        this.getControl('NoResults').hide();
    },

    _onFilterClicked: function(e)
    {
        var li = e.target.tagName == 'LI' ? $(e.target) : $(e.target).parent('li');
        if (li.data('filter') != null)
        {
            this.getControl('FilterList').find('li').removeClass('active');
            li.addClass('active');
            this.search();
        }
    }

});

