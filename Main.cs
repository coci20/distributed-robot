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
	/// <summary>
	/// Main class.
	/// </summary>
	class MainClass
	{
		public static void Main (string[] args)
		{

			bool render=true;

			MazeInstance maze1 = new MazeInstance ();

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
					foreach(Points p in maze1.get_list_of_Obstacles()){
						GL.Begin(PrimitiveType.Points);
						GL.Color3(Color.Yellow);
						GL.Vertex2(p.getX(),p.getY());
						GL.End();
						game.SwapBuffers();
					}
					//move across the grind handling obstacles
					foreach(Points p in maze1.get_list_of_Points()){
						GL.Begin(PrimitiveType.Points);
						moveAcrossGrid(p);
						GL.End();
						game.SwapBuffers();
					}

					render=false;

                };
                // Run the game at 60 updates per second
				game.Run (60.0);
            }
		}

		/// <summary>
		/// Moves the point across the grid.
		/// </summary>
		/// <param name="pt">Point.</param>
		static void moveAcrossGrid (Points pt)
		{
			//start movement
			//if not around an obstacle then draw the point
			//if (!aroundObstacle (pt,obstacleList)) {
			if(pt.isObstaclePoint()==false){
				GL.Color3(Color.DarkGreen);
				GL.Vertex2(pt.getX(),pt.getY());
			}

		}	

	}
}
