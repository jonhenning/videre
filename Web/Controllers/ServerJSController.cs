using System;
using System.Linq;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Services = Videre.Core.Services;
using CodeEndeavors.Extensions;

namespace Videre.Web.Controllers
{
    public class ServerJSController : System.Web.Mvc.Controller
    {
        [ETagAttribute]
        public JavaScriptResult GlobalClientTranslations()
        {
            Services.Localization.GetPortalText("RequiredField.Client", "{0} is a required field");
            Services.Localization.GetPortalText("DataTypeInvalid.Client", "{0} is not a valid {1}");
            Services.Localization.GetPortalText("ValuesMustMatch.Client", "{0} requires a matching value");
            Services.Localization.GetPortalText("None.Client", "(None)");
            if (Services.Repository.Current.PendingUpdates > 0)
                Services.Repository.SaveChanges();

            var locs = Services.Localization.GetLocalizations(Core.Models.LocalizationType.Portal, l => l.Key.EndsWith(".Client"));

            var script = string.Format("videre.localization.items = {0};",
                locs.Select(l => new { key = l.Key.Replace(".Client", ""), value = l.Text, ns = "global" }).ToJson());

            var dateFormat = Services.Account.GetUserDateFormat("date", false);
            if (dateFormat != null)
            {
                //register dateFormats per user
                script += string.Format("videre.localization.dateFormats = {{datetime: '{0}', date: '{1}', time: '{2}', zone: '{3}'}};", Services.Account.GetUserDateFormat("datetime", false), dateFormat, Services.Account.GetUserDateFormat("time", false), Services.Account.GetUserTimeZone());
            }

            var numberFormat = Services.Account.GetUserNumberFormat();
            if (numberFormat != null)
                script += string.Format("videre.localization.numberFormat = {0}", numberFormat.ToJson());

            if (Services.Account.CurrentUser != null && !string.IsNullOrEmpty(Services.Account.CurrentUser.Locale))
                script += string.Format("videre.localization.setLocale('{0}');", Services.Account.CurrentUser.Locale);

            var eTagHash = Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(script)));

            return new JavaScriptResult()
            {
                 Script  = script 
            };
        }

        [ETagAttribute]
        public JavaScriptResult TimeZoneInformation(string key)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(key);
            if (tz == null)
                throw new Exception("Unknown Timezone: " + key);

            var script = string.Format("videre.timeZones.register('{0}', {1});", tz.Id, GetTimeZoneJson(tz));

            //todo: register dateFormats per user or per portal?
            //var script = string.Format("videre.localization.dateFormats = {datetime: 'm-d-yy H:MM TT', date: 'm-d-yy', time: 'H:MM TT'};",

            var eTagHash = Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(script)));

            return new JavaScriptResult()
            {
                Script = script
            };
        }

        private string GetTimeZoneJson(TimeZoneInfo tz)
        {
            var o = new
            {
                Id = tz.Id,
                OffsetMinutes = tz.BaseUtcOffset.TotalMinutes,
                FormatString = string.Format("{0}:{1}", tz.BaseUtcOffset.Hours.ToString("00"), tz.BaseUtcOffset.Minutes.ToString("00")),
                UsesDST = tz.SupportsDaylightSavingTime,
                Rules = tz.GetAdjustmentRules().Select(r =>
                    new
                    {
                        RuleStart = r.DateStart,
                        RuleEnd = r.DateEnd,
                        DSTInfo = new
                        {
                            DeltaMinutes = r.DaylightDelta.TotalMinutes,
                            FormatString = string.Format("{0}:{1}", tz.BaseUtcOffset.Add(r.DaylightDelta).Hours.ToString("00"), tz.BaseUtcOffset.Add(r.DaylightDelta).Minutes.ToString("00")),
                            Start = new
                            {
                                DayOfWeek = r.DaylightTransitionStart.DayOfWeek,
                                Month = r.DaylightTransitionStart.Month,
                                Week = r.DaylightTransitionStart.Week,
                                Day = r.DaylightTransitionStart.Day,
                                Time = r.DaylightTransitionStart.TimeOfDay.ToString("HH:mm:ss"),
                                Fixed = r.DaylightTransitionStart.IsFixedDateRule
                            },
                            End = new 
                            {
                                DayOfWeek = r.DaylightTransitionEnd.DayOfWeek,
                                Month = r.DaylightTransitionEnd.Month,
                                Week = r.DaylightTransitionEnd.Week,
                                Day = r.DaylightTransitionEnd.Day,
                                Time = r.DaylightTransitionEnd.TimeOfDay.ToString("HH:mm:ss"),
                                Fixed = r.DaylightTransitionEnd.IsFixedDateRule
                            }
                        }
                    }
                )
            };

            return o.ToJson();
        }

