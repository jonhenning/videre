
var videre =
{
    enableLogging: false,
    _timings: {},
    _lastLog: new Date(),
    _timers: {},
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

    serialize: function(s)
    {
        return JSON.stringify(s);
    },

    deserialize: function(data)
    {
        return JSON.parse(data);
    },

    formatDate: function(value, format) //json2.net - reviver
    {
        if (typeof value === 'string') {
            var a = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)(?:([\+-])(\d{2})\:(\d{2}))?Z?$/.exec(value);
            if (a) {
                var utcMilliseconds = Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6]);
                var d = new Date(utcMilliseconds);
                if (format)
                    return d.format(format);
                return d;
            }
        }
        return value;
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

    ajax: function(url, data, success, error, ctx)
    {
        $.ajax({
            type: "POST",
            url: videre.resolveUrl(url),
            processData: false,
            data: videre.serialize(data),
            contentType: 'application/json; charset=utf-8',
            //dataType: 'json',
            success: function(result) { success(result, ctx); },
            error: error
        });
    },

    timer: function(key, func, interval, ctx)
    {
        clearTimeout(videre._timers[key]);
        videre._timers[key] = setTimeout(function() { func(ctx); }, interval);
    }

};

videre.registerNamespace('videre.UI');

videre.UI = {

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
            ddl.append($('<option></option>').html(blankText));
        $.each(data, function(idx, item)
        {
            ddl.append($('<option></option>').val(this[valCol]).html(this[textCol]));
        });
        if (selectedVal != null)
            ddl.val(selectedVal);
        return ddl;
    }

};

videre.UI.eventHandlerList = function ()
{
    this._list = {};
}

videre.UI.eventHandlerList.prototype =
{
    addHandler: function (id, handler)
    {
        var event = this._getEvent(id, true);
        event.push(handler);
    },

    removeHandler: function (id, handler)
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

    getHandler: function (id)
    {
        var evt = this._getEvent(id);
        if (!evt || !evt.length) return null;
        //evt = Array.clone(evt);
        var evt = evt.length === 1 ? [evt[0]] : Array.apply(null, evt);

        return function (source, args)
        {
            for (var i = 0, l = evt.length; i < l; i++)
            {
                evt[i](source, args);
            }
        };
    },

    _getEvent: function (id, create)
    {
        var e = this._list[id];
        if (!e)
        {
            if (!create) return null;
            this._list[id] = e = [];
        }
        return e;
    }
}

