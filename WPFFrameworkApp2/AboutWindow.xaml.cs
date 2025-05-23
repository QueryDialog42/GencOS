﻿using System.Windows;
using System.ComponentModel;

namespace WPFFrameworkApp
{
    /// <summary>
    /// AboutWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            WhatAbout = null;
            Version = null;
            AboutMessage = null;
        }
    }
}
