var videre =
{
    enableLogging: false,
    _timings: {},
    _lastLog: new Date(),
    _timers: {},
    _csrfToken: null,
    registerNamespace: function(namespacePath)
    {
        var rootObject = window;
        var namespaceParts = namespacePath.split('.');

        for (var i = 0, l = namespaceParts.length; i < l; i++)
        {
            var currentPart = namespaceParts[i];
            var ns = rootObject[currentPart];
            if (!ns)
                ns = rootObject[currentPart] = {};
            rootObject = ns;
        }
    },

    createDelegate: function(instance, method)
    {
        return function() { return method.apply(instance, arguments); };
    },

    serialize: function(s, replacer, space)
    {
        return JSON.stringify(s, replacer, space);
    },

    deserialize: function(data)
    {
        //return JSON.parse(data);
        return $.parseJSON(data);   //have jquery attempt to use browser's native implementation
    },

    parseDate: function(value, format, zone)
    {
        var d = moment.parseZone(value);

        if (!d.isValid())
        {
            //allow dates to be entered without separators
            var lformat = moment.localeData().longDateFormat('L');
            var separator = lformat.indexOf('/') > -1 ? '/' : '-';
            var parts = lformat.split(separator);
            if (parts.length > 0 && value.length == parts.join('').length)  //only apply this if exact match...
            {
                var newVal = '';
                var last = 0;
                $.each(parts, function(idx, part)
                {
                    newVal += (idx > 0 ? separator : '') + value.substring(last, last + part.length);
                    last += part.length;
                });
                if (moment(newVal, lformat).isValid())
                    d = moment.parseZone(newVal, lformat);
            }
        }

        if (zone)
        {
            var offset = videre.timeZones.getOffset(zone, d);
            if (offset != null)
            {
                var zoneOffset = offset.Format;
                if (zoneOffset.indexOf('-') != 0 && zoneOffset.indexOf('+') != 0)
                    zoneOffset = '+' + zoneOffset;
                d = d.zone(zoneOffset);
            }
        }

        if (format)
            return d.format(format);
        return d;
    },

    jsonClone: function(data)
    {
        return videre.deserialize(videre.serialize(data));
    },

    //http://joncom.be/code/realtypeof/
    typename: function(v)
    {
        if (typeof (v) == 'object')
        {
            if (v === null) return 'null';
            if (v.constructor == (new Array).constructor) return 'array';
            if (v.constructor == (new Date).constructor) return 'date';
            if (v.constructor == (new RegExp).constructor) return 'regex';
            return 'object';
        }
        return typeof (v);
    },

    log: function(msg)
    {
        if (videre.enableLogging && window.console)
        {
            if (new Date() - videre._lastLog > 1000)
                console.log('----------------------------------------------');
            videre._lastLog = new Date();
            console.log(new Date().getTime() + ' ' + msg);
        }
    },

    cleanUp: function()
    {

        for (var s in videre.widgets.controls)
        {
            var component = videre.widgets.controls[s];
            if (component instanceof videre.widgets.base)
            {
                for (var id in component._controls)
                {
                    var ctl = component._controls[id];
                    ctl._delegates = null;
                    ctl._baseDelegates = null;
                    //ctl.unbind();
                    ctl.removeData();
                    //ctl.remove(); //DO NOT DO THIS!
                    //if (ctl.length > 0)
                    //    ctl[0].parentNode.removeChild(ctl[0]);
                }
                component._controls = null;
            }
        }
        //jQuery('*').unbind();
    },

    rootUrl: function() //todo: hacky!
    {
        if (window.ROOT_URL != null)
            return window.ROOT_URL;
        return '';
    },

    resolveUrl: function(url)
    {
        if (url.indexOf('~/') == 0)
            url = url.replace('~/', videre.rootUrl());
        return url;
    },

    getQueryParam: function(name)   //http://stackoverflow.com/questions/901115/how-can-i-get-query-string-values-in-javascript
    {
        var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
        return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
    },

    ajax: function(url, data, success, error, ctx, options)
    {
        options = options || {};
        if (videre._csrfToken != null)
        {
            if (options.headers == null)
                options.headers = {};
            options.headers.RequestVerificationToken = videre._csrfToken;
        }
        return $.ajax({
            type: options.type || "POST",
            url: videre.resolveUrl(url),
            headers: options.headers,
            processData: false,
            async: options.async == null ? true : options.async,
            data: videre.serialize(data),
            contentType: options.contentType || 'application/json; charset=utf-8',
            success: function(result) { if (success != null) success(result, ctx); },
            error: function(request) { if (error != null) error(request, ctx); }
        });
    },

    download: function(url, data, method)
    {
        var form = $('<form action="' + videre.resolveUrl(url) + '" method="' + (method || 'post') + '"></form>');
        for (var key in data)
            form.append($('<input type="hidden" />').attr('name', key).val(videre.serialize(data[key])));
        //send request
        form.appendTo('body').submit().remove();
    },

    timer: function(key, func, interval, ctx)
    {
        clearTimeout(videre._timers[key]);
        videre._timers[key] = setTimeout(function() { func(ctx); }, interval);
    }
};

videre.registerNamespace('videre.UI');