//videre.timeZones.register('Central Standard Time', {
//  "Id": "Central Standard Time",
//  "DisplayName": "(UTC-06:00) Central Time (US & Canada)",
//  "StandardName": "Central Standard Time",
//  "DaylightName": "Central Daylight Time",
//  "BaseUtcOffset": "-06:00:00",
//  "AdjustmentRules": [
//    {
//      "DateStart": "0001-01-01T06:00:00Z",
//      "DateEnd": "2006-12-31T06:00:00Z",
//      "DaylightDelta": "01:00:00",
//      "DaylightTransitionStart": {
//        "TimeOfDay": "0001-01-01T08:00:00Z",
//        "Month": 4,
//        "Week": 1,
//        "Day": 1,
//        "DayOfWeek": 0,
//        "IsFixedDateRule": false
//      },
//      "DaylightTransitionEnd": {
//        "TimeOfDay": "0001-01-01T08:00:00Z",
//        "Month": 10,
//        "Week": 5,
//        "Day": 1,
//        "DayOfWeek": 0,
//        "IsFixedDateRule": false
//      }
//    },
//    {
//      "DateStart": "2007-01-01T06:00:00Z",
//      "DateEnd": "9999-12-31T06:00:00Z",
//      "DaylightDelta": "01:00:00",
//      "DaylightTransitionStart": {
//        "TimeOfDay": "0001-01-01T08:00:00Z",
//        "Month": 3,
//        "Week": 2,
//        "Day": 1,
//        "DayOfWeek": 0,
//        "IsFixedDateRule": false
//      },
//      "DaylightTransitionEnd": {
//        "TimeOfDay": "0001-01-01T08:00:00Z",
//        "Month": 11,
//        "Week": 1,
//        "Day": 1,
//        "DayOfWeek": 0,
//        "IsFixedDateRule": false
//      }
//    }
//  ],
//  "SupportsDaylightSavingTime": true
//});

        //http://msdn.microsoft.com/en-us/library/system.timezoneinfo.transitiontime.isfixeddaterule.aspx
        //private void DisplayTransitionInfo(TimeZoneInfo.TransitionTime transition, int year, string label)
        //{
        //    // For non-fixed date rules, get local calendar
        //    Calendar cal = CultureInfo.CurrentCulture.Calendar;
        //    // Get first day of week for transition 
        //    // For example, the 3rd week starts no earlier than the 15th of the month 
        //    int startOfWeek = transition.Week * 7 - 6;
        //    // What day of the week does the month start on? 
        //    int firstDayOfWeek = (int)cal.GetDayOfWeek(new DateTime(year, transition.Month, 1));
        //    // Determine how much start date has to be adjusted 
        //    int transitionDay;
        //    int changeDayOfWeek = (int)transition.DayOfWeek;

        //    if (firstDayOfWeek <= changeDayOfWeek)
        //        transitionDay = startOfWeek + (changeDayOfWeek - firstDayOfWeek);
        //    else
        //        transitionDay = startOfWeek + (7 - firstDayOfWeek + changeDayOfWeek);

        //    // Adjust for months with no fifth week 
        //    if (transitionDay > cal.GetDaysInMonth(year, transition.Month))
        //        transitionDay -= 7;

        //    Console.WriteLine("   {0} {1}, {2:d} at {3:t}",
        //                      label,
        //                      transition.DayOfWeek,
        //                      new DateTime(year, transition.Month, transitionDay),
        //                      transition.TimeOfDay);
        //}   

    }
}