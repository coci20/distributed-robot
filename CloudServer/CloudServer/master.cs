using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MsgTypes;

/* Class definition for the Master server */

namespace CloudServer
{
   
    class master
    {
        private int workers_port_base = (int)4000;                          // base value for port of worker servers (common between master and workers)
        private uint total_no_of_groups;                                    // command line input
        private uint workers_in_group;                                      // command line input
        private uint total_workers;                                         // total workers in system
        private List<workerGroup> group_list = new List<workerGroup>();     // List of worker group objects
        private SortedList robot_list = new SortedList();
        private uint[] worker_status;                                       // 0 means dead and 1 means alive  

        /* Constructor - Do all initializations here */
        public master(uint groups, uint workers)
        {
            total_no_of_groups = groups;
            workers_in_group = workers;
            total_workers = total_no_of_groups * workers_in_group; // At present assuming all groups contain equal number of replicas

            /* Populate the group list */
            for (uint i = 0; i < groups; i++)
            {
                workerGroup new_group = new workerGroup();
                new_group.group_id = i + 1;
                new_group.number_of_robots = 0;
                new_group.number_of_workers = workers_in_group;
                new_group.group_port_base = (int) (workers_port_base + ((int)i * workers_in_group) + 1);
                group_list.Add(new_group);
            }

            worker_status = new uint[total_workers];

            for (int i = 0; i < workers; i++)
            {
                worker_status[i] = 0; // Initially all are considered dead, they will send an alive message when they come up. 
            }

            /* Finally start the master to listen to clients */

            Console.WriteLine("Starting Master ...");
            startMaster();
        }
         

        private void gatherWorkerStatus(TcpListener tcpListener)
        {
            uint workers_remaining = total_workers;
            byte[] message = new byte[Packet.packet_size];
            int bytesRead;          

            while(workers_remaining!=0)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                NetworkStream clientStream = client.GetStream();

                while (true)
                {
                    bytesRead = 0;

                    try
                    {
                        bytesRead = clientStream.Read(message, 0, message.Length);
                        //Console.WriteLine(bytesRead);
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

                }

                Packet packet = MsgTypes.Packet.ArrayToPacket(message);

                /* Update worker status based on the info received */
                worker_status[packet.ss_msg.serverId - 1] = packet.ss_msg.ss_info.status;
                workers_remaining--;
                client.Close();
            }

            //for (int i = 0; i < total_workers; i++)
            //{
            //    Console.WriteLine(worker_status[i]);
            //}        
        }

        /* Start the Master as a server */
        private void startMaster()
        {
            TcpListener tcpListener_local = new TcpListener(IPAddress.Loopback, 3000);
            tcpListener_local.Start();
            gatherWorkerStatus(tcpListener_local);  // blocks till we gather information from all workers of all groups
            tcpListener_local.Stop();

            /* Start listening for messages from workers servers and robots from now on */ 
            TcpListener tcpListener_any = new TcpListener(IPAddress.Any, 3000);
            tcpListener_any.Start();

            while (true)
            {
               //blocks until a client has connected to the server
               TcpClient client = tcpListener_any.AcceptTcpClient();

               //create a thread to handle communication with connected client
               Thread clientThread = new Thread(() => handleClients(client));
               clientThread.Start();            
            }            
        }

        /* Do Load Balancing and provide the least balanced group */
        private workerGroup get_least_loaded_group()
        {
            group_list.Sort();     // Overriding default compareTo 
            return group_list[0];
        }


/*********************************************** Handlers for different Message types **************************************************************/
        
        /* Handle a server-status info message type packet */
        private void handle_ss_info_msg(server_server_msgs ss_msg)
        {
            /* Update worker status based on the info received */
            worker_status[ss_msg.serverId - 1] = ss_msg.ss_info.status;
            Console.WriteLine("Received ss_info msg from {0} with status {1} \n", ss_msg.serverId,  ss_msg.ss_info.status);
        }

        /* Handle a robot-status info message type packet from a worker server */
        private void handle_rs_info_msg_from_worker(server_server_msgs ss_msg)
        {
            if (ss_msg.rs_info.hasReachedEnd)
            { 
                Console.WriteLine("Robot {0} on parition {1} has completed its task", ss_msg.rs_info.robot_id, ss_msg.rs_info.partition_id);
                ((robotInfo)robot_list[ss_msg.rs_info.robot_id]).partition_id = 0 ;  // put the robot in the free list
            }
        }

        /*
         * Handles a robot-status info message from a robot. 
         * Usually sent by a robot to the master when before it starts its run on the
         * maze partition.
         */
        private void handle_rs_info_msg_from_robot(robot_server_msgs rs_msg, NetworkStream clientStream)
        {
            /* The robot is telling the master that it is alive and needs to attach to a worker server */
            Console.WriteLine("Received message from robot {0} {1} {2}", rs_msg.msgType, rs_msg.rs_info.robot_id, rs_msg.rs_info.partition_id);
            MsgTypes.Packet newPacket = new Packet();

            newPacket.packet_type = (uint)MsgTypes.PACKET_TYPE.RS_MSG;
            newPacket.rs_msg.msgType = (uint)MsgTypes.MSG_TYPE.RS_REPLY;
         
            workerGroup leastLoadedGroup = get_least_loaded_group();

            newPacket.rs_msg.rs_reply.server_port = leastLoadedGroup.group_port_base;
            newPacket.rs_msg.rs_reply.no_of_replicas = leastLoadedGroup.number_of_workers;

            leastLoadedGroup.number_of_robots++;

            /* Add this robot to the Master server's robot list */
            robotInfo robot = new robotInfo();
            robot.robot_id = rs_msg.rs_info.robot_id;
            robot.partition_id = rs_msg.rs_info.partition_id;
            robot.status = (uint)MsgTypes.STATUS.ALIVE;

            robot_list.Add(robot.robot_id, robot);

            byte[] packetArray = MsgTypes.Packet.PacketToArray(newPacket);

            clientStream.Write(packetArray, 0, packetArray.Length);
            clientStream.Flush();
        }

        private void handle_ss_msg(server_server_msgs ss_msg)
        {
            switch (ss_msg.msgType)
            {
                case (uint)MSG_TYPE.SS_INFO:
                    handle_ss_info_msg(ss_msg);
                    break;

                case (uint)MSG_TYPE.RS_INFO:
                    handle_rs_info_msg_from_worker(ss_msg);
                    break;

                default:
                    Console.WriteLine("Invalid Server-Server Msg type received .. {0} ", ss_msg.msgType);
                    break;
            }        
        }

        private void handle_rs_msg(robot_server_msgs rs_msg, NetworkStream clientStream)
        {
            switch (rs_msg.msgType)
            {
                case (uint)MSG_TYPE.RS_INFO:
                    handle_rs_info_msg_from_robot(rs_msg, clientStream);
                    break;

                default:
                    Console.WriteLine("Invalid Robot-Server Msg type received .. {0} ",rs_msg.msgType);
                    break;
            }            
        }

        /* 
         * Handler function for every packet to Master, once it completes all 
         * initializations and is open to packets even outside the system.
         */
        private void handleClients(TcpClient client)
        {
            byte[] message = new byte[Packet.packet_size];
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
                    case (uint)MsgTypes.PACKET_TYPE.SS_MSG:
                        handle_ss_msg(packet.ss_msg);
                        break;

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
    }     
}
