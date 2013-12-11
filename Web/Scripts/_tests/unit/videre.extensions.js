/// <reference path="../videre.extensions.js"/>
$(function ()
{

    module("videre.extensions")

    test("Object.convertTo tests", function ()
    {
        ok(Object.convertTo('123', 'number') == 123, 'convert to number failed');
        ok(Object.convertTo('123.12', 'int') == 123, 'convert to int failed');
        ok(Object.convertTo('123.12', 'float') == 123.12, 'convert to float failed');
        ok(Object.convertTo('true', 'boolean') == true, 'convert to boolean failed');
    });

    test("deep get/set tests", function ()
    {
        var o = { User: { Name: 'John' } };
        ok(Object.deepGet(o, 'User.Name') == 'John', 'deep get failed');
        Object.deepSet(o, 'User.Name', 'Jeff');
        ok(Object.deepGet(o, 'User.Name') == 'Jeff', 'deep set failed');
    });

    test("string trimmings", function ()
    {
        var s = '   abc   ';
        ok(s.ltrim() == 'abc   ', 'ltrim failed');
        ok(s.rtrim() == '   abc', 'rtrim failed');
        ok(s.trim() == 'abc', 'trim failed');
    });

    test("string formats", function ()
    {
        ok(String.format('Hello {0}, my name is {1}.  Yours is {0}', 'Joe', 'Sally') == 'Hello Joe, my name is Sally.  Yours is Joe', 'String.format failed');
    });

    test("string isNullOrEmpty", function ()
    {
        ok(String.isNullOrEmpty(null) == true, 'String.isNullOrEmpty failed');
        ok(String.isNullOrEmpty('') == true, 'String.isNullOrEmpty failed');
        ok(String.isNullOrEmpty(' ') == false, 'String.isNullOrEmpty failed');
    });

    test("string endsWith / startsWith", function ()
    {
        ok('abc123'.endsWith('123') == true, 'String.endsWith failed');
        ok('abc123'.endsWith('1234') == false, 'String.endsWith failed');
        ok('abc123'.startsWith('abc') == true, 'String.endsWith failed');
        ok('abc123'.startsWith(' abc') == false, 'String.endsWith failed');
    });

    test("Array.orderBy", function ()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var ordered = a.orderBy(function (d) { return d.name; });
        ok(ordered[0].name == 'aaa' && ordered[1].name == 'bbb' && ordered[2].name == 'zzz', 'orderBy failed');
    });

    //TODO:  MORE

})