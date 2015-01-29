//Objects
Object.convertTo = function(o, targetTypeName)
{
    targetTypeName = targetTypeName || 'string';
    targetTypeName = targetTypeName.toLowerCase();
    switch (targetTypeName)
    {
        case 'int':
            return parseInt(o);
        case 'float':
            return parseFloat(o);
        case 'number':
            return Number(o);
        case 'string':
            return String(o);
        case 'date':
            return Date(o);
        case 'boolean':
            return Boolean(o);
        default:
            return o;
    }
};

Object.deepGet = function(o, s)
{
    var a, n, leftBracket, rightBracket, typename = null;
    if (!o || !s) return null;
    leftBracket = s.indexOf('<');
    if (leftBracket > -1)
    {
        rightBracket = s.indexOf('>');
        if (rightBracket > leftBracket)
            typename = s.substring(leftBracket + 1, rightBracket);
        s = s.substring(0, leftBracket);
    }
    if (s.charAt(0) == '$')
    {
        if (s.length > 1)
            return o[s.substring(1)];
        return null;
    }
    s = s.replace(/\[(\w+)\]/g, '.$1');
    s = s.replace(/^\./, '');
    a = s.split('.');
    while (a.length)
    {
        n = a.shift();
        if (o != null && n in o)
            o = o[n];
        else
            return null;
    }
    if (typename != null)
        return Object.convertTo(o, typename);
    else
        return o;
};

Object.deepSet = function(o, s, v)
{
    var a, n, self = o, leftBracket, rightBracket, typename = null;
    if (!o || !s) return null;
    leftBracket = s.indexOf('<');
    if (leftBracket > -1)
    {
        rightBracket = s.indexOf('>');
        if (rightBracket > leftBracket)
            typename = s.substring(leftBracket + 1, rightBracket);
        s = s.substring(0, leftBracket);
    }
    if (s.charAt(0) == '$')
    {
        if (s.length > 1)
            o[s.substring(1)] = v;
        return self;
    }
    s = s.replace(/\[(\w+)\]/g, '.$1');
    s = s.replace(/^\./, '');
    a = s.split('.');
    while (a.length > 1)
    {
        n = a.shift();
        if (o != null && n in o)
            o = o[n];
        else
        {
            o[n] = {};
            o = o[n];
        }
    }
    n = a.shift();
    if (typename != null)
        o[n] = Object.convertTo(v, typename);
    else
        o[n] = v;
    return self;
};

//Strings
String.format = function ()
{
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }

    return s;
};

String.isNullOrEmpty = function ()
{
    var s = arguments[0];
    return (s == null || s.length == 0);
};

//Arrays
Array.addRange = function(a, b) { a.push.apply(a, b); };
Array.clear = function(a) { a.length = 0; };
Array.clone = function(a)
{
    if (a.length === 1)
        return [a[0]];
    else
        return Array.apply(null, a);
};

Array.contains = function(a, b) { return Array.indexOf(a, b) >= 0; };
Array.indexOf = function _indexOf(array, item, start)
{
    if (typeof (item) === "undefined") return -1;
    var length = array.length;
    if (length !== 0)
    {
        // Coerce into number ("1a" will become NaN, which is consistent with the built-in behavior of similar Array methods)
        start = start - 0;
        // NaN becomes zero
        if (isNaN(start))
        {
            start = 0;
        }
        else
        {
            // If start is positive or negative infinity, don't try to truncate it.
            // The infinite values will be handled correctly by the subsequent code.
            if (isFinite(start))
            {
                // This is faster than doing Math.floor or Math.ceil
                start = start - (start % 1);
            }
            // Negative start indices start from the end
            if (start < 0)
            {
                start = Math.max(0, length + start);
            }
        }

        // A do/while loop seems to have equal performance to a for loop in this scenario
        for (var i = start; i < length; i++)
        {
            if (array[i] === item)
            {
                return i;
            }
        }
    }
    return -1;
};

