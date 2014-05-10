using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace CloudClient
{
	public class MazeInstance
	{
		List<Points> list_of_Points=new List<Points>();//create points and put them in a list
		Points last_drawn_point;
		List<Points> list_of_Obstacles=new List<Points>();

		public Color mazeColor;

		const int OBSTACLES=100;
		/// <summary>
		/// Check if the maze has been rendered.
		/// </summary>
		public bool render=false;

		string filename = "input1.txt";

		/// <summary>
		/// Get_list_of_s the points.
		/// </summary>
		/// <returns>The list of points.</returns>
		public List<Points> get_list_of_Points(){
			return list_of_Points;
		}

		public Points getLastPoint(){
			return last_drawn_point;
		}

		public void setLastPoint(Points pt){
			last_drawn_point = pt;
		}
		/// <summary>
		/// Get_list_of_s the obstacles.
		/// </summary>
		/// <returns>The list of obstacles.</returns>
		public List<Points> get_list_of_Obstacles(){
			return list_of_Obstacles;
		}

		/// <summary>
		/// Get_list_of_s the points_length.
		/// </summary>
		/// <returns>The points_length.</returns>
		public int get_list_of_Points_length(){
			if(list_of_Points!=null)
			return list_of_Points.Count;
			return 0;
		}

		/// <summary>
		/// Get_list_of_s the obstacles_length.
		/// </summary>
		/// <returns>The obstacles_length.</returns>
		public int get_list_of_Obstacles_length(){
			if (list_of_Obstacles != null) {
				return list_of_Obstacles.Count;
			}
			return 0;
		}

		/// <summary>
		/// Initializes a new instance of the Maze class.
		/// create points
		/// create obstacles
		/// </summary>
		public MazeInstance ()
		{
			list_of_Points = createPoints (filename);
			list_of_Obstacles = fillObstacles (list_of_Points);
		}

		/// <summary>
		/// Creates the points.
		/// </summary>
		/// <returns>The points in my grid.</returns>
		/// <param name="filename">Filename.</param>
		List<Points> createPoints (string filename)
		{
//			using (StreamWriter F = new StreamWriter(filename)) { 
//				for (double x=-0.8f; x<=0.8f; x+=0.08) {
//					for (double y=-0.8f; y<=0.8f; y+=0.08) {
//						Points p = new Points ();
//						p.putPoint (x, y);
//						F.WriteLine (p.toString ());
//					}
//				}
//			}
			List<Points> myList = listOfPoints(filename);
			return myList;
		}

		/// <summary>
		/// Lists the of points.
		/// </summary>
		/// <returns>The list of points.</returns>
		/// <param name="filename">Filename.</param>
		List<Points> listOfPoints (string filename)
		{
			List<Points> myList = new List<Points>();
			using (StreamReader reader= new StreamReader(filename)) {
				string line;
				while ((line=reader.ReadLine())!=null) {
					string[] delim={","};
					string[] s=line.Split(delim,StringSplitOptions.RemoveEmptyEntries);
					double X=Convert.ToDouble(s[0]);
					double Y=Convert.ToDouble(s[1]);
					Points p=new Points();
					p.putPoint (X,Y);
					myList.Add(p);
				}
			}
			return myList;
		}


		/// <summary>
		/// Fills the obstacles.
		/// </summary>
		/// <returns>The obstacles list.</returns>
		/// <param name="list_of_Points">List_of_ points.</param>
		List<Points> fillObstacles (List<Points> list_of_Points)
		{
			List<Points> list_of_Obstacles = new List<Points> ();
			Random rand = new Random ();
			int size = get_list_of_Points_length ();
			for (int i=0; i<OBSTACLES; i++) {
				int randIndex = rand.Next (0, size);
				Points pt = list_of_Points [randIndex];
				pt.setAsObstacle ();
				list_of_Obstacles.Add (pt);
			}
			return list_of_Obstacles;
		}
	}
}

