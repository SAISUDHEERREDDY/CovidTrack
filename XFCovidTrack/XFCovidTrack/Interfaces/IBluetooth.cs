using System.Collections.ObjectModel;
using System.Threading.Tasks;
using XFCovidTrack.Models;

namespace XFCovidTrack.Interfaces
{
    public interface IBluetooth
    {
        Task<bool> ConnectAsync(string _deviceName);
        Task<bool> Isconnected();
        void Disconnect();
        Task SendMessage(string message);
        Task<ObservableCollection<BtDevice>> GetBondedDevices();
    }
}
