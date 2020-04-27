using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Configuration;
using System.Timers;

namespace gNotify
{
    internal static class Program
    {
        private static IConfiguration Configuration { get; set; }
        private static System.Timers.Timer _timer;
        private static NotifyIcon _notify;

        private static void Main()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("config.json", false, false);
            Configuration = configurationBuilder.Build();

            SetIcon();
            SetTimer();

            Application.Run();

            _timer.Stop();
            _timer.Dispose();
        }

        private static void SetIcon()
        {
            _notify = new NotifyIcon();
            _notify.BalloonTipClicked += NotifyIconInteracted;
            // _notify.BalloonTipClosed += NotifyIconInteracted;
            _notify.BalloonTipIcon = ToolTipIcon.Info;
            _notify.BalloonTipText = "Кликните на сообщение для перехода в web-интерфейс";
            _notify.BalloonTipTitle = "Непрочитанных сooбщений: 0";
            _notify.Click += NotifyIconInteracted;
            _notify.Icon = new Icon("gmail.ico");
            _notify.Text = _notify.BalloonTipTitle;
            _notify.Visible = true;

            _notify.Visible = false; /*DO NOT MOVE UP*/
        }

        private static void SetTimer()
        {
            _timer = new System.Timers.Timer(153089);
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer fired");
            var unreadCount = GetUnreadCount();

            if (unreadCount > 0)
            {
                DisplayNotification(unreadCount);
            }
        }

        /// <summary>Get number of unread mails in account</summary>
        /// <returns>Number of unread mails</returns>
        private static int GetUnreadCount()
        {
            var client = new ImapClient();
            client.Connect("imap.gmail.com", 993, true);
            client.Authenticate(Configuration["gmail_login"], Configuration["gmail_password"]);
            IMailFolder allMailFolder;
            try
            {
                allMailFolder = client.GetFolder("[Gmail]/All Mail");
            }
            catch (Exception)
            {
                allMailFolder = client.GetFolder("[Gmail]/Вся почта");
            }
            allMailFolder.Open(FolderAccess.ReadOnly);
            var uids = allMailFolder.Search(SearchQuery.NotSeen);
            return uids.Count;
        }

        /// <summary>Display icon and notification in tray</summary>
        private static void DisplayNotification(int unreadCount)
        {
            _notify.BalloonTipTitle = $"Непрочитанных сooбщений: {unreadCount}";
            _notify.ShowBalloonTip(12345);
            _notify.Text = _notify.BalloonTipTitle;
            _notify.Visible = true;
        }

        private static void NotifyIconInteracted(object sender, EventArgs e)
        {
            Console.WriteLine("Icon clicked");
            Utils.OpenUrl("https://mail.google.com/mail/u/0/#all");
            _notify.Visible = false;
        }
    }
}