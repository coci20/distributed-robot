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

		public void incPoint (double incV)
		{
			x += incV;
			y += incV;
		}

		public double getX ()
		{
			return x;
		}

		public double getY ()
		{	
			return y;
		}

		public void putPoint (double s1, double s2)
		{
			x=s1;
			y=s2;
		}

		public string toString ()
		{	
			return x + "," + y;
		}

		public void setAsObstacle(){
			this.isObstacle=true;
		}

		public bool isObstaclePoint(){
			return isObstacle;
		}
	}
}

