﻿using Echevil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MyConfigure;
using System.Threading;

namespace NetworkSpeedViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NetworkAdapter[] adapters;
        private NetworkAdapter adapterTemp;
        private NetworkMonitor monitor;
        System.Timers.Timer swTimer = new System.Timers.Timer();
        System.Timers.Timer speedTimer = new System.Timers.Timer();
        public MainWindow()
        {
            InitializeComponent();
            swTimer.Interval = 1000;
            swTimer.Elapsed += swTimer_Elapsed;
            speedTimer.Interval = 1000;
            speedTimer.Elapsed += speedTimer_Elapsed;
        }

        void speedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            NetworkAdapter adapter = adapterTemp;
            new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    //下载速度显示
                    mainDownload.Text = Utility.NumberToDlSpeed(adapter.DownloadSpeed);
                    //上载速度显示
                    mainUpload.Text = Utility.NumberToUlSpeed(adapter.UploadSpeed);
                }));
            }).Start();
        }

        void swTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SettingsWin sw = SettingsWin.GetInstance();
            new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (!sw.IsVisible)
                    {
                        mainFunction();
                        swTimer.Stop();
                    }
                }));
            }).Start();

        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainFunction();
        }
        private void mainFunction()
        {
            this.adapters = null;
            this.adapters = FoundNetworkAdapter();
            if (adapters.Length == 0)
            {
                MessageBox.Show("未在本机找到网络适配器！请检查硬件或者相应驱动是否正确安装");
                return;
            }
            else if (!File.Exists(Environment.CurrentDirectory + @"/settings.ini") || ConfigureFile.INIGetStringValue(Environment.CurrentDirectory + @"/settings.ini", "AdapterProperty", "SelectedAdapter", "$null") == "$null")
            {
                MessageBox.Show("您的电脑上存在多个网络适配器，请选择当前使用的适配器");
                if (!File.Exists(Environment.CurrentDirectory + @"/settings.ini"))
                    File.Create(Environment.CurrentDirectory + @"/settings.ini");
                ConfigureFile.INIWriteItems(Environment.CurrentDirectory + @"/settings.ini", "AdapterProperty", "SelectedAdapter=$null");
                SettingsWin sw = SettingsWin.GetInstance();
                sw.Show();
                swTimer.Start();
            }
            else
            {
                string selectedAdapter = ConfigureFile.INIGetStringValue(Environment.CurrentDirectory + @"/settings.ini", "AdapterProperty", "SelectedAdapter", "$null");
                foreach (NetworkAdapter item in this.adapters)
                {
                    if (item.Name == selectedAdapter)
                    {
                        adapterTemp = item;
                        monitor.StopMonitoring();
                        monitor.StartMonitoring(item);
                        speedTimer.Start();
                        break;
                    }
                }
            }
        }

        private NetworkAdapter[] FoundNetworkAdapter()
        {
            NetworkAdapter[] adaptersTemp;
            monitor = new NetworkMonitor();
            adaptersTemp = monitor.Adapters;
            return adaptersTemp;
        }
    }
}
