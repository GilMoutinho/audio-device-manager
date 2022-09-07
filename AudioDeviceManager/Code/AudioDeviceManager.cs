using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Class with methods to show and switch audio devices.
/// </summary>
public static class AudioDeviceManager
{
    /// <summary>
    /// Gets the current default audio output device.
    /// </summary>
    /// <param name="deviceRole">Role of the device (optional, defaults to Console)</param>
    /// <returns>The current audio output device</returns>
    public static Device GetCurrentDevice(DeviceRole deviceRole = DeviceRole.Console)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return GetCurrentDeviceWindows(deviceRole);
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                return GetCurrentDeviceMacOS();
            default:
                Debug.LogError("AudioDeviceManager is only available for Windows and macOS!");
                return null;
        }
    }

    /// <summary>
    /// Gets the current default audio output device in Windows.
    /// </summary>
    /// <param name="deviceRole">Role of the device</param>
    /// <returns>The current audio output device</returns>
    private static Device GetCurrentDeviceWindows(DeviceRole deviceRole)
    {
        string deviceRoleColumn = deviceRole switch
        {
            DeviceRole.Console => "Default",
            DeviceRole.Multimedia => "Default Multimedia",
            DeviceRole.Communications => "Default Communications",
            _ => "Default",
        };
        string arguments = "/scomma \"\" /Columns \"Name,Device Name," + deviceRoleColumn + "\"";
        Device currentDevice = null;
        AudioDeviceManagerCommands.ExecuteCommand(arguments, (output) =>
        {
            foreach (string deviceString in output)
            {
                string[] splitDeviceString = deviceString.Split(',');
                if (splitDeviceString[2] == "Render")
                {
                    currentDevice = new Device(splitDeviceString[0], splitDeviceString[1]);
                    break;
                }
            }
        });
        return currentDevice;
    }

    /// <summary>
    /// Gets the current default audio output device in macOS.
    /// </summary>
    /// <returns>The current audio output device</returns>
    private static Device GetCurrentDeviceMacOS()
    {
        string arguments = "-c -t output";
        Device currentDevice = null;
        AudioDeviceManagerCommands.ExecuteCommand(arguments, (output) =>
        {
            if (output.Count > 0)
            {
                currentDevice = new Device(output[0]);
            }
        });
        return currentDevice;
    }

    /// <summary>
    /// Gets an array with all the available audio output devices.
    /// </summary>
    /// <returns>Array with the current audio output devices</returns>
    public static Device[] GetAllDevices()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return GetAllDevicesWindows();
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                return GetAllDevicesMacOS();
            default:
                Debug.LogError("AudioDeviceManager is only available for Windows and macOS!");
                return null;
        }
    }

    /// <summary>
    /// Gets an array with all the available audio output devices in Windows.
    /// </summary>
    /// <returns>Array with the current audio output devices</returns>
    private static Device[] GetAllDevicesWindows()
    {
        string arguments = "/scomma \"\" /Columns \"Name,Type,Direction,Device Name\"";
        List<Device> devices = new List<Device>();
        AudioDeviceManagerCommands.ExecuteCommand(arguments, (output) =>
        {
            foreach (string deviceString in output)
            {
                string[] splitDeviceString = deviceString.Split(',');
                if (splitDeviceString[1] == "Device" && splitDeviceString[2] == "Render")
                {
                    devices.Add(new Device(splitDeviceString[0], splitDeviceString[3]));
                }
            }
        });
        return devices.ToArray();
    }

    /// <summary>
    /// Gets an array with all the available audio output devices in macOS.
    /// </summary>
    /// <returns>Array with the current audio output devices</returns>
    private static Device[] GetAllDevicesMacOS()
    {
        string arguments = "-a -t output";
        List<Device> devices = new List<Device>();
        AudioDeviceManagerCommands.ExecuteCommand(arguments, (output) =>
        {
            foreach (string deviceName in output)
            {
                devices.Add(new Device(deviceName));
            }
        });
        return devices.ToArray();
    }

    /// <summary>
    /// Asynchronously changes the current default audio output device.
    /// </summary>
    /// <param name="deviceName">Name of the device to set as default</param>
    /// <param name="deviceRole">Role of the device (optional, defaults to all)</param>
    public static void SetDefaultDevice(string deviceName, DeviceRole? deviceRole = null)
    {
        string arguments;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                deviceName = deviceName.Replace("(default)", string.Empty);
                GroupCollection regexGroups = Regex.Match(deviceName, @"([^)(]+)\((.+)\)").Groups;
                string name = regexGroups[1].Value.Trim();
                string manufactor = regexGroups[2].Value.Trim();
                string commandLineFriendlyID = manufactor + @"\Device\" + name + @"\Render";
                string commandLineDeviceRole = deviceRole == null ? "all" : ((int)deviceRole).ToString();
                arguments = "/SetDefault \"" + commandLineFriendlyID + "\" " + commandLineDeviceRole;
                break;
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                arguments = "-t output -s '" + deviceName + "'";
                break;
            default:
                Debug.LogError("AudioDeviceManager is only available for Windows and macOS!");
                return;
        }

        AudioDeviceManagerCommands.ExecuteAsyncCommand(arguments);
    }

    /// <summary>
    /// Class with audio device information.
    /// </summary>
    public class Device
    {
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the device</param>
        public Device(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the device</param>
        /// <param name="manufactor">Manufactor of the device</param>
        public Device(string name, string manufactor)
        {
            Name = $"{name} ({manufactor})";
        }
    }

    /// <summary>
    /// Enum with the different types of audio device roles.
    /// </summary>
    public enum DeviceRole
    {
        Console,
        Multimedia,
        Communications
    }
}
