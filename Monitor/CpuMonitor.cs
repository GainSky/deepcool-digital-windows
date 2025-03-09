using LibreHardwareMonitor.Hardware;

namespace DeepCool_Display.Monitor;

public class CpuMonitor : IDisposable
{
    private readonly Computer _computer;
    private IHardware? _cpuHardware;
    private ISensor? _tempSensor;
    private ISensor? _usageSensor;

    public CpuMonitor()
    {
        _computer = new Computer { IsCpuEnabled = true };
        _computer.Open();
        InitializeSensors();
    }

    private void InitializeSensors()
    {
        _cpuHardware = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        if (_cpuHardware == null) return;

        _cpuHardware.Update();

        foreach (var sensor in _cpuHardware.Sensors)
        {
            sensor.ValuesTimeWindow = TimeSpan.Zero; // Disable historical data storage
        }

        _tempSensor = _cpuHardware.Sensors
            .Where(s => s.SensorType == SensorType.Temperature)
            .OrderBy(s => s.Index)
            .FirstOrDefault();

        _usageSensor = _cpuHardware.Sensors
            .Where(s => s.SensorType == SensorType.Load)
            .OrderBy(s => s.Index)
            .FirstOrDefault();
    }

    public float GetTemperature(bool inFahrenheit = false)
    {
        _cpuHardware?.Update();
        var temperature = _tempSensor?.Value ?? 0;

        return inFahrenheit ? ConvertCelsiusToFahrenheit(temperature) : temperature;
    }

    public float GetUsage()
    {
        _cpuHardware?.Update();

        return _usageSensor?.Value ?? 0;
    }

    private static float ConvertCelsiusToFahrenheit(float temperature)
    {
        return (temperature * 9 / 5) + 32;
    }

    public void Dispose()
    {
        _computer.Close();
        GC.SuppressFinalize(this);
    }
}