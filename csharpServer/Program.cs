//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Net.Sockets;
//using System.Net;
//using System.Threading;

//namespace csharpServer
//{
//    public class Cmd
//    {
//        public const byte M2SQueryLink = 0xA9;//查询链接（握手）
//        public const byte M2SDeliverRequest = 0xA3;//发送出货命令

//        public const byte M2SEnterTestModelRequest = 0xC1;//发送进入测试命令
//        public const byte M2SExitTestModelRequest = 0xC2;//发送退出测试命令

//        public const byte M2SAllClearRequest = 0xC3;//发送一键清除故障
//        public const byte M2SReplenishRequest = 0xC4;//发送补货

//        public const byte M2SDeliverCupRequest = 0xC5;//发送出杯

//        public const byte S2MQueryAck = 0xB9;//查询的应答 
//        public const byte S2MDeliverAck = 0x08;//出货命令的应答
//        public const byte S2MDeliverComplete = 0x06;//货道出货成功
//        public const byte S2MotorError = 0xB1;//转盘电机故障或者该货道缺货  故障  

//        public const byte S2DeliverCup = 0xB6;//出杯子
//    }

//    public class WriteData
//    {
//        public byte Cmd;//当前的Cmd
//        public byte[] Data;//请求的数据类型 
//    }

//    class Program
//    {
//        private static byte[] result = new byte[1024];
//        private static int myProt = 8885;   //端口 
//        static Socket serverSocket;
//        static void Main(string[] args)
//        {
//            //服务器IP地址 
//            IPAddress ip = IPAddress.Parse("127.0.0.1");
//            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口 
//            serverSocket.Listen(10);    //设定最多10个排队连接请求 
//            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
//            //通过Clientsoket发送数据 
//            Thread myThread = new Thread(ListenClientConnect);
//            myThread.Start();
//            Console.ReadLine();
//        }


//        public static void SayHello(Socket socket)
//        {
//            WriteData data = new WriteData() { Cmd = Cmd.M2SQueryLink, Data = null };
//            Program.Send(socket,data);
//        }
//        /// <summary> 
//        /// 监听客户端连接 
//        /// </summary> 
//        private static void ListenClientConnect()
//        {
//            while (true)
//            {
//                Socket clientSocket = serverSocket.Accept();
//                Console.WriteLine("clientSocket进来了");
//                SayHello(clientSocket);
//                //clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
//                Thread receiveThread = new Thread(ReceiveMessage);
//                receiveThread.Start(clientSocket);
//            }
//        }

     
//        private static void ReceiveMessage(object clientSocket)
//        {
//            Socket myClientSocket = (Socket)clientSocket;
//            while (true)
//            {
//                try
//                {
//                    //通过clientSocket接收数据 
//                    int receiveNumber = myClientSocket.Receive(result);
//                    byte[] get = new byte[receiveNumber];
//                    Array.Copy(result, get, receiveNumber);

//                    Read(get);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                    myClientSocket.Shutdown(SocketShutdown.Both);
//                    myClientSocket.Close();
//                    break;
//                }
//            }
//        }
//        private static byte[] _writeBuf = new byte[128];
//        private static byte _cmdIndex = 0;
   
//        public static void Send(Socket socket,WriteData data, bool reliable = false)
//        {
//            if (data == null) return;
//            int idx = 0;
//            byte len = (byte)(3 + (data.Data == null ? 0 : data.Data.Length));
//            _writeBuf[idx++] = 0xAA;
//            Console.WriteLine("_writeBuf[0]:" + _writeBuf[0]);
//            Console.WriteLine("0xAA:" + 0xAA);

//            _writeBuf[idx++] = len;
//            _writeBuf[idx++] = data.Cmd;
//            _writeBuf[idx++] = _cmdIndex;
//            byte check = (byte)(len ^ data.Cmd ^ _cmdIndex);
//            if (data.Data != null)
//            {
//                for (int i = 0; i < data.Data.Length; i++)
//                {
//                    _writeBuf[idx++] = data.Data[i];
//                    check ^= data.Data[i];
//                }
//            }
//            _writeBuf[idx++] = check;
//            _writeBuf[idx++] = 0x55;

//            Console.WriteLine("发送指令q:"+ _writeBuf.Length);
//            socket.Send(_writeBuf);
//            Console.WriteLine("发送指令h:" + _writeBuf.Length);

//            _cmdIndex++;

//            byte cmd = _writeBuf[2];
//            if (cmd == Cmd.M2SQueryLink)
//            {
//                Console.WriteLine("发送出货指令");
//            }

//        }

//        //接收数据
//        public static void Read(byte[] bytes)
//        {
//            // Debug.Log("VemIO接收的数据为：" + Bytes2String(bytes));
//            if (bytes.Length <= 0) return;

//            Console.WriteLine("接收客户端消息:"+ bytes.Length);
//            byte cmd = bytes[2];
//            Console.WriteLine("cmd:"+ cmd);

//            if (cmd == Cmd.M2SQueryLink)//应答的握手指令
//            {
//                Console.WriteLine("应答的握手指令" );
//            }
//        }
//    }
//}