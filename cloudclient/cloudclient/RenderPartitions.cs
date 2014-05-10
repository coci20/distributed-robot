	using System;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using System.Drawing;
	using OpenTK.Input;
	using System.Threading;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Net.Sockets;
	using MsgTypes;
	using System.ComponentModel;

namespace CloudClient
	{
		/// <summary>
		/// Main class.
		/// </summary>
		public class RenderPartitions
		{
		public object resetRobot (List<MazeBlock> list)
		{
			throw new NotImplementedException ();
		}

			List<MazeBlock> mainBlockList = new List<MazeBlock> ();
			//static GameWindow myWindow = new GameWindow();

			public void putBlocks(List<MazeBlock> blocks){
				mainBlockList = blocks;
			}

			public List<MazeBlock> getBlocks(){
				return mainBlockList;
			}

			private int sendAliveMsgToMaster(uint robot_id,uint partition_id)
		{
			TcpClient tcpclient = new TcpClient();
			tcpclient.Connect("192.168.1.2", 3000);
			Console.WriteLine ("Connection is " + tcpclient.Connected);
			NetworkStream stm1 = tcpclient.GetStream();

			MsgTypes.Packet newPacket = new Packet();

			newPacket.packet_type = (uint) MsgTypes.PACKET_TYPE.RS_MSG;
			newPacket.rs_msg.msgType = (uint)MsgTypes.MSG_TYPE.RS_INFO;
			newPacket.rs_msg.rs_info.robot_id =  robot_id;
			newPacket.rs_msg.rs_info.partition_id = partition_id;
			newPacket.rs_msg.rs_info.status = (uint)MsgTypes.STATUS.ALIVE;
			//newPacket.rs_msg.rs_info.x = 0;
			//newPacket.rs_msg.rs_info.y = 0;
			//newPacket.rs_msg.rs_info.isHuman = false;

			byte[] packetArray = MsgTypes.Packet.PacketToArray(newPacket);

			stm1.Write(packetArray, 0, packetArray.Length);
			stm1.Flush();

			byte[] replyArray = new byte[4096];
			Console.WriteLine (stm1.CanRead);

//			tcpclient.Close();
			stm1.Read (replyArray, 0, replyArray.Length);
			Packet new_Packet = MsgTypes.Packet.ArrayToPacket(replyArray);
			Console.WriteLine (new_Packet.rs_msg.rs_reply.server_port);
			tcpclient.Close ();		
//			Console.WriteLine ("here");
//			NetworkStream stm2 = tcpclient.GetStream ();
//
//			stm2.Read (packetArray, 0, packetArray.Length);
//			Console.WriteLine ("Message sent to master");
//			Packet new_Packet = MsgTypes.Packet.ArrayToPacket(packetArray);
//
			return (int)new_Packet.rs_msg.rs_reply.server_port;
				
			//return 3000;

			/* Receive Reply from master */
			//tcpclient.Close();        
		}

			public void startRobot (uint partition_index, uint robot_id)
			{
			//send to the master the "is-alive" signal
			int port_no = sendAliveMsgToMaster (robot_id,partition_index);

			//receive from the master, get the port no

			//open the port with the master
			TcpClient tcpclient = new TcpClient ();
			tcpclient.Connect ("192.168.1.2", port_no);//to be changed based on the port no given by the master
			NetworkStream stm = tcpclient.GetStream ();

			//mainBlockList = blocks;
	//			bool render=true;
			if (partition_index == 0) {
				return;
			}

			Console.WriteLine ("current_index is " + partition_index);
			//Thread.Sleep (100);
				Console.WriteLine (Thread.CurrentThread.ManagedThreadId + " Drawing this maze");
				using (var game = new GameWindow())
	            {

					game.Load += (sender, e) =>
	                {
	                    // setup settings, load textures, sounds
	                    game.VSync = VSyncMode.On;
						game.WindowState=WindowState.Normal;
	                };
	 
	                game.Resize += (sender, e) =>
	                {
	                    GL.Viewport(0, 0, game.Width, game.Height);
	                };
	 
	                game.UpdateFrame += (sender, e) =>
	                {
	                    // add game logic, input handling
	                    if (game.Keyboard[Key.Escape])
	                    {
	                        game.Exit();
	                    }
	                };
	 
				game.RenderFrame += (sender, e) =>
				{
					//pass the socket with the below fn

					//drawThisMaze (partition_index - 1, game, mainBlockList, stm);

					if (tcpclient != null){
						drawThisMaze (partition_index - 1, game, mainBlockList, stm);
						tcpclient.Close ();
						tcpclient=null;
					}
					game.Exit();

					

				};//show the obstacles on the grid
						//game.SwapBuffers();
					
	                
	                // Run the game at 60 updates per second
				//tcpclient.Close ();	
				game.Run (100.0);
	            }
			}

			/// <summary>
			/// Moves the point across the grid.
			/// </summary>
			/// <param name="pt">Point.</param>
			static void moveAcrossGrid (Points pt,MazeBlock maze)
			{
				//start movement
				//if not around an obstacle then draw the point
				//if (!aroundObstacle (pt,obstacleList)) {
				if(pt.isObstaclePoint()==false){

					GL.Color3(pt.pointColor);

					GL.Vertex2(pt.getX(),pt.getY());
				}

			}	

//			public int getRandomIndex(List<MazeBlock> blocks){
//				Random rand = new Random ();
//				int randIndex = rand.Next (0, blocks.Count);
//				return randIndex;
//			}

			public void drawThisMaze (uint partition_index,GameWindow game,List<MazeBlock> partitions,NetworkStream stm)
			{
				
			//block_index--;
				Console.WriteLine (Thread.CurrentThread.ManagedThreadId + " will draw " + partition_index);
				MazeBlock partition = partitions [(int)(partition_index)];
				
				//partition.isrendered = true;
				uint r_id = partition_index + 1;
				uint p_id = partition_index;
				partition.setRobotID (r_id);
				GL.ClearColor(new Color4(0f, 0, 0, 1));
				//set the number of processors to 2
				//int no_of_procs = nop;

				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadIdentity();
				GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
				GL.PointSize(7.9f);
				GL.Begin (PrimitiveType.Points);
				//GL.Color3 (Color.Yellow);
				//get each of the maze blocks one by one
				//for (int i=0; i<blocks.Count; i++) {
				foreach (Points p in partition.get_list_of_Obstacles()) {
					GL.Color3 (p.pointColor);
					GL.Vertex2 (p.getX (), p.getY ());
				}
				foreach (Points p in partition.get_list_of_Humans()) {
					GL.Color3 (p.pointColor);
					GL.Vertex2 (p.getX (), p.getY ());
				}
				//}
				GL.End ();
				game.SwapBuffers ();

				//for (int i=0; i<blocks.Count; i++) {
					//move across the grind handling obstacles
				GL.PointSize(3.9f);
			int len = partition.get_list_of_Points_length ();
			int count = 0;
			bool hasReached = false;
			foreach(Points p in partition.get_list_of_Points()){
				//tell master this point is being drawn
				if (count == len - 1) {
					hasReached = true;
					Console.WriteLine ("robot_id is" + r_id);
					Client.robot_info [r_id-1] = 0;
				}
				send_back_this_point (stm, p,r_id,p_id,hasReached);	
				GL.Begin(PrimitiveType.Points);
				moveAcrossGrid(p,partition);
				GL.End();
				count++;
				Thread.Sleep(60);
				game.SwapBuffers();
			}
			//blocks[i].render = false;

		
			//int i = 0;
			//partition.get_random_color (partitions.Count);
				
					//blocks[i].render = false;

			}

		void send_back_this_point (NetworkStream stm, Points p,uint robot_id,uint partition_id,bool hasReached)
		{
		
			MsgTypes.Packet newPacket = new Packet ();
			newPacket.packet_type = (uint) MsgTypes.PACKET_TYPE.RS_MSG;
			newPacket.rs_msg.msgType = (uint)MsgTypes.MSG_TYPE.RS_INFO;
			newPacket.rs_msg.rs_info.robot_id =  robot_id;
			newPacket.rs_msg.rs_info.partition_id = partition_id;
			newPacket.rs_msg.rs_info.x = p.getX ();
			newPacket.rs_msg.rs_info.y = p.getY ();

			if (p.pointColor == Color.Red) {
				newPacket.rs_msg.rs_info.isHuman = true;
			} else {
				newPacket.rs_msg.rs_info.isHuman = false;
			}

			if (hasReached == true) {
				newPacket.rs_msg.rs_info.hasReachedEnd = true;
			}

			byte[] packetArray = MsgTypes.Packet.PacketToArray(newPacket);

			stm.Write(packetArray, 0, packetArray.Length);
			stm.Flush();
		}




	}




		
}




