﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace Colab.Develop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    partial class App : Application
    {
       // private TaskbarIcon notifyIcon;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
          //  notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

        }

        protected override void OnExit(ExitEventArgs e)
        {
           // notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