videre.UI = {

    _nextId: 0,
    getNextId: function()
    {
        videre.UI._nextId++;
        return '_ui' + videre.UI._nextId;
    },

    //allows focus to be set on first data-column
    showModal: function(ctr)
    {
        ctr.modal('show').on('shown.bs.modal', function() { $('[data-column]:first', this).focus(); });
    },

    hideModal: function(ctr)
    {
        ctr.modal('hide');
    },

    handleEnter: function(ctl, func)
    {
        ctl.keypress(function(e)
        {
            if (e.keyCode == 13)
            {
                func();
                return false;
            }
        });
    },

    bindDropdown: function(ddl, data, valCol, textCol, blankText, selectedVal)
    {
        ddl[0].options.length = 0;
        if (blankText != null)
            ddl.append($('<option></option>').val('').html(blankText));
        $.each(data, function(idx, item)
        {
            ddl.append($('<option></option>').val(this[valCol]).html(this[textCol]));
        });
        if (selectedVal != null)
            ddl.val(selectedVal);
        return ddl;
    },

    confirm: function(title, text, onConfirm)
    {
        videre.UI.prompt(videre.UI.getNextId(), title, text, null,
            [{
                text: 'Ok', css: 'btn-primary', close: true, handler: function()
                {
                    onConfirm();
                    return true;
                }
            }, { text: 'Cancel', css: 'btn-default', close: true }]);
    },

    alert: function(id, title, text, icon, style)
    {
        if (style == null)
            style = '';
        //todo:  hardcoded styles?
        videre.UI.prompt(id, title, String.format('<div style="{0}"><div class="{1}" style="font-size: 32px; margin: 10px; vertical-align: middle; display: inline-block;"></div> {2}</div>', style, icon, text), [], [{ text: 'Ok', close: true }]);
    },

    //usage:
    //videre.UI.prompt('test', 'My Title', null,
    //    [
    //        { label: 'Favorite Color?', dataColumn: 'color', css: 'span3', value: 'blue' },
    //        { label: 'Favorite Cola?', dataColumn: 'beverage', css: 'span2', value: 'Coke' }
    //    ],
    //    [
    //        { text: 'Ok', icon: 'icon-pencil', css: 'btn-primary', handler: function(data) { return confirm(videre.serialize(data)); }, close: true },
    //        { text: 'Cancel', close: true }
    //    ]).addClass('videre-compact');
    prompt: function(id, title, description, inputs, buttons)
    {
        description = description || '';
        inputs = inputs || [];
        var dlg = $(String.format('<div id="{0}" data-target="{0}" class="modal fade" style="display: none;"><div class="modal-dialog"><div class="modal-content"><div class="modal-header"><button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>{1}</div><div class="modal-body">{2}<div class="form-horizontal"></div></div><div class="modal-footer"></div></div></div></div>',
            id, title, description)).appendTo($("body")).modal('show').on('hidden.bs.modal', function() { dlg.remove(); });

        var body = dlg.find('.form-horizontal');
        var footer = dlg.find('.modal-footer');
        $.each(inputs, function(idx, input)
        {
            $(String.format('<div class="form-group"><label class="control-label col-md-4">{0}</label><div class="col-md-8"><input type="text" data-column="{1}" class="{2} form-control" /></div></div>',
                input.label, input.dataColumn, input.css)).appendTo(body).find('[data-column]').val(input.value);
        });
        $.each(buttons, function(idx, btn)
        {
            $(String.format('<a class="btn btn-default {0}">{1}{2}</a>', btn.css, btn.icon != null ? String.format('<i class="{0}"></i> ', btn.icon) : '', btn.text))
                .click(function(e)
                {
                    var data = {};
                    $.each(body.find('[data-column]'), function(idx, input)
                    {
                        var ctl = $(input);
                        data[ctl.data('column')] = ctl.val();
                    });
                    if ((btn.handler == null || btn.handler(data)) && btn.close)
                        dlg.modal('hide');
                }).appendTo(footer);
        });
        return dlg;
    },

    registerDataType: function(name, definition)
    {
        videre.UI._dataTypes[name] = definition;
    },


    //control types allow developer to register jquery plugins
    //by registering one, the bind action, the retrieval of the value, 
    //and the initialization of the code for system (template) generated will run
    registerControlType: function(name, definition)
    {
        videre.UI._controlTypes[name] = definition;
        if (definition.init != null) //on registration call init for whole doc
            definition.init($(document));
    },

    initializeControlTypes: function(ctr)
    {
        for (var name in videre.UI._controlTypes)
        {
            if (videre.UI._controlTypes[name].init != null)
            {
                videre.UI._controlTypes[name].init(ctr);
                //ctr.attr('data-controltype', name);
            }
        }
    },

    bindData: function(data, parent)
    {
        var ctls = parent.find('[data-column]');
        ctls.each(function(idx, element)
        {
            var ctl = $(element);
            var val = Object.deepGet(data, ctl.data('column'));
            var controlType = videre.UI._controlTypes[ctl.data('controltype')];
            if (controlType != null && controlType.set != null)
                controlType.set(ctl, val, data);    //
            else
            {
                //var dataType = videre.UI._dataTypes[ctl.data('datatype')];
                //if (dataType != null && dataType.set != null)
                //    dataType.set(ctl, val);
                //else
                videre.UI.setControlValue(ctl, val, data);
            }
        });
    },

    persistData: function(data, clone, parent, includeReadOnly)
    {
        var cloneData = clone ? videre.jsonClone(data) : data;
        var ctls = parent.find('[data-column]');

        ctls.each(function(idx, element)
        {
            var ctl = $(element);
            if (!ctl.attr('readonly') || includeReadOnly)
            {
                var col = ctl.attr('data-column');
                if (!String.isNullOrEmpty(col))
                    Object.deepSet(cloneData, col, videre.UI.getControlValue(ctl));
            }
        });
        return cloneData;
    },

    getFormGroup: function(ctl)
    {
        var item = { ctl: ctl };
        item.group = item.ctl.closest('.form-group');
        item.lbl = ctl.closest('[for="' + item.ctl.attr('id') + '"]');
        if (item.lbl.length == 0)
            item.lbl = item.group.find('label');
        item.labelText = item.lbl.html();
        if (item.labelText == null)
            item.labelText = item.ctl.data('label-text');
        return item;
    },

    getControlValue: function(ctl)
    {
        var controlType = videre.UI._controlTypes[ctl.data('controltype')];
        if (controlType != null && controlType.get != null)
            return controlType.get(ctl);
        else
        {
            var dataType = this._dataTypes[ctl.data('datatype')];
            if (dataType != null && dataType.get != null)
                return dataType.get(ctl.val(), ctl.data());
            else
            {
                //var tagName = ctl.prop('tagName').toLowerCase();
                //if (tagName == 'label' || tagName == 'span' || tagName == 'div' || tagName == 'p')  //todo:  better way to detect to set html or val?
                if (!ctl.is(':input'))
                    return ctl.text();
                else
                {
                    var type = (ctl.prop('type') || '').toLowerCase();
                    if (type == 'checkbox' || type == 'radio')
                        return ctl.prop('checked');
                    else
                        return ctl.val();
                }
            }
        }
    },

    setControlValue: function(ctl, val, data)
    {
        var dataType = videre.UI._dataTypes[ctl.data('datatype')];
        if (dataType != null && dataType.get != null && ctl.data('datatype') != 'datetime') //cannot lose timezone (or reset it).  
            val = dataType.get(val, ctl.data());

        var controlType = videre.UI._controlTypes[ctl.data('controltype')];
        if (controlType != null && controlType.set != null)
            controlType.set(ctl, val, data);
        else
        {
            val = val != null ? val.toString() : '';
            if (!ctl.is(':input'))
                ctl.text(val);
            else
            {
                var type = (ctl.prop('type') || '').toLowerCase();
                if (type == 'checkbox' || type == 'radio')
                    ctl.prop('checked', !String.isNullOrEmpty(val) && val.toLowerCase() != 'false');
                else
                    ctl.val(val);
            }
        }
    },

    setControlEnabled: function(ctl, enabled)
    {
        var controlType = videre.UI._controlTypes[ctl.data('controltype')];
        if (controlType != null && controlType.enable != null)
            controlType.enable(ctl, enabled);
        else
            ctl.attr("disabled", enabled ? null : "disabled");
    },

    validateCtl: function(item)
    {
        var uniqueId = !String.isNullOrEmpty(item.ctl.attr('id')) ? item.ctl.attr('id') : item.ctl.data('column');
        if (item.ctl.data('controltype') != null && !videre.UI.validateControlType(item.ctl.data('controltype'), item.ctl))
            return { id: uniqueId + 'ControlTypeInvalid', text: item.ctl.attr('data-custom-error') != null ? item.ctl.attr('data-custom-error') : String.format(videre.localization.getText('global', 'ControlTypeInvalid'), item.labelText, item.ctl.data('controltype')), isError: true };

        if (item.ctl.data('dependencymatch') == false)  //if dependent control and it is not matched (shown) it is valid!
            return null;
        if (item.ctl.attr('required') && String.isNullOrEmpty(videre.UI.getControlValue(item.ctl)))
            return { id: uniqueId + 'Required', text: String.format(videre.localization.getText('global', 'RequiredField'), item.labelText), isError: true };
        if (item.ctl.data('datatype') != null && !videre.UI.validDataType(item.ctl.data('datatype'), item.ctl.val(), item.ctl.data()))
            return { id: uniqueId + 'DataTypeInvalid', text: String.format(videre.localization.getText('global', 'DataTypeInvalid'), item.labelText, item.ctl.data('datatype')), isError: true };
        if (item.ctl.data('match') != null && item.ctl.val() != $('#' + item.ctl.data('match')).val())
            return { id: uniqueId + 'ValuesMustMatch', text: String.format(videre.localization.getText('global', 'ValuesMustMatch'), item.labelText), isError: true };
    },

    validDataType: function(type, val, options)
    {
        if (!String.isNullOrEmpty(val))
        {
            var item = this._dataTypes[type];
            if (item != null)
                return item.isValid != null ? item.isValid(val, options) : true; // always valid if not otherwise specified
            else
                alert('Invalid data type: ' + type); //todo: what to do?
        }
        return true;
    },

    validateControlType: function(type, ctl)
    {
        var item = videre.UI._controlTypes[type];
        if (item != null)
            return item.isValid != null ? item.isValid(ctl) : true; // always valid if not otherwise specified
        else
            alert('Invalid control type: ' + type); //todo: what to do?
        return true;
    },

    _controlTypes: {},

    _dataTypes:
    {
        'number':
        {
            isValid: function(val) { return !isNaN(val); }
        },

        'email':
        {
            isValid: function(val)
            {
                //http://ask.altervista.org/demo/jquery-validate-e-mail-address-regex/
                return new RegExp(/^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i)
                    .test(val);
            }
        },

        'datetime':
        {
            isValid: function(val, options)
            {
                var format = options != null && options.format != null ? options.format : videre.localization.dateFormats.datetime;
                return moment(val, format).isValid();
            },
            //set: function(ctl, val) //obsolete
            //{
            //    var text = val != null ? videre.parseDate(val, ctl.data('format') != null ? ctl.data('format') : videre.localization.dateFormats.datetime, ctl.data('timezone')) : '';
            //    videre.UI.setControlValue(ctl, text);
            //},
            get: function(val, options)
            {
                var format = options != null && options.format != null ? options.format : videre.localization.dateFormats.datetime;
                var zone = options != null && options.zone != null ? options.zone : videre.localization.dateFormats.zone;
                return val != null ? videre.parseDate(val, format, zone) : '';
            }
        },

        'time':
        {
            isValid: function(val) { return moment(val, videre.localization.dateFormats.time).isValid(); }
        },

        'date':
        {
            isValid: function(val, options)
            {
                var format = options != null && options.format != null ? options.format : videre.localization.dateFormats.date;
                return moment(val, format).isValid();
            },
            //set: function(ctl, val) //obsolete
            //{
            //    var text = val != null ? videre.parseDate(val, ctl.data('format') != null ? ctl.data('format') : videre.localization.dateFormats.date, ctl.data('timezone')) : '';
            //    videre.UI.setControlValue(ctl, text);
            //},
            get: function(val, options)
            {
                var format = options != null && options.format != null ? options.format : videre.localization.dateFormats.date;
                var zone = options != null && options.zone != null ? options.zone : videre.localization.dateFormats.zone;
                return val != null ? videre.parseDate(val, format, zone) : '';
            }
        },

        'money':
        {
            isValid: function(val) { return !isNaN(val); },
            //set: function(ctl, val) //obsolete
            //{
            //    var precision = ctl.data('precision') != null ? ctl.data('precision') : 2;
            //    var text = (val != null ? val : 0).toFixed(precision);
            //    videre.UI.setControlValue(ctl, text);
            //},
            get: function(val, options)
            {
                var precision = options != null && options.precision != null ? options.precision : videre.localization.numberFormat.CurrencyDecimalDigits;//ctl.data('precision') != null ? ctl.data('precision') : 2;
                return Number(val != null ? val : 0).toFixed(precision);
            }
        }

    }

};

