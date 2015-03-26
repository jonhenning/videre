videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.template = videre.widgets.base.extend(
{
    get_isLayout: function() { return this._isLayout; },
    set_isLayout: function(v) { this._isLayout = v; },
    get_manifestData: function() { return this._manifestData; },
    set_manifestData: function(v)
    {
        this._manifestData = v;
        this._manifestDataDict = this._manifestData.toDictionary(function(m) { return m.Id; });
    },
    get_layoutData: function() { return this._layoutData; },
    set_layoutData: function(v)
    {
        this._layoutData = v;
        this._layoutDataDict = this._layoutData.toDictionary(function(l) { return l.Name; });
    },
    get_layoutTemplateData: function() { return this._layoutData; },
    set_layoutTemplateData: function(v)
    {
        this._layoutTemplateData = v;
        this._layoutTemplateDataDict = this._layoutTemplateData.toDictionary(function(l) { return l.Id; });
    },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._tree = null;
        this._data = null;
        this._isLayout = false;
        this._treeData = null;
        this._templateData = null;
        this._templateDataDict = null;
        this._manifestData = null;
        this._manifestDataDict = {};
        this._layoutData = null;
        this._layoutDataDict = {};
        this._layoutTemplateData = null;
        this._layoutTemplateDataDict = {};
        this._contentData = null;
        this._editors = {};

        this._genId = 0;
        this._widgets = {};

        this._selectedKey = null;
        this._selectedTemplate = null;
        this._templatePanes = {};

        this._templateCtr = null;
        this._templateEdit = null;

        this._delegates = {
            onTemplateReturn: videre.createDelegate(this, this._onTemplateReturn),
            onTemplateSaveReturn: videre.createDelegate(this, this._onTemplateSaveReturn),
            onWidgetContentReturn: videre.createDelegate(this, this._onWidgetContentReturn),
            onNodeSelected: videre.createDelegate(this, this._onNodeSelected),
            onActionClicked: videre.createDelegate(this, this._onActionClicked),
            onWidgetActionClicked: videre.createDelegate(this, this._onWidgetActionClicked),
            onWidgetSave: videre.createDelegate(this, this._onWidgetSave)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base

        this._tree = this.getControl('Tree').dynatree(
        {
            minExpandLevel: 2,
            onActivate: this._delegates.onNodeSelected
        });
        this._templateCtr = this.getControl('TemplateCtr').hide();
        this._templateEdit = this.getControl('TemplateEdit').hide();
        this.getControl('btnNew').click(videre.createDelegate(this, this._onNewClicked));
        this.getControl('btnSave').click(videre.createDelegate(this, this._onSaveClicked));
        this.getControl('btnCancel').click(videre.createDelegate(this, this._onCancelClicked));
        this.getControl('LayoutTab').find('[data-manifestid]').draggable({
            addClasses: false,
            opacity: 0.7,
            appendTo: this._widget,
            connectToSortable: '.pane',
            helper: 'clone',
            start: function (e, ui)
            {
                $(ui.helper).css('width', '200px');
            }
        });

        this._widget.find('[data-column="LayoutViewName"]').change(videre.createDelegate(this, this._onLayoutChanged));
        this._widget.find('[data-column="LayoutId"]').change(videre.createDelegate(this, this._onLayoutChanged));
        this.refreshTemplates();
    },

    refreshTemplates: function()
    {
        this.ajax(this._getControllerPath() + 'Get', {}, this._delegates.onTemplateReturn);
    },

    bindTree: function()
    {
        this._tree.dynatree("getRoot").removeChildren();
        this._tree.dynatree("getRoot").append(this._treeData);
        this._tree.find('.dynatree-container').height(this._tree.innerHeight() - 8).css('margin', 0);   //todo: better way?

        if (this._selectedKey != null)
        {
            var node = (this._tree.dynatree('getTree').activateKey(this._selectedKey));
            if (node == null)
            {
                this._templateData = null;
                this.bindTemplates(this._selectedKey);
            }
        }
    },

    bindTemplates: function(key)
    {
        var items = this._data.where(function(d) { return d.treeKey.indexOf(key) == 0; }).orderBy(function(d) { return d.Title != null ? d.Title : d.Layout; });  
        if (items.length > 0)
        {
            this._templateDataDict = items.toDictionary(function(d) { return d.Id; });

            this.showList();
            videre.dataTables.clear(this.getControl('ItemTable'));
            this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(items));
            this.getControl('ItemList').find('.btn').click(this._delegates.onActionClicked);
            videre.dataTables.bind(this.getControl('ItemTable'), { aoColumns: [{ bSortable: false}] });
        }
        else
            this._templateCtr.hide();
    },

    bindPanes: function(layoutViewName, layoutId)
    {
        var html = '';
        if (String.isNullOrEmpty(layoutViewName))   //if pagetemplate, we only have layoutId
            layoutViewName = this._layoutTemplateDataDict[layoutId].LayoutViewName;
        var layout = this._layoutDataDict[layoutViewName];
        if (layout != null)
        {
            this.bindAttributes(layout);

            for (var row = 0; row < layout.Panes.length; row++)
            {
                html += '<div class="row">';
                for (var cell = 0; cell < layout.Panes[row].length; cell++)
                {
                    var p = layout.Panes[row][cell];
                    html += String.format('<div data-title="{0}" data-pane="{0}" class="pane {2}" style="border: solid 1px; {1}"></div>', p.Name, p.DesignerStyle != null ? p.DesignerStyle : 'min-height: 100px;',
                        p.DesignerCss != null ? p.DesignerCss : 'col-md-12');
                }
                html += '</div>';
            }
        }

        this.getControl('Panes').html(html);
        this.getControl('Panes').find('.pane').sortable({
            connectWith: '.pane',
            cursor: 'move',
            placeholder: 'ui-sortable-placeholder',
            stop: videre.createDelegate(this, this._onWidgetPlaced)
        });

        this.getControl('Panes').find('.pane').tooltip({ placement: 'right' });

        this.bindWidgets(layoutViewName);
    },

    bindWidgets: function(layoutViewName)
    {
        this._widgets = {};
        if (this._selectedTemplate != null)
        {
            for (var i = 0; i < this._selectedTemplate.Widgets.length; i++)
            {
                var widget = this._selectedTemplate.Widgets[i];

                //if (this._layoutDataDict[layoutViewName].Panes.where(function(p) { return p == widget.PaneName; }).length == 0)
                //    widget.PaneName = this._layoutDataDict[layoutViewName].Panes[0];

                this._createWidget(widget.ManifestId, widget.PaneName, widget);

                widget.ContentJson = this._contentData[widget.Id];
                if (widget.ContentJson != null)
                    widget.Content = videre.deserialize(widget.ContentJson);
            }
        }

    },

    bindAttributes: function(layout)
    {
        var showAttributes = this._isLayout && layout.AttributeDefinitions != null && layout.AttributeDefinitions.length > 0;
        this.findControl('li>a[href="#' + this.getId('LayoutAttributeTab') + '"]').toggle(showAttributes);
        if (showAttributes)
        {
            var template = this._selectedTemplate;
            var ctr = this.getControl('LayoutAttributeList');
            ctr.html(this.getControl('LayoutAttributeListTemplate').render(layout.AttributeDefinitions.orderBy(function(d) { return d.Label; }), { attributes: template.Attributes }));
            videre.UI.initializeControlTypes(ctr);
        }
    },

    newTemplate: function()
    {
        this._selectedTemplate = this._isLayout ?
            { Id: '', PortalId: '', LayoutName: '', LayoutViewName: '', Widgets: []} :
            { Id: '', PortalId: '', Title: '', LayoutId: '', Urls: [], Widgets: [] };
        //this.editTemplate();
        this._contentData = {};
        this.showEditTemplate();

    },

    editTemplate: function()
    {
        if (this._selectedTemplate != null)
        {
            this.ajax(this._getControllerPath() + 'GetWidgetContent', { templateId: this._selectedTemplate.Id }, this._delegates.onWidgetContentReturn, null, this._templateEdit);
        }
        else
            this.showList();
    },

    //todo: rename!
    showEditTemplate: function()
    {
        this.bindData(this._selectedTemplate, this.getControl('GeneralTab'));
        this.bindPanes(this._widget.find('[data-column="LayoutViewName"]').val(), this._widget.find('[data-column="LayoutId"]').val());
        this.showEdit();
    },

    saveTemplate: function()
    {
        if (this._selectedTemplate != null)
        {
            var t = this.persistData(this._selectedTemplate);

            //t.Roles = String.isNullOrEmpty(t.Roles) ? [] : t.Roles.split(',');   //todo: hack?
            this.ajax(this._getControllerPath() + 'Save', { template: t }, this._delegates.onTemplateSaveReturn, null, this._templateEdit);
        }
    },

    persistData: function(data, clone, parent)
    {
        var tempData = this._base(data, true, this.getControl('GeneralTab'));  //call base class 
        //tempData.Urls = this.getControl('txtUrls').val().split('\n');
        tempData.Widgets = this._getWidgetData();
        //alert(videre.serialize(tempData));

        if (this._isLayout)
        {
            var layout = this._layoutDataDict[tempData.LayoutViewName];
            if (layout.AttributeDefinitions != null && layout.AttributeDefinitions.length > 0)
                this._base(tempData.Attributes, false, this.getControl('LayoutAttributeList'));
        }

        return tempData;
    },

    deleteTemplate: function(id)
    {
        if (confirm('Are you sure you wish to remove this entry?'))
            this.ajax(this._getControllerPath() + 'Delete', { id: id }, this._delegates.onTemplateSaveReturn, null, this._templateEdit);
    },

    handleAction: function(action, id)
    {
        this._selectedTemplate = this._templateDataDict[id];
        if (this._selectedTemplate != null)
        {
            if (action == 'edit')
                this.editTemplate();
            else if (action == 'delete')
                this.deleteTemplate(id);
        }
    },

    showList: function()
    {
        this._templateEdit.hide();
        this._templateCtr.show();

    },

    showEdit: function()
    {
        this.clearMsgs(this._templateEdit);
        this._templateEdit.show();
        this._templateCtr.hide();
    },

    removeWidget: function(widgetId)
    {
        if (confirm('Are you sure you wish to remove this widget?'))    //todo: localize
        {
            $('#' + widgetId).remove();
            delete this._widgets[widgetId];
        }
    },

    editWidget: function(widgetId)
    {
        var widget = this._widgets[widgetId];
        if (widget != null)
        {
            var manifest = this._manifestDataDict[widget.ManifestId];
            var editorType = manifest.EditorType != null ? manifest.EditorType : 'videre.widgets.editor.common';
            if (editorType != null)
            {
                if (this._editors[editorType] == null)
                {
                    this._editors[editorType] = videre.widgets.findFirstByType(editorType);
                    this._editors[editorType].add_onCustomEvent(this._delegates.onWidgetSave);
                }
                this._editors[editorType].show(widget, manifest);
            }
        }
    },

    _onNodeSelected: function(node)
    {
        this._selectedKey = node.data.key;
        this.bindTemplates(this._selectedKey);
    },

    _onTemplateReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._data = result.Data.select(this._isLayout ? function(t) { t.treeKey = t.LayoutName; return t; } : function(t) { t.treeKey = t.Urls.length > 0 ? t.Urls[0] : ''; return t; });
            this._treeData = videre.tree.getTreeData('Templates', this._data, function(d) { return d.treeKey; });
            this.bindTree();
        }
    },

    _onTemplateSaveReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            this.showList();
            this.refreshTemplates();
        }
    },

    _onWidgetContentReturn: function(result)
    {
        if (!result.HasError && result.Data)
        {
            //alert(videre.serialize(result.Data));
            this._contentData = result.Data;
            this.showEditTemplate();
        }
    },

    _onActionClicked: function(e)
    {
        var ctl = $(e.target).closest('[data-action]');
        this.handleAction(ctl.data('action'), ctl.data('id'));
    },

    _onNewClicked: function(e)
    {
        this.newTemplate();
    },

    _onSaveClicked: function(e)
    {
        this.saveTemplate();
    },

    _onCancelClicked: function(e)
    {
        this.showList();
    },

    _onWidgetPlaced: function(e, ui)
    {
        var manifestCtr = ui.item;
        var paneName = manifestCtr.parents('[data-pane]').data('pane');
        var widget = this._widgets[manifestCtr.attr('id')];
        if (widget == null)
        {
            widget = this._createWidget(manifestCtr.data('manifestid'), paneName, null, manifestCtr);
            manifestCtr.remove();
        }
        widget.PaneName = paneName;
    },

    _onWidgetActionClicked: function(e)
    {
        var mnu = $(e.target).closest('[data-action]');
        //var mnu = e.target.tagName == 'A' ? $(e.target) : $(e.target).parents('a');
        var action = mnu.data('action');
        var widgetCtr = mnu.parents('.widget-container');
        var widgetId = widgetCtr.attr('id');
        widgetCtr.find('.dropdown-toggle').dropdown('toggle');  //hide menu

        //alert(action + ' ' + widgetId);
        if (action == 'remove')
            this.removeWidget(widgetId);
        if (action == 'edit')
            this.editWidget(widgetId);
    },

    _onWidgetSave: function(e, args)
    {
        if (args.type == 'Save')
        {
            args.data.ContentJson = videre.serialize(args.data.Content);
            args.src.hide();
        }
    },

    _getWidgetData: function()
    {
        var data = [];
        var sortedIds = [];
        this.getControl('Panes').find('.pane').each(function(idx, element)
        {
            sortedIds.addRange($(element).sortable('toArray'));
        });
        var paneSeq = {};
        for (var i = 0; i < sortedIds.length; i++)
        {
            var widget = this._widgets[sortedIds[i]];
            if (widget != null)
            {
                if (paneSeq[widget.PaneName] == null)
                    paneSeq[widget.PaneName] = 0;
                paneSeq[widget.PaneName] += 1;
                widget.Seq = paneSeq[widget.PaneName];
                data.push(widget);
            }
        }

        return data;
    },

    _nextId: function()
    {
        this._genId += 1;
        return 'genID_' + this._genId;
    },

    _createWidget: function(manifestId, pane, widget, insertAfter)
    {
        if (widget == null)
            widget = { ManifestId: manifestId, PaneName: pane, Seq: 0, Css: '', Style: '', Authenticated: '', RoleIds: [], Attributes: {}, ContentJson: null };

        widget.GenId = this._nextId(); //client-side only Id

        var ctr = this._createWidgetContainer(manifestId, widget.GenId); //.data('widget', widget);
        this._widgets[widget.GenId] = widget;
        ctr.find('[data-action]').click(this._delegates.onWidgetActionClicked);

        if (insertAfter != null)
            insertAfter.after(ctr);
        else
            this.getControl('Panes').find('[data-pane="' + pane + '"]').append(ctr);


        var p = ctr.find('.hide-overflow').parent().width(ctr.parent().width() - 65);

        return widget;
    },

    _createWidgetContainer: function(manifestId, genId)
    {
        var manifest = this._manifestDataDict[manifestId];
        var name = manifest.Title;
        return $(String.format(
            '<div class="navbar navbar-default widget-container" id="{0}" style="min-height: 20px;">' +
                '<div class="navbar-collapse collapse">' +
                        '<ul class="nav navbar-nav" style="margin-left: -5px; margin-right: -5px;"><li><a style="padding: 4px 0 4px 0;" title="{1}"><span class="glyphicon glyphicon-cog"></span> {1}</a></li></ul>' +
                        '<ul class="nav navbar-nav navbar-right">' +
                            '<li class="dropdown">' +
                                '<a class="dropdown-toggle" data-toggle="dropdown" style="padding-top: 4px; padding-bottom: 4px;" ><b class="caret"></b></a>' +
                                '<ul class="dropdown-menu">' +
                                    '<li><a data-action="edit"><span class="glyphicon glyphicon-pencil"></span> Edit</a></li>' +
                                    '<li><a data-action="remove"><span class="glyphicon glyphicon-trash"></span> Remove</a></li>' +
                                '</ul>' +
                            '</li>' +
                        '</ul>' +
                    '</div>' +
                '</div>' +
            '</div>', genId, name));
    },

    _getControllerPath: function()
    {
        return this._isLayout ? '~/core/LayoutTemplate/' : '~/core/PageTemplate/';
    },

    _onLayoutChanged: function()
    {
        this.bindPanes(this._widget.find('[data-column="LayoutViewName"]').val(), this._widget.find('[data-column="LayoutId"]').val());
    }

});

