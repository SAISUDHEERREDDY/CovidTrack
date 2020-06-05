using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Newtonsoft.Json;
using Xamarin.Forms;
using XFCovidTrack.Droid;
using XFCovidTrack.Interfaces;
using XFCovidTrack.Models;

[assembly: Xamarin.Forms.Dependency(typeof(Bluetooth))]
namespace XFCovidTrack.Droid
{
    public class Bluetooth : IBluetooth
    {

        const string TARGET_UUID = "38e851bc-7144-44b4-9cd8-80549c6f2912";
        private static readonly object Instancelock = new object();
        public static Bluetooth _instance = null;
        BluetoothSocket socket = null;
        OutputStreamInvoker outStream = null;
        InputStreamInvoker inStream = null;
        public static Bluetooth GetInstance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Instancelock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Bluetooth();
                        }
                    }

                }
                return _instance;
            }

        }

        public bool Listening { get; private set; }


        public async Task<bool> ConnectAsync(string _deviceName)
        {

            try
            {


                BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null) Log("No Bluetooth adapter found.");
                else if (!adapter.IsEnabled) Log("Bluetooth adapter is not enabled.");

                List<BluetoothDevice> L = new List<BluetoothDevice>();
                foreach (BluetoothDevice d in adapter.BondedDevices)
                {
                    Log("D: " + d.Name + " " + d.Address + " " + d.BondState.ToString());
                    L.Add(d);
                }

                BluetoothDevice device = null;
                device = L.Find(j => j.Name == _deviceName);

                if (device == null) Log("Named device not found.");
                else
                {
                    Log("Device has been found: " + device.Name + " " + device.Address + " " + device.BondState.ToString());
                }

                socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString(TARGET_UUID));
                await socket.ConnectAsync();

                if (socket != null && socket.IsConnected) Log("Connection successful!");
                else Log("Connection failed!");

                inStream = (InputStreamInvoker)socket.InputStream;
                outStream = (OutputStreamInvoker)socket.OutputStream;

                if (socket != null && socket.IsConnected)
                {
                    Task t = new Task(() => Listen(inStream));
                    t.Start();

                    //Task t1 = new Task(() => PingEvent());
                    //t1.Start();
                }
                else
                {
                    //throw new Exception("Socket not existing or not connected.");
                }
            }
            catch (Exception e)
            {
                //throw new Exception("Socket not existing or not connected.");
            }

            return socket.IsConnected;
        }
        public void Disconnect()
        {
            Listening = false;
            if (socket != null) { socket.Dispose(); socket = null; }
        }
        public async Task<bool> Isconnected()
        {
            if (socket != null)
            {
                return socket.IsConnected;
            }
            else
            {
                return false;
            }
        }
        public async Task SendMessage(string msg)
        {
            var message = msg;
            if (outStream != null)
            {
                uint messageLength = (uint)message.Length;
                byte[] countBuffer = BitConverter.GetBytes(messageLength);
                byte[] buffer = Encoding.UTF8.GetBytes(message);

                await outStream.WriteAsync(countBuffer, 0, countBuffer.Length);
                await outStream.WriteAsync(buffer, 0, buffer.Length);
                await outStream.FlushAsync();
            }

        }
        private async void Log(string msg)
        {
            Console.WriteLine(msg);
        }
        public async Task<ObservableCollection<BtDevice>> GetBondedDevices()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null) Log("No Bluetooth adapter found.");
            else if (!adapter.IsEnabled) Log("Bluetooth adapter is not enabled.");

            ObservableCollection<BtDevice> L = new ObservableCollection<BtDevice>();
            foreach (BluetoothDevice d in adapter.BondedDevices)
            {

                var device = new BtDevice();
                device.Name = d.Name;
                device.Address = d.Address;
                Log("D: " + d.Name + " " + d.Address + " " + d.BondState.ToString());
                L.Add(device);


            }
            return L;
        }
        public async Task PingEvent()
        {

            while (true)
            {

                if (socket != null && socket.IsConnected)
                {
                    MessageContract ping = new MessageContract();
                    //ping.Type = "Ping";
                    await SendMessage(JsonConvert.SerializeObject(ping));
                }

                Thread.Sleep(2 * 60 * 1000);


            }
        }
        private async void Listen(Stream inStream)
        {
            bool Listening = true;
            byte[] buffer = new byte[1048576];
            int bytes;

            while (Listening)
            {
                try
                {



                    if (inStream.IsDataAvailable())
                    {
                        bytes = 0;
                        while (inStream.IsDataAvailable())
                        {
                            bytes += inStream.Read(buffer, bytes, 1);
                        }
                        String message = Encoding.ASCII.GetString(buffer, 0, bytes);
                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = message.Split(stringSeparators, StringSplitOptions.None);
                        foreach (string s in lines)
                        {
                            if (s != "")
                            {
                                MessagingCenter.Send(s, "LocalMessagebroadcast");

                            }

                        }
                    }

                }
                catch (Java.IO.IOException e)
                {
                    Log("Error: " + e.Message);
                    Listening = false;
                    break;
                }
            }
            Log("Listening has ended.");
        }


    }
}