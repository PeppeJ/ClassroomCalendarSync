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

        public CalendarList GetCalendarList()
        {
            CalendarListResource.ListRequest res = Service.CalendarList.List();
            return res.Execute();
        }
    }
}
