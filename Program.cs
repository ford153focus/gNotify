using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace gNotify {
    static class Program {
        static public IConfiguration Configuration { get; set; }

        static void Main () {
            var configurationBuilder = new ConfigurationBuilder ();
            configurationBuilder.SetBasePath (Directory.GetCurrentDirectory ());
            configurationBuilder.AddJsonFile ("config.json", optional : false, reloadOnChange : false);
            Configuration = configurationBuilder.Build ();

            var unreadCount = GetUnreadCount ();

            if (unreadCount > 0) {
                DisplayNotification (unreadCount);
            }

            Application.Run ();
        }

        /// <summary>Display icon and notification in tray</summary>
        static void DisplayNotification (int unreadCount) {
            var notifyIcon = new NotifyIcon ();

            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipText = "Кликните на сообщение для перехода в web-интерфейс";
            notifyIcon.BalloonTipTitle = "Непрочитанных сooбщений: " + unreadCount;
            notifyIcon.Icon = new Icon ("gmail.ico");
            notifyIcon.Text = notifyIcon.BalloonTipTitle;
            notifyIcon.Visible = true;

            notifyIcon.BalloonTipClicked += (sender, e) => { OpenUrl ("https://mail.google.com/mail/u/0/#all"); };
            notifyIcon.BalloonTipClosed += (sender, e) => { OpenUrl ("https://mail.google.com/mail/u/0/#all"); };
            notifyIcon.Click += (sender, e) => { OpenUrl ("https://mail.google.com/mail/u/0/#all"); };

            notifyIcon.ShowBalloonTip (12345);
        }

        /// <summary>Get path to system default browser</summary>
        static string GetSystemDefaultBrowser () {
            string name = string.Empty;
            RegistryKey regKey = null;

            try {
                var regDefault = Registry.CurrentUser.OpenSubKey ("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.htm\\UserChoice", false);
                var stringDefault = regDefault.GetValue ("ProgId");

                regKey = Registry.ClassesRoot.OpenSubKey (stringDefault + "\\shell\\open\\command", false);
                name = regKey.GetValue (null).ToString ().ToLower ().Replace ("" + (char) 34, "");

                if (!name.EndsWith ("exe"))
                    name = name.Substring (0, name.LastIndexOf (".exe") + 4);

            } catch {
                name = "";
            } finally {
                if (regKey != null)
                    regKey.Close ();
            }

            return name;
        }

        /// <summary>Get number of unread mails in account</summary>
        /// <returns>Number of unread mails</returns>
        static int GetUnreadCount () {
            var client = new ImapClient ();
            client.Connect ("imap.gmail.com", 993, true);
            client.Authenticate (Configuration["gmail_login"], Configuration["gmail_password"]);
            IMailFolder allMailFolder;
            try {
                allMailFolder = client.GetFolder ("[Gmail]/All Mail");
            } catch (System.Exception) {
                allMailFolder = client.GetFolder ("[Gmail]/Вся почта");
            }
            allMailFolder.Open (FolderAccess.ReadOnly);
            var uids = allMailFolder.Search (SearchQuery.NotSeen);
            return uids.Count;
        }

        /// <summary>Put browser window to foreground</summary>
        /// <remarks>unfinished</remarks>
        static void moveBrowserWindow () {
            var browserExecutablePath = GetSystemDefaultBrowser ();
            Process[] processes = Process.GetProcesses();
            Console.WriteLine();
        }

        /// <summary>Open given URL in browser window</summary>
        private static void OpenUrl (string url) {
            // workaround for https://github.com/dotnet/corefx/issues/10361
            try {
                Process.Start (url);
            } catch {
                if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
                    url = url.Replace ("&", "^&");
                    Process.Start (new ProcessStartInfo ("cmd", $"/c start {url}") { CreateNoWindow = true });
                } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
                    Process.Start ("xdg-open", url);
                } else if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
                    Process.Start ("open", url);
                } else {
                    throw;
                }
            }

            Application.Exit ();
        }
    }
}