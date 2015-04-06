using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NLog;

namespace UsbServer
{

    public partial class Server : Form
    {

        private Logger _logger = LogManager.GetCurrentClassLogger();
        AlertForm _alertForm = new AlertForm();
                 
        ~Server()
        {
        }

        public Server()
        {
            InitializeComponent();       
        }

        private void StartServer(object obj)
        {
            IPAddress ipAddr;
            if (!IPAddress.TryParse(textBox1.Text, out ipAddr))
            {
                AppendNewTextToRichTextBox("Неверно введен ip");
            }
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 13000);

            // Создаем сокет Tcp/Ip
            var sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
            }
            catch (SocketException e)
            {
                MessageBox.Show(@"Возможно приложение уже запущено или неверно указан IP");
                _logger.Warn("Возможно приложение уже запущено или неверно указан IP");
                Application.Restart();
            }
                
                // Начинаем слушать соединения
                while (true)
                {
                    sListener.Listen(10);
                    ThreadPool.QueueUserWorkItem(HandleRequest, sListener.Accept());
                }
        }

        private delegate void app_char(string c);
        private void AppendNewTextToRichTextBox(string c)
        {
            if (InvokeRequired)
            {
                Invoke(new app_char(AppendNewTextToRichTextBox), c);
            }
            else
            {
                richTextBox1.AppendText(c);
            }
        }

        private void HandleRequest(object clientObj)
        {

            // Программа приостанавливается, ожидая входящее соединение
            Socket handler = clientObj as Socket;
            string data = null;

            // Мы дождались клиента, пытающегося с нами соединиться

            byte[] bytes = new byte[1024];
            if (handler != null)
            {
                int bytesRec = handler.Receive(bytes);

                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                AppendNewTextToRichTextBox(handler.RemoteEndPoint.ToString() + " " + data + "\n\n");
                _logger.Info(handler.RemoteEndPoint.ToString() + " " + data);
                System.Media.SystemSounds.Beep.Play();
                System.Media.SystemSounds.Exclamation.Play();
                this.Invoke((MethodInvoker)delegate()
                 {
                     _alertForm.Show();
                     _alertForm.Visible = true;
                 });
                
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void Server_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Server_Load(object sender, EventArgs e)
        {
            _alertForm.Owner = this;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            _logger.Info("Приложение запущено.");
            ThreadPool.QueueUserWorkItem(StartServer);
        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
        }
    }
}