videre.UI.registerControlType('list',
    {
        get: function(ctl)
        {
            var val = ctl.find('li');
            if (val.length > 0)
            {
                var ret = [];
                $.each(val, function(idx, item)
                {
                    ret.push($(item).text());
                });
                return ret;
            }
            return null;

        },
        set: function(ctl, val)
        {
            if (val != null)
                ctl.html('').append(val.select(function(i) { return $('<li></li>').html(i); }));
            else
                ctl.html('');
        },
        init: function(ctl) { }
    });

videre.UI.registerControlType('checkbox',
    {
        get: function(ctl) { return ctl.is(':checked'); },
        set: function(ctl, val)  //todo: this is a bit messy
        {
            if (videre.typename(val) == 'string')
                val = eval(val);

            ctl.attr('checked', val);   //if rendering html to markup, prop will not work
            ctl.prop('checked', val);
        },
        init: function(ctl) { }
    });


videre.UI.eventHandlerList = function()
{
    this._list = {};
};

videre.UI.eventHandlerList.prototype =
{
    addHandler: function(id, handler)
    {
        var event = this._getEvent(id, true);
        event.push(handler);
    },

    removeHandler: function(id, handler)
    {
        var evt = this._getEvent(id);
        if (!evt) return;
        for (var i = 0; i < evt.length; i++)
        {
            if (evt[i] === handler)
            {
                evt.splice(i, 1);
                return true;
            }
        }
        return false;
    },

    getHandler: function(id)
    {
        var evt;

        evt = this._getEvent(id);
        if (!evt || !evt.length) return null;
        //evt = Array.clone(evt);
        evt = evt.length === 1 ? [evt[0]] : Array.apply(null, evt);

        return function(source, args)
        {
            for (var i = 0, l = evt.length; i < l; i++)
            {
                evt[i](source, args);
            }
        };
    },

    _getEvent: function(id, create)
    {
        var e = this._list[id];
        if (!e)
        {
            if (!create) return null;
            this._list[id] = e = [];
        }
        return e;
    }
};

