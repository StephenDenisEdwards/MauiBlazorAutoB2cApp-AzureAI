using Microsoft.Maui.Devices.Sensors;

namespace MauiBlazorAutoB2cApp.Shared.Services;

public class AccelerometerService : IAccelerometerService
{
	public event EventHandler<AccelerometerChangedEventArgs>? OnReadingChanged;

	public void Start()
	{
		if (Accelerometer.Default.IsSupported && !Accelerometer.Default.IsMonitoring)
		{
			Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
			Accelerometer.Default.Start(SensorSpeed.UI);
		}
	}

	public void Stop()
	{
		if (Accelerometer.Default.IsMonitoring)
		{
			Accelerometer.Default.ReadingChanged -= Accelerometer_ReadingChanged;
			Accelerometer.Default.Stop();
		}
	}

	public bool IsSupported => true;

	private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
	{
		OnReadingChanged?.Invoke(this, e);
	}

	public async ValueTask DisposeAsync()
	{
		await Task.Run(() => Stop());
	}
}
