using MauiBlazorAutoB2cApp.Shared.Services;
using Microsoft.Maui.Devices.Sensors;

namespace MauiBlazorAutoB2bApp.Web.Services;

public class AccelerometerService : IAccelerometerService
{
	public ValueTask DisposeAsync()
	{
		// throw new NotImplementedException();
		return ValueTask.CompletedTask;
	}

	public event EventHandler<AccelerometerChangedEventArgs>? OnReadingChanged;
	public void Start()
	{
		throw new NotSupportedException();
	}

	public void Stop()
	{
		throw new NotSupportedException();
	}
}