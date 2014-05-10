using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using MsgTypes;


namespace WorkerServer
{
    class worker
    {
        static uint port_base = 4000;
        private uint my_group_id;
        private uint my_worker_id; // in group
        private uint my_port;
        
        public worker(UInt32 group_no, UInt32 worker_no)
        {
            my_group_id = group_no;
            my_worker_id = worker_no;
            my_port = port_base + worker_no;

            Console.Write("I belong to group {0} \n", my_group_id);
            Console.Write("My id is {0} \n", my_worker_id);            

            startWorker();
            Console.ReadKey();
        }

        private void sendAliveMsgToMaster(TcpClient tcpclient)
        {
            tcpclient.Connect("localhost", 3000);

            NetworkStream stm = tcpclient.GetStream();

            MsgTypes.Packet newPacket = new Packet();

            newPacket.packet_type = (uint) MsgTypes.PACKET_TYPE.SS_MSG;
            newPacket.ss_msg.msgType = (uint)MsgTypes.MSG_TYPE.SS_INFO;
            newPacket.ss_msg.serverId = my_worker_id;
            newPacket.ss_msg.ss_info.status = (uint)MsgTypes.STATUS.ALIVE;

            byte[] packetArray = MsgTypes.Packet.PacketToArray(newPacket);

            stm.Write(packetArray, 0, packetArray.Length);
            stm.Flush();
            
            tcpclient.Close();        
        }

        private void sendRobotIsFreeToMaster(TcpClient tcpclient, uint robot_id)
        {
            tcpclient.Connect("localhost", 3000);

            NetworkStream stm = tcpclient.GetStream();

            MsgTypes.Packet newPacket = new Packet();

            newPacket.packet_type = (uint)MsgTypes.PACKET_TYPE.SS_MSG;
            newPacket.ss_msg.msgType = (uint)MsgTypes.MSG_TYPE.RS_INFO;
            newPacket.ss_msg.serverId = my_worker_id;
            newPacket.ss_msg.rs_info.robot_id = robot_id;
            newPacket.ss_msg.rs_info.hasReachedEnd = true;
            newPacket.ss_msg.rs_info.status = (uint)MsgTypes.STATUS.ALIVE;

            byte[] packetArray = MsgTypes.Packet.PacketToArray(newPacket);

            stm.Write(packetArray, 0, packetArray.Length);
            stm.Flush();

            tcpclient.Close();
        }


        private void startWorker()
        {
            TcpClient tcpclient = new TcpClient();

            /* Worker sends Notification to Master about its status.
             * At present all workers send Alive as the status, but
             * depending upon start up checks this could vary.
             */
            sendAliveMsgToMaster(tcpclient);

            /* Start a thread to send regular heart beat messages to
             * the Master server about the Worker's status
             */
           Thread hbThread = new Thread(() => startHeartBeat());
           hbThread.Start();
           
            /* Now become the server for robots */
            TcpListener tcpListener = new TcpListener(IPAddress.Any, (int)my_port);
            tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = tcpListener.AcceptTcpClient();

                //create a thread to handle communication with connected client
                Thread clientThread = new Thread(() => handleRobots(client));
                clientThread.Start();
            }
        }

        private void handle_rs_info_msg_from_robot(robot_server_msgs rs_msg, NetworkStream clientStream)
        {
            /* The robot is telling the master that it is alive and needs to attach to a worker server */
            String logfileName = "log_" + my_worker_id + "_" + rs_msg.rs_info.robot_id + "_" + rs_msg.rs_info.partition_id + ".txt";
            String logfileName_human = "log_" + my_port + "_human.txt";
            using (StreamWriter logWriter = new StreamWriter(logfileName))
            {

                logWriter.WriteLine("Received message from robot {0} {1} {2} {3} {4}", rs_msg.msgType, rs_msg.rs_info.robot_id, rs_msg.rs_info.partition_id, rs_msg.rs_info.x, rs_msg.rs_info.y);
                if (rs_msg.rs_info.isHuman)
                {
                    using (StreamWriter logWriter_human = File.AppendText(logfileName_human))
                    {
                        logWriter_human.WriteLine("Human encountered at {0} {1}", rs_msg.rs_info.x, rs_msg.rs_info.y);
                    }

                }
                
            }

            if (rs_msg.rs_info.hasReachedEnd)
            { 
                /* Send message to Master to add this robot to free list */
                TcpClient tcpclient = new TcpClient();
                Console.WriteLine("robot {0} at partition {1} has reached end", rs_msg.rs_info.robot_id, rs_msg.rs_info.partition_id);
                sendRobotIsFreeToMaster(tcpclient, rs_msg.rs_info.robot_id);            
            }
            
            Console.WriteLine("Received message from robot {0} {1} {2} {3} {4}", rs_msg.msgType, rs_msg.rs_info.robot_id, rs_msg.rs_info.partition_id, rs_msg.rs_info.x, rs_msg.rs_info.y);
        }

        private void handle_rs_msg(robot_server_msgs rs_msg, NetworkStream clientStream)
        {
            switch (rs_msg.msgType)
            {
                case (uint)MSG_TYPE.RS_INFO:
                    handle_rs_info_msg_from_robot(rs_msg, clientStream);
                    break;

                default:
                    Console.WriteLine("Invalid Robot-Server Msg type received .. {0} ", rs_msg.msgType);
                    break;
            }
        }

        private void handleRobots(TcpClient client) 
        {
            byte[] message = new byte[4096];
            int bytesRead;

            NetworkStream clientStream = client.GetStream();

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, message.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                Packet packet = MsgTypes.Packet.ArrayToPacket(message);

                switch (packet.packet_type)
                {
                    case (uint)MsgTypes.PACKET_TYPE.RS_MSG:
                        handle_rs_msg(packet.rs_msg, clientStream);
                        break;

                    default:
                        Console.WriteLine("Invalid Packet type received .. ");
                        break;

                }
            }

            client.Close(); 
        
        }

        private void startHeartBeat()
        {            
            while (true)
            {
                System.Threading.Thread.Sleep(5000);

                TcpClient tcpclient = new TcpClient();

                tcpclient.Connect("localhost", 3000); // connect to master

                NetworkStream stm = tcpclient.GetStream();

                /* At present we always send Alive status */
                MsgTypes.Packet newPacket = new Packet();

                newPacket.packet_type = (uint)MsgTypes.PACKET_TYPE.SS_MSG;
                newPacket.ss_msg.msgType = (uint)MsgTypes.MSG_TYPE.SS_INFO;
                newPacket.ss_msg.serverId = my_worker_id;
                newPacket.ss_msg.ss_info.status = (uint)MsgTypes.STATUS.ALIVE;

                byte[] packetArray = MsgTypes.Packet.PacketToArray(newPacket);

                stm.Write(packetArray, 0, packetArray.Length);
                stm.Flush();

                tcpclient.Close();      
            
            }
        }
    }
}
