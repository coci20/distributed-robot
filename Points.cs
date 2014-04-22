using System;

namespace ConsoleApp1
{
	public class Points
	{
		double x;
		double y;
		bool isObstacle;

		public Points ()
		{
			x=0.0f;
			y=0.0f;
			isObstacle = false;
		}

		/// <summary>
		/// Gets the x.
		/// </summary>
		/// <returns>The x.</returns>
		public double getX ()
		{
			return x;
		}

		/// <summary>
		/// Gets the y.
		/// </summary>
		/// <returns>The y.</returns>
		public double getY ()
		{	
			return y;
		}

		/// <summary>
		/// Puts the point.
		/// </summary>
		/// <param name="s1">S1.</param>
		/// <param name="s2">S2.</param>
		public void putPoint (double s1, double s2)
		{
			x=s1;
			y=s2;
		}

		public string toString ()
		{	
			return x + "," + y;
		}

		/// <summary>
		/// Sets as obstacle.
		/// </summary>
		public void setAsObstacle(){
			this.isObstacle=true;
		}

		/// <summary>
		/// Checks the obstacle point.
		/// </summary>
		/// <returns><c>true</c>, if obstacle point, <c>false</c> otherwise.</returns>
		public bool isObstaclePoint(){
			return isObstacle;
		}
	}
}

