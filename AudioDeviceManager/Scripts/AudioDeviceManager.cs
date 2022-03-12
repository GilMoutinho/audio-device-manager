using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Class with methods to show and switch audio devices.
/// </summary>
public static class AudioDeviceManager
{
    private const string AudioDevicesFile = "audioDevices.txt";

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
                // TODO: Uncomment after the method is fixed.
                //return GetCurrentDeviceWindows(deviceRole);
                Debug.LogError("GetCurrentDevice is not working in Windows.");
                return null;
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
    /// FIXME: This method is not working.
    /// </summary>
    /// <param name="deviceRole">Role of the device</param>
    /// <returns>The current audio output device</returns>
    private static Device GetCurrentDeviceWindows(DeviceRole deviceRole)
    {
        const int DefaultConsoleDeviceToken = 5;
        int defaultDeviceToken = DefaultConsoleDeviceToken + ((int)deviceRole);
        string arguments = "/scomma " + AudioDevicesFile + " && " +
            "for /f \"tokens=1,4," + defaultDeviceToken + " delims=,\" " +
            "%a in ('type " + AudioDevicesFile + " ^| find \"Render\"') " +
            "do @echo %a (%b),%c";
        Device currentDevice = null;
        AudioDeviceManagerCommands.ExecuteCommand(arguments, (output) =>
        {
            foreach (string deviceString in output)
            {
                string[] splitDeviceString = deviceString.Split(',');
                if (splitDeviceString[1] == "Render")
                {
                    currentDevice = new Device(splitDeviceString[0]);
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
        string arguments;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                // FIXME: This is not working.
                //arguments = "/scomma " + AudioDevicesFile + " && " +
                //    "for /f \"tokens=1,4 delims=,\" " +
                //    "%a in ('type " + AudioDevicesFile + " ^| find \"Render\"') " +
                //    "do @echo %a (%b)";
                //break;
                Debug.LogError("GetAllDevices is not working in Windows.");
                return null;
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                arguments = "-a -t output";
                break;
            default:
                Debug.LogError("AudioDeviceManager is only available for Windows and macOS!");
                return null;
        }

        Device[] devices = new Device[0];
        AudioDeviceManagerCommands.ExecuteCommand(arguments, (output) =>
        {
            devices = output.Select(deviceName => new Device(deviceName)).ToArray();
        });
        return devices;
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
