using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace ClassroomCalendarSync
{
    class ClassroomManager
    {
        public ClassroomService Service { get; private set; }

        private Course[] _courses;
        public Course[] Courses
        {
            get
            {
                if (_courses == null) { Courses = GetActiveCourses().ToArray(); }
                return _courses;
            }
            private set { _courses = value; }
        }

        private Dictionary<string, StudentWrapper> _students;
        public Dictionary<string, StudentWrapper> Students
        {
            get
            {
                if (_students == null) { Students = GetAllActiveStudents(); }
                return _students;
            }
            private set { _students = value; }
        }

        private Dictionary<string, StudentWrapper> GetAllActiveStudents()
        {
            var students = new Dictionary<string, StudentWrapper>(512);

            ConsoleHelper.Info("Retrieving all students.");
            int i = 0;
            foreach (var course in Courses)
            {
                i++;
                var req = Service.Courses.Students.List(course.Id);
                var res = req.Execute();
                ConsoleHelper.Info($"Retrieved students from {course.Name}. {i} of {Courses.Length}");
                if (res.Students != null && res.Students.Count > 0)
                {
                    foreach (var student in res.Students)
                    {
                        Console.WriteLine($"Found {student.Profile.Name.FullName}");
                        if (!students.ContainsKey(student.Profile.EmailAddress))
                        {
                            StudentWrapper sw = new StudentWrapper(student);
                            students.Add(student.Profile.EmailAddress, sw);
                        }
                        else
                        {
                            students[student.Profile.EmailAddress].ImportInfo(student);
                        }
                    }
                }
            }

            return students;
        }

        public ClassroomManager(string applicationName, UserCredential credential)
        {
            InitializeService(applicationName, credential);
        }

        public void PrintWorksFrom(string pattern)
        {
            List<KeyValuePair<string, StudentWrapper>> found = Students.Where(x => x.Value.Profile.EmailAddress.Contains(pattern)).ToList();
            if (found.Count > 0)
            {
                int val;
                if (found.Count > 1)
                {
                    while (true)
                    {
                        Console.WriteLine("Found multiple users");
                        for (int i = 0; i < found.Count; i++)
                        {
                            Console.WriteLine($"{i}: {found[i].Value.Profile.EmailAddress}");
                        }
                        Console.Write("Selection: ");
                        string resp;
                        resp = Console.ReadLine();
                        bool success = int.TryParse(resp, out val);
                        if (success) break;
                    }
                }
                else
                {
                    val = 0;
                }
                foreach (var course in found[val].Value.Courses)
                {
                    var req = Service.Courses.CourseWork.List(course);
                    var res = req.Execute();
                    if (res.CourseWork != null && res.CourseWork.Count > 0)
                    {
                        var ex = res.CourseWork.Where(x => x.WorkType == "ASSIGNMENT");

                        foreach (var work in ex)
                        {
                            Console.WriteLine($"{work.Title} - {work.State}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Found nothing");
            }
        }

        public void InitializeService(string appName, UserCredential cred)
        {
            Service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = appName,
            });
            ConsoleHelper.Info($"Initialized ClassroomService for {Service.ApplicationName}.");
        }

        public IEnumerable<string> GetClassromCalendars() => Courses.Select(x => x.CalendarId);

        public IList<Course> GetActiveCourses()
        {
            var req = Service.Courses.List();
            req.CourseStates = CoursesResource.ListRequest.CourseStatesEnum.ACTIVE;
            var res = req.Execute();
            ConsoleHelper.Success($"Retrieved {res.Courses.Count} courses.");
            return res.Courses;
        }
    }
}
