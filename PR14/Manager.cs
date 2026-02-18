using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PR14
{
    public static class Manager
    {
        public static Frame MainFrame { get; set; }
        public static Users CurrentUser { get; set; }

        public static CinemaEntities GetContext()
        {
            return new CinemaEntities();
        }
    }
}