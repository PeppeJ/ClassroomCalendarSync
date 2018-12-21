using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace ClassroomCalendarSync
{
    class SyncApplication
    {
        static string ApplicationName = "Classroom Calendar Sync";
        static string[] Scopes = { ClassroomService.Scope.ClassroomCoursesReadonly, CalendarService.Scope.CalendarReadonly };

        public UserCredentialManager UserCredential { get; set; }
        public CalendarManager CalendarManager { get; set; }
        public ClassroomManager ClassroomManager { get; set; }

        public SyncApplication()
        {
            ConsoleHelper.Info($"Init {ApplicationName}");
            UserCredential = new UserCredentialManager(Scopes);
            CalendarManager = new CalendarManager(ApplicationName, UserCredential.Credential);
            ClassroomManager = new ClassroomManager(ApplicationName, UserCredential.Credential);
        }

        public void Run()
        {
            CalendarManager.CalendarList.Items.ToList()
                .Where(x => x.Id.ToLower().Contains("classroom"))
                .ToList()
                .ForEach(x => Console.WriteLine(x.Summary));

            ClassroomManager.Courses.ToList().ForEach(x => Console.WriteLine($"{x.Name} with {x.Section}"));

            Console.ReadLine();
        }
    }
}
