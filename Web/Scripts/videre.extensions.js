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
String.prototype.ltrim = function() { return this.replace(/^\s*/, ""); };
String.prototype.rtrim = function() { return this.replace(/\s*$/, ""); };
String.prototype.trim = function() { return this.ltrim().rtrim(); };

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

String.prototype.endsWith = function (suffix)
{
    return (this.substr(this.length - suffix.length) === suffix);
};

String.prototype.startsWith = function (prefix)
{
    return (this.substr(0, prefix.length) === prefix);
};

//js linq
Array.prototype.orderBy = function(f) {
    return this.sort(function(a, b) {
        var x = f(a);
        var y = f(b);
        return ((x < y) ? -1 : ((x > y) ? 1 : 0));
    });
};

Array.prototype.where = function(f) {
    var ret = [];

    for (var i = 0; i < this.length; i++) {
        if (f(this[i], i))
            ret.push(this[i]);
    }
    return ret;
};

Array.prototype.findIndex = function(f)
{
    for (var i = 0; i < this.length; i++)
    {
        if (f(this[i], i))
            return i;
    }
    return -1;
};

Array.prototype.innerJoin = function(array, f)
{
    var ret = [];

    for (var i = 0; i < this.length; i++)
    {
        //var matches = f(this[i], array, i);
        var b = this[i];
        var matches = array.where(function(a) { return f(a, b); });
        for (var j = 0; j < matches.length; j++)
            ret.push($.extend(this[i], matches[j]));
    }
    return ret;
};

Array.prototype.sum = function(f)
{
    var ret = 0;

    for (var i = 0; i < this.length; i++)
    {
        var v = f(this[i], i);
        ret += !isNaN(v) ? v : 0;
    }
    return ret;
};

Array.prototype.toDictionary = function(f)
{
    var dict = {};
    for (var i = 0; i < this.length; i++)
        dict[f(this[i])] = this[i];
    return dict;
};

Array.prototype.select = function(f)
{
    var ret = [];
    for (var i = 0; i < this.length; i++)
    {
        var a = f(this[i]);
        if (a)
            ret.push(a);
    }
    return ret;
};

Array.prototype.selectMany = function(f)
{
    var ret = [];
    for (var i = 0; i < this.length; i++)
        ret.addRange(f(this[i]));
    return ret;
};


Array.prototype.singleOrDefault = function()
{
    if (this.length > 0)
        return this[0];
    return null;
};

Array.prototype.values = function()
{
    var values = [];
    for (var key in this)
    {
        if (typeof (this[key]) != 'function')   //todo: better way?
            values.push(this[key]);
    }
    return values;
};

Array.prototype.forEach = function(f)
{
    for (var i = 0; i < this.length; i++)
        f(this[i], i);
};

Array.prototype.addRange = function(b) { this.push.apply(this, b); return this; };
Array.prototype.clear = function () { this.length = 0; };
Array.prototype.clone = function() { if (this.length === 1) return [this[0]]; else return Array.apply(null, this); };

Array.addRange = function (a, b) { a.push.apply(a, b); };
Array.clear = function (a) { a.length = 0; };
Array.clone = function (a)
{
    if (a.length === 1)
        return [a[0]];
    else
        return Array.apply(null, a);
};

// Array Remove - By John Resig (MIT Licensed) - http://ejohn.org/blog/javascript-array-remove/
Array.prototype.remove = function(from, to)
{
    var rest = this.slice((to || from) + 1 || this.length);
    this.length = from < 0 ? this.length + from : from;
    return this.push.apply(this, rest);
};

Array.prototype.swap = function(x, y)
{
    var o = this[x];
    this[x] = this[y];
    this[y] = o;
};

Array.prototype.contains = function (a) { return Array.indexOf(this, a) >= 0; };
Array.prototype.indexOf = function(item, start) { Array.indexOf(this, item, start); };

Array.contains = function (a, b) { return Array.indexOf(a, b) >= 0; };
Array.indexOf = function _indexOf(array, item, start)
{
    if (typeof(item) === "undefined") return -1;
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

Array.prototype.toListDictionary = function(key, nullKey)
{
    var dict = {};
    for (var i = 0; i < this.length; i++)
    {
        if (this[i])
        {
            var dictKey = this[i][key];
            if (dictKey == null)
                dictKey = nullKey;
            if (dict[dictKey] == null)
                dict[dictKey] = [];
            dict[dictKey].push(this[i]);
        }
    }
    return dict;
};

Array.prototype.toList = function(f)
{
    var list = [];
    for (var i = 0; i < this.length; i++)
    {
        var v = f(this[i], i);
        if (v != null)
            list.push(v);
    }
    return list;
};

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


//Array.prototype.merge = function(newArray, key)
//{
//    if (key != null)
//    {
//        var keys = {};
//        for (var i = 0; i < this.length; i++)
//            keys[this[i][key]] = 1;
//        for (var i = 0; i < newArray.length; i++)
//        {
//            if (keys[newArray[i][key]] != 1)
//                this.push(newArray[i]);
//        }
//    }
//    else
//        return Array.addRange(this, newArray);
//};

//todo: hack!
Number.prototype.addCommas = function(decimals)
{
    var s = decimals != null ? this.toFixed(decimals) : this.toString();
    return s.split(/(?=(?:\d{3})+(?:\.|$))/g).join(",");
};

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
Date.prototype.format = function(mask, utc) { return dateFormat(this, mask, utc) };
