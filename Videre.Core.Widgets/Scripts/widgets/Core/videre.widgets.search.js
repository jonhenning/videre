videre.registerNamespace('videre.widgets');

videre.widgets.search = videre.widgets.base.extend(
{

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._term = '';
        this._text = null;
        this._typeahead = null;
        this._timerId = null;
        this._lastLookup = null;

        this._delegates = {
            onTermDelay: videre.createDelegate(this, this._onTermDelay)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base
        this._text = this.getControl('Term');
        this._typeahead = this._text.typeahead({ matcher: function() { return true; } }).data('typeahead'); //matcher by default ensures that displayed element contains the text... we want all results to show
        this._typeahead.select = videre.createDelegate(this, this._onTermSelect);
        videre.UI.handleEnter(this._text, videre.createDelegate(this, this.search));
        this._text.keyup(videre.createDelegate(this, this._onTermKeyUp));
    },

    _onTermSelect: function()
    {
        var active = this._typeahead.$menu.find('.active');
        if (active.length > 0)
        {
            var item = active.data('item');
            if (item != null && item.Url != null)
                window.location.href = videre.resolveUrl(item.Url);
        }
        else
            this.search();
        this._typeahead.hide();
    },

    search: function()
    {
        this._text.parent('form')[0].submit();
    },

    suggest: function(val)
    {
        //set typeahead source to empty
        this._typeahead.source = [];

        //active used so we aren't triggering duplicate keyup events
        if (!this._text.data('active') && val.length > 0)
        {
            this._text.data('active', true);
            this._lastLookup = val;

            var self = this;
            //Do data request. Insert your own API logic here.
            //$.getJSON("http://search.twitter.com/search.json?callback=?", { q: this._text.val() }, function(data)
            videre.ajax('~/core/Search/Query', { term: val, max: 8 }, function(result)
            {
                var data = result.Data;
                self.setTypeAheadSource(result.Data);//.select(function(d) { return d.Name; }));
            });
        }
    },

    setTypeAheadSource: function(data)
    {
        this._text.data('active', true);
        this._typeahead.source = data.select(function(d) { return d.Name; });

        //trigger keyup on the typeahead to make it search
        this._text.trigger('keyup');

        //don't default first item selected
        this._typeahead.$menu.find('.active').removeClass('active');

        //allow whole data source item to be retrieved (without modification to core bootstrap)
        var items = this._typeahead.$menu.find('li');
        for (var i = 0; i < items.length; i++)
            $(items[i]).data('item', data[i]);

        this._text.data('active', false);
    },

    //https://gist.github.com/1848558
    _onTermKeyUp: function(e)
    {
        if (this._text.data('active') != true)
        {
            e.stopPropagation();
            e.preventDefault();
            //filter out up/down, tab, enter, and escape keys
            if ($.inArray(e.keyCode, [40, 38, 9, 13, 27]) === -1 && this._text.val() != this._lastLookup)
                this._delaySuggest();
        }
    },

    _onTermDelay: function()
    {
        this.suggest(this._text.val());
    },
    
    _delaySuggest: function()
    {
        if (this._timerId != null)
            clearTimeout(this._timerId);
        this._timerId = setTimeout(this._delegates.onTermDelay, 500);
        this.setTypeAheadSource([]);
    }

});

