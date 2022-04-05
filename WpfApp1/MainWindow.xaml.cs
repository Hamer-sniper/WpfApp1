using System;
using System.IO;
using System.Collections.Generic;
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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TelegramMessageClient client;

        public MainWindow()
        {
            InitializeComponent();

            client = new TelegramMessageClient(this);

            logList.ItemsSource = client.BotMessageLog;
        }

        private void btnMsgSendClick(object sender, RoutedEventArgs e)
        {
            client.SendMessage(txtMsgSend.Text, TargetSend.Text);
            txtMsgSend.Text = String.Empty;
        }

        private void MenuItem_ListFiles_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", Directory.GetCurrentDirectory());
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "Закрыть приложение?",
                "Сообщение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
            if (res == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void MenuItem_SaveHistory_Click(object sender, RoutedEventArgs e)
        {
            client.SerializeToJson(client.BotMessageLog);
        }
    }
}
