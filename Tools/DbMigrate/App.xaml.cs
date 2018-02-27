using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DbMigrate
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var t = new Thread(Migration.DoIt);
            t.SetApartmentState(ApartmentState.MTA);
            t.Start();
        }
    }
}
