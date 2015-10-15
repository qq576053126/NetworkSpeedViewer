#region

using Echevil;
using MyConfigure;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

#endregion

namespace NetworkSpeedViewer
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Timer _speedTimer = new Timer();
        private readonly Timer _swTimer = new Timer();
        private NetworkAdapter[] _adapters;
        private NetworkAdapter _adapterTemp;
        private NetworkMonitor _monitor;
        private NotifyIcon _nofityIcon;

        public MainWindow()
        {
            InitializeComponent();
            _swTimer.Interval = 1000;
            _swTimer.Elapsed += swTimer_Elapsed;
            _speedTimer.Interval = 1000;
            _speedTimer.Elapsed += speedTimer_Elapsed;

            NotifyIconSettings();
        }

        private void NotifyIconSettings()
        {
            //托盘
            _nofityIcon = new NotifyIcon();
            _nofityIcon.Icon = Properties.Resources.meter_128px;
            _nofityIcon.Text = "程序正在运行";
            _nofityIcon.Visible = true;
            //菜单项
            MenuItem settingsMenu = new MenuItem("设置");
            MenuItem exitMenu = new MenuItem("退出");
            exitMenu.Click += exitMenu_Click;
            MenuItem[] menuItems = { settingsMenu, exitMenu };
            _nofityIcon.ContextMenu = new ContextMenu(menuItems);
        }

        //托盘退出按钮
        private void exitMenu_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        //速度刷新计时器方法
        private void speedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            NetworkAdapter adapter = _adapterTemp;
            new Thread(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    //下载速度显示
                    MainDownload.Text = Utility.NumberToDlSpeed(adapter.DownloadSpeed);
                    //上载速度显示
                    MainUpload.Text = Utility.NumberToUlSpeed(adapter.UploadSpeed);
                }));
            }).Start();
        }

        //探测设置窗口是否关闭的计时器方法
        private void swTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SettingsWin sw = SettingsWin.GetInstance();
            new Thread(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (!sw.IsVisible)
                    {
                        MainFunction();
                        _swTimer.Stop();
                    }
                }));
            }).Start();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double screenWidth = SystemParameters.PrimaryScreenWidth;

            double saveLeft =
                Convert.ToDouble(ConfigureFile.INIGetStringValue(Environment.CurrentDirectory + @"/settings.ini",
                    "ProgramSettings", "WinLeft", "-1000"));
            double saveTop =
                Convert.ToDouble(ConfigureFile.INIGetStringValue(Environment.CurrentDirectory + @"/settings.ini",
                    "ProgramSettings", "WinTop", "-1000"));

            if (saveLeft == -1000 || saveLeft >= screenWidth - 2 || saveLeft <= -Width + 2)
                saveLeft = screenWidth - Width;
            if (saveTop == -1000 || saveTop >= screenHeight - 2 || saveTop <= -Top + 2)
                saveTop = screenHeight - Height - (screenHeight / 20);

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = saveLeft;
            Top = saveTop;
            MainFunction();
        }

        private void MainFunction()
        {
            _adapters = null;
            _adapters = FoundNetworkAdapter();
            if (_adapters.Length == 0)
            {
                MessageBox.Show("未在本机找到网络适配器！请检查硬件或者相应驱动是否正确安装");
            }
            if (!File.Exists(Environment.CurrentDirectory + @"/settings.ini") ||
                ConfigureFile.INIGetStringValue(Environment.CurrentDirectory + @"/settings.ini", "AdapterProperty", "SelectedAdapter", "$null") == "$null")
            {
                MessageBox.Show("您的电脑上存在多个网络适配器，请选择当前使用的适配器");
                if (!File.Exists(Environment.CurrentDirectory + @"/settings.ini"))
                    File.Create(Environment.CurrentDirectory + @"/settings.ini");
                ConfigureFile.INIWriteItems(Environment.CurrentDirectory + @"/settings.ini", "AdapterProperty", "SelectedAdapter=$null");
                SettingsWin sw = SettingsWin.GetInstance();
                sw.Show();
                _swTimer.Start();
            }
            else
            {
                string selectedAdapter = ConfigureFile.INIGetStringValue(
                    Environment.CurrentDirectory + @"/settings.ini", "AdapterProperty", "SelectedAdapter", "$null");
                foreach (NetworkAdapter item in _adapters)
                {
                    if (item.Name == selectedAdapter)
                    {
                        _adapterTemp = item;
                        _monitor.StopMonitoring();
                        _monitor.StartMonitoring(item);
                        _speedTimer.Start();
                        break;
                    }
                }
            }
        }

        private NetworkAdapter[] FoundNetworkAdapter()
        {
            _monitor = new NetworkMonitor();
            var adaptersTemp = _monitor.Adapters;
            return adaptersTemp;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            ConfigureFile.INIWriteValue(Environment.CurrentDirectory + @"/settings.ini", "ProgramSettings", "WinLeft", Left.ToString());
            ConfigureFile.INIWriteValue(Environment.CurrentDirectory + @"/settings.ini", "ProgramSettings", "WinTop", Top.ToString());
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _nofityIcon.Visible = false;
            _nofityIcon.Dispose();
            _nofityIcon = null;
        }
    }
}