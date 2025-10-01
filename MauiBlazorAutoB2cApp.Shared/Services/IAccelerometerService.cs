using Microsoft.Maui.Devices.Sensors;

namespace MauiBlazorAutoB2cApp.Shared.Services;

public interface IAccelerometerService : IAsyncDisposable
{
	event EventHandler<AccelerometerChangedEventArgs>? OnReadingChanged;
	void Start();
	void Stop();

	bool IsSupported => false;
}