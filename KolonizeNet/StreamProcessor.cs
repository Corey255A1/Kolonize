using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace KolonizeNet
{
    public delegate void StreamWriter(byte[] bytes, int offset = 0, int size = -1);
    public delegate bool PacketProcessor(StreamWriter s, PacketTypes p, DataTypes d, byte[] buffer, ref int offset);
    public delegate void StreamStatus(StreamProcessor sp, string msg);
    public class StreamProcessor
    {
        NetworkStream theStream;
        PacketProcessor theProcessor;
        public event StreamStatus StreamStatusEvent;
        byte[] DataBuffer;
        int BUFFER_SIZE;
        int DataReadOffset = 0;

        public StreamProcessor(NetworkStream stream, PacketProcessor process, int buffersize = 1000)
        {
            theProcessor = process;
            theStream = stream;
            BUFFER_SIZE = buffersize;
            DataBuffer = new byte[BUFFER_SIZE];
            theStream.BeginRead(DataBuffer, 0, DataBuffer.Length, StreamRead, this);
        }
        public void StreamWrite(byte[] bytes, int offset=0, int size = -1)
        {
            theStream.Write(bytes, offset, size > 0 ? size : (bytes.Length - offset));
        }

        public void StreamRead(IAsyncResult ar)
        {
            try
            {
                int offset = 0;
                int prevoffset = offset;

                int bytes = theStream.EndRead(ar);
                if (bytes > 0)
                {
                    bytes += DataReadOffset;
                    DataReadOffset = 0;
                    offset = 0;
                    do
                    {
                        if(DataBuffer.Length - offset > 2)//2 is Header Size Currently...
                        {
                            var packetData = NetHelpers.GetHeaderInfo(DataBuffer, offset);
                            prevoffset = offset;
                            if (!theProcessor(StreamWrite, packetData.Item1, packetData.Item2, DataBuffer, ref offset))
                            {
                                DataReadOffset = DataBuffer.Length - offset;
                                Array.Copy(DataBuffer, offset, DataBuffer, 0, DataReadOffset);
                                offset = bytes;
                            }
                        }
                        else
                        {
                            DataReadOffset = DataBuffer.Length - offset;
                            Array.Copy(DataBuffer, offset, DataBuffer, 0, DataReadOffset);
                            offset = bytes;
                        }

                    } while (offset < bytes);
                }
                theStream.BeginRead(DataBuffer, DataReadOffset, BUFFER_SIZE - DataReadOffset, StreamRead, this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                StreamStatusEvent?.Invoke(this, "Disconnected");
            }
        }
    }
}
