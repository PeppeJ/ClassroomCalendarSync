using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace ClassroomCalendarSync
{
    class CalendarManager
    {
        private CalendarList _calendarList;

        public CalendarService Service { get; private set; }
        public CalendarList CalendarList
        {
            get { if (_calendarList == null) { CalendarList = GetCalendarList(); } return _calendarList; }
            private set { _calendarList = value; }
        }

        public CalendarManager(string applicationName, UserCredential credential)
        {
            InitializeService(applicationName, credential);
        }

        public void InitializeService(string appName, UserCredential cred)
        {
            Service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = appName,
            });
            ConsoleHelper.Info($"Initialized CalendarService for {Service.ApplicationName}");
        }

        public IEnumerable<KeyValuePair<string, Acl>> GetACLs(IEnumerable<string> calendarIDs)
        {
            ConsoleHelper.Info($"Retrieving {calendarIDs.Count()} ACLs.");
            var ACLs = new List<KeyValuePair<string, Acl>>();
            int i = 0;
            foreach (var id in calendarIDs)
            {
                var req = Service.Acl.List(id);
                var res = req.Execute();
                ACLs.Add(new KeyValuePair<string, Acl>(id, res));
                i++;
                ConsoleHelper.Success($"{id} added. {i} of {calendarIDs.Count()}");
            }
            ConsoleHelper.Success($"{calendarIDs.Count()} ACLs retrieved.");
            return ACLs;
        }

        public IEnumerable<Calendar> GetCalendars(IEnumerable<string> ids)
        {
            ConsoleHelper.Info($"Retrieving {ids.Count()} calendars.");
            var cals = new List<Calendar>();
            int i = 0;
            foreach (var id in ids)
            {
                cals.Add(GetCalendarByID(id));
                i++;
                ConsoleHelper.Success($"{id} added. {i} of {ids.Count()}");
            }
            ConsoleHelper.Success($"{ids.Count()} calendars retrieved.");
            return cals;
        }

        public Calendar GetCalendarByID(string id)
        {
            ConsoleHelper.Info($"Retrieving calendar: {id}");
            var req = Service.Calendars.Get(id);
            var res = req.Execute();
            return res;
        }

        public IList<KeyValuePair<string, Events>> GetEventsFromCalendars(IEnumerable<string> ids)
        {
            var events = new List<KeyValuePair<string, Events>>();
            foreach (var id in ids)
            {
                events.Add(new KeyValuePair<string, Events>(id, GetEventsFromCalendar(id)));
            }
            return events;
        }

        public Event RemovePrefix(Event target)
        {
            if (target.Status != "cancelled")
            {
                string[] filters = { "Assignment: ", "Uppgift: " };
                foreach (var filter in filters)
                {
                    if (target.Summary.Length < filter.Length)
                    {
                        continue;
                    }
                    string sub = target.Summary.Substring(0, filter.Length);
                    if (sub == filter)
                    {
                        target.Summary = target.Summary.Remove(0, filter.Length);
                        break;
                    }
                }
            }
            return target;
        }

        public Events GetEventsFromCalendar(string id)
        {
            ConsoleHelper.Info($"Retrieving events: {id}");
            var req = Service.Events.List(id);
            var res = req.Execute();
            return res;
        }

        public CalendarList GetCalendarList()
        {
            ConsoleHelper.Info("Retrieving CalendarList");
            var res = Service.CalendarList.List();
            var req = res.Execute();
            return req;
        }

        public AclRule NewDomainRule()
        {
            //{acl.Id} -- {acl.Kind} @@ {acl.Role} ## {acl.Scope.Type} -- {acl.Scope.Value}
            //domain:ga.lbs.se -- calendar#aclRule @@ reader ## domain -- ga.lbs.se
            return new AclRule
            {
                Id = "domain:ga.lbs.se",
                Role = "reader",
                Scope = new AclRule.ScopeData() { Type = "domain", Value = "ga.lbs.se" }
            };
        }

        public ICollection<KeyValuePair<string, Acl>> AddDomainACLRule(ICollection<KeyValuePair<string, Acl>> target)
        {
            foreach (var item in target)
            {
                item.Value.Items.Add(NewDomainRule());
            }
            return target;
        }

        public IList<KeyValuePair<string, Acl>> GetMissingDomainACLs(IEnumerable<KeyValuePair<string, Acl>> calendarAcls)
        {
            var missing = new List<KeyValuePair<string, Acl>>();
            foreach (var acl in calendarAcls)
            {
                bool miss = true;
                foreach (var item in acl.Value.Items)
                {
                    if (item.Scope.Type.Contains("domain"))
                    {
                        miss = false;
                        continue;
                    }
                }
                if (miss)
                {
                    missing.Add(acl);
                }
            }
            return missing;
        }
    }
}
