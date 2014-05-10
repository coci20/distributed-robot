using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


/* This file defines the message structures used for communication between 
 * server-server and robot-server. Each message is encapuslated as a packet
 * whose first field is always the packet type which tells if the message is
 * from another server or a robot. The next part of the packet is a specific
 * structure based on the packet type. There are 2 different structures for 
 * the 2 packet types and both these structures start from the same offset
 * (emulating a Union from C/C++). The receiver should access the right structure
 * based on the packet type.
 * 
 * The packet structure also provides methods to serialize and deserialize the 
 * structures for a packet.
 * 
 * Max Size of a packet : 4096 bytes.
 */
 
namespace MsgTypes
{
    /* Enums for messages */
    public enum PACKET_TYPE { SS_MSG = 1, RS_MSG };
    public enum MSG_TYPE { SS_INFO = 1, RS_INFO, RS_REPLY };
    public enum STATUS { ALIVE = 1, UNKNOWN_ERROR };


    /* Packet Structure */
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Packet
    {
        public static uint packet_size = 4096; // Constant

        [FieldOffset(0)]
        public uint packet_type;  // from PACKET_TYPE enum          

        /* Depending on the packet type above, one of the structures from below will be populated */
        [FieldOffset(8)]
        public server_server_msgs ss_msg;
        [FieldOffset(8)]
        public robot_server_msgs rs_msg;     
   
        /* Serializer function */
        
        public static byte[] PacketToArray(Packet packet)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            formatter.Serialize(stream, packet);
            byte[] packetArray = stream.GetBuffer();

            stream.Close();

            return packetArray;
        }

        /* Deserializer fuction */

        public static Packet ArrayToPacket(byte[] array)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(array);

            Packet packet = new Packet();
            packet = (Packet)formatter.Deserialize(stream);
            stream.Close();

            return packet;
        }
    } // End of packet structure

    /* Server Status Info structure. Mostly workers will send 
     * their status to the master using this structure encapsulated
     * inside a packet of type server-server message.
     */

    [Serializable]
    public struct server_status_info
    {
        public uint status;
    }

    /* Robot Status Info structure. Robots will send their 
     * status to the server using this structure encapsulated
     * inside a packet of type robot-server message.
     */

    [Serializable]
    public struct robot_status_info
    {
        public uint robot_id;
        public uint partition_id;
        public uint status;
        public double x;
        public double y;
        public bool isHuman;
        public bool hasReachedEnd;
    }

    [Serializable]
    public struct robot_status_reply
    {
        public int server_port;
        public uint no_of_replicas;
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct server_server_msgs
    {
        [FieldOffset(0)]
        public uint msgType;  // uses the enum MSG_TYPE
        [FieldOffset(4)]
        public uint serverId; // 0 for master and any other number indicates the worker_id  
        [FieldOffset(8)]
        public server_status_info ss_info;
        [FieldOffset(8)]
        public robot_status_info rs_info;        
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct robot_server_msgs
    {
        [FieldOffset(0)]
        public uint msgType;  // uses the enum MSG_TYPE
        [FieldOffset(4)]
        public robot_status_info rs_info;
        [FieldOffset(4)]
        public robot_status_reply rs_reply;
    } 
}