//JSON Parsing - See http://www.JSON.org/js.html
var JSON; if (!JSON) { JSON = {} } (function() { function f(n) { return n < 10 ? "0" + n : n } if (typeof Date.prototype.toJSON !== "function") { Date.prototype.toJSON = function(key) { return isFinite(this.valueOf()) ? this.getUTCFullYear() + "-" + f(this.getUTCMonth() + 1) + "-" + f(this.getUTCDate()) + "T" + f(this.getUTCHours()) + ":" + f(this.getUTCMinutes()) + ":" + f(this.getUTCSeconds()) + "Z" : null }; String.prototype.toJSON = Number.prototype.toJSON = Boolean.prototype.toJSON = function(key) { return this.valueOf() } } var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, escapable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, gap, indent, meta = { "\b": "\\b", "\t": "\\t", "\n": "\\n", "\f": "\\f", "\r": "\\r", '"': '\\"', "\\": "\\\\" }, rep; function quote(string) { escapable.lastIndex = 0; return escapable.test(string) ? '"' + string.replace(escapable, function(a) { var c = meta[a]; return typeof c === "string" ? c : "\\u" + ("0000" + a.charCodeAt(0).toString(16)).slice(-4) }) + '"' : '"' + string + '"' } function str(key, holder) { var i, k, v, length, mind = gap, partial, value = holder[key]; if (value && typeof value === "object" && typeof value.toJSON === "function") { value = value.toJSON(key) } if (typeof rep === "function") { value = rep.call(holder, key, value) } switch (typeof value) { case "string": return quote(value); case "number": return isFinite(value) ? String(value) : "null"; case "boolean": case "null": return String(value); case "object": if (!value) { return "null" } gap += indent; partial = []; if (Object.prototype.toString.apply(value) === "[object Array]") { length = value.length; for (i = 0; i < length; i += 1) { partial[i] = str(i, value) || "null" } v = partial.length === 0 ? "[]" : gap ? "[\n" + gap + partial.join(",\n" + gap) + "\n" + mind + "]" : "[" + partial.join(",") + "]"; gap = mind; return v } if (rep && typeof rep === "object") { length = rep.length; for (i = 0; i < length; i += 1) { if (typeof rep[i] === "string") { k = rep[i]; v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ": " : ":") + v) } } } } else { for (k in value) { if (Object.prototype.hasOwnProperty.call(value, k)) { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ": " : ":") + v) } } } } v = partial.length === 0 ? "{}" : gap ? "{\n" + gap + partial.join(",\n" + gap) + "\n" + mind + "}" : "{" + partial.join(",") + "}"; gap = mind; return v } } if (typeof JSON.stringify !== "function") { JSON.stringify = function(value, replacer, space) { var i; gap = ""; indent = ""; if (typeof space === "number") { for (i = 0; i < space; i += 1) { indent += " " } } else { if (typeof space === "string") { indent = space } } rep = replacer; if (replacer && typeof replacer !== "function" && (typeof replacer !== "object" || typeof replacer.length !== "number")) { throw new Error("JSON.stringify") } return str("", { "": value }) } } if (typeof JSON.parse !== "function") { JSON.parse = function(text, reviver) { var j; function walk(holder, key) { var k, v, value = holder[key]; if (value && typeof value === "object") { for (k in value) { if (Object.prototype.hasOwnProperty.call(value, k)) { v = walk(value, k); if (v !== undefined) { value[k] = v } else { delete value[k] } } } } return reviver.call(holder, key, value) } text = String(text); cx.lastIndex = 0; if (cx.test(text)) { text = text.replace(cx, function(a) { return "\\u" + ("0000" + a.charCodeAt(0).toString(16)).slice(-4) }) } if (/^[\],:{}\s]*$/.test(text.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, "@").replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, "]").replace(/(?:^|:|,)(?:\s*\[)+/g, ""))) { j = eval("(" + text + ")"); return typeof reviver === "function" ? walk({ "": j }, "") : j } throw new SyntaxError("JSON.parse") } } }());

