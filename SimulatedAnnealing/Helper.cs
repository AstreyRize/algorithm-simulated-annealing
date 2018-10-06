using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimulatedAnnealing
{
	public class Helper
	{
		private readonly MainWindow _mainWindow;
		private static Helper _instance;

		private Helper(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
		}

		public static Helper GetInstance(MainWindow mainWindow)
		{
			return _instance ?? (_instance = new Helper(mainWindow));
		}

		public void DrawLines(IReadOnlyList<City> citys, Brush solidColorBrush)
		{
			//Рисуем все новые линии
			for (var i = 1; i < citys.Count; i++)
			{
				var line = new Line
				{
					Stroke = solidColorBrush,
					X1 = citys[i - 1].Point.X,
					X2 = citys[i].Point.X,
					Y1 = citys[i - 1].Point.Y,
					Y2 = citys[i].Point.Y,
					StrokeThickness = 2
				};

				_mainWindow.Plane.Children.Add(line);
			}

			var line2 = new Line
			{
				Stroke = solidColorBrush,
				X1 = citys[citys.Count - 1].Point.X,
				X2 = citys[0].Point.X,
				Y1 = citys[citys.Count - 1].Point.Y,
				Y2 = citys[0].Point.Y,
				StrokeThickness = 2
			};

			_mainWindow.Plane.Children.Add(line2);
		}

		public void DrawGrafic(IReadOnlyCollection<double> list, Color color)
		{
			double step = 0;

			foreach (var temp in list)
			{
				var rect = new Rectangle
				{
					Width = 3,
					Height = 3,
					Fill = new SolidColorBrush(color)
				};

				Canvas.SetTop(rect, _mainWindow.Grafic.Height - temp);
				Canvas.SetLeft(rect, step);

				_mainWindow.Grafic.Children.Add(rect);
				step += _mainWindow.Grafic.Width / list.Count;
			}
		}

		public void DeleteLines()
		{
			var lines = new List<UIElement>();

			foreach (UIElement child in _mainWindow.Plane.Children)
			{
				if (child is Line)
				{
					lines.Add(child);
				}
			}

			lines.ForEach(x => { _mainWindow.Plane.Children.Remove(x); });
		}
	}
}
