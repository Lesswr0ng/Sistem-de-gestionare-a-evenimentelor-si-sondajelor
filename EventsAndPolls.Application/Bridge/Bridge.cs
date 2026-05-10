namespace EventsAndPolls.Application.Bridge;

public interface IDevice
{
     bool IsEnabled { get; }
     int Volume { get; }
     int Channel { get; }
     void Enable();
     void Disable();
     void SetVolume(int percent);
     void SetChannel(int channel);
     void PrintStatus();
}

// Concrete Implementors — each device has its own internal logic
public class Television : IDevice
{
     public bool IsEnabled { get; private set; }
     public int Volume { get; private set; } = 30;
     public int Channel { get; private set; } = 1;

     public void Enable() { IsEnabled = true; Console.WriteLine("[TV] Turned ON"); }
     public void Disable() { IsEnabled = false; Console.WriteLine("[TV] Turned OFF"); }
     public void SetVolume(int percent) { Volume = percent; Console.WriteLine($"[TV] Volume set to {percent}%"); }
     public void SetChannel(int channel) { Channel = channel; Console.WriteLine($"[TV] Channel set to {channel}"); }
     public void PrintStatus() =>
         Console.WriteLine($"[TV] Status: {(IsEnabled ? "ON" : "OFF")} | Vol: {Volume}% | Ch: {Channel}");
}

public class Radio : IDevice
{
     public bool IsEnabled { get; private set; }
     public int Volume { get; private set; } = 50;
     public int Channel { get; private set; } = 1;

     public void Enable() { IsEnabled = true; Console.WriteLine("[Radio] Turned ON"); }
     public void Disable() { IsEnabled = false; Console.WriteLine("[Radio] Turned OFF"); }
     public void SetVolume(int percent) { Volume = percent; Console.WriteLine($"[Radio] Volume set to {percent}%"); }
     public void SetChannel(int channel) { Channel = channel; Console.WriteLine($"[Radio] Frequency set to {channel} MHz"); }
     public void PrintStatus() =>
         Console.WriteLine($"[Radio] Status: {(IsEnabled ? "ON" : "OFF")} | Vol: {Volume}% | Freq: {Channel} MHz");
}

public class AirConditioner : IDevice
{
     public bool IsEnabled { get; private set; }
     public int Volume { get; private set; }   // fan speed
     public int Channel { get; private set; }  // temperature

     public void Enable() { IsEnabled = true; Console.WriteLine("[AC] Turned ON"); }
     public void Disable() { IsEnabled = false; Console.WriteLine("[AC] Turned OFF"); }
     public void SetVolume(int percent) { Volume = percent; Console.WriteLine($"[AC] Fan speed set to {percent}%"); }
     public void SetChannel(int channel) { Channel = channel; Console.WriteLine($"[AC] Temperature set to {channel}°C"); }
     public void PrintStatus() =>
         Console.WriteLine($"[AC] Status: {(IsEnabled ? "ON" : "OFF")} | Fan: {Volume}% | Temp: {Channel}°C");
}

// Abstraction — basic remote control, holds the bridge reference to IDevice
public class RemoteControl
{
     // The Bridge — reference to the implementor
     protected readonly IDevice _device;

     public RemoteControl(IDevice device)
     {
          _device = device;
     }

     public void TogglePower()
     {
          if (_device.IsEnabled)
               _device.Disable();
          else
               _device.Enable();
     }

     public void VolumeDown() => _device.SetVolume(_device.Volume - 10);
     public void VolumeUp() => _device.SetVolume(_device.Volume + 10);
     public void ChannelDown() => _device.SetChannel(_device.Channel - 1);
     public void ChannelUp() => _device.SetChannel(_device.Channel + 1);
     public void PrintStatus() => _device.PrintStatus();
}

public class AdvancedRemoteControl : RemoteControl
{
     public AdvancedRemoteControl(IDevice device) : base(device) { }

     public void Mute()
     {
          Console.WriteLine("[AdvancedRemote] Muting device");
          _device.SetVolume(0);
     }

     public void SetFavoriteChannel(int channel)
     {
          Console.WriteLine($"[AdvancedRemote] Jumping to favourite channel: {channel}");
          _device.SetChannel(channel);
     }
}

//   Exemplu utilizare
//   var tvRemote    = new AdvancedRemoteControl(new Television());
//   var radioRemote = new AdvancedRemoteControl(new Radio());
//   var acRemote    = new RemoteControl(new AirConditioner());
//
//   tvRemote.TogglePower();          // [TV] Turned ON
//   tvRemote.SetFavoriteChannel(5);  // [TV] Channel set to 5
//   tvRemote.Mute();                 // [TV] Volume set to 0
//
//   radioRemote.TogglePower();       // [Radio] Turned ON
//   radioRemote.VolumeUp();          // [Radio] Volume set to 60%
//
//   acRemote.TogglePower();          // [AC] Turned ON
//   acRemote.ChannelUp();            // [AC] Temperature set to 1°C