# AudioDeviceManager

Unity plugin to show and switch audio output devices on Windows and macOS.

## Description

This plugin uses [SoundVolumeView](https://www.nirsoft.net/utils/sound_volume_view.html) in Windows and [switchaudio-osx](https://github.com/deweller/switchaudio-osx) in macOS.

## Usage

```cs
// Gets the current default audio output device.
AudioDeviceManager.Device currentDevice = AudioDeviceManager.GetCurrentDevice();

// Gets an array with all the available audio output devices.
AudioDeviceManager.Device[] devices = AudioDeviceManager.GetAllDevices();

// Asynchronously changes the current default audio output device. DeviceRole defaults to all.
AudioDeviceManager.SetDefaultDevice("deviceName", AudioDeviceManager.DeviceRole.Console)
```
