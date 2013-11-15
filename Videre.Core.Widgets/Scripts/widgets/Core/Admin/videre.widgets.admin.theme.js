videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.theme = videre.widgets.base.extend(
{
    get_themeAPIUrl: function() { return this._themeAPIUrl; },
    set_themeAPIUrl: function(v) { this._themeAPIUrl = v; },
    get_installedThemes: function() { return this._installedThemes; },
    set_installedThemes: function(v)
    {
        this._installedThemes = v;
        this._installedThemeDict = this._installedThemes.toDictionary(function(d) { return d.Name; });
    },
    get_selectedTheme: function() { return this._selectedTheme; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._installedThemes = [];
        this._installedThemeDict = null;
        this._themeDialog = null;
        this._themeData = null;
        this._themeDataDict = null;
        this._selectedTheme = null;
        this._themeAPIUrl = null;
        this._defaultTheme = null;
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
        this.bind();
    },

    bind: function()
    {
        this.bindThemes();
    },

    refreshDownloadThemes: function()
    {
        this.lock(this._themeDialog);
        $.ajax({ url: this._themeAPIUrl, dataType: 'jsonp', success: this._delegates.onDownloadThemesReturn });
    },

    bindThemes: function()
    {
        var none = videre.localization.getText('global', 'None');
        var list = [{ Name: none }].addRange(this._installedThemes);

        this.getControl('ThemeDropdown').find('.dropdown-menu').html($.render.themeDropdown(list));
        this.getControl('ThemeDropdown').find('ul a').click(videre.createDelegate(this, this._onThemeChanged));
        this.getControl('ThemeDropdown').find('.text').html(this._selectedTheme == null ? none : this._selectedTheme.Name);

        this.getControl('ThemeList').html(this.getControl('ThemeListTemplate').render(this._installedThemes, { currentTheme: this._selectedTheme }));
        this.getControl('ThemeList').find('[data-action]').click(videre.createDelegate(this, this._onThemeAction));
    },

    bindDownloadThemes: function()
    {
        this.getControl('DownloadThemeList').html(this.getControl('DownloadThemeListTemplate').render(this._themeData, { installedThemes: this._installedThemeDict }));
        this.getControl('DownloadThemeList').find('[data-action]').click(videre.createDelegate(this, this._onThemeAction));
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

    changeTheme: function(name)
    {
        this.getControl('ThemeDropdown').find('.text').html(name);
        this._selectedTheme = this._installedThemeDict[name];
        if (this._defaultTheme == null)
            this._defaultTheme = $('[data-type="theme"]');

        $('[data-type="theme"]').remove();
        var head = $('head');
        if (this._selectedTheme != null)
        {
            $.each(this._selectedTheme.Files, function(idx, file)
            {
                if (file.Type == 'Css' || file.Type == 0)   //todo: 0???  Css... best practice?
                {
                    //IE is very picky how this gets done... 
                    var link = $(document.createElement('link')).attr({ type: 'text/css', rel: 'stylesheet', href: videre.resolveUrl(file.Path), media: 'screen' }).data('type', 'theme');
                    head[0].appendChild(link[0]);
                    //$(String.format('<link href="{0}" type="text/css" rel="stylesheet"  data-type="theme" />', videre.resolveUrl(file.Path))).appendTo(head);
                }
            });
        }
        else if (this._defaultTheme.length > 0)
            head[0].appendChild(this._defaultTheme[0]);
        this.bindThemes();
    },

    _onDownloadThemesReturn: function(result)
    {
        this.unlock(this._themeDialog);
        this._themeData = [];
        this._themeDataDict = {};
        for (var i = 0; i < result.themes.length; i++)
        {
            var theme = result.themes[i];
            var themeData = {
                Source: 'bootswatch', Name: theme.name, Description: theme.description, Thumbnail: theme.thumbnail,
                Files: [
                    { Type: 'Css', Path: theme['cssMin'] }
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
            this.changeTheme(theme);
        if (action == 'install')
            this.installTheme(theme);
        if (action == 'uninstall')
            this.uninstallTheme(theme);
    },

    _onThemeChanged: function(e)
    {
        var ctl = $($(e.target).parents('li'));
        this.getControl('ThemeDropdown').find('.dropdown-toggle').dropdown('toggle');
        this.changeTheme(ctl.data('theme'));
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

