using Echevil;
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

namespace NetworkSpeedViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NetworkAdapter[] adapters;
        private NetworkMonitor monitor;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.adapters = null;
            this.adapters = FoundNetworkAdapter();
            if (adapters.Length == 0)
            {
                MessageBox.Show("未在本机找到网络适配器！请检查硬件或者相应驱动是否正确安装");
                return;
            }
            if (!File.Exists(Environment.CurrentDirectory + @"/settings.ini") || ConfigureFile.INIGetStringValue(Environment.CurrentDirectory + @"/settings.ini", "AdapterProperty", "SelectedAdapter", "$null") == "$null")
            {
                MessageBox.Show("您的电脑上存在多个网络适配器，请选择当前使用的适配器");
                if (!File.Exists(Environment.CurrentDirectory + @"/settings.ini"))
                    File.Create(Environment.CurrentDirectory + @"/settings.ini");
                ConfigureFile.INIWriteItems(Environment.CurrentDirectory + @"/settings.ini", "AdapterProperty", "SelectedAdapter=$null");
                SettingsWin sw = SettingsWin.GetInstance();
                sw.Show();
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
