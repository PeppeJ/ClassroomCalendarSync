using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomCalendarSync
{
    public class StudentWrapper
    {
        public string ID => Profile.Id;
        public List<string> Courses { get; set; } = new List<string>();
        public UserProfile Profile { get; set; }

        public StudentWrapper(Student stud)
        {
            Profile = stud.Profile;
        }

        public void ImportInfo(Student student)
        {
            if (!Courses.Contains(student.CourseId))
                Courses.Add(student.CourseId);
        }
    }
}
