/// <reference path="../videre.extensions.js"/>
/// <reference path="../videre.js"/>
$(function ()
{

    module("videre")

    test("timeZone tests", function ()
    {
        videre.timeZones.register('Central Standard Time', {
            "Id": "Central Standard Time",
            "OffsetMinutes": -360.0,
            "FormatString": "-06:00",
            "UsesDST": true,
            "Rules": [
              {
                  "RuleStart": "0001-01-01T06:00:00Z",
                  "RuleEnd": "2006-12-31T06:00:00Z",
                  "DSTInfo": {
                      "DeltaMinutes": 60.0,
                      "FormatString": "-05:00",
                      "Start": {
                          "DayOfWeek": 0,
                          "Month": 4,
                          "Week": 1,
                          "Day": 1,
                          "Time": "02:00:00",
                          "Fixed": false
                      },
                      "End": {
                          "DayOfWeek": 0,
                          "Month": 10,
                          "Week": 5,
                          "Day": 1,
                          "Time": "02:00:00",
                          "Fixed": false
                      }
                  }
              },
              {
                  "RuleStart": "2007-01-01T06:00:00Z",
                  "RuleEnd": "9999-12-31T06:00:00Z",
                  "DSTInfo": {
                      "DeltaMinutes": 60.0,
                      "FormatString": "-05:00",
                      "Start": {
                          "DayOfWeek": 0,
                          "Month": 3,
                          "Week": 2,
                          "Day": 1,
                          "Time": "02:00:00",
                          "Fixed": false
                      },
                      "End": {
                          "DayOfWeek": 0,
                          "Month": 11,
                          "Week": 1,
                          "Day": 1,
                          "Time": "02:00:00",
                          "Fixed": false
                      }
                  }
              }
            ]
        });
        
        ok(videre.timeZones.getOffset('aaa', '2015-03-07T12:22:00Z') == null, 'invalid timezone returned value');
        ok(videre.timeZones.getOffset('Central Standard Time', '2015-03-07T12:22:00Z').Format == '-06:00', 'central timezone DST failed');
        ok(videre.timeZones.getOffset('Central Standard Time', '2015-03-08T08:00:00Z').Format == '-06:00', 'central timezone DST failed');
        ok(videre.timeZones.getOffset('Central Standard Time', '2015-03-08T08:01:00Z').Format == '-05:00', 'central timezone DST failed');

        ok(videre.timeZones.getOffset('Central Standard Time', '2015-11-01T01:22:00Z').Format == '-05:00', 'central timezone DST failed');
        ok(videre.timeZones.getOffset('Central Standard Time', '2015-11-01T06:59:00Z').Format == '-05:00', 'central timezone DST failed');
        ok(videre.timeZones.getOffset('Central Standard Time', '2015-11-01T07:00:00Z').Format == '-06:00', 'central timezone DST failed');

        //test prior to 2006 for different ruleset
        ok(videre.timeZones.getOffset('Central Standard Time', '2005-04-02T12:22:00Z').Format == '-06:00', 'central timezone DST failed');
        ok(videre.timeZones.getOffset('Central Standard Time', '2005-04-03T08:00:00Z').Format == '-06:00', 'central timezone DST failed');
        ok(videre.timeZones.getOffset('Central Standard Time', '2005-04-03T08:01:00Z').Format == '-05:00', 'central timezone DST failed');

    });

    //TODO:  MORE

})