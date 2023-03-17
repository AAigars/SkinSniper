using SkinSniper.Config;
using SkinSniper.Services.Buff;
using SkinSniper.Services.Skinport;
using SkinSniper.Services.Telegram;
using System.Diagnostics;
using System.Reflection;

namespace SkinSniper
{
    internal class Program
    {
        private static BuffClient? _buff;
        private static SkinportClient? _skinport;
        private static TelegramClient? _telegram;

        static void Main(string[] args)
        {
            SetupTrace();
            ConfigHandler.Load();

            _telegram = new TelegramClient();
            _buff = new BuffClient();
            _skinport = new SkinportClient(_telegram, _buff);

            Console.ReadLine();
        }
        
        static void SetupTrace()
        {
            Trace.Listeners.Clear();

            TextWriterTraceListener twtl = new TextWriterTraceListener(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "log.txt"));
            twtl.Name = "TextLogger";
            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            ctl.TraceOutputOptions = TraceOptions.DateTime;

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
        }
    }
}