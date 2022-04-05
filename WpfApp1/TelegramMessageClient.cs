using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;

namespace WpfApp1
{

    class TelegramMessageClient
    {
        private MainWindow w;

        private static TelegramBotClient bot;
        public ObservableCollection<MessageLog> BotMessageLog { get; set; }

        private void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";
           
            Random random = new Random();

            Debug.WriteLine($"{text} TypeMessage: {e.Message.Type.ToString()}");

            switch (e.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Document: DownLoad(e.Message.Document.FileId, e.Message.Document.FileName); break;
                case Telegram.Bot.Types.Enums.MessageType.Audio: DownLoad(e.Message.Audio.FileId, e.Message.Audio.FileName); break;
                case Telegram.Bot.Types.Enums.MessageType.Photo:
                    string fileName = $"{DateTimeToName()}_{random.Next(0, 100)}.jpg";
                    DownLoad(e.Message.Photo[3].FileId, fileName);
                    string dir = Directory.GetCurrentDirectory() + @"\";
                    Uri baseUri = new Uri(dir);                    
                    Thread.Sleep(2000);
                    w.Dispatcher.Invoke(() =>
                    {
                       this.w.SendedPicture.Source = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri (baseUri, @"_" + fileName));
                    });
                    break;
                case Telegram.Bot.Types.Enums.MessageType.Video: DownLoad(e.Message.Video.FileId, e.Message.Video.FileName); break;
                case Telegram.Bot.Types.Enums.MessageType.Voice: DownLoad(e.Message.Voice.FileId, $"{DateTimeToName()}_{random.Next(0, 100)}.ogg"); break;
                default: if (e.Message.Text == null) return; break;
            }
            SendedMessages(e);

            var messageText = e.Message.Text;

            if (messageText == null)
                messageText = "Прикрепление файла";

            w.Dispatcher.Invoke(() =>
            {
                BotMessageLog.Add(
                new MessageLog(
                    DateTime.Now.ToLongTimeString(), messageText, e.Message.Chat.FirstName, e.Message.Chat.Id));
            });
        }

        public TelegramMessageClient(MainWindow W, string PathToken = @"D:\Programming\DzSkillbox\ConsoleApp1\Info\t_bot.txt")
        {
            this.BotMessageLog = new ObservableCollection<MessageLog>();
            this.w = W;

            bot = new TelegramBotClient(System.IO.File.ReadAllText(PathToken, Encoding.UTF8));

            bot.OnMessage += MessageListener;

            bot.StartReceiving();
        }

        public void SendMessage(string Text, string Id)
        {
            long id = Convert.ToInt64(Id);
            bot.SendTextMessageAsync(id, Text);
        }

        static async void SendedMessages(MessageEventArgs e)
        {
            var messageText = e.Message.Text;
            string text = $"Загрузите свой файл, просмотрите список файлов и скачайте любой из них!\nТакже через <Название> можно найти информацию о чем угодно из Wikipedia)";

            if ((messageText == "d" || messageText == "D") && System.IO.File.Exists(e.Message.ReplyToMessage.Text))
            {
                DownloadChosenFile(e, e.Message.ReplyToMessage.Text);
                return;
            }
            switch (messageText)
            {
                case "Список файлов": ListMyDownloadedFiles(e); break;
                case "/start":
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, $"Приветствие от бота!");
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, text, replyMarkup: GetButtons());
                    break;
                case null:
                case "Прикрепление файла": break;
                default: await bot.SendTextMessageAsync(e.Message.Chat.Id, WikiInfo(e), replyMarkup: GetButtons()); break;
            }
        }

        static string WikiInfo(MessageEventArgs e)
        {
            string myWebAdress = "https://ru.wikipedia.org/wiki/";            
            myWebAdress += e.Message.Text.Replace(" ", "_");
            return myWebAdress;
        }

        static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup()
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = "Список файлов"} }
                }
            };
        }

        static void ListMyDownloadedFiles(MessageEventArgs e)
        {
            var myFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "_*");
            foreach (var file in myFiles)
                bot.SendTextMessageAsync(e.Message.Chat.Id, Path.GetFileName(file));
            bot.SendTextMessageAsync(e.Message.Chat.Id, "Чтобы скачать какой-либо файл, ответьте на сообщение с ним буквой \"d\"");
        }

        static async void DownloadChosenFile(MessageEventArgs e, string myFileDownload)
        {
            using (FileStream stream = System.IO.File.OpenRead(myFileDownload))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, myFileDownload);
                await bot.SendDocumentAsync(e.Message.Chat.Id, inputOnlineFile);
            }
        }        

        static async void DownLoad(string fileId, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            FileStream fs = new FileStream("_" + path, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();

            fs.Dispose();
        }

        private static string DateTimeToName()
        {
            string str = DateTime.Now.ToString();
            str = str.Replace(" ", string.Empty);
            str = str.Replace(":", string.Empty);
            str = str.Replace(".", string.Empty);
            return str;
        }    

        public async void SerializeToJson(ObservableCollection<MessageLog> BotMessageLog)
        {
            string json = JsonConvert.SerializeObject(BotMessageLog);
            System.IO.File.WriteAllText($"Chat_history_" + DateTimeToName() + ".json",json);
        }
    }
}
