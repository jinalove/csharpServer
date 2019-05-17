using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Timers;

namespace csharpServer
{

    [Serializable]
    public class Person : CmdBase
    {
        public int age;
        public string name;
    }

    [Serializable]
    public class PersonHandler : BaseHandler
    {
        public int age;
        public string name;

        public override void Deserialize(byte[] bytes)
        {
            PersonHandler person = new PersonHandler();
            person = SerializeHelper.DeserializeWithBinary<PersonHandler>(bytes);
        }
    }

    [Serializable]
    public class CoffeeHandler : BaseHandler
    {
        public int age;
        public string name;

        public override void Deserialize(byte[] bytes)
        {
            PersonHandler person = new PersonHandler();
            person = SerializeHelper.DeserializeWithBinary<PersonHandler>(bytes);

        }
    }

    [Serializable]
    public class CmdBase
    {
        public int cmd;
    }


    [Serializable]
    public class BaseHandler
    {
        public int cmd;
        public virtual void Deserialize(byte[] bytes)
        {

        }

    }

    class FanSe
    {
        private static byte[] result = new byte[1024];
        private static int myProt = 8885;   //端口 
        static Socket serverSocket;
        static byte[] bytes = new byte[1024];
        private static byte[] senderByte;
        private static Socket clientSocket;
        static List<byte> list = new List<byte>();
        private static byte comIndex =0;

        static void Main(string[] args)
        {
            //服务器IP地址 
            //IPAddress ip = IPAddress.Parse("127.0.0.1");
            //serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口 
            //serverSocket.Listen(10);    //设定最多10个排队连接请求 
            //Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            ////通过Clientsoket发送数据 
            //Thread myThread = new Thread(ListenClientConnect);
            //myThread.Start();




            Console.WriteLine("clientSocket进来了");

            NetBufferWriter writer = new NetBufferWriter();
            Person person = new Person { cmd = 2, age = 18, name = "OK" };

            writer.WriteInt(person.cmd);
            writer.WriteInt(person.age);
            writer.WriteString(person.name);

            byte[] data = writer.Finish();

            NetBufferReader reader = new NetBufferReader(data);

            Console.WriteLine(reader.ReadInt());
            Console.WriteLine(reader.ReadString());
            Console.WriteLine(reader.ReadInt());
            Console.ReadLine();
        }

        /// <summary> 
        /// 监听客户端连接 
        /// </summary> 
        private static void ListenClientConnect()
        {
            while (true)
            {
                clientSocket = serverSocket.Accept();
                Console.WriteLine("clientSocket进来了");
                Sendor(clientSocket);

                //clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }

        private static void Sendor(Socket clientSocket)
        {
            list.Clear();

            for (int k = 0; k < 10; k++)
            {
                Person person = new Person { cmd = 2, age = 18, name = "OK" };
                bytes = SerializeHelper.SerializeToBinary(person);

                MemoryStream buff = new MemoryStream();
                byte head = 0xAA; 

                byte[] pLen = BitConverter.GetBytes(3+bytes.Length);

                byte Cmd = 0xB9;

                byte CmdIndex = ++comIndex;

                byte check = (byte)(Cmd ^ CmdIndex);
                if (pLen != null)
                {
                    for (int i = 0; i < pLen.Length; i++)
                    {
                        check ^= pLen[i];
                    }
                }
                if (bytes != null)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        check ^= bytes[i];
                    }
                }
                byte Final = 0x55;

                buff.WriteByte(head);//写包头
                buff.Write(pLen, 0, 4);//写长度
                buff.WriteByte(Cmd);
                buff.WriteByte(CmdIndex);
                buff.Write(bytes, 0, bytes.Length);
                Console.WriteLine("Cmd:" + Cmd);
                Console.WriteLine("CmdIndex:" + CmdIndex);
                Console.WriteLine("check:" + check);

                buff.WriteByte(check);
                buff.WriteByte(Final);


                senderByte = buff.ToArray();
          
                for (int i = 0; i < senderByte.Length; i++)
                {
                   // Console.WriteLine("senderByte[i]:" + senderByte[i]);
                    list.Add(senderByte[i]);
                }
            }

            Console.WriteLine("-----------------------------------------list.Count:" + list.Count);
            //Read(bytes);
            System.Timers.Timer t = new System.Timers.Timer(100);//实例化Timer类，设置间隔时间为10000毫秒；

            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；

            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；

            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

           
        }

        private static void theout(object sender, ElapsedEventArgs e)
        {
            if (list.Count>0)
            {
                //byte[] senders = new byte[1];
                //senders[0] = list[0];
                //list.RemoveAt(0);
                clientSocket.Send(list.ToArray());
            }
        }

 


        //接收数据
        public static void Read(byte[] bytes)
        {
            //BaseHandler handler = SerializeHelper.DeserializeWithBinary<BaseHandler>(bytes);
            //if (handler != null)
            //{
            //    handler.Deserialize(bytes);
            //}
            //Console.WriteLine(handler.cmd);
            //foreach (var item in bytes)
            //{
            //    Console.WriteLine(item);
            //}

            CmdBase handler = SerializeHelper.DeserializeWithBinary<CmdBase>(bytes);

            if (handler != null)
            {
                Console.WriteLine("handler.cmd:" + handler.cmd);
            }
            else
            {
                Console.WriteLine("接收数据为空");
            }
        }

        private static void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据 
                    int receiveNumber = myClientSocket.Receive(result);
                    byte[] get = new byte[receiveNumber];
                    Array.Copy(result, get, receiveNumber);

                    Read(get);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                    break;
                }
            }
        }
  

 
   
    }
}