using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPCConsoleTest
{
    public class StartMonitoringItemsReq
    {
        public string ServiceId { get; set; }
        public List<string> Items { get; set; }

        public StartMonitoringItemsReq(string _serviceProgId, List<string> _items)
        {
            ServiceId = _serviceProgId;
            Items = _items;
        }
    }

}
