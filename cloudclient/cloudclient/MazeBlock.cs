using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;


namespace CloudClient
{
	/// <summary>
	/// Maze block.
	/// A maze block is comprised of points from pinitial to pfinal
	/// this information is stored in a file and then in a list
	/// this is to be then send to the renderer to draw the maze block
	/// by the individual robot moving in this block
	/// items present
	/// pinit
	/// pfinal
	/// blockColor
	/// 
	/// storage required
	/// file-read, write ops
	/// list of points
	/// </summary>
	public class MazeBlock
	{
		Points pinit;
		Points pfinal;
		Color blockColor;
		List<Points> pointList;
		uint robot_id;

		List<Points> list_of_Obstacles=new List<Points>();
		List<Points> list_of_Humans = new List<Points> ();

		const int OBSTACLES=10;
		const int HUMANS = 5;

		public Points retLastPoint(){
			return pfinal;
		}

		public bool isrendered;
		public Points last_drawn_point;

		public void setLastDrawnPoint(Points pt){
			last_drawn_point = pt;
		}

		public Points getLastDrawnPoint(){
			return last_drawn_point;
		}

		public bool isPartitionRendered(){
			return (last_drawn_point.getX () == pfinal.getX () 
				&& last_drawn_point.getY () == pfinal.getY ());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConsoleApp1.MazeBlock"/> class.
		/// </summary>
		/// <param name="pi">Pi.</param>
		/// <param name="pf">Pf.</param>
		/// <param name="n">N.</param>
		public MazeBlock (Points pi, Points pf,int n){
		
				pinit = pi;
				pfinal = pf;

				//create the file
				String fileName = "input_" + n + ".txt";
				//Console.WriteLine ("created file: " + fileName);

				//fill the file with the points
				write_to_the_file (fileName, pinit, pfinal);
				
				//return the list of the points in the filename
				pointList = read_from_the_file (fileName);

				list_of_Obstacles = fillObstacles (pointList);
				list_of_Humans = fillHumans (pointList);
			last_drawn_point = pf;
				
			isrendered = false;
			
		}//end of constructor

		public void setRobotID(uint id){
			robot_id = id;
		}

		public uint getRobotID(){
			return robot_id;
		}
		public void put_block_color(Color color){
			blockColor = color;
		}

		public Color get_block_color(){
			return blockColor;
		}

		/// <summary>
		/// Get_list_of_s the points.
		/// </summary>
		/// <returns>The list of points.</returns>
		public List<Points> get_list_of_Points(){
			return pointList;
		}

		/// <summary>
		/// Get_list_of_s the obstacles.
		/// </summary>
		/// <returns>The list of obstacles.</returns>
		public List<Points> get_list_of_Obstacles(){
			return list_of_Obstacles;
		}

		public List<Points> get_list_of_Humans(){
			return list_of_Humans;
		}

		/// <summary>
		/// Puts the color.
		/// </summary>
		/// <param name="thisColor">This color.</param>
		public void putColor(Color thisColor){
				blockColor=	thisColor;
		}

		/// <summary>
		/// Get_list_of_s the points_length.
		/// </summary>
		/// <returns>The points_length.</returns>
		public int get_list_of_Points_length(){
			if(pointList!=null)
				return pointList.Count;
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

		public int get_list_of_Humans_length(){
			if (list_of_Humans != null) {
				return list_of_Humans.Count;
			}
			return 0;
		}

		/// <summary>
		/// Write_to_the_file the specified fileName, pinit and pfinal.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="pinit">Pinit.</param>
		/// <param name="pfinal">Pfinal.</param>
		void write_to_the_file (string fileName, Points pinit, Points pfinal)
		{
			using (StreamWriter F = new StreamWriter(fileName)) {
				for (double x=pinit.getX(); x<=pfinal.getX(); x+=0.04) {
					for (double y=pinit.getY(); y<=pfinal.getY(); y+=0.04) {
						Points p = new Points ();
						p.putPoint (x, y);
						F.WriteLine (p.toString ());
					}
				}
			}
		}

		/// <summary>
		/// Read_from_the_file the specified fileName.
		/// </summary>
		/// <param name="fileName">File name.</param>
		List<Points> read_from_the_file (string fileName)
		{
			List<Points> myList = new List<Points> ();
			using (StreamReader reader= new StreamReader(fileName)) {
				string line;
				while ((line=reader.ReadLine())!=null) {
					string[] delim = { "," };
					string[] s = line.Split (delim, StringSplitOptions.RemoveEmptyEntries);
					double X = Convert.ToDouble (s [0]);
					double Y = Convert.ToDouble (s [1]);
					Points p = new Points ();
					p.putPoint (X, Y);
					myList.Add (p);
				}
			}
			return myList;
		}

		/// <summary>
		/// Fills the obstacles.
		/// </summary>
		/// <returns>The obstacles.</returns>
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
				pt.pointColor = Color.Yellow;
				list_of_Obstacles.Add (pt);
			}
			return list_of_Obstacles;
		}

		List<Points> fillHumans (List<Points> list_of_Points)
		{
			List<Points> list_of_Humans = new List<Points> ();
			Random rand = new Random ();
			int size = get_list_of_Points_length ();
			for (int i=0; i<HUMANS; i++) {
				int randIndex = rand.Next (0, size);
				Points pt = list_of_Points [randIndex];
				pt.setAsObstacle ();
				pt.pointColor = Color.Red;
				list_of_Humans.Add (pt);
			}
			return list_of_Humans;
		}

//		public void get_random_color (int count)
//		{
//			//get r
//			Random rand1 = new Random ();
//			int randIndex1 = rand1.Next (1, count);
//
//			//get g
//			Random rand2 = new Random ();
//			int randIndex2 = rand2.Next (1, count);
//
//			//get b
//			Random rand3 = new Random ();
//			int randIndex3 = rand3.Next (1, count);
//			put_block_color(Color.FromArgb (255 / randIndex1 , 255 / randIndex2, 255 / randIndex3));
//		}

	}//end of class
}//end of namespace

