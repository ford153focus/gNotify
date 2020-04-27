using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace gNotify
{
    internal static class Utils
    {
        /// <summary>Get path to system default browser</summary>
        public static string GetSystemDefaultBrowser()
        {
            string name;
            RegistryKey regKey = null;

            try
            {
                var regDefault = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.htm\\UserChoice", false);
                var stringDefault = regDefault.GetValue("ProgId");

                regKey = Registry.ClassesRoot.OpenSubKey($"{stringDefault}\\shell\\open\\command", false);
                name = regKey.GetValue(null).ToString().ToLower().Replace("" + (char)34, "");

                if (!name.EndsWith("exe"))
                    name = name.Substring(0, name.LastIndexOf(".exe", StringComparison.Ordinal) + 4);

            }
            catch
            {
                name = "";
            }
            finally
            {
                regKey?.Close();
            }

            return name;
        }

        /// <summary>Put browser window to foreground</summary>
        /// <remarks>unfinished</remarks>
        public static void MoveBrowserWindow()
        {
            var browserExecutablePath = GetSystemDefaultBrowser();
            var processes = Process.GetProcesses();
            Console.WriteLine();
        }

        /// <summary>Open given URL in browser window</summary>
        public static void OpenUrl(string url)
        {
            // workaround for https://github.com/dotnet/corefx/issues/10361
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}