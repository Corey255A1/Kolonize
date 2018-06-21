using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace KolonizeNet
{
    public enum PacketTypes { REQUEST, REQUESTED, SET, UPDATE }
    public enum DataTypes {
        WORLD_INFO,
        REGION_INFO,
        CELL_INFO,
        PLAYER_INFO,
        PLAYER_CONTROL,
        PLAYER_ACTION_UPDATE,
        PLAYER_PERFORM_ACTION,
        OBJECT_INFO
    }
    public static class WorldConstants
    {
        public const byte SPACE = 0;
        public const byte WATER = 1;
        public const byte SAND = 2;
        public const byte DIRT = 3;
        public const byte ROCK = 4;
        public const byte LAVA = 5;
        public const byte ICE = 6;

        public const int TYPE_GENERIC = 0;
        public const int TYPE_MOVEABLE = 1;
        public const int TYPE_PLAYER = 2;
        public const int TYPE_MARKER = 3;

    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Header
    {
        public byte packetType;
        public byte dataType;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct WorldInfo
    {
        public Header header;
        public int width;
        public int height;
        public WorldInfo(PacketTypes p)
        {
            header.packetType = (byte)p;
            header.dataType = (byte)DataTypes.WORLD_INFO;
            width = 0;
            height = 0;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct CellInfo
    {
        public Header header;
        public int remainingCells;
        public byte cellType;
        public int x;
        public int y;
        public CellInfo(PacketTypes p)
        {
            header.packetType = (byte)p;
            header.dataType = (byte)DataTypes.CELL_INFO;
            x = 0;
            y = 0;
            cellType = 0;
            remainingCells = 0;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct RegionInfo
    {
        public Header header;
        public int x1;
        public int x2;
        public int y1;
        public int y2;
        public RegionInfo(PacketTypes p)
        {
            header.packetType = (byte)p;
            header.dataType = (byte)DataTypes.REGION_INFO;
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 0;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ObjectInfo
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string id;
        public int objecttype;
        public int x;
        public int y;
        public float vx;
        public float vy;
        public ObjectInfo(PacketTypes p)
        {
            header.packetType = (byte)p;
            header.dataType = (byte)DataTypes.OBJECT_INFO;
            id = "";
            x = 0;
            y = 0;
            vx = 0;
            vy = 0;
            objecttype = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct PlayerInfo
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string key;
        public int x;
        public int y;
        public float vx;
        public float vy;
        public PlayerInfo(PacketTypes p)
        {
            header.packetType = (byte)p;
            header.dataType = (byte)DataTypes.PLAYER_INFO;
            id = "";
            key = "";
            x = 0;
            y = 0;
            vx = 0;
            vy = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct PlayerActionUpdate
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string id;
        public int ActionBitField; //32 Possible Action (for now)
        public PlayerActionUpdate(PacketTypes p)
        {
            header.packetType = (byte)p;
            header.dataType = (byte)DataTypes.PLAYER_ACTION_UPDATE;
            id = "";
            ActionBitField = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct PlayerPerformAction
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string key;
        public int ActionID; //32 Possible Action (for now)
        public PlayerPerformAction(PacketTypes p)
        {
            header.packetType = (byte)p;
            header.dataType = (byte)DataTypes.PLAYER_PERFORM_ACTION;
            id = "";
            key = "";
            ActionID = 0;
        }
    }

     [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct PlayerControl
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string key;
        public int direction;
        public int paces;
        public PlayerControl(PacketTypes p)
        {
            header.packetType = (byte)p;
            header.dataType = (byte)DataTypes.PLAYER_CONTROL;
            id = "";
            key = "";
            direction = 0;
            paces = -1;
        }
    }


    public class NetHelpers
    {

        public static byte[] ConvertStructToBytes<T>(T thestruct)
        {
            int s = Marshal.SizeOf(thestruct);
            byte[] b = new byte[s];
            IntPtr ptr = Marshal.AllocHGlobal(s);
            Marshal.StructureToPtr(thestruct, ptr, false);
            Marshal.Copy(ptr, b, 0, s);
            Marshal.FreeHGlobal(ptr);
            return b;
        }

        public static T ConvertBytesToStruct<T>(byte[] bytes, ref int offset) where T: new()
        {
            T theStruct = new T();

            int size = Marshal.SizeOf(theStruct);

            if (bytes.Length - offset > size)
            {
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, offset, ptr, size);
                offset += size;
                theStruct = (T)Marshal.PtrToStructure(ptr, theStruct.GetType());
                Marshal.FreeHGlobal(ptr);                
            }
            else
            {
                throw new Exception("Not Enough Bytes Left");
            }
            return theStruct;
        }

        public static Tuple<PacketTypes,DataTypes> GetHeaderInfo(byte[] b, int offset)
        {
            return new Tuple<PacketTypes, DataTypes>((PacketTypes)b[offset++], (DataTypes)b[offset++]);
        }
    }


}
