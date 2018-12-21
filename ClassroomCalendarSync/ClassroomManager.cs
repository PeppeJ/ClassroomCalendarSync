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
                if (_courses == null) { Courses = GetCourses().ToArray(); }
                return _courses;
            }
            private set { _courses = value; }
        }

        public ClassroomManager(string applicationName, UserCredential credential)
        {
            InitializeService(applicationName, credential);
        }

        public void InitializeService(string appName, UserCredential cred)
        {
            Service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = appName,
            });
            ConsoleHelper.Info($"Initialized ClassroomService for {Service.ApplicationName}");
        }

        public IList<Course> GetCourses()
        {
            CoursesResource.ListRequest req = Service.Courses.List();
            req.CourseStates = CoursesResource.ListRequest.CourseStatesEnum.ACTIVE;
            return req.Execute().Courses;
        }
    }
}
