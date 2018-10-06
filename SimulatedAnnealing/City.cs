using System;
using System.Windows;
using System.Windows.Shapes;

namespace SimulatedAnnealing
{
	public class City : ICloneable
	{
		public Rectangle Rect;
		public Point Point;

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}