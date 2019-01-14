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
        static string[] Scopes = {
            ClassroomService.Scope.ClassroomAnnouncementsReadonly,
            ClassroomService.Scope.ClassroomCoursesReadonly,
            ClassroomService.Scope.ClassroomCourseworkStudentsReadonly,
            ClassroomService.Scope.ClassroomGuardianlinksStudentsReadonly,
            ClassroomService.Scope.ClassroomStudentSubmissionsMeReadonly,
            ClassroomService.Scope.ClassroomStudentSubmissionsStudentsReadonly,
            ClassroomService.Scope.ClassroomProfileEmails,
            ClassroomService.Scope.ClassroomRostersReadonly,
            CalendarService.Scope.Calendar,
            CalendarService.Scope.CalendarSettingsReadonly };

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
            //ClassroomManager.Courses.ToList().ForEach(x =>
            //{
            //    Console.Write($"{x.Name} - ");
            //    Console.ForegroundColor = ConsoleColor.Cyan;
            //    Console.Write($"{x.Section}\n");
            //    Console.ResetColor();
            //});

            //ValidateCalendarDomainShare();
            //RemovePrefixFromClassroomEvents();
            foreach (var st in ClassroomManager.Students)
            {
                Console.WriteLine(st.Value.Profile.EmailAddress);
                foreach (var course in st.Value.Courses)
                {
                    Console.WriteLine($"\t{course}");
                }
            }

            while (true)
            {
                Console.WriteLine("Search for user email:");
                string input = Console.ReadLine();
                ClassroomManager.PrintWorksFrom(input);
            }
        }

        private void RemovePrefixFromClassroomEvents()
        {
            var eventsList = CalendarManager.GetEventsFromCalendars(CalendarManager.CalendarList.Items.Where(x => x.Id.Contains("classroom")).Select(x => x.Id));
            foreach (var events in eventsList)
            {
                foreach (var ev in events.Value.Items)
                {
                    if (CalendarManager.RemovePrefix(ev))
                    {
                        var req = CalendarManager.Service.Events.Patch(ev, events.Key, ev.Id);
                        var res = req.Execute();
                        ConsoleHelper.Info($"Patched {ev.Summary} in {events.Value.Summary}.");
                    }
                }
            }
            Print(eventsList);
        }


        private static void Print(IList<KeyValuePair<string, Events>> eventsList)
        {
            foreach (var events in eventsList)
            {
                Console.WriteLine($"{events.Value.Summary}");
                foreach (var ev in events.Value.Items)
                {
                    if (ev.Status != "cancelled")
                    {
                        Console.WriteLine($"\t{ev.Summary} - {ev.Start.Date}");
                    }
                }
            }
        }

        private void ValidateCalendarDomainShare()
        {
            var acls = CalendarManager.GetACLs(ClassroomManager.GetClassromCalendars());
            var result = CalendarManager.GetMissingDomainACLs(acls);

            if (result.Count > 0)
            {
                ConsoleHelper.Error($"Found {result.Count} calendars missing domain rule.");
                foreach (var item in result)
                {
                    ConsoleHelper.Error(item.Key);
                }
                var updated = CalendarManager.AddDomainACLRule(result);
                int i = 0;
                foreach (var item in updated)
                {
                    var req = CalendarManager.Service.Acl.Insert(CalendarManager.NewDomainRule(), item.Key);
                    var res = req.Execute();
                    i++;
                    ConsoleHelper.Info($"Updated calendar {item.Key} - {i} of {updated.Count}");
                }
            }
            else
            {
                ConsoleHelper.Success("No calendars missing domain rule.");
            }
        }
    }
}
