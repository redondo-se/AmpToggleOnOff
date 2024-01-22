using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AmpToggleOnOff
{
    class Program
    {
        [DllImport("usb_relay_device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_init();

        [DllImport("usb_relay_device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_exit();

        [DllImport("usb_relay_device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_open_with_serial_number(string serial_number, uint len);

        [DllImport("usb_relay_device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_open_all_relay_channel(int hHandle);

        [DllImport("usb_relay_device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_close_all_relay_channel(int hHandle);

        [DllImport("usb_relay_device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_open_one_relay_channel(int hHandle, int index);

        [DllImport("usb_relay_device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_relay_device_close_one_relay_channel(int hHandle, int index);

        private static bool _isTriggeredOn = false;
        private static TimeSpan _turnOffAfterTime;
        private static string _relaySerial = null;
        private static int _relayIndex = 0;
        private static int _relayHandle = 0;

        //private static List<MMDevice> _monitoredDevices = new List<MMDevice>();
        private static List<MeterInfo> _monitoredMeters = new List<MeterInfo>();

        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "-l")
            {
                ListAudioDevices();
                return;
            }
            else if (args.Length != 3)
            {
                Console.WriteLine("Usage: AmpToggleOnOff -l");
                Console.WriteLine("Usage: AmpToggleOnOff <relaySerial> <relayIndex> <secondsQuietBeforeOff>");

                // 3X9XI 1 60

                return;
            }

            try
            {
                _relaySerial = args[0];
                _relayIndex = Int32.Parse(args[1]);
                int secondsQueit = Int32.Parse(args[2]);
                _turnOffAfterTime = new TimeSpan(0, 0, secondsQueit);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Usage: AmpToggleOnOff <relaySerial> <relayIndex> <secondsQuietBeforeOff>");
                return;
            }

            int initRes = usb_relay_init();
            //int initRes = 0;
            if (initRes != 0)
            {
                Console.WriteLine("Unable to initialize usb relay device");
                return;
            }

            _relayHandle = usb_relay_device_open_with_serial_number(_relaySerial, (uint)_relaySerial.Length);
            //_relayHandle = 1;
            if (_relayHandle == 0)
            {
                Console.WriteLine("Unable to get usb relay handle");
                return;
            }

            TurnOnTrigger();
            System.Threading.Thread.Sleep(500);

            while (_isTriggeredOn)
            {
                TurnOffTrigger();
                System.Threading.Thread.Sleep(1000);
            }

            HashSet<string> deviceNames = GetMonitoredDeviceNames();
            GetMonitoredDevices(deviceNames);
            if (_monitoredMeters.Count == 0)
            {
                Console.WriteLine(Environment.NewLine + Environment.NewLine + "!!!!!  No monitored devices found  !!!!!!");
                return;
            }

            DateTime lastTimeFoundOn = DateTime.Now;

            while (true)
            {
                bool anyOn = AreAnyMonitoredOn(deviceNames);

                if (anyOn)
                {
                    lastTimeFoundOn = DateTime.Now;
                }

                if (anyOn && !_isTriggeredOn)
                {
                    TurnOnTrigger();
                }
                else if (!anyOn && _isTriggeredOn)
                {
                    var timeSinceLastOn = DateTime.Now - lastTimeFoundOn;
                    if (timeSinceLastOn > _turnOffAfterTime)
                    {
                        TurnOffTrigger();
                    }
                    else
                    {
                        Console.WriteLine(string.Format("All devices off but time since last on not long enough: timeSinceLastOn = {0}, _turnOffAfterTime = {1}", timeSinceLastOn, _turnOffAfterTime));
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private static bool AreAnyMonitoredOn(HashSet<string> deviceNames)
        {
            bool anyOn = false;

            Console.WriteLine(Environment.NewLine + string.Format("{0}, trigger on: {1}", DateTime.Now, _isTriggeredOn));

            foreach (var meter in _monitoredMeters)
            {
                float peakValue = meter.Meter.PeakValue;

                if (peakValue > 0.001)
                {
                    anyOn = true;
                }

                Console.WriteLine(meter.DeviceName + ": " + peakValue);
            }

            //using (var adEnum = new MMDeviceEnumerator())
            //{
            //    var devices = adEnum.EnumAudioEndpoints(DataFlow.Render, DeviceState.All);
            //    foreach (var device in devices)
            //    {
            //        try
            //        {
            //            using (var meter = AudioMeterInformation.FromDevice(device))
            //            {
            //                bool isMonitored = deviceNames.Contains(device.FriendlyName);

            //                float peakValue = meter.PeakValue;

            //                if ((peakValue > 0.001) && isMonitored)
            //                {
            //                    anyOn = true;
            //                }

            //                string monitoredInfo = isMonitored ? string.Empty : "NOT MONITORED";

            //                Console.WriteLine(device.FriendlyName + ": " + peakValue + "  " + monitoredInfo);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(Environment.NewLine + ex.ToString());
            //            Console.WriteLine(Environment.NewLine + Environment.NewLine + device.FriendlyName);

            //            Environment.Exit(1);
            //        }
            //    }
            //}

            return anyOn;
        }

        private static void TurnOnTrigger()
        {
            Console.WriteLine(Environment.NewLine + Environment.NewLine + "Turning on trigger");

            try
            {
                //int initRes = usb_relay_init();
                //if (initRes != 0)
                //{
                //    throw new Exception("unable to initialize usb relay device");
                //}

                //int relayHandle = usb_relay_device_open_with_serial_number(_relaySerial, (uint)_relaySerial.Length);
                //if (relayHandle == 0)
                //{
                //    throw new Exception("unable to get usb relay handle");
                //}

                int openRes = usb_relay_device_open_one_relay_channel(_relayHandle, _relayIndex);
                if (openRes != 0)
                {
                    throw new Exception("unable to open usb relay");
                }

                //int exitRes = usb_relay_exit();
                //if (exitRes != 0)
                //{
                //    throw new Exception("unable to clean up relay resources");
                //}

                _isTriggeredOn = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void TurnOffTrigger()
        {
            Console.WriteLine(Environment.NewLine + Environment.NewLine + "Turning off trigger");

            try
            {
                //int initRes = usb_relay_init();
                //if (initRes != 0)
                //{
                //    throw new Exception("unable to initialize usb relay device");
                //}

                //int relayHandle = usb_relay_device_open_with_serial_number(_relaySerial, (uint)_relaySerial.Length);
                //if (relayHandle == 0)
                //{
                //    throw new Exception("unable to get usb relay handle");
                //}

                int closeRes = usb_relay_device_close_one_relay_channel(_relayHandle, _relayIndex);
                if (closeRes != 0)
                {
                    throw new Exception("unable to close usb relay");
                }

                //int exitRes = usb_relay_exit();
                //if (exitRes != 0)
                //{
                //    throw new Exception("unable to clean up relay resources");
                //}

                _isTriggeredOn = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static HashSet<string> GetMonitoredDeviceNames()
        {
            HashSet<string> deviceNames = new HashSet<string>();

            foreach (string line in File.ReadLines("DeviceList.txt"))
            {
                string name = line.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    deviceNames.Add(line.Trim());
                }
            }

            return deviceNames;
        }

        private static void GetMonitoredDevices(HashSet<string> deviceNames)
        {
            Console.WriteLine(Environment.NewLine + "Getting monitored devices:");

            using (var adEnum = new MMDeviceEnumerator())
            {
                var devices = adEnum.EnumAudioEndpoints(DataFlow.Render, DeviceState.All);
                foreach (var device in devices)
                {
                    try
                    {
                        if (deviceNames.Contains(device.FriendlyName))
                        {
                            try
                            {
                                var meter = AudioMeterInformation.FromDevice(device);
                                float peakValue = meter.PeakValue;
                                Console.WriteLine(device.FriendlyName + ": " + peakValue);
                                _monitoredMeters.Add(new MeterInfo()
                                {
                                    DeviceName = device.FriendlyName,
                                    Meter = meter
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(device.FriendlyName + " exception getting peak volume");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Caught exception trying to fetch device information");
                    }
                }
            }
        }

        private static void ListAudioDevices()
        {
            Console.WriteLine(Environment.NewLine + "Listing audio devices:");

            using (var adEnum = new MMDeviceEnumerator())
            {
                var devices = adEnum.EnumAudioEndpoints(DataFlow.Render, DeviceState.All);
                foreach (var device in devices)
                {
                    try
                    {
                        Console.WriteLine(device.FriendlyName);
                    }
                    catch (Exception ex)
                    {
                        // do nothing
                        //Console.WriteLine(device.FriendlyName + " exception getting getting device name");
                    }
                }
            }

        }
    }

    class MeterInfo
    {
        public string DeviceName { get; set; }
        public AudioMeterInformation Meter { get; set; }
    }
}