//JSON Parsing - See http://www.JSON.org/js.html
var JSON; if (!JSON) { JSON = {} } (function () { function f(n) { return n < 10 ? "0" + n : n } if (typeof Date.prototype.toJSON !== "function") { Date.prototype.toJSON = function (key) { return isFinite(this.valueOf()) ? this.getUTCFullYear() + "-" + f(this.getUTCMonth() + 1) + "-" + f(this.getUTCDate()) + "T" + f(this.getUTCHours()) + ":" + f(this.getUTCMinutes()) + ":" + f(this.getUTCSeconds()) + "Z" : null }; String.prototype.toJSON = Number.prototype.toJSON = Boolean.prototype.toJSON = function (key) { return this.valueOf() } } var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, escapable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, gap, indent, meta = { "\b": "\\b", "\t": "\\t", "\n": "\\n", "\f": "\\f", "\r": "\\r", '"': '\\"', "\\": "\\\\" }, rep; function quote(string) { escapable.lastIndex = 0; return escapable.test(string) ? '"' + string.replace(escapable, function (a) { var c = meta[a]; return typeof c === "string" ? c : "\\u" + ("0000" + a.charCodeAt(0).toString(16)).slice(-4) }) + '"' : '"' + string + '"' } function str(key, holder) { var i, k, v, length, mind = gap, partial, value = holder[key]; if (value && typeof value === "object" && typeof value.toJSON === "function") { value = value.toJSON(key) } if (typeof rep === "function") { value = rep.call(holder, key, value) } switch (typeof value) { case "string": return quote(value); case "number": return isFinite(value) ? String(value) : "null"; case "boolean": case "null": return String(value); case "object": if (!value) { return "null" } gap += indent; partial = []; if (Object.prototype.toString.apply(value) === "[object Array]") { length = value.length; for (i = 0; i < length; i += 1) { partial[i] = str(i, value) || "null" } v = partial.length === 0 ? "[]" : gap ? "[\n" + gap + partial.join(",\n" + gap) + "\n" + mind + "]" : "[" + partial.join(",") + "]"; gap = mind; return v } if (rep && typeof rep === "object") { length = rep.length; for (i = 0; i < length; i += 1) { if (typeof rep[i] === "string") { k = rep[i]; v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ": " : ":") + v) } } } } else { for (k in value) { if (Object.prototype.hasOwnProperty.call(value, k)) { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ": " : ":") + v) } } } } v = partial.length === 0 ? "{}" : gap ? "{\n" + gap + partial.join(",\n" + gap) + "\n" + mind + "}" : "{" + partial.join(",") + "}"; gap = mind; return v } } if (typeof JSON.stringify !== "function") { JSON.stringify = function (value, replacer, space) { var i; gap = ""; indent = ""; if (typeof space === "number") { for (i = 0; i < space; i += 1) { indent += " " } } else { if (typeof space === "string") { indent = space } } rep = replacer; if (replacer && typeof replacer !== "function" && (typeof replacer !== "object" || typeof replacer.length !== "number")) { throw new Error("JSON.stringify") } return str("", { "": value }) } } if (typeof JSON.parse !== "function") { JSON.parse = function (text, reviver) { var j; function walk(holder, key) { var k, v, value = holder[key]; if (value && typeof value === "object") { for (k in value) { if (Object.prototype.hasOwnProperty.call(value, k)) { v = walk(value, k); if (v !== undefined) { value[k] = v } else { delete value[k] } } } } return reviver.call(holder, key, value) } text = String(text); cx.lastIndex = 0; if (cx.test(text)) { text = text.replace(cx, function (a) { return "\\u" + ("0000" + a.charCodeAt(0).toString(16)).slice(-4) }) } if (/^[\],:{}\s]*$/.test(text.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, "@").replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, "]").replace(/(?:^|:|,)(?:\s*\[)+/g, ""))) { j = eval("(" + text + ")"); return typeof reviver === "function" ? walk({ "": j }, "") : j } throw new SyntaxError("JSON.parse") } } }());

//override date serialization - http://www.asp.net/ajaxlibrary/jquery_webforms_serialize_dates_to_json.ashx - .NET 3.5 REMOVES THE NEED FOR THIS!
//Date.prototype.toJSON = function (key) { return isFinite(this.valueOf()) ? '/Date(' + this.getTime() + ')/' : null };

