videre.registerNamespace('videre.widgets');
videre.registerNamespace('videre.widgets.admin');

videre.widgets.admin.packageexportjob = videre.widgets.base.extend(
{
    get_newJob: function() { return this._newJob; },
    set_newJob: function(v) { this._newJob = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method

        this._newJob = null;

        this._packageDialog = null;

        this._itemData = null;
        this._itemDataDict = null;
        this._jobData = null;
        this._jobDataDict = null;
        this._selectedJob = null;

        this._delegates = {
            onJobsDataReturn: videre.createDelegate(this, this._onJobsDataReturn),
            onSaveJobReturn: videre.createDelegate(this, this._onSaveJobReturn),
            onContentItemsReturn: videre.createDelegate(this, this._onContentItemsReturn)
        };
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base

        this._packageDialog = this.getControl('PackageDialog').modal('hide');
        this.getControl('ExportTypeList').find('[data-filter]').click(videre.createDelegate(this, this._onFilterClicked));
        this._widget.find('.action-container,.action-button').click(videre.createDelegate(this, this._onActionClicked));

        this.refreshJobs();
    },

    refreshJobs: function()
    {
        this.getControl('JobsContainer').show();
        this.getControl('EditJobContainer').hide();
        this.ajax('~/core/Package/GetExportJobs', {}, this._delegates.onJobsDataReturn);
    },

    newJob: function()
    {
        this.showJob(videre.jsonClone(this._newJob));
    },

    resetEditJob: function()
    {
        this.getControl('ItemListCtr').hide();
        this.getControl('ExportTypeList').find('a').removeClass('active');
    },

    editJob: function(job)
    {
        if (job != null)
            this.showJob(job);
    },

    deleteJob: function(job)
    {
        var self = this;
        videre.UI.confirm('Delete Entry', 'Are you sure you wish to remove this entry?', function()
        {
            self.ajax('~/core/Package/DeleteExportJob', { id: job.Id }, self._delegates.onSaveJobReturn);
        });
    },

    showJob: function(job)
    {
        this.getControl('JobsContainer').hide();
        this.getControl('EditJobContainer').show();
        this.resetEditJob();
        this._selectedJob = job;
        if (this._selectedJob.Package == null)
            this._selectedJob.Package = {};

        this.bindPackage();
        this.clearMsgs();
        this.clearMsgs(this._packageDialog);
    },

    addItemToJob: function(item)
    {
        this._selectedJob.Content.push(item);
        this.bindItems();
    },

    removeContentItemFromJob: function(id)
    {
        for (var i = 0; i < this._selectedJob.Content.length; i++)
        {
            if (this._selectedJob.Content[i].Id == id)
            {
                this._selectedJob.Content.remove(i);
                break;
            }
        }
        this._bindContentItems();
    },

    saveJob: function()
    {
        this.ajax('~/core/Package/SaveExportJob', { job: this._selectedJob }, this._delegates.onSaveJobReturn);
    },

    showPackageDialog: function()
    {
        this._packageDialog.modal('show');
        this._bindPackage();
    },

    bindPackage: function()
    {
        this.getControl('PackageCtr').find('[data-action="download-package"]').toggle(!String.isNullOrEmpty(this._selectedJob.Package.Name));
        this.bindData(this._selectedJob.Package, this.getControl('PackageCtr'));
    },

    getContent: function()
    {
        this.ajax('~/core/Package/GetExportContentItems', { type: this._selectedType, exportPackage: null }, this._delegates.onContentItemsReturn);
    },

    downloadPackage: function()
    {
        videre.download('~/core/Package/DownloadJobPackage', { jobData: this._selectedJob });   //passed as string
    },

    publishPackage: function(id)
    {
        var job = !String.isNullOrEmpty(id) ? this._jobsDict[id] : this._selectedJob;
        if (job != null)
            this.ajax('~/core/Package/PublishJobPackage', { job: job }, this._delegates.onSaveJobReturn);
    },

    bindJobs: function()
    {
        videre.dataTables.clear(this.getControl('JobsTable'));
        this.getControl('JobsList').html(this.getControl('JobsTemplate').render(this._jobsData));
        videre.dataTables.bind(this.getControl('JobsTable'), { columnDefs: [{ orderable: false }] });
    },

    bindItems: function()
    {
        this.getControl('ItemListCtr').show();
        videre.dataTables.clear(this.getControl('ItemTable'));
        var contentDict = this._selectedJob.Content.toDictionary(function(d) { return d.Type + '-' + d.Id; });
        this.getControl('ItemList').html(this.getControl('ItemListTemplate').render(this._itemData, { existing: contentDict }));
        videre.dataTables.bind(this.getControl('ItemTable'), { columnDefs: [{ orderable: false }] });   //width???
    },

    _handleAction: function(action, id)
    {
        if (action == 'add')
        {
            if (this._itemDataDict[id] != null)
                this.addItemToJob(this._itemDataDict[id]);
        }
        else if (action == 'edit-package')
            this.showPackageDialog();
        else if (action == 'remove-content')
            this.removeContentItemFromJob(id);
        else if (action == 'edit-job')
            this.editJob(this._jobsDataDict[id]);
        else if (action == 'delete-job')
            this.deleteJob(this._jobsDataDict[id]);
        else if (action == 'download-package')
            this.downloadPackage();
        else if (action == 'download-job')
        {
            if (this._jobsDataDict[id] != null)
            {
                this._selectedJob = this._jobsDataDict[id];
                this.downloadPackage();
            }
        }
        else if (action == 'publish-package')
            this.publishPackage();
        else if (action == 'save-job')
            this.saveJob();
        else if (action == 'cancel-job')
            this.refreshJobs();
        else if (action == 'new-job')
            this.newJob();
        else if (action == 'package-ok')
        {
            if (this._persistPackage())
            {
                this._packageDialog.modal('hide');
                this.bindPackage();
            }
        }

        this.bindPackage();
    },

    _bindPackage: function()
    {
        this.bindData(this._selectedJob.Package, this._packageDialog);
        this._bindContentItems();
    },

    _bindContentItems: function()
    {
        this.getControl('ContentItemList').html(this.getControl('ContentItemListTemplate').render(this._selectedJob.Content));
    },

    _persistPackage: function()
    {
        if (this.validControls(this._packageDialog, this._packageDialog))
        {
            this.persistData(this._selectedJob.Package, false, this._packageDialog);
            return true;
        }
        return false;
    },

    _onFilterClicked: function(e)
    {
        var ctl = $(e.target).closest('[data-filter]'); 
        this.getControl('ExportTypeList').find('a').removeClass('active');
        ctl.addClass('active');

        this._selectedType = ctl.data('filter');
        this.getContent();
    },

    _onActionClicked: function(e)
    {
        var ctl = $(e.target).closest('[data-action]');
        if (ctl.data('action') != null)
            this._handleAction(ctl.data('action'), ctl.data('id'));
    },

    _onContentItemsReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._itemData = result.Data;
            this._itemDataDict = this._itemData.toDictionary(function(d) { return d.Id; });
            this.bindItems();
        }
    },

    _onJobsDataReturn: function(result, ctx)
    {
        if (!result.HasError)
        {
            this._jobsData = result.Data;
            this._jobsDataDict = this._jobsData.toDictionary(function(d) { return d.Id; });
            this.bindJobs();
        }
    },

    _onSaveJobReturn: function(result, ctx)
    {
        this.refreshJobs();
    }

});

