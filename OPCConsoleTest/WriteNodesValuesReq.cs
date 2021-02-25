using System.Collections.Generic;

namespace OPCConsoleTest
{
    class WriteNodesValuesReq
    {
        public string ServiceId { get; set; }
        public string Id { get; set; }
        public Dictionary<string, object> itemValuePairs { get; set; }
    }
}