//override date serialization - http://www.asp.net/ajaxlibrary/jquery_webforms_serialize_dates_to_json.ashx - .NET 3.5 REMOVES THE NEED FOR THIS!
//Date.prototype.toJSON = function (key) { return isFinite(this.valueOf()) ? '/Date(' + this.getTime() + ')/' : null };

//http://ejohn.org/blog/simple-javascript-inheritance
// Inspired by base2 and Prototype
(function()
{
    var initializing = false, fnTest = /xyz/.test(function() { xyz; }) ? /\b_base\b/ : /.*/;

    // The base Class implementation (does nothing)
    videre.Class = function() { };

    // Create a new Class that inherits from this class
    videre.Class.extend = function(prop)
    {
        var _base = this.prototype;

        // Instantiate a base class (but only create the instance,
        // don't run the init constructor)
        initializing = true;
        var prototype = new this();
        initializing = false;

        // Copy the properties over onto the new prototype
        for (var name in prop)
        {
            // Check if we're overwriting an existing function
            prototype[name] = typeof prop[name] == "function" &&
        typeof _base[name] == "function" && fnTest.test(prop[name]) ?
        (function(name, fn)
        {
            return function()
            {
                var tmp = this._base;

                // Add a new ._base() method that is the same method
                // but on the super-class
                this._base = _base[name];

                // The method only need to be bound temporarily, so we
                // remove it when we're done executing
                var ret = fn.apply(this, arguments);
                this._base = tmp;

                return ret;
            };
        })(name, prop[name]) :
        prop[name];
        }

        // The dummy class constructor
        function Class()
        {
            // All construction is actually done in the init method
            if (!initializing && this.init)
                this.init.apply(this, arguments);
        }

        // Populate our constructed prototype object
        Class.prototype = prototype;

        // Enforce the constructor to be what we expect
        Class.constructor = Class;

        // And make this class extendable
        Class.extend = arguments.callee;

        return Class;
    };
})();

videre.widgets = {
    registeredWidgets: [],

    register: function(id, type, properties)
    {
        var ctl = new type();
        properties['id'] = id;
        videre.widgets._setProperties(ctl, properties);
        videre.widgets.registeredWidgets[id] = ctl;
        return ctl;
    },

    find: function(id)
    {
        return videre.widgets.registeredWidgets[id];
    },

    findFirstByType: function(type)
    {
        return this.findByType(type).singleOrDefault();
    },

    findByType: function(type)
    {
        var t = eval(type);
        if (t != null)
            return videre.widgets.registeredWidgets.getValues().where(function(c) { return c instanceof t; });
        return [];
    },

    _setProperties: function(target, properties)
    {
        for (var name in properties)
        {
            var val = properties[name];
            var setter = target['set_' + name];
            if (typeof (setter) === 'function')
                setter.apply(target, [val]);
            else
            {
                setter = target['add_' + name]; //try for an event
                if (typeof (setter) === 'function')
                    setter.apply(target, [val]);
            }
        }
    }
};