(function(window)
{
    var noConflict = false; //eventually turn this on...

    //String Extensions
    var stringExt = function(string) { this._string = string; }

    stringExt.prototype = {
        _getString: function() { return (this._string != null ? this._string : this);},
        ltrim: function() { return this._getString().replace(/^\s*/, ""); },
        rtrim: function() { return this._getString().replace(/\s*$/, ""); },
        trim: function() { return this._getString().ltrim().rtrim(); },
        endsWith: function (suffix)
        {
            return (this.substr(this.length - suffix.length) === suffix);
        },
        startsWith: function (prefix)
        {
            return (this.substr(0, prefix.length) === prefix);
        }
    };

    //Number Extensions
    var numberExt = function(number) { this._number = number; }

    numberExt.prototype = {
        _getNumber: function() { return (this._number != null ? this._number : this); },
        addCommas: function(decimals)
        {
            var s = decimals != null ? this._getNumber().toFixed(decimals) : this._getNumber().toString();
            return s.split(/(?=(?:\d{3})+(?:\.|$))/g).join(",");
        }
    };

    //Number Extensions
    var dateExt = function(date) { this._date = date; }
    dateExt.prototype = {
        _getDate: function() { return (this._date != null ? this._date : this); },
        format: function(mask, utc)
        {
            /*
             * Date Format 1.2.3
             * (c) 2007-2009 Steven Levithan <stevenlevithan.com>
             * MIT license
             *
             * Includes enhancements by Scott Trenda <scott.trenda.net>
             * and Kris Kowal <cixar.com/~kris.kowal/>
             *
             * Accepts a date, a mask, or a date and a mask.
             * Returns a formatted version of the given date.
             * The date defaults to the current date/time.
             * The mask defaults to dateFormat.masks.default.
             */
            var dateFormat = function() { var token = /d{1,4}|m{1,4}|yy(?:yy)?|([HhMsTt])\1?|[LloSZ]|"[^"]*"|'[^']*'/g, timezone = /\b(?:[PMCEA][SDP]T|(?:Pacific|Mountain|Central|Eastern|Atlantic) (?:Standard|Daylight|Prevailing) Time|(?:GMT|UTC)(?:[-+]\d{4})?)\b/g, timezoneClip = /[^-+\dA-Z]/g, pad = function(val, len) { val = String(val); len = len || 2; while (val.length < len) val = "0" + val; return val }; return function(date, mask, utc) { var dF = dateFormat; if (arguments.length == 1 && Object.prototype.toString.call(date) == "[object String]" && !/\d/.test(date)) { mask = date; date = undefined } date = date ? new Date(date) : new Date; if (isNaN(date)) throw SyntaxError("invalid date"); mask = String(dF.masks[mask] || mask || dF.masks["default"]); if (mask.slice(0, 4) == "UTC:") { mask = mask.slice(4); utc = true } var _ = utc ? "getUTC" : "get", d = date[_ + "Date"](), D = date[_ + "Day"](), m = date[_ + "Month"](), y = date[_ + "FullYear"](), H = date[_ + "Hours"](), M = date[_ + "Minutes"](), s = date[_ + "Seconds"](), L = date[_ + "Milliseconds"](), o = utc ? 0 : date.getTimezoneOffset(), flags = { d: d, dd: pad(d), ddd: dF.i18n.dayNames[D], dddd: dF.i18n.dayNames[D + 7], m: m + 1, mm: pad(m + 1), mmm: dF.i18n.monthNames[m], mmmm: dF.i18n.monthNames[m + 12], yy: String(y).slice(2), yyyy: y, h: H % 12 || 12, hh: pad(H % 12 || 12), H: H, HH: pad(H), M: M, MM: pad(M), s: s, ss: pad(s), l: pad(L, 3), L: pad(L > 99 ? Math.round(L / 10) : L), t: H < 12 ? "a" : "p", tt: H < 12 ? "am" : "pm", T: H < 12 ? "A" : "P", TT: H < 12 ? "AM" : "PM", Z: utc ? "UTC" : (String(date).match(timezone) || [""]).pop().replace(timezoneClip, ""), o: (o > 0 ? "-" : "+") + pad(Math.floor(Math.abs(o) / 60) * 100 + Math.abs(o) % 60, 4), S: ["th", "st", "nd", "rd"][d % 10 > 3 ? 0 : (d % 100 - d % 10 != 10) * d % 10] }; return mask.replace(token, function($0) { return $0 in flags ? flags[$0] : $0.slice(1, $0.length - 1) }) } }(); dateFormat.masks = { "default": "ddd mmm dd yyyy HH:MM:ss", shortDate: "m/d/yy", mediumDate: "mmm d, yyyy", longDate: "mmmm d, yyyy", fullDate: "dddd, mmmm d, yyyy", shortTime: "h:MM TT", mediumTime: "h:MM:ss TT", longTime: "h:MM:ss TT Z", isoDate: "yyyy-mm-dd", isoTime: "HH:MM:ss", isoDateTime: "yyyy-mm-dd'T'HH:MM:ss", isoUtcDateTime: "UTC:yyyy-mm-dd'T'HH:MM:ss'Z'" }; dateFormat.i18n = { dayNames: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"], monthNames: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"] };
            return dateFormat(this._getDate(), mask, utc);
        }
    };

    //Array Extensions
    var arrayExt = function(array) { this._array = array; };

    arrayExt.prototype = {
        _getArray: function() { return (this._array != null ? this._array : this); },
        orderBy: function(f)
        {
            return this.sort(function(a, b)
            {
                var x = f(a);
                var y = f(b);
                return ((x < y) ? -1 : ((x > y) ? 1 : 0));
            });
        },
        where: function(f)
        {
            var ret = [];
            for (var i = 0; i < this._getArray().length; i++)
            {
                if (f(this._getArray()[i], i))
                    ret.push(this._getArray()[i]);
            }
            return ret;
        },
        distinct: function(f)
        {
            var ret = [];
            var temp = {};
            for (var i = 0; i < this._getArray().length; i++)
            {
                var key = f(this._getArray()[i]);
                if (temp[key] == null)
                {
                    ret.push(this._getArray()[i]);
                    temp[key] = 1;
                }
            }
            return ret;
        },
        findIndex: function(f)
        {
            for (var i = 0; i < this._getArray().length; i++)
            {
                if (f(this._getArray()[i], i))
                    return i;
            }
            return -1;
        },
        innerJoin: function(array, f)
        {
            var ret = [];

            for (var i = 0; i < this._getArray().length; i++)
            {
                //var matches = f(this[i], array, i);
                var b = this._getArray()[i];
                var matches = $v(array).where(function(a) { return f(a, b); });
                for (var j = 0; j < matches.length; j++)
                    ret.push($.extend(this._getArray()[i], matches[j]));
            }
            return ret;
        },
        sum: function(f)
        {
            var ret = 0;

            for (var i = 0; i < this._getArray().length; i++)
            {
                var v = f(this._getArray()[i], i);
                ret += !isNaN(v) ? v : 0;
            }
            return ret;
        },
        max: function(f)
        {
            var ret = 0;
            for (var i = 0; i < this._getArray().length; i++)
            {
                var v = f(this._getArray()[i], i);
                ret = !isNaN(v) && v > ret ? v : ret;
            }
            return ret;
        },
        toDictionary: function(f)
        {
            var dict = {};
            for (var i = 0; i < this._getArray().length; i++)
                dict[f(this._getArray()[i])] = this._getArray()[i];
            return dict;
        },
        select: function(f)
        {
            var ret = [];
            for (var i = 0; i < this._getArray().length; i++)
            {
                var a = f(this._getArray()[i]);
                if (a != null)
                    ret.push(a);
            }
            return ret;
        },
        toList: function(f) //same as select
        {
            var list = [];
            for (var i = 0; i < this._getArray().length; i++)
            {
                var v = f(this._getArray()[i], i);
                if (v != null)
                    list.push(v);
            }
            return list;
        },
        toListDictionary: function(fKey, fSelect)
        {
            var dict = {};
            for (var i = 0; i < this._getArray().length; i++)
            {
                var key = fKey(this._getArray()[i]);
                if (dict[key] == null)
                    dict[key] = [];
                dict[key].push(fSelect == null ? this._getArray()[i] : fSelect(this._getArray()[i]));
            }
            return dict;
        },
        selectMany: function(f)
        {
            var ret = [];
            for (var i = 0; i < this._getArray().length; i++)
                ret.addRange(f(this._getArray()[i]));
            return ret;
        },
        singleOrDefault: function()
        {
            if (this._getArray().length > 0)
                return this._getArray()[0];
            return null;
        },
        firstOrDefault: function()
        {
            if (this._getArray().length > 0)
                return this._getArray()[0];
            return null;
        },
        values: function()
        {
            var values = [];
            for (var key in this._getArray())
            {
                if (typeof (this._getArray()[key]) != 'function')   //todo: better way?
                    values.push(this._getArray()[key]);
            }
            return values;
        },
        forEach: function(f)
        {
            for (var i = 0; i < this._getArray().length; i++)
                f(this._getArray()[i], i, this._getArray());
        },
        addRange: function(b)
        {
            var array = this._getArray();
            array.push.apply(array, b); return array;
        },
        clear: function() { this._getArray().length = 0; },
        clone: function() { if (this._getArray().length === 1) return [this._getArray()[0]]; else return Array.apply(null, this._getArray()); },

        // Array Remove - By John Resig (MIT Licensed) - http://ejohn.org/blog/javascript-array-remove/
        remove: function(from, to)
        {
            var array = this._getArray();
            if (typeof (from) == 'function')
            {
                for (var i = 0; i < array.length; i++)
                {
                    if (from(array[i]))
                        array.remove(i);
                }
                return array;
            }
            else
            {
                var rest = array.slice((to || from) + 1 || array.length);
                array.length = from < 0 ? array.length + from : from;
                return array.push.apply(array, rest);
            }
        },
        swap: function(x, y)
        {
            var array = this._getArray();
            var o = array[x];
            array[x] = array[y];
            array[y] = o;
        },
        contains: function(a) { return Array.indexOf(this._getArray(), a) >= 0; },
        indexOf: function(item, start) { return Array.indexOf(this._getArray(), item, start); }
    };

    if (!noConflict)
    {
        var extensions = new stringExt([]);
        for (var f in extensions)
        {
            if (f != '_string' && String.prototype[f] == null)    //don't override an existing prototype
                String.prototype[f] = extensions[f];
        }

        var extensions = new numberExt([]);
        for (var f in extensions)
        {
            if (f != '_number' && Number.prototype[f] == null)    //don't override an existing prototype
                Number.prototype[f] = extensions[f];
        }

        extensions = new arrayExt([]);
        for (var f in extensions)
        {
            if (f != '_array' && Array.prototype[f] == null)    //don't override an existing prototype
                Array.prototype[f] = extensions[f];
        }

        extensions = new dateExt([]);
        for (var f in extensions)
        {
            if (f != '_date' && Date.prototype[f] == null)    //don't override an existing prototype
                Date.prototype[f] = extensions[f];
        }
    }

    window.$v = function(o)
    {
        if (o == null)
            return new arrayExt([]);

        if (typeof (o) == 'string')
            return new stringExt(o);
        else if (typeof (o) == 'number')
            return new numberExt(o);
        else if (typeof (o) == 'object')
        {
            if (o.constructor == (new Array).constructor)
                return new arrayExt(o);
            else if (o.constructor == (new Date).constructor)
                return new dateExt(o);
        }
    };

})(window);

//don't rely on jquery for this library...  this could use some improvement
if (window.jQuery == null)
{
    $ = {};
    $.extend = function(target, source)
    {
        var name;
        target = target || {};
        for (name in source)
        {
            target[name] = source[name];
        }
        return target;
    };
}
