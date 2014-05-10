using System;
using System.Collections.Generic;
using System.Threading;

namespace CloudClient
{
	public class Client
	{
		private static uint no_of_robots;
		private static uint no_of_partitions;
		private static uint[] robot_info; // stores the block id to which each robot is assigned
		                                  // 0 implies the robot is free

		public static int Main(string[] args){

			no_of_robots = Convert.ToUInt32(args[0]);
			no_of_partitions = Convert.ToUInt32(args[1]);

			robot_info = new uint[no_of_robots];

			for (int i=0; i<no_of_robots; i++) {
				robot_info[i] = 0; // initialize all to 0
			}


			int no_of_procs= (int)Math.Sqrt(no_of_partitions);
			double xgap, ygap;

			//set the initial co-ordinates of the maze
			double xinitial = -1.0f;
			double xfinal = 1.0f;
			double yinitial = -1.0f;
			double yfinal = 1.0f;

			//calculate the gap size over x and y
			xgap = (xfinal - xinitial) / no_of_procs;
			ygap = (yfinal - yinitial) / no_of_procs;

			//loop for the no of processors, 
			//at each iteration create a new maze block
			List<MazeBlock> partitions = new List<MazeBlock> ();

			int count = 0;
			for (double x=xinitial; x<xfinal; x+=xgap) {
				for (double y=yinitial; y<yfinal; y+=ygap) {
					//create the end-points
					Points pin = new Points ();
					pin.putPoint (x, y);
					Points pfin = new Points ();
					pfin.putPoint (x + xgap, y + ygap);

					//create the maze block
					MazeBlock newBlock = new MazeBlock (pin, pfin, count);
					partitions.Add (newBlock);
					count++;
				}

			}

			//			using (StreamWriter F = new StreamWriter("input1.txt")) { 
			//				for (double x=-0.8f; x<=0.8f; x+=0.08) {
			//					for (double y=-0.8f; y<=0.8f; y+=0.08) {
			//						Points p = new Points ();
			//						p.putPoint (x, y);
			//						F.WriteLine (p.toString ());
			//					}
			//				}
			//			}
			//			Console.WriteLine ("Thread start/stop/join sample");
			RenderPartitions renderPartition = new RenderPartitions ();
			renderPartition.putBlocks (partitions); // Shares the partitions with the renderPartition object

//			/* Randomly generate a robot id and assign it to a block */
//			/			int rand_robot_index = rand.Next (no_of_robots);
//
			for (uint i=0; i<partitions.Count;i++) {
				robot_info [i] = i+1;
				//Console.WriteLine (i);
				Thread robotThread = new Thread(() => renderPartition.startRobot(i, i+1));
				robotThread.Start ();
				Thread.Sleep (1000);
			}

			return 0;
		}
	}
}

