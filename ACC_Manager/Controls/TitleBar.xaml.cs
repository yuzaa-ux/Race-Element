﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        public TitleBar()
        {
            InitializeComponent();
            this.Title.Text = $"ACC Manager {GetAssemblyFileVersion()}";

            buttonExit.Click += ButtonExit_Click;

            buttonMinimize.Click += (e, s) => { App.Current.MainWindow.WindowState = WindowState.Minimized; };
            buttonMaximize.Click += (e, s) =>
            {
                if (App.Current.MainWindow.WindowState == WindowState.Maximized)
                    App.Current.MainWindow.WindowState = WindowState.Normal;
                else
                    App.Current.MainWindow.WindowState = WindowState.Maximized;
            };

            this.MouseDoubleClick += TitleBar_MouseDoubleClick;

        }


        private void TitleBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WindowState targetState = WindowState.Normal;
            switch (App.Current.MainWindow.WindowState)
            {
                case WindowState.Normal:
                    {
                        targetState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        targetState = WindowState.Normal;
                        break;
                    }
            }

            App.Current.MainWindow.WindowState = targetState;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public static string GetAssemblyFileVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.FileVersion;
        }
    }
}