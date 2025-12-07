using DeepCool_Display.Devices;
using DeepCool_Display.Monitor;
using DeepCool_Display.Utils;

namespace DeepCool_Display;

public class TrayApplicationContext : ApplicationContext
{
    private readonly TrayIconManager _trayIconManager;
    private readonly CpuMonitor _cpuMonitor;
    private readonly AkSeriesDevice _akDevice;
    private readonly System.Windows.Forms.Timer _updateTimer;

    private const string CpuTemperatureOptionText = "Show CPU Temperature";
    private const string CpuUsageOptionText = "Show CPU Usage";

    private bool _isFahrenheit;
    private bool _showCpuUsage;
    private bool _autoChange;
    private readonly Random _random = new Random();

    public TrayApplicationContext()
    {
        _isFahrenheit = Properties.Settings.Default.UseFahrenheit;
        _showCpuUsage = Properties.Settings.Default.ShowCpuUsage;

        _trayIconManager = new TrayIconManager();
        _cpuMonitor = new CpuMonitor();
        _akDevice = new AkSeriesDevice();

        _updateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();

        _changeTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        _changeTimer.Tick += ChangeTimer_Tick;
        _changeTimer.start();

        _trayIconManager.TemperatureUnitToggled += TrayIconManager_TemperatureUnitToggled;
        _trayIconManager.DisplayModeToggled += TrayIconManager_DisplayModeToggled;
        _trayIconManager.ExitClicked += TrayIconManager_ExitClicked;
        _trayIconManager.AutoStartToggled += TrayIconManager_AutoStartToggled;

        InitializeAutoStartState();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (_showCpuUsage)
        {
            var usage = _cpuMonitor.GetUsage();
            _akDevice.SetDisplayMode(DisplayMode.CpuUsage);
            _akDevice.UpdateDisplay(cpuTemperature: 0, cpuUsage: usage);
        }
        else
        {
            var temperature = _cpuMonitor.GetTemperature(_isFahrenheit);
            _akDevice.SetDisplayMode(DisplayMode.CpuTemperature);
            _akDevice.UpdateDisplay(cpuTemperature: temperature, cpuUsage: 0, isFahrenheit: _isFahrenheit);
        }
    }

    private void ChangeTimer_Tick(object? sender, EventArgs e)
    {
        if (_autoChange)
        {
            _showCpuUsage = !_showCpuUsage;
        }
    }
    
    private void TrayIconManager_TemperatureUnitToggled(object? sender, bool isFahrenheit)
    {
        _isFahrenheit = isFahrenheit;
    }

    private void TrayIconManager_DisplayModeToggled(object? sender, EventArgs e)
    {
        _showCpuUsage = !_showCpuUsage;
        _trayIconManager.SetDisplayModeText(_showCpuUsage ? CpuTemperatureOptionText : CpuUsageOptionText);
    }

    private static void TrayIconManager_ExitClicked(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    private static void TrayIconManager_AutoStartToggled(object? sender, bool enabled)
    {
        try
        {
            AutoStartManager.SetAutoStart(enabled);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update Auto-Start setting: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InitializeAutoStartState()
    {
        var isAutoStartEnabled = AutoStartManager.IsAutoStartEnabled();
        _trayIconManager.SetAutoStartMenuItemState(isAutoStartEnabled);

        _trayIconManager.SetTemperatureUnitState(_isFahrenheit);

        _trayIconManager.SetDisplayModeText(_showCpuUsage ? CpuTemperatureOptionText : CpuUsageOptionText);
    }

    private void SaveSettings()
    {
        Properties.Settings.Default.ShowCpuUsage = _showCpuUsage;
        Properties.Settings.Default.UseFahrenheit = _isFahrenheit;

        Properties.Settings.Default.Save();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SaveSettings();

            _updateTimer.Stop();
            _trayIconManager.Dispose();
            _cpuMonitor.Dispose();
            _akDevice.Dispose();
        }

		base.Dispose(disposing);
	}
}