videre.widgets.base = videre.Class.extend(
{
    get_id: function() { return this._id; },
    set_id: function(v) { this._id = v; },
    get_ns: function() { return this._ns; },
    set_ns: function(v) { this._ns = v; },
    get_mns: function() { return this._mns; },
    set_mns: function(v) { this._mns = v; },
    get_events: function() { return this._eventHandlerList; },
    get_childWidgets: function() { return this._childWidgets; },

    get_messages: function() { return this._messages; },
    set_messages: function(v) { this._messages = v; },
    set_user: function(v) { this._user = v; },
    get_user: function() { return this._user; },
    get_wid: function() { return this._wid; },
    set_wid: function(v) { this._wid = v; },

    init: function()
    {
        this._id = null;
        this._wid = null;
        this._ns = '';
        this._mns = '';
        this._controls = {};
        this._childWidgets = {};
        this._eventHandlerList = new videre.UI.eventHandlerList();

        $(document).ready(videre.createDelegate(this, function() { window.setTimeout(videre.createDelegate(this, this._onLoad), 0) })); //timeout for what again?  

        this._messages = [];
        this._user = {};
        this._locked = false;
        this._useProgressDialog = false;
        this._progressDialog = null;

        //controls
        this._widget = null;

        this._baseDelegates = {
            onAjaxSuccess: videre.createDelegate(this, this._onAjaxSuccess),
            onAjaxFail: videre.createDelegate(this, this._onAjaxFail)
        };
    },

    _onLoad: function(src, args)
    {
        //this._base();
        this._widget = this.getControl('Widget').keydown(videre.createDelegate(this, this._onWidgetKeyDown));
        this._widget.find('form').submit(function() { return false; }) //prevent submit
        $(document.body).keydown(function(e) { if (e.keyCode == 27) return false; }); //prevent escape clearing form
    },

    getControl: function(id, scope)
    {
        if (this._controls[id] == null)
        {
            if (scope == null)
                scope = document;
            scope = $(scope);
            var ctl = scope.find('#' + id);
            if (ctl.length == 0)
                ctl = scope.find('#' + this.getId(id));

            this._controls[id] = ctl;
        }
        return this._controls[id];
    },

    findControl: function(selector, scope)
    {
        if (scope == null)
            scope = this._widget;
        if (scope == null)
            scope = document;
        return scope.find(selector);
    },

    getId: function(id)
    {
        return this._ns + '_' + id;
    },

    registerControl: function(id, type, properties, listenOnError)
    {
        if (this.getControl(id).length == 0)
            id = this.getId(id);
        properties.ns = this._ns;
        var ctl = videre.widgets.register(id, type, properties);
        this._childWidgets[id] = ctl;

        if (ctl.add_onError != null && listenOnError != false)
            ctl.add_onError(videre.createDelegate(this, this._onError));
        return ctl;
    },

    getWidget: function(id)
    {
        return this._childWidgets[id];
    },

    ajax: function(url, params, onSuccess, onFail, parent, ctx, options)
    {
        this.clearMsgs(parent);
        this.lock(parent);

        var defaultOptions =
        {
            headers: { 'X-Videre-WidgetId': this._wid }
        };

        return videre.ajax(
            url,
            params,
            this._baseDelegates.onAjaxSuccess,
            this._baseDelegates.onAjaxFail,
            {
                onSuccess: onSuccess,
                onFail: onFail,
                parent: parent,
                ctx: ctx
            },
            $.extend(true, {}, defaultOptions, options));
    },

    bindData: function(data, parent)
    {
        if (data == null)
            data = {};
        if (parent == null)
            parent = this._widget;
        videre.UI.bindData(data, parent);
    },

    persistData: function(data, clone, parent, includeReadOnly)
    {
        if (clone == null)
            clone = true;
        if (parent == null)
            parent = this._widget;
        return videre.UI.persistData(data, clone, parent, includeReadOnly);
    },

    validControls: function(controlCtr, messageCtr)
    {
        controlCtr = controlCtr != null ? controlCtr : this._widget;
        messageCtr = messageCtr != null ? messageCtr : controlCtr;
        this.clearMsgs();
        var ctls = this._getValidationCtls(controlCtr);
        var errors = [];
        ctls.forEach(function(item)
        {
            var error = videre.UI.validateCtl(item);
            if (error != null)
            {
                item.group.addClass('has-error');
                item.group.attr('data-error-message', error.text);
                errors.push(error);
            }
            else
            {
                item.group.removeClass('has-error');
                item.group.attr('data-error-message', null);
            }
        });
        this.addMsgs(errors, messageCtr);
        return errors.length == 0;
    },

    isActivityAuthorized: function(area, name, user)
    {
        return this.getAuthorizedActivities(area, user).where(function(a) { return a.Name == name; }).length > 0;
    },

    getAuthorizedActivities: function(area, user)
    {
        if (user == null)
            user = this._user;
        return user.SecureActivities.where(function(a) { return (String.isNullOrEmpty(area) || a.Area == area); });
    },

    _onAjaxSuccess: function(result, ctx)
    {
        this.unlock(ctx.parent);
        if (!result.HasError)
        {
            if (ctx.onSuccess != null)
                ctx.onSuccess(result, ctx.ctx);
        }
        else if (ctx.onFail != null)
            ctx.onFail(result, ctx.ctx);
        this.addMsgs(result.Messages, ctx.parent);
    },

    lock: function(parent)
    {
        this._locked = true;
        if (this._useProgressDialog)
            this.getProgressBar(parent).modal('show');
        else
            this.getProgressBar(parent).show();
    },

    unlock: function(parent)
    {
        this._locked = false;
        if (this._useProgressDialog)
            this.getProgressBar(parent).modal('hide');
        else
            this.getProgressBar(parent).hide();
    },

    getProgressBar: function(parent)
    {
        if (parent == null)
            parent = this._widget;
        if (this._useProgressDialog)
        {
            if (this._progressDialog == null)
                this._progressDialog = parent.find('.progress-dialog').first().appendTo($(document.body)).modal().css('padding-top', '15%'); //only need one
            return this._progressDialog;
        }
        else
            return parent.find('.progress-inline');
    },

    getMsgCtr: function(parent)
    {
        if (parent == null)
            parent = this._widget;
        return parent.find('.alert-block:first');
    },

    addMsgs: function(msgs, parent)
    {
        for (var i = 0; i < msgs.length; i++)
            this.addMsg(msgs[i].id, msgs[i].text, msgs[i].isError, parent);
    },

    addMsg: function(id, text, isError, parent)
    {
        videre.log('addMsg: id=' + id + ' text=' + text);
        if (isError)
            this.clearInfoMsgs(); //todo: perf?
        var msg = this.getMsg(id);
        if (msg != null)
            msg.text = text;
        else
            this._messages.push({ id: id, text: text, isError: isError });
        this.refreshMsgs(parent);
    },

    getMsg: function(id)
    {
        return this._messages.where(function(m) { return m.id == id; }).singleOrDefault();
    },

    removeMsg: function(id, parent)
    {
        this._messages = $.grep(this._messages, function(o)
        {
            return o.id != id;
        });
        this.refreshMsgs(parent);
    },

    refreshMsgs: function(parent)
    {
        var msgCtr = this.getMsgCtr(parent);
        if (this._messages.length > 0)
        {
            var msg = this._messages[0];
            var text = this._messages.toList(function(d) { return d.text; }).join('<br/>');
            if (text.length > 0)
            {
                msgCtr.removeClass('alert-danger').removeClass('alert-info').addClass(msg.isError ? 'alert-danger' : 'alert-info');
                msgCtr.find('.alert-text').html(text);
                msgCtr.show();
            }
            else
                msgCtr.hide();
        }
        else
        {
            msgCtr.find('.alert-text').html('');
            msgCtr.hide();
        }
    },

    clearInfoMsgs: function()
    {
        this._messages = $.grep(this._messages, function(o)
        {
            return o.isError;
        });
    },

    clearMsgs: function(parent)
    {
        this._messages = [];
        this.refreshMsgs(parent);
        this._getValidationCtls(parent != null ? parent : this._widget).forEach(function(item) { item.group.removeClass('has-error'); });
    },

    getText: function(key, defaultValue)
    {
        return videre.localization.getText(this._mns, key, defaultValue);
    },

    //events
    _onWidgetKeyDown: function(e)
    {
    },

    _onAjaxFail: function(request, ctx)
    {
        this.unlock(ctx.parent);

        if (!String.isNullOrEmpty(request.responseText))
        {
            if (request.status == '200')
            {
                var msgs = videre.deserialize(request.responseText);
                if (msgs.Message != null)
                    this.addMsg('AJAX', msgs.Message + '<br/>' + msgs.StackTrace, true, ctx.parent);
                else
                    this.addMsgs(msgs, ctx.parent);
            }
            else
                this.addMsg('AJAX', request.status + ' - ' + request.statusText + '<br/>' + request.responseText, true, ctx.parent);
        }
        else
            this.addMsg('AJAX', request.statusText, true, ctx.parent);
    },

    _onError: function(src, args)
    {
        if (args.errors.length > 0)
            this.addMsgs(args.errors, args.parent);
        else
            this.removeMsg(src._id, args.parent);
    },

    add_onCustomEvent: function(handler) { this.get_events().addHandler('OnCustomEvent', handler); },
    remove_onCustomEvent: function(handler) { this.get_events().removeHandler('OnCustomEvent', handler); },
    raiseCustomEvent: function(type, data)
    {
        var handler = this.get_events().getHandler('OnCustomEvent');
        if (handler)
            handler(this, { type: type, data: data, src: this });
        return true;
    },

    _getValidationCtls: function(ctr)
    {
        var ret = [];
        var ctls = ctr.find('[required="required"],[data-datatype],[data-match],[data-controltype]');
        ctls.each(function(idx, element)
        {
            var item = videre.UI.getFormGroup($(element));
            if (item.ctl.attr('bypassvalidation') != 'true')    //todo:  we need a way to allow for panes to opt out of validation of their controls...  this is ok, but still feels a bit dirty
                ret.push(item);
        });
        return ret;
    }

});

