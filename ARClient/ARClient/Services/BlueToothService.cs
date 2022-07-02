using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARClient.EventArguments;
using InTheHand.Bluetooth;

namespace ARClient.Services
{
    public class BlueToothService
    {
        private List<string> UuIds = new List<string>() { "C7228C2F-429C-47D7-9943-52C36FB7EF85", "0CBF7217-A08D-4B70-8050-BC773484B7A4" };
        private string serviceUuid = "6566B82D-7C80-447D-B221-FF55761F8401";

        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;

        public Dictionary<string, GattCharacteristic> Charachteristics { get; set; } = new Dictionary<string, GattCharacteristic>();

        BluetoothLEScan scan;

        public BlueToothService()
        {
            
        }

        public async void Start()
        {
            bool availability = false;

            while (!availability)
            {
                availability = await Bluetooth.GetAvailabilityAsync();
                await Task.Delay(500);
            }

            Debug.WriteLine("Got To One");

            Bluetooth.AdvertisementReceived += Bluetooth_AdvertisementReceived;
            scan = await Bluetooth.RequestLEScanAsync();

            RequestDeviceOptions options = new RequestDeviceOptions();
            options.AcceptAllDevices = true;
            BluetoothDevice device = await Bluetooth.RequestDeviceAsync(options);
            if (device != null)
            {
                device.GattServerDisconnected += Device_GattServerDisconnected;
                await device.Gatt.ConnectAsync();

                var servs = await device.Gatt.GetPrimaryServicesAsync();

                foreach (var serv in servs)
                {
                    var rssi = await device.Gatt.ReadRssi();

                    string uuid = serv.Uuid.ToString();

                    if (uuid.ToLower() == serviceUuid.ToLower())
                    {
                        Debug.WriteLine("Got Service");

                        foreach (var chars in await serv.GetCharacteristicsAsync())
                        {
                            uuid = chars.Uuid.ToString().ToUpper();

                            if (UuIds.Contains(uuid))
                            {
                                var desc = await chars.GetDescriptorsAsync();

                                foreach (var des in desc)
                                {
                                    Debug.WriteLine("Got Descriptors");

                                    await des.WriteValueAsync(new byte[2] { 1, 0 });
                                    
                                    var val2 = await des.ReadValueAsync();

                                    if (des.Uuid == GattDescriptorUuids.ClientCharacteristicConfiguration)
                                    {
                                        Debug.WriteLine($"Notifying:{val2[0] > 0}");
                                        //bool not = val2[0] > 0;
                                    }
                                    else if (des.Uuid == GattDescriptorUuids.CharacteristicUserDescription)
                                    {
                                        //Debug.WriteLine($"UserDescription:{ByteArrayToString(val2)}");
                                    }
                                    else
                                    {
                                        //Debug.WriteLine(ByteArrayToString(val2));
                                    }

                                }

                                await chars.StartNotificationsAsync();

                                //Take out for UWP
                                chars.CharacteristicValueChanged += Chars_CharacteristicValueChanged;

                                //Put this back for UWP
                                //chars.CharacteristicValueChanged += (s, e) => {
                                //    Debug.WriteLine("In Message");

                                //    var chara = (GattCharacteristic)s;

                                //    string message = Encoding.ASCII.GetString(chara.Value);

                                //    Debug.WriteLine("Got Message");

                                //    MessageRecieved?.Invoke(this, new MessageRecievedEventArgs(message));
                                //};

                                Charachteristics.Add(uuid, chars);

                                Debug.WriteLine("Added Char");

                                continue;
                            }
                        }

                        break;
                    }
                }
            }
        }

        private void Chars_CharacteristicValueChanged(object sender, GattCharacteristicValueChangedEventArgs e)
        {
            string message = Encoding.ASCII.GetString(e.Value);

            MessageRecieved?.Invoke(this, new MessageRecievedEventArgs(message));
        }

        private void Bluetooth_AdvertisementReceived(object sender, BluetoothAdvertisingEvent e)
        {
            
        }

        private async void Device_GattServerDisconnected(object sender, EventArgs e)
        {
            var device = sender as BluetoothDevice;
            await device.Gatt.ConnectAsync();
        }

        private static string ByteArrayToString(byte[] data)
        {
            if (data == null)
                return "<NULL>";

            StringBuilder sb = new StringBuilder();

            foreach (byte b in data)
            {
                sb.Append(b.ToString("X"));
            }

            return sb.ToString();
        }
    }
 }
