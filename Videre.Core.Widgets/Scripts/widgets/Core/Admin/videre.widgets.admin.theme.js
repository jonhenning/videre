videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.theme = videre.widgets.base.extend(
{
    get_data: function() { return this._data; },
    set_data: function(v) { this._data = v; },
    get_installedThemes: function() { return this._installedThemes; },
    set_installedThemes: function(v)
    {
        this._installedThemes = v;
        this._installedThemeDict = this._installedThemes.toDictionary(function(d) { return d.Name; });
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._data = null;
        this._installedThemes = [];
        this._installedThemeDict = null;
        this._themeDialog = null;
        this._themeData = null;
        this._themeDataDict = null;
        this._selectedTheme = null;

        /* Compile markup as named templates */
        $.templates({
            themeDropdown: '<li data-theme="{{:Name}}"><a>{{:Name}}</a></li>'
        });

        this._delegates = {
            onDownloadThemesReturn: videre.createDelegate(this, this._onDownloadThemesReturn),
            onThemeInstalled: videre.createDelegate(this, this._onThemeInstalled),
            onThemeUninstalled: videre.createDelegate(this, this._onThemeUninstalled)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base

        this.getControl('btnDownloadTheme').click(videre.createDelegate(this, this._onDownloadThemeClicked));
        this._themeDialog = this.getControl('ThemeDialog').modal('hide');
        this._themeDataDict = this._installedThemes.toDictionary(function(d) { return d.Name; });
        this._selectedTheme = this._themeDataDict[this._data.ThemeName];
        this.bind();
    },

    bind: function()
    {
        this.bindThemes();
    },

    refreshDownloadThemes: function()
    {
        $.get("http://simplejsonp.nodester.com/bootswatch", this._delegates.onDownloadThemesReturn);
        //this._delegates.onDownloadThemesReturn(videre.deserialize('{"themes":[{"name":"Amelia","description":"Sweet and cheery.","thumbnail":"http://bootswatch.com/amelia/thumbnail.png","preview":"http://bootswatch.com/amelia/","css":"http://bootswatch.com/amelia/bootstrap.css","css-min":"http://bootswatch.com/amelia/bootstrap.min.css","less":"http://bootswatch.com/amelia/bootswatch.less","less-variables":"http://bootswatch.com/amelia/variables.less"},{"name":"Cerulean","description":"A calm blue sky.","thumbnail":"http://bootswatch.com/cerulean/thumbnail.png","preview":"http://bootswatch.com/cerulean/","css":"http://bootswatch.com/cerulean/bootstrap.css","css-min":"http://bootswatch.com/cerulean/bootstrap.min.css","less":"http://bootswatch.com/cerulean/bootswatch.less","less-variables":"http://bootswatch.com/cerulean/variables.less"},{"name":"Cyborg","description":"Jet black and electric blue.","thumbnail":"http://bootswatch.com/cyborg/thumbnail.png","preview":"http://bootswatch.com/cyborg/","css":"http://bootswatch.com/cyborg/bootstrap.css","css-min":"http://bootswatch.com/cyborg/bootstrap.min.css","less":"http://bootswatch.com/cyborg/bootswatch.less","less-variables":"http://bootswatch.com/cyborg/variables.less"},{"name":"Journal","description":"Crisp like a new sheet of paper.","thumbnail":"http://bootswatch.com/journal/thumbnail.png","preview":"http://bootswatch.com/journal/","css":"http://bootswatch.com/journal/bootstrap.css","css-min":"http://bootswatch.com/journal/bootstrap.min.css","less":"http://bootswatch.com/journal/bootswatch.less","less-variables":"http://bootswatch.com/journal/variables.less"},{"name":"Readable","description":"Optimized for legibility.","thumbnail":"http://bootswatch.com/readable/thumbnail.png","preview":"http://bootswatch.com/readable/","css":"http://bootswatch.com/readable/bootstrap.css","css-min":"http://bootswatch.com/readable/bootstrap.min.css","less":"http://bootswatch.com/readable/bootswatch.less","less-variables":"http://bootswatch.com/readable/variables.less"},{"name":"Simplex","description":"Mini and minimalist.","thumbnail":"http://bootswatch.com/simplex/thumbnail.png","preview":"http://bootswatch.com/simplex/","css":"http://bootswatch.com/simplex/bootstrap.css","css-min":"http://bootswatch.com/simplex/bootstrap.min.css","less":"http://bootswatch.com/simplex/bootswatch.less","less-variables":"http://bootswatch.com/simplex/variables.less"},{"name":"Slate","description":"Shades of gunmetal gray.","thumbnail":"http://bootswatch.com/slate/thumbnail.png","preview":"http://bootswatch.com/slate/","css":"http://bootswatch.com/slate/bootstrap.css","css-min":"http://bootswatch.com/slate/bootstrap.min.css","less":"http://bootswatch.com/slate/bootswatch.less","less-variables":"http://bootswatch.com/slate/variables.less"},{"name":"Spacelab","description":"Silvery and sleek.","thumbnail":"http://bootswatch.com/spacelab/thumbnail.png","preview":"http://bootswatch.com/spacelab/","css":"http://bootswatch.com/spacelab/bootstrap.css","css-min":"http://bootswatch.com/spacelab/bootstrap.min.css","less":"http://bootswatch.com/spacelab/bootswatch.less","less-variables":"http://bootswatch.com/spacelab/variables.less"},{"name":"Spruce","description":"Camping in the woods.","thumbnail":"http://bootswatch.com/spruce/thumbnail.png","preview":"http://bootswatch.com/spruce/","css":"http://bootswatch.com/spruce/bootstrap.css","css-min":"http://bootswatch.com/spruce/bootstrap.min.css","less":"http://bootswatch.com/spruce/bootswatch.less","less-variables":"http://bootswatch.com/spruce/variables.less"},{"name":"Superhero","description":"Batman meets... Aquaman?","thumbnail":"http://bootswatch.com/superhero/thumbnail.png","preview":"http://bootswatch.com/superhero/","css":"http://bootswatch.com/superhero/bootstrap.css","css-min":"http://bootswatch.com/superhero/bootstrap.min.css","less":"http://bootswatch.com/superhero/bootswatch.less","less-variables":"http://bootswatch.com/superhero/variables.less"},{"name":"United","description":"Ubuntu orange and unique font.","thumbnail":"http://bootswatch.com/united/thumbnail.png","preview":"http://bootswatch.com/united/","css":"http://bootswatch.com/united/bootstrap.css","css-min":"http://bootswatch.com/united/bootstrap.min.css","less":"http://bootswatch.com/united/bootswatch.less","less-variables":"http://bootswatch.com/united/variables.less"}]}'));
    },

    bindThemes: function()
    {
        var none = videre.localization.getText('global', 'None');
        var list = [{ Name: none }].addRange(this._installedThemes);

        this.getControl('ThemeDropdown').find('.dropdown-menu').html($.render.themeDropdown(list));
        this.getControl('ThemeDropdown').find('ul a').click(videre.createDelegate(this, this._onThemeChanged));
        this.getControl('ThemeDropdown').find('.text').html(this._selectedTheme == null ? none : this._selectedTheme.Name);

        this.getControl('ThemeList').html(this.getControl('ThemeListTemplate').render(this._installedThemes, { currentTheme: this._selectedTheme }));
        this.getControl('ThemeList').find('a').click(videre.createDelegate(this, this._onThemeAction));
    },

    bindDownloadThemes: function()
    {
        this.getControl('DownloadThemeList').html(this.getControl('DownloadThemeListTemplate').render(this._themeData, { installedThemes: this._installedThemeDict }));
        this.getControl('DownloadThemeList').find('a').click(videre.createDelegate(this, this._onThemeAction));
    },

    persistData: function(portal)
    {
        portal.ThemeName = this._selectedTheme != null ? this._selectedTheme.Name : '';
    },

    installTheme: function(name)
    {
        this.ajax('~/core/Portal/InstallTheme', { theme: this._themeDataDict[name] }, this._delegates.onThemeInstalled, null, this.getControl('ThemeDialog'));
    },

    uninstallTheme: function(name)
    {
        this.ajax('~/core/Portal/UninstallTheme', { name: name }, this._delegates.onThemeUninstalled);
    },

    showDownloadThemes: function()
    {
        this.refreshDownloadThemes();
        this._themeDialog.modal('show');
    },

    _handleThemeChanged: function(name)
    {
        this.getControl('ThemeDropdown').find('.text').html(name);
        this._selectedTheme = this._themeDataDict[name];

        $('[data-type="theme"]').remove();
        if (this._selectedTheme != null)
        {
            var head = $('head');
            $.each(this._selectedTheme.Files, function(idx, file)
            {
                if (file.Type == 'Css' || file.Type == 0)   //todo: 0???  Css... best practice?
                    $(String.format('<link href="{0}" type="text/css" rel="stylesheet"  data-type="theme" />', videre.resolveUrl(file.Path))).appendTo(head);
            });
        }
        this.bindThemes();
    },

    _onDownloadThemesReturn: function(result)
    {
        this._themeData = [];
        for (var i = 0; i < result.themes.length; i++)
        {
            var theme = result.themes[i];
            var themeData = {
                Source: 'bootswatch', Name: theme.name, Description: theme.description, Thumbnail: theme.thumbnail,
                Files: [
                    { Type: 'Css', Path: theme['css-min'] }
                ]
            };
            this._themeData.push(themeData);
            this._themeDataDict[themeData.Name] = themeData;
        }
        this.bindDownloadThemes();
    },

    _onDownloadThemeClicked: function(e)
    {
        this.showDownloadThemes();
    },

    _onThemeAction: function(e)
    {
        var ctl = $(e.target).closest('[data-action]');
        var action = ctl.data('action');
        var theme = ctl.closest('[data-theme]').data('theme');
        if (action == 'apply')
            this._handleThemeChanged(theme);
        if (action == 'install')
            this.installTheme(theme);
        if (action == 'uninstall')
            this.uninstallTheme(theme);
    },

    _onThemeChanged: function(e)
    {
        var ctl = $($(e.target).parents('li'));
        this._handleThemeChanged(ctl.data('theme'));
    },

    _onThemeInstalled: function(result)
    {
        if (!result.HasError)
        {
            this.set_installedThemes(result.Data);
            this.bindThemes();
            this.bindDownloadThemes();
        }
    },

    _onThemeUninstalled: function(result)
    {
        if (!result.HasError)
        {
            this.set_installedThemes(result.Data);
            this.bindThemes();
        }
    }

});

