﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomCalendarSync
{
    class Program
    {
        static void Main(string[] args)
        {
            var sync = new SyncApplication();
            sync.Run();
        }
    }
}
