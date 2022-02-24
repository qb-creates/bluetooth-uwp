using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace BluetoothPairing.Messaging
{
    public class BluetoothRfcommMessage
    {        
        public string PageName { get; set; }
        public DeviceInformation DeviceInformation { get; set; }
    }
}
