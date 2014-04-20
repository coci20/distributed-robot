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

namespace ConsoleApp1
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//populate points in a File named "input1.txt"

			bool render=true;
			//double incV=0.0005f;
			Points pt=new Points();

			Console.WriteLine("created a point");

			Console.WriteLine("create a list of obstacle points");

			List<Points> obstacleList=new List<Points>();
			List<Points> pointList = populatePoints("input1.txt");
			int size = pointList.Count;

			fillObstaclePoints(obstacleList,size,pointList);
			Thread.Sleep (20);

			using (var game = new GameWindow())
            {

				game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                };
 
                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };
 
                game.UpdateFrame += (sender, e) =>
                {
                    // add game logic, input handling
                    if (game.Keyboard[Key.Escape] || render==false)
                    {
                        game.Exit();
                    }
                };
 
                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
 
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
 					GL.PointSize(4.9f);
                    
					//show the obstacles on the grid
					foreach(Points p in obstacleList){
						GL.Begin(PrimitiveType.Points);
						GL.Color3(Color.Yellow);
						GL.Vertex2(p.getX(),p.getY());
						GL.End();
						game.SwapBuffers();
					}
					//move across the grind handling obstacles
					foreach(Points p in pointList){
						GL.Begin(PrimitiveType.Points);
						moveAcrossGrid(p,obstacleList);
						GL.End();
						game.SwapBuffers();
					}

					//single obstacle
                    //GL.Color3(Color.MidnightBlue);
					//GL.Vertex2(0.5f,0.5f);
					//for(double x=-0.8;x<=0.8;x+=0.1){
					//	for(double y=0.8;y>=-0.8;y-=0.1){
					//create movement across the grid
					//present obstacles

//					if(pt.getX() <0.4f || pt.getX() >0.6){
//								GL.Color3(Color.DarkSeaGreen);
//								GL.Vertex2(pt.getX(),pt.getY());
//								Console.WriteLine(pt.toString());
//					}
					//	}
					//}
					render=false;

					//pt.incPoint(incV);
                    
					//Thread.Sleep(15);
                };
                // Run the game at 60 updates per second
				game.Run (60.0);
            }
		}

		static void fillObstaclePoints (List<Points> obstacleList,
		                                int size,
		                                List<Points> pointList)
		{
			//hard-coding the list of obstacles
			//define here the #obstacles
			int no_obstacles = 100;
			Random rand = new Random ();
			for (int i=0; i<100; i++) {
				int randIndex = rand.Next (0, size);
				Points pt = pointList [randIndex];
				pt.setAsObstacle ();
				obstacleList.Add (pt);
			}
			Console.WriteLine (obstacleList.Count);

		}
		static bool aroundObstacle (Points pt, List<Points> obstacleList)
		{
			//create obstacle points
			//bool check=false;
			double range=0.05f;
			double currX=pt.getX();
			double currY=pt.getY();
			foreach (Points p in obstacleList) {
				double obsX=p.getX();
				double obsY=p.getY();

				//Console.WriteLine(p.toString());
				if((currX<obsX-range || currX>obsX+range)
					&&(currY<obsY-range||currY>obsY+range)){
					;//not an obstacle, do nothing
				}
				else{
					GL.Color3(Color.Aqua);
					GL.Vertex2(pt.getX(),pt.getY());
					//Console.WriteLine(pt.toString());
					return true;
				}
			}
			return false;
			//throw new NotImplementedException ();
		}

		static void moveAcrossGrid (Points pt,List<Points> obstacleList)
		{
			//start movement
			//if not around an obstacle then draw the point
			//if (!aroundObstacle (pt,obstacleList)) {
			if(pt.isObstaclePoint()==false){
				GL.Color3(Color.DarkGreen);
				GL.Vertex2(pt.getX(),pt.getY());
			}


		}	


		static List<Points> populatePoints (string inputtxt)
		{
			using (StreamWriter F = new StreamWriter("input1.txt")) { 
				for (double x=-0.8f; x<=0.8f; x+=0.05) {
					for (double y=-0.8f; y<=0.8f; y+=0.05) {
						Points p = new Points ();
						p.putPoint (x, y);
						//byte bArray=convertToByte(p.toString());
						F.WriteLine (p.toString ());
					}
				}
			}
			List<Points> myList = listPoints(inputtxt);
			return myList;
		}
			//F.Close();		
		static List<Points> listPoints (string inputtxt)
		{
			//int x = 0;
			List<Points> myList = new List<Points>();
				using (StreamReader reader= new StreamReader(inputtxt)) {
					string line;
					while ((line=reader.ReadLine())!=null) {
			//			Console.WriteLine (x + " " + line);
			//			x++;
					string[] delim={","};
					string[] s=line.Split(delim,StringSplitOptions.RemoveEmptyEntries);
					double X=Convert.ToDouble(s[0]);
					double Y=Convert.ToDouble(s[1]);
					Points p=new Points();
					p.putPoint (X,Y);
					myList.Add(p);
					}
				}

	//List<Points> myList = listPoints(inputtxt);
			return myList;
			}



		


	}
}
