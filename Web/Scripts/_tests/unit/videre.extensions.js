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

    test("number addCommas", function()
    {
        var n = 1234567;
        ok(n.addCommas() == '1,234,567', 'number addCommas failed');
    });

    test("date format", function()
    {
        var d = new Date(2011, 3, 10);
        ok(d.format('yyyy-mm-dd') == '2011-04-10', 'date format failed');
    });

    test("Array.orderBy", function ()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var ordered = a.orderBy(function (d) { return d.name; });
        ok(ordered[0].name == 'aaa' && ordered[1].name == 'bbb' && ordered[2].name == 'zzz', 'orderBy failed');
    });

    test("Array.where", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var where = a.where(function(d) { return d.name == 'aaa'; });
        ok(where[0].id == 2, 'where failed');
    });

    test("Array.distinct", function()
    {
        var a = [1,1,1,2,2,3,3,3,4,5,6,6];
        var distinct = a.distinct(function(d) { return d; });
        ok(distinct.length == 6, 'distinct failed');
    });

    test("Array.findIndex", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var findIndex = a.findIndex(function(d) { return d.name == 'bbb'; });
        ok(findIndex == 2, 'findIndex failed');
    });

    test("Array.innerJoin", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var a2 = [{ id: 1, name2: 'zzz' }, { id: 2, name2: 'aaa' }, { id: 3, name2: 'bbb' }];

        var join = a.innerJoin(a2, function(a, b) { return a.id == b.id; } );
        ok(join[0].name == join[0].name2, 'innerJoin failed');
    });

    test("Array.sum", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var sum = a.sum(function(d) { return d.id; });
        ok(sum == 6, 'sum failed');
    });

    test("Array.max", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var max = a.max(function(d) { return d.id; });
        ok(max == 3, 'max failed');
    });

    test("Array.toDictionary", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var dict = a.toDictionary(function(d) { return d.name; });
        ok(dict['bbb'].id == 3, 'toDictionary failed');
    });

    test("Array.select", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var list = a.select(function(d) { return { id: d.id, name: d.name + '2' }; });
        ok(list[2].name == a[2].name + '2', 'select failed');
    });

    test("Array.toList", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var list = a.toList(function(d) { return { id: d.id, name: d.name + '2' }; });
        ok(list[2].name == a[2].name + '2', 'toList failed');
    });

    test("Array.toListDictionary", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }, { id: 4, name: 'bbb' }];
        var listDictionary = a.toListDictionary(function(d) { return d.name; });
        ok(listDictionary['bbb'].length == 2, 'toListDictionary failed');

        listDictionary = a.toListDictionary(function(d) { return d.name; }, function(d) { return { id: d.id, name: d.name, displayName: d.id + ' ' + d.name }; });
        ok(listDictionary['bbb'][1].displayName == listDictionary['bbb'][1].id + ' ' + listDictionary['bbb'][1].name, 'toListDictionary failed');
    });

    test("Array.firstOrDefault", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var data = a.firstOrDefault(function(d) { return { id: d.id, name: d.name + '2' }; });
        ok(data.name == 'zzz', 'firstOrDefault failed');
    });

    test("Array.values", function()
    {
        var a = ['a', 'b', 'c'];
        //for (var x in a)  //why we need .values
        //    alert(x);
        var data = a.values();
        ok(a.length == 3, 'values failed');
    });

    test("Array.forEach", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        a.forEach(function(d) { d.test = 1 });
        ok(a[0].test == 1, 'forEach failed');
    });

    test("Array.addRange", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var a2 = [{ id: 4, name: 'xxx' }, { id: 5, name: 'yyy' }];
        a.addRange(a2);
        ok(a.length == 5, 'addRange failed');
        ok(a[4].id == 5, 'addRange failed');
    });

    test("Array.clear", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        a.clear();
        ok(a.length == 0, 'clear failed');
    });

    test("Array.clone", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        var a2 = a.clone();
        a.push({ id: 4, name: 'foo' });
        ok(a2.length == 3, 'clone failed');
    });

    test("Array.remove", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        a.remove(1, 2);
        ok(a.length == 1, 'remove failed');

        a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        a.remove(function(d) { return d.id == 2; });
        ok(a[1].id == 3, 'remove (function) failed');

    });

    test("Array.swap", function()
    {
        var a = [{ id: 1, name: 'zzz' }, { id: 2, name: 'aaa' }, { id: 3, name: 'bbb' }];
        a.swap(0, 1);
        ok(a[0].id == 2, 'swap failed');
    });

    test("Array.contains", function()
    {
        var a = [1,2,3,4,7];
        ok(a.contains(3) && !a.contains(6), 'contains failed');
    });

    test("Array.indexOf", function()
    {
        var a = [1, 2, 3, 4, 7];
        ok(a.indexOf(3) == 2, 'indexOf failed');
    });

    //TODO:  MORE
})