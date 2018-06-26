using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Net;

namespace KolonizeNet
{
    public delegate void StreamWriter(byte[] bytes, int offset = 0, int size = -1);
    public delegate bool PacketProcessor(StreamWriter s, PacketTypes p, DataTypes d, byte[] buffer, ref int offset);
    public delegate void StreamStatus(string msg);

    public abstract class StreamProcessor
    {
        protected PacketProcessor theProcessor;
        
        protected byte[] DataBuffer;
        protected int BUFFER_SIZE;
        protected int DataReadOffset = 0;
        public abstract void StreamWrite(byte[] bytes, int offset = 0, int size = -1);
        protected bool ProcessDataBuffer(int count)
        {
            try
            {
                int offset = 0;
                int prevoffset = offset;
                if (count > 0)
                {
                    count += DataReadOffset;
                    DataReadOffset = 0;
                    offset = 0;
                    do
                    {
                        if (DataBuffer.Length - offset > 2)//2 is Header Size Currently...
                        {
                            var packetData = NetHelpers.GetHeaderInfo(DataBuffer, offset);
                            prevoffset = offset;
                            if (!theProcessor(StreamWrite, packetData.Item1, packetData.Item2, DataBuffer, ref offset))
                            {
                                DataReadOffset = DataBuffer.Length - offset;
                                Array.Copy(DataBuffer, offset, DataBuffer, 0, DataReadOffset);
                                offset = count;
                            }
                        }
                        else
                        {
                            DataReadOffset = DataBuffer.Length - offset;
                            Array.Copy(DataBuffer, offset, DataBuffer, 0, DataReadOffset);
                            offset = count;
                        }

                    } while (offset < count);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }
    }

    public class NetworkStreamProcessor: StreamProcessor
    {
        NetworkStream theStream;
        public event StreamStatus StreamStatusEvent;
        public NetworkStreamProcessor(NetworkStream stream, PacketProcessor process, int buffersize = 1024)
        {
            theProcessor = process;
            theStream = stream;
            BUFFER_SIZE = buffersize;
            DataBuffer = new byte[BUFFER_SIZE];
            theStream.BeginRead(DataBuffer, 0, DataBuffer.Length, StreamRead, this);
        }
        public override void StreamWrite(byte[] bytes, int offset=0, int size = -1)
        {
            theStream.Write(bytes, offset, size > 0 ? size : (bytes.Length - offset));
        }
        protected void StreamRead(IAsyncResult ar)
        {
            try
            {
                int offset = 0;
                int prevoffset = offset;

                int bytes = theStream.EndRead(ar);
                if(ProcessDataBuffer(bytes))
                {
                    theStream.BeginRead(DataBuffer, DataReadOffset, BUFFER_SIZE - DataReadOffset, StreamRead, this);
                }
                else
                {
                    StreamStatusEvent?.Invoke("Disconnected");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                StreamStatusEvent?.Invoke("Disconnected");
            }
        }
    }

    public class WebSocketStreamProcessor: StreamProcessor
    {
        WebSocket theSocket;
        public event StreamStatus StreamStatusEvent;
        public WebSocketStreamProcessor(WebSocket ws, PacketProcessor p, int buffersize = 1024)
        {
            BUFFER_SIZE = buffersize;
            DataBuffer = new byte[BUFFER_SIZE];
            theProcessor = p;
            theSocket = ws;
            StreamRead();
        }
        public override void StreamWrite(byte[] bytes, int offset = 0, int size = -1)
        {
            theSocket.SendAsync(new ArraySegment<byte>(bytes, offset, size>0?size:bytes.Length), WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
        }
        protected async void StreamRead()
        {
            while(theSocket.State == WebSocketState.Open)
            {

                var res = await theSocket.ReceiveAsync(new ArraySegment<byte>(DataBuffer), System.Threading.CancellationToken.None);
                if(res.MessageType == WebSocketMessageType.Close)
                {
                    await theSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "You told me to close", System.Threading.CancellationToken.None);
                    StreamStatusEvent?.Invoke("Disconnected");
                    break;
                }
                else
                {
                    if (!ProcessDataBuffer(res.Count))
                    {
                        await theSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Something happened on my end", System.Threading.CancellationToken.None);
                        StreamStatusEvent?.Invoke("Disconnected");
                        break;
                    }
                }

            }
        }       

    }
}
