using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OPCConsoleTest
{
    class Program
    {
        private static Socket _client = null;
        private static byte[] _bufferPool = new byte[1024 * 1024];
        private static string _id = string.Empty;
        static void Main(string[] args)
        {
            _client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(IPAddress.Parse("192.168.1.129"), 10010);

            ThreadPool.QueueUserWorkItem(new WaitCallback((callback) => {
                while (true)
                {
                    int receivedByteCount = _client.Receive(_bufferPool, _bufferPool.Length, SocketFlags.None);
                    if (receivedByteCount > 0)
                    {
                        Header cmdHeader = new Header();
                        if (cmdHeader.ErrorCode == 0)
                        {
                            string payload = string.Empty;
                            StructUtility.Unpackage(_bufferPool, out cmdHeader, out payload);
                            Console.WriteLine("Cmd:" + cmdHeader.Cmd + " payload:" + payload);
                            if (cmdHeader.Cmd == (int)Command.Get_Nodes_Rsp) //获取所有节点
                            {
                                Console.WriteLine("收到获取所有节点响应");
                            }
                            else if (cmdHeader.Cmd == (int)Command.Start_Monitor_Nodes_Rsp)
                            {
                                var rsp = JsonConvert.DeserializeObject<StartMonitoringItemsRsp>(payload);
                                _id = rsp.GroupId;
                                //测试读取值 SunFull.X2OPC.1
                                //TestWriteNodeValue();

                                TestReadNode(_id);
                            }
                            else if (cmdHeader.Cmd == (int)Command.Notify_Nodes_Values_Ex)
                            {
                                Console.WriteLine("值已修改:" + "payload:" + payload);
                            }
                        }
                        else
                        {
                            Console.WriteLine("返回 Cmd:" + cmdHeader.Cmd + " 错误:" + cmdHeader.ErrorCode);
                        }
                    }
                }


            }));
            //测试获取所有节点
            TestGetAllNode();
            TestMonitorNode();
            Console.ReadKey();
        }


        static void TestGetAllNode()
        {
            Header header = new Header(1, (int)Command.Get_Nodes_Req,0, 1);
            byte[] bufferList = StructUtility.Package(header, string.Empty);
            int sendByteCount = _client.Send(bufferList, SocketFlags.None);
            Console.WriteLine("发送Cmd:" + header.Cmd);
        }

        static void TestMonitorNode()
        {
            Console.WriteLine("--------------------测试读取节点值--------------------");
            Header header = new Header(1, (int)Command.Start_Monitor_Nodes_Req, 0, 1);
            List<string> items = new List<string>();
            items.Add("MODTEST.Channel_1.Device_1.Tag_1");
            items.Add("MODTEST.Channel_1.Device_1.Tag_2");
            items.Add("MODTEST.Channel_1.Device_1.Tag_3");
            StartMonitoringItemsReq monitoringItemsReq = new StartMonitoringItemsReq("SunFull.X2OPC.1", items);
            string payload = JsonConvert.SerializeObject(monitoringItemsReq);

            byte[] bufferList = StructUtility.Package(header, payload);
            int sendByteCount = _client.Send(bufferList, SocketFlags.None);
            Console.WriteLine("发送Cmd:" + header.Cmd);
        }

        static void TestReadNode(String GroupId)
        {
            Console.WriteLine("--------------------测试读取节点值--------------------");
            Header header = new Header(1, (int)Command.Read_Nodes_Values_Req, 0, 1);
            List<string> items = new List<string>();
            items.Add("MODTEST.Channel_1.Device_1.Tag_1");
            items.Add("MODTEST.Channel_1.Device_1.Tag_2");
            items.Add("MODTEST.Channel_1.Device_1.Tag_3");
            ReadItemsReq readItemsReq = new ReadItemsReq("SunFull.X2OPC.1", items, GroupId);
            string payload = JsonConvert.SerializeObject(readItemsReq);

            byte[] bufferList = StructUtility.Package(header, payload);
            int sendByteCount = _client.Send(bufferList, SocketFlags.None);
            Console.WriteLine("发送Cmd:" + header.Cmd);
        }

        static void TestWriteNodeValue()
        {
            Console.WriteLine("--------------------测试写节点值--------------------");
            Header header = new Header(1, (int)Command.Write_Nodes_Values_Req, 0, 1);
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
            keyValuePairs["MODTEST.Channel_1.Device_1.Tag_1"] = 222;
            keyValuePairs["MODTEST.Channel_1.Device_1.Tag_2"] = 111;
            keyValuePairs["MODTEST.Channel_1.Device_1.Tag_3"] = 333;
            WriteNodesValuesReq req = new WriteNodesValuesReq();
            req.Id = _id;
            req.ServiceId = "SunFull.X2OPC.1";
            req.itemValuePairs = keyValuePairs;
            string payload = JsonConvert.SerializeObject(req);

            byte[] bufferList = StructUtility.Package(header, payload);
            int sendByteCount = _client.Send(bufferList, SocketFlags.None);
            Console.WriteLine("发送Cmd:" + header.Cmd);

        }

    }
}