videre.modals =
{
    //will be removed soon
    autoWidth: function(modal)
    {
        return modal.css({ width: 'auto', 'margin-left': function() { return -($(this).width() / 2); } }); //https://github.com/twitter/bootstrap/issues/675
    }
};

videre.timeZones =
{
    zones: {},
    register: function(key, data)
    {
        this.zones[key] = data;
    },
    getOffset: function(key, date)
    {
        if (this.zones[key] != null)
        {
            date = moment(date);
            if (this.zones[key].Rules == null)
                return { Minutes: this.zones[key].OffsetMinutes, Format: this.zones[key].Format };

            var ret = { Minutes: this.zones[key].OffsetMinutes, Format: this.zones[key].FormatString };
            for (var i = 0; i < this.zones[key].Rules.length; i++)
            {
                var rule = this.zones[key].Rules[i];
                if (moment(rule.RuleStart) < date && moment(rule.RuleEnd) > date)
                {
                    var start = videre.timeZones._getCutoff(rule, date, 'Start', this.zones[key].FormatString); //time is local time, so before change use use non-DST time (-06:00)
                    var end = videre.timeZones._getCutoff(rule, date, 'End', rule.DSTInfo.FormatString);    //time is local time, so before we change BACK we use DST time (-05:00)
                    if (date > start && date < end)
                        ret = { Minutes: this.zones[key].OffsetMinutes + rule.DSTInfo.DeltaMinutes, Format: rule.DSTInfo.FormatString };
                }
            }
            return ret;
        }

    },

    _getCutoff: function(rule, date, type, cutoffTimeFormat)
    {
        var dst = rule.DSTInfo;
        if (!dst.Fixed)
        {
            //Day - Gets the day on which the time change occurs. ***is for Fixed only***
            //DayOfWeek - Gets the day of the week on which the time change occurs.
            //Month - Gets the month in which the time change occurs.
            //Time - Gets the hour, minute, and second at which the time change occurs.
            //Week - Gets the week of the month in which a time change occurs. - if = 5 then LAST WEEK IN MONTH (for when there is 4 weeks)

            var firstDayOfMonth = moment.utc(date).month(dst[type].Month - 1).date(1).hour(0).minute(0).second(0).millisecond(0);    //passed in year, rule's month, first day of month so we can calculate week of year and do math

            var weekOfYear = firstDayOfMonth.isoWeek();
            var ruleDate = firstDayOfMonth.add('weeks', dst[type].Week);
            if (firstDayOfMonth.month() != ruleDate.month())
                ruleDate = ruleDate.add('weeks', -1);
            ruleDate = ruleDate.isoWeekday(dst[type].DayOfWeek);
            //somehow we need to take the time here and convert it to UTC based on passed in date (2AM in local time)
            ruleDate = moment.utc(ruleDate.format('YYYY-MM-DD') + 'T' + dst[type].Time + cutoffTimeFormat);
            return ruleDate;
        }
        else
            alert('Fixed not supported yet...'); //anyone use this? - should be easy!
    }

};

