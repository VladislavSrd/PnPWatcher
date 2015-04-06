using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NLog;

namespace UsbWatcher
{
    public partial class Client : Form
    {
        private List<string> _DeviceList;
        private int _countKeyboard;
        private int _countMouse;
        static Logger _logger = LogManager.GetLogger("Client");
        private const string _serverIP = "192.168.101.180";

        static void SendMessageFromSocket(string ip, int port,string dId)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];

            // Устанавливаем удаленную точку для сокета
            IPAddress ipAddr = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            try
            {
                sender.Connect(ipEndPoint);
                var message = DateTime.Now + ": Отключенно устройство: id " + dId;
                byte[] msg = Encoding.UTF8.GetBytes(message);
                // Отправляем данные через сокет
                sender.Send(msg);
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }   catch (SocketException e)   {
                _logger.Error(DateTime.Now + ": Исключение: Не удалось подключиться к серверу " + e.Message);
            }   catch (ArgumentNullException ane)   {
                _logger.Error(DateTime.Now + ": Исключение: NULL " + ane.Message);
            }       catch (Exception e)     {
                _logger.Error(DateTime.Now + ": Неизвестное исключение:  " + e.Message);
            }      
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject) e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                if (property.Name.Equals("Description") || (property.Name.Equals("DeviceID")))
                {
                    if (_DeviceList.Contains(property.Value.ToString()))
                    {
                        _logger.Info(DateTime.Now + " : Отключено: " + property.Value.ToString());
                        SendMessageFromSocket(_serverIP, 13000, property.Value.ToString());
                    }
                }
            }
        }

        private ManagementObjectCollection ps2KeyboardInit()
        {
            var opts = new ConnectionOptions();
            var scope = new ManagementScope(@"\\.\root\cimv2", opts);
            const string query = "select * from Win32_Keyboard";
            //const string query = "select * from Win32_PointingDevice";
            var oQuery = new ObjectQuery(query);
            var searcher = new ManagementObjectSearcher(scope, oQuery);
            ManagementObjectCollection recordSet = searcher.Get();
            return recordSet;
        }

        private ManagementObjectCollection ps2MouseInit()
        {
            var opts = new ConnectionOptions();
            var scope = new ManagementScope(@"\\.\root\cimv2", opts);
            const string query = "select * from Win32_PointingDevice";
            var oQuery = new ObjectQuery(query);
            var searcher = new ManagementObjectSearcher(scope, oQuery);
            ManagementObjectCollection recordSet = searcher.Get();
            return recordSet;
        }

        public Client()
        {       
            InitializeComponent();
            
            _DeviceList = new List<string>() { "Клавиатура HID", "Универсальный монитор PnP", "HID-совместимая мышь" };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Opacity = 0.0f;
            Hide();
            Visible = false;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();    
        }
        // Win32_USBHub
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var removeQuery = new WqlEventQuery(
                    "SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");
            var removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ManagementObjectCollection ps2Keyboard = ps2KeyboardInit();
            //ManagementObjectCollection ps2Mouse = ps2MouseInit();
            if (ps2Keyboard.Count < _countKeyboard)
            {
                SendMessageFromSocket(_serverIP, 13000, "ps2/usb mouse or keyboard");
                _logger.Info(DateTime.Now + " : Отключено: ps2/usb mouse or keyboard");
            }
           /* if (ps2Mouse.Count < _countMouse)
            {
                SendMessageFromSocket("127.0.0.1", 13000, "ps2/usb mouse");
            }
             _countMouse = ps2Mouse.Count;
            */
            _countKeyboard = ps2Keyboard.Count;
            
        }
    }
}
