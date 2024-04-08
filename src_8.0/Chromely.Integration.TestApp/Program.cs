using System.Diagnostics;
using Chromely;
using Chromely.Core;
using Chromely.Core.Configuration;
using Chromely.Core.Host;
using Chromely.Browser;
using Chromely.Core.Infrastructure;

string TraceSignature = "CI-TRACE:";
Stopwatch _startupTimer;

void CiTrace(string key, string value)
{
    Console.WriteLine($"{TraceSignature} {key}={value}");
}

CiTrace("Application", "Started");
// measure startup time (maybe including CEF download)
_startupTimer = new Stopwatch();
_startupTimer.Start();
var core = typeof(IChromelyConfiguration).Assembly;
CiTrace("Chromely.Core", core.GetName().Version?.ToString() ?? "");
CiTrace("Platform", ChromelyRuntime.Platform.ToString());

var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
CiTrace("AppDirectory", appDirectory);
var startUrl = $"file:///{appDirectory}/index.html";

var config = DefaultConfiguration.CreateForRuntimePlatform();
config.CefDownloadOptions = new CefDownloadOptions(true, true);
config.WindowOptions.Position = new WindowPosition(1, 2);
config.WindowOptions.Size = new WindowSize(1000, 600);
config.StartUrl = startUrl;
config.DebuggingMode = true;
config.WindowOptions.RelativePathToIconFile = "chromely.ico";

CiTrace("Configuration", "Created");
try
{
    var builder = AppBuilder.Create();
    builder = builder.UseConfig<DefaultConfiguration>(config);
    builder = builder.UseWindow<TestWindow>();
    builder = builder.UseApp<ChromelyBasicApp>();
    builder = builder.Build();
    builder.Run(args);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    throw;
}
CiTrace("Application", "Done");
public class TestWindow : Window
{
    public TestWindow(IChromelyNativeHost nativeHost,
        IChromelyConfiguration config,
        ChromelyHandlersResolver handlersResolver)
        : base(nativeHost, config, handlersResolver)
    {

        #region Events
        FrameLoadStart += TestWindow_FrameLoadStart;
        FrameLoadEnd += TestWindow_FrameLoadEnd;
        LoadingStateChanged += TestWindow_LoadingStateChanged;
        ConsoleMessage += TestWindow_ConsoleMessage;
        AddressChanged += TestWindow_AddressChanged;
        #endregion Events

    }

    #region Events
    private void TestWindow_AddressChanged(object sender, AddressChangedEventArgs e)
    {
        Console.WriteLine("AddressChanged event called.");
    }

    private void TestWindow_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
    {
        Console.WriteLine("ConsoleMessage event called.");
    }

    private void TestWindow_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
    {
        Console.WriteLine("LoadingStateChanged event called.");
    }

    private void TestWindow_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
    {
        Console.WriteLine("FrameLoadStart event called.");
    }

    private void TestWindow_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
    {
        Console.WriteLine("FrameLoadEnd event called.");
    }

    #endregion Events
}