videre.localization = {
    items: [],
    dateFormats: { datetime: 'M/D/YY h:mm A', date: 'M/D/YY', time: 'h:mm A' }, //switching to moment - { datetime: 'm/d/yy h:MM TT', date: 'm/d/yy', time: 'h:MM TT' },
    numberFormat: { CurrencyDecimalDigits: 2, CurrencyDecimalSeparator: '.', CurrencyGroupSeparator: ',', CurrencySymbol: '$' },
    locale: null,
    setLocale: function(locale)
    {
        videre.localization.locale = locale;
        if (moment != null)
            moment.locale(locale);
        else
            alert('moment not found!');
    },
    getLocaleFormat: function(format)
    {
        if (videre.localization.locale != null)
        {
            if (format == 'time')
                return 'LT';
            else if (format == 'date')
                return 'L'; //or l
            else if (format == 'datetime')
                return 'L LT';
        }
        return null;
    },
    getText: function(ns, key, defaultValue)
    {
        if (defaultValue == null)
            defaultValue = ns + ' ' + key + ' is missing';
        var loc = videre.localization.items.where(function(l) { return l.ns == ns && l.key == key; }).singleOrDefault();
        if (loc == null)
            return defaultValue;
        return loc.value;
    }
};

//jsrender helpers
if ($.views != null)
{
    $.views.helpers({
        resolveUrl: function(val) { return val != null ? videre.resolveUrl(val) : ''; },
        formatDateTime: function(val, format, zone) { return val != null ? videre.parseDate(val, format != null ? format : videre.localization.dateFormats.datetime, zone) : ''; },
        formatDate: function(val, format, zone) { return val != null ? videre.parseDate(val, format != null ? format : videre.localization.dateFormats.date, zone) : ''; },
        formatTime: function(val, format, zone) { return val != null ? videre.parseDate(val, format != null ? format : videre.localization.dateFormats.time, zone) : ''; },
        formatString: function() { return String.format.apply(this, arguments); },
        nullOrEmpty: function(val) { return String.isNullOrEmpty(val); },
        coalesce: function(val, label) { return val || (label || ''); },
        deepCoalesce: function(o, s, label) { return Object.deepGet(o, s) || (label || ''); },
        bindInputs: function(data, attributes, keyName) //todo: not sure this belongs in videre.js...  
        {
            keyName = keyName != null ? keyName : data.Name;
            var ctl;
            var tempParent = $('<div></div>');
            var dataValue = Object.deepGet(attributes, keyName);
            dataValue = dataValue != null ? dataValue : data.DefaultValue;
            if (data.Values != null && data.Values.length > 0)
            {
                ctl = $('<select>').addClass('form-control').attr('data-column', keyName);

                if (data.Multiple)
                    ctl.attr('multiple', 'multiple');

                if (data.ControlType)
                    ctl.attr('data-controltype', data.ControlType);

                $.each(data.Values, function(idx, item)
                {
                    $('<option>').attr('value', item).html(item).appendTo(ctl);
                });
                var controlType = videre.UI._controlTypes[ctl.data('controltype')];
                if (controlType != null && controlType.set != null)
                {
                    controlType.set(ctl, dataValue);
                    dataValue = ctl.val();
                }
                else
                    ctl.val(dataValue);

                ctl.find(':selected').attr('selected', 'selected'); //need value written into html

                if (data.ReadOnly)
                    ctl.attr({ disabled: 'true', readonly: 'readonly' });
            }
            else
            {
                ctl = $('<input>').addClass('form-control').attr({ type: data.InputType || 'text', 'data-column': keyName }); //.val(dataValue);

                if (data.ControlType)
                    ctl.attr('data-controltype', data.ControlType);
                if (!String.isNullOrEmpty(dataValue))
                {
                    //ideally setControlValue would do _dataTypes check, but causes recursive calls...  refactoring in order
                    var controlType = videre.UI._controlTypes[ctl.data('controltype')];
                    if (controlType != null && controlType.set != null)
                    {
                        controlType.set(ctl, dataValue);
                        dataValue = ctl.val();
                    }

                    ctl.attr('value', dataValue); //need value written into html
                }

                if (data.ReadOnly)
                    ctl.attr('readonly', 'readonly');

            }
            ctl.attr('data-label-text', keyName); //todo: mini-hack as labels have no for="" specified
            if (data.Required)
                ctl.attr('required', 'required');
            if (data.DataType)
                ctl.attr('data-datatype', data.DataType);
            if (data.ControlType)
                ctl.attr('data-controltype', data.ControlType);
            if (data.Dependencies != null && data.Dependencies.length > 0)
                ctl.attr('data-dependencies', videre.serialize(data.Dependencies));
            if (data.Tooltip != null)
                ctl.attr('title', data.Tooltip);
            ctl.appendTo(tempParent);

            if (data.InputType == 'checkbox')
            {
                ctl.removeClass('form-control');
                tempParent.addClass('checkbox');
            }

            return tempParent.html(); //minor hack to get outerHTML
        }
    });
}
$(window).unload(videre.cleanUp);