//http://ejohn.org/blog/simple-javascript-inheritance
// Inspired by base2 and Prototype
(function ()
{
    var initializing = false, fnTest = /xyz/.test(function () { xyz; }) ? /\b_base\b/ : /.*/;

    // The base Class implementation (does nothing)
    videre.Class = function () { };

    // Create a new Class that inherits from this class
    videre.Class.extend = function (prop)
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
        (function (name, fn)
        {
            return function ()
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
        return videre.widgets.registeredWidgets.values().where(function(c) { return c instanceof t; });
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
    get_events: function() { return this._eventHandlerList; },
    get_childWidgets: function() { return this._childWidgets; },

    get_messages: function() { return this._messages; },
    set_messages: function(v) { this._messages = v; },
    set_user: function(v) { this._user = v; },
    get_user: function() { return this._user; },

    init: function()
    {
        this._id = null;
        this._ns = '';
        this._controls = {};
        this._childWidgets = {};
        this._eventHandlerList = new videre.UI.eventHandlerList();

        $(document).ready(videre.createDelegate(this, function() { window.setTimeout(videre.createDelegate(this, this._onLoad), 0) }));  //timeout for what again?  

        this._messages = [];
        this._user = {};
        this._locked = false;

        //controls
        this._widget = null;

        this._baseDelegates = {
            onAjaxSuccess: videre.createDelegate(this, this._onAjaxSuccess),
            onAjaxFail: videre.createDelegate(this, this._onAjaxFail)
        }
    },

    _onLoad: function(src, args)
    {
        //this._base();
        this._widget = this.getControl('Widget').keydown(videre.createDelegate(this, this._onWidgetKeyDown));
        this._widget.find('form').submit(function() { return false; }) //prevent submit
        $(document.body).keydown(function(e) { if (e.keyCode == 27) return false; });    //prevent escape clearing form
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

    ajax: function(url, params, onSuccess, onFail, parent, ctx)
    {
        this.clearMsgs(parent);
        this.lock(parent);
        if (onFail == null)
            onFail = this._baseDelegates.onAjaxFail;

        videre.ajax(url, params, this._baseDelegates.onAjaxSuccess, onFail, { onSuccess: onSuccess, parent: parent, ctx: ctx });
    },

    bindData: function(data, parent)
    {
        if (data == null)
            data = {};
        if (parent == null)
            parent = this._widget;
        var ctls = parent.find('[data-column]');
        ctls.each(function(idx, element)
        {
            var ctl = $(element);
            if (ctl.data('controltype') == 'list')
                ctl.val(data[ctl.data('column')].join('\n'));
            else if(ctl.data('controltype') == 'multiselect')
            {
                ctl.val(data[ctl.data('column')]);
                ctl.multiselect('refresh');
            }
            else
            {
                var val = data[ctl.data('column')];
                switch (ctl.data('datatype'))
                {
                    case 'datetime': case 'time':
                        {
                            ctl.datetimepicker('setDate', videre.formatDate(val));
                            break;
                        }
                    case 'date': 
                        {
                            ctl.datepicker('setDate', videre.formatDate(val));
                            break;
                        }
                    default:
                        {
                            val = val != null ? val.toString() : '';
                            var tagName = ctl.prop('tagName').toLowerCase();
                            if (tagName == 'label' || tagName == 'span' || tagName == 'div' || tagName == 'p')  //todo:  better way to detect to set html or val?
                                ctl.text(val);
                            else 
                                ctl.val(val);
                            break;
                        }
                }
            }
        });
    },

    validControls: function(controlCtr, messageCtr)
    {
        //var result = controlCtr.jqBootstrapValidation("validate", true);
        //return result.warningsFound == 0;
        
        messageCtr = messageCtr != null ? messageCtr : controlCtr;
        this.clearMsgs();
        var ctls = videre.validation.getValidationCtls(controlCtr);
        var errors = [];
        ctls.forEach(function(item)
        {
            var error = videre.validation.validateCtl(item);
            if (error != null)
            {
                item.group.addClass('error');
                errors.push(error);
            }
            else 
                item.group.removeClass('error');
        });
        this.addMsgs(errors, messageCtr);
        return errors.length == 0;
    },

    persistData: function(data, clone, parent, includeReadOnly)
    {
        if (clone == null)
            clone = true;
        if (parent == null)
            parent = this._widget;

        var cloneData = clone ? videre.jsonClone(data) : data;
        var ctls = parent.find('[data-column]');
        ctls.each(function(idx, element)
        {
            var ctl = $(element);
            if (!ctl.attr('readonly') || includeReadOnly)
            {
                var col = ctl.attr('data-column');
                if (!String.isNullOrEmpty(col))
                {
                    var val = ctl.val();
                    if (ctl.data('controltype') == 'list')
                        cloneData[col] = val.length > 0 ? val.split('\n') : null;       //todo: ok for template URL?
                    else if (ctl.data('controltype') == 'multiselect')
                    {
                        cloneData[col] = val;
                    }
                    else
                    {
                        if (ctl.data('datatype') == 'datetime')
                            val = ctl.datetimepicker('getDate');
                        if (ctl.data('datatype') == 'date')
                            val = videre.formatDate(val, 'iso');
                        if (ctl.data('datatype') == 'time')
                            val = videre.formatDate(val, 'iso');

                        cloneData[col] = val;
                    }
                }
            }
        });
        return cloneData;
    },

    _onAjaxSuccess: function(result, ctx)
    {
        this.unlock(ctx.parent);
        if (!result.HasError)
        {
            if (ctx.onSuccess != null)
                ctx.onSuccess(result, ctx.ctx);
        }
        this.addMsgs(result.Messages, ctx.parent);
    },

    lock: function(parent)
    {
        this._locked = true;
        this.getProgressBar(parent).show();
    },

    unlock: function(parent)
    {
        this._locked = false;
        this.getProgressBar(parent).hide();
    },

    getProgressBar: function(parent)
    {
        if (parent == null)
            parent = this._widget;
        return parent.find('.progress');
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
            this.clearInfoMsgs();   //todo: perf?
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
                msgCtr.removeClass('alert-error').addClass(msg.isError ? 'alert-error' : '');
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
        videre.validation.getValidationCtls(parent != null ? parent : this._widget).forEach(function(item) { item.group.removeClass('error'); });
    },

    //events
    _onWidgetKeyDown: function(e) { },

    _onAjaxFail: function(error, ctx, method)
    {
        this.unlock();

        if (!String.isNullOrEmpty(error.responseText))
        {
            var msgs = videre.deserialize(error.responseText);
            if (msgs.Message != null)
                this.addMsg('AJAX', msgs.Message + '<br/>' + msgs.StackTrace, true);
            else
                this.addMsgs(msgs);
        }
        else
            alert(error.StatusText);
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
    }

});

//todo:  specific code to dynatree... should it go here?
videre.tree = {

    getTreeData: function(rootName, data, func, delim)
    {
        delim = (delim == null) ? '/' : delim;
        var root = { title: rootName, isFolder: true, key: '', children: [] };
        var lookup = {};
        lookup[''] = root;
        for (var row = 0; row < data.length; row++)
        {
            var key = func(data[row]); //[column];
            var path = '';
            var parts = key.split(delim);
            for (var i = 0; i < parts.length; i++)
            {
                var parent = lookup[path];
                path += (path.length ? delim : '') + parts[i];
                if (lookup[path] == null)
                {
                    lookup[path] = { title: parts[i], isFolder: i < parts.length - 1, key: path, children: [] };
                    parent.children.push(lookup[path]);
                }
            }
        }
        return root;
    }
};

//todo:  specific code to datatables... should it go here?
videre.dataTables = {

    clear: function(tbl, options)
    {
        tbl.dataTable(options).fnDestroy();
    },

    //todo: phase out passing columns as specific argument and just use in options
    bind: function(tbl, columns, options)
    {
        var cellCount = tbl.find('th').length;
        if (columns != null && cellCount > 0)
            columns.length = cellCount;
        if (options == null)
            options = {};

        //options.sPaginationType = options.sPaginationType != null ? options.sPaginationType : 'bootstrap';
        options.aoColumns = options.aoColumns != null ? options.aoColumns : columns;
        //http://datatables.net/blog/Twitter_Bootstrap_2
        tbl.dataTable(options);
    }

};

videre.modals =
{
    autoWidth: function(modal)
    {
        return modal.css({ width: 'auto', 'margin-left': function() { return -($(this).width() / 2); } }); //https://github.com/twitter/bootstrap/issues/675
    }
};


videre.localization = {
    items: [],
    dateFormats: { datetime: 'm/d/yy h:MM TT', date: 'm/d/yy', time: 'h:MM TT' },
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

//todo:  wish there was a clean library for this...  hate writing my own...
videre.validation = {
    getValidationCtls: function(ctr)
    {
        var ret = [];
        var ctls = ctr.find('[required="required"],[data-datatype],[data-match]');
        ctls.each(function(idx, element)
        {
            var item = { ctl: $(element) };
            if (item.ctl.attr('bypassvalidation') != 'true')    //todo:  we need a way to allow for panes to opt out of validation of their controls...  this is ok, but still feels a bit dirty
            {
                item.lbl = ctr.find('[for="' + item.ctl.attr('id') + '"]');
                item.group = item.ctl.closest('.control-group');
                item.labelText = item.lbl.html();
                if (item.labelText == null)
                    item.labelText = item.ctl.data('label-text');
                ret.push(item);
            }
        });
        return ret;
    },

    validateCtl: function(item)
    {
        if (item.ctl.data('dependencymatch') == false)  //if dependent control and it is not matched (shown) it is valid!
            return null;
        if (item.ctl.attr('required') && item.ctl.val() == '')
            return { id: item.ctl.attr('id') + 'Required', text: String.format(videre.localization.getText('global', 'RequiredField'), item.labelText), isError: true };
        if (item.ctl.data('datatype') != null && !videre.validation.validDataType(item.ctl.data('datatype'), item.ctl.val()))
            return { id: item.ctl.attr('id') + 'DataTypeInvalid', text: String.format(videre.localization.getText('global', 'DataTypeInvalid'), item.labelText, item.ctl.data('datatype')), isError: true };
        if (item.ctl.data('match') != null && item.ctl.val() != $('#' + item.ctl.data('match')).val())
            return { id: item.ctl.attr('id') + 'ValuesMustMatch', text: String.format(videre.localization.getText('global', 'ValuesMustMatch'), item.labelText), isError: true };
    },

    validDataType: function(type, val)
    {
        if (!String.isNullOrEmpty(val))
        {
            var item = videre.validation.datatypes[type];
            if (item != null)
            {
                if (item.type == 'regex')
                    return (new RegExp(item.regex).test(val));
                else if (item.type == 'function')
                    return item.func(val);
                return false;
            }
            else
                alert('Invalid data type: ' + type);    //todo: what to do?
        }
        else
            return true;
    },

    datatypes:
    {
        number: { type: 'function', func: function(d) { return !isNaN(d); } },
        date: { type: 'function', func: function(d) { return (new Date(videre.formatDate(d, videre.localization.dateFormats.date))) != 'Invalid Date'; } }, //todo: think we can do better!
        email: { type: 'regex', regex: /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i }   //http://ask.altervista.org/demo/jquery-validate-e-mail-address-regex/
    }
};

//jsrender helpers
$.views.helpers({
    resolveUrl: function(val) { return val != null ? videre.resolveUrl(val) : ''; },
    formatDateTime: function(val) { return val != null ? videre.formatDate(val).format(videre.localization.dateFormats.datetime) : ''; },
    formatDate: function(val) { return val != null ? videre.formatDate(val).format(videre.localization.dateFormats.date) : ''; },
    formatTime: function(val) { return val != null ? videre.formatDate(val).format(videre.localization.dateFormats.time) : ''; },
    bindInputs: function(data, attributes, keyName)
    {
        keyName = keyName != null ? keyName : data.Name;
        var ctl;
        var tempParent = $('<div></div>');
        var dataValue = (attributes != null && attributes[keyName] != null) ? attributes[keyName] : data.DefaultValue;
        if (data.Values.length > 0)
        {
            ctl = $('<select>').attr('data-column', keyName);
            $.each(data.Values, function(idx, item)
            {
                $('<option>').attr('value', item).html(item).appendTo(ctl);
            });
            ctl.val(dataValue);
            ctl.find(':selected').attr('selected', 'selected'); //need value written into html
        }
        else
        {
            ctl = $('<input>').attr({ type: 'text', 'data-column': keyName });//.val(dataValue);
            if (dataValue != null)
                ctl.attr('value', dataValue);   //need value written into html
        }
        ctl.attr('data-label-text', keyName);   //todo: mini-hack as labels have no for="" specified
        if (data.Required)
            ctl.attr('required', 'required');
        if (data.DataType)
            ctl.attr('data-datatype', data.DataType);
        if (data.Dependencies != null && data.Dependencies.length > 0)
            ctl.attr('data-dependencies', videre.serialize(data.Dependencies));
        ctl.appendTo(tempParent);
        return tempParent.html();   //minor hack to get outerHTML
    }
});

$(window).unload(videre.cleanUp);
