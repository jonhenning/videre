videre.registerNamespace('videre.controls');
videre.registerNamespace('videre.controls.texteditor');

videre.controls.chart = videre.widgets.base.extend(
{
    get_seriesType: function() { return this._seriesType; },
    set_seriesType: function(v) { this._seriesType = v; },
    get_title: function() { return this._title; },
    set_title: function(v) { this._title = v; },
    get_subTitle: function() { return this._subTitle; },
    set_subTitle: function(v) { this._subTitle = v; },
    get_yAxis: function() { return this._yAxis; },
    set_yAxis: function(v) { this._yAxis = v; },
    get_xAxis: function() { return this._xAxis; },
    set_xAxis: function(v) { this._xAxis = v; },
    get_seriesData: function() { return this._seriesData; },
    set_seriesData: function(v) { this._seriesData = v; },
    get_legend: function() { return this._legend; },
    set_legend: function(v) { this._legend = v; },
    get_toolTip: function() { return this._toolTip; },
    set_toolTip: function(v) { this._toolTip = v; },

    //constructor
    init: function()
    {
        this._base();  //call base method
        this._chart = null;
        this._seriesType = 'line';
        this._title = '';
        this._subTitle = '';
        this._xAxis = [];   //['Jan 11', 'Feb 11', 'Mar 11', 'Apr 11', 'May 11', 'Jun 11', 'Jul 11', 'Aug 11', 'Sep 11', 'Oct 11', 'Nov 11', 'Dec 11']
        this._yAxis = null;
        this._seriesData = [];  //[{ name: 'Value', data: [1400, 1450, 1500, 1550, 1575, 1700, 1525, 1945, 1236]}]
        this._legend = null;
        this._toolTip = null;

        this._delegates = {
        }
    },

    _onLoad: function(src, args)
    {
        this._base(); //call base

        var options = {
            credits: { enabled: false },
            chart: {
                renderTo: this._id,
                defaultSeriesType: this._seriesType
            },
            title: this._title,
            subtitle: { text: this._subTitle },
            xAxis: {
                categories: this._xAxis
            },
            tooltip: {
                enabled: true,
                formatter: function()
                {
                    return '<b>' + this.series.name + '</b><br/>' +
        								        (this.key != null ? (this.key + '<br/>') : '') +
                                                (this.x != null ? (this.x + ': ') : '') + this.y;
                }
            },
            plotOptions: {
                line: {
                    dataLabels: { enabled: false },
                    enableMouseTracking: true
                }
            },
            series: this._seriesData
        };
        if (this._legend != null)
            options.legend = this._legend;

        if (this._yAxis != null)
        {
            if (this._yAxis.labels != null && this._yAxis.labels.format != null)
            {
                var format = this._yAxis.labels.format;
                this._yAxis.labels.formatter = function() { return String.format(format, this.value); };
            }
            options.yAxis = this._yAxis;
        }

        if (this._toolTip != null)
        {
            if (this._toolTip.format != null)
            {
                var fmt = this._toolTip.format;
                this._toolTip.formatter = function() { return fmt.replace(/{series.name}/g, this.series.name).replace(/{key}/g, this.key).replace(/{x}/g, this.x).replace(/{y}/g, this.y); };
            }
            options.tooltip = this._toolTip;
        }

        this._chart = new Highcharts.Chart(options);
        //this.refresh(this._seriesData);

    },

    refresh: function(data)
    {
        //this._chart.
    }


});

