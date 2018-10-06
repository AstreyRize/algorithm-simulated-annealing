using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimulatedAnnealing
{
	public partial class MainWindow
	{
		private readonly BackgroundWorker _bg = new BackgroundWorker();
		private readonly BackgroundWorker _bg2 = new BackgroundWorker();

		private List<City> _citys = new List<City>();                       //Выбранная последовательность городов
		private List<City> _newCitys = new List<City>();                    //Рабочая последовательность городов
		private List<City> _bestWay = new List<City>();                     //Лучшая последовательность городов за все время расчетов
		private readonly List<double> _temperatures = new List<double>();   //График снижения температуры
		private readonly List<double> _energys = new List<double>();        //График полученных энергий (расстояния, которое нужно пройти)
		private readonly List<double> _bestEnergys = new List<double>();    //Лучшие энергии (лучшие расстояния)
		private double _bestEnergy;                                         //Лучшая длинна пути за все время расчетов

		private readonly Random _rand = new Random();

		private const float TEMPERATURE = 70f;                    //Начальная температура
		private const float FINAL_TEMPERATURE = 0.5f;             //Конечная температура
		private const int ITERATIONS = 500;                       //Количество попыток, до того, как температура будет понижена
		private const float ALPHA = 0.98f;                        //Что то )

		private double _oldEnergy;                                //Предыдущая энергия
		private double _newEnergy;                                //Новая энергия

		public MainWindow()
		{
			InitializeComponent();

			_bg.DoWork += bg_DoWork;
			_bg.RunWorkerCompleted += bg_RunWorkerCompleted;

			_bg2.DoWork += bg2_DoWork;
			_bg2.RunWorkerCompleted += bg2_RunWorkerCompleted;

			//Размещаем города в случайном порядке
			for (var i = 0; i < 20; i++)
			{
				var city = new City
				{
					Rect = new Rectangle
					{
						Width = 5,
						Height = 5
					},
					Point = new Point(_rand.Next(0, 499), _rand.Next(0, 499))
				};

				city.Rect.Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(_rand.Next(0, 255)), Convert.ToByte(_rand.Next(0, 255)), Convert.ToByte(_rand.Next(0, 255))));

				Canvas.SetTop(city.Rect, city.Point.Y);
				Canvas.SetLeft(city.Rect, city.Point.X);

				_citys.Add(city);
				Plane.Children.Add(city.Rect);
			}


			//Получить энергию, энергия - это вся длинна пути, чем короче путь, тем меньше энергии
			_oldEnergy = GetEnergy(_citys);
			//По умолчанию считаем, что это самый лучший результат
			_bestEnergy = _oldEnergy;
			//Выводим начальную длинну пути
			StartEnergyLabel.Content = _oldEnergy.ToString(CultureInfo.InvariantCulture);
			//Рисуем начальный путь
			Helper.GetInstance(this).DrawLines(_citys, Brushes.LightSteelBlue);
		}

		private void bg_DoWork(object sender, DoWorkEventArgs e)
		{
			var temperature = TEMPERATURE;

			//Копируем действующий массив в новвый
			_citys.ForEach(x =>
			{
				_newCitys.Add(x.Clone() as City);
			});


			while (temperature > FINAL_TEMPERATURE)
			{
				for (var a = 0; a < ITERATIONS; a++)
				{
					#region Меняем местами два произвольных элемента
					int firstCity;
					int secondCity;

					do
					{
						firstCity = _rand.Next(0, _newCitys.Count - 1);
						secondCity = _rand.Next(0, _newCitys.Count - 1);
					} while (firstCity == secondCity);

					var temp = _newCitys[firstCity];

					_newCitys[firstCity] = _newCitys[secondCity];
					_newCitys[secondCity] = temp;
					#endregion

					//Проверяем новую энергию
					_newEnergy = GetEnergy(_newCitys);

					if (_newEnergy <= _oldEnergy)
					{
						_citys = new List<City>();

						_newCitys.ForEach(x =>
						{
							_citys.Add(x.Clone() as City);
						});

						_oldEnergy = _newEnergy;

						#region Лучший результат
						if (_oldEnergy < _bestEnergy)
						{
							_bestEnergy = _oldEnergy;
							_bestEnergys.Add(_bestEnergy / 100);
							_bestWay = new List<City>();

							_newCitys.ForEach(x =>
							{
								_bestWay.Add(x.Clone() as City);
							});
						}
						#endregion
					}
					else
					{
						var delta = _newEnergy - _oldEnergy;
						var calc = Math.Exp(-delta / temperature);

						if (calc > _rand.NextDouble())
						{
							_citys = new List<City>();

							_newCitys.ForEach(x =>
							{
								_citys.Add(x.Clone() as City);
							});

							_oldEnergy = _newEnergy;

							#region Лучший результат
							if (_oldEnergy < _bestEnergy)
							{
								_bestEnergy = _oldEnergy;
								_bestEnergys.Add(_bestEnergy / 100);
								_bestWay = new List<City>();

								_newCitys.ForEach(x =>
								{
									_bestWay.Add(x.Clone() as City);
								});
							}
							#endregion
						}
						else
						{
							_newCitys = new List<City>();

							_citys.ForEach(x =>
							{
								_newCitys.Add(x.Clone() as City);
							});
						}
					}
				}

				temperature *= ALPHA;
				_temperatures.Add(temperature);
				_energys.Add(_newEnergy / 100);
			}
		}

		private void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var helper = Helper.GetInstance(this);

			StopEnergyLabel.Content = _newEnergy.ToString(CultureInfo.InvariantCulture);
			BestEnergyLabel.Content = _bestEnergy.ToString(CultureInfo.InvariantCulture);

			//Удаляем все старые линии
			helper.DeleteLines();

			GetEnergy(_citys);
			GetEnergy(_bestWay);

			helper.DrawLines(_citys, Brushes.LightSteelBlue);
			helper.DrawLines(_bestWay, Brushes.Red);

			Grafic.Children.Clear();
			helper.DrawGrafic(_temperatures, Color.FromRgb(0, 255, 0));
			helper.DrawGrafic(_energys, Color.FromRgb(0, 0, 255));
			helper.DrawGrafic(_bestEnergys, Color.FromRgb(255, 0, 0));
		}

		private void bg2_DoWork(object sender, DoWorkEventArgs e)
		{
			for (var i = 0; i < _citys.Count - 1; i++)
			{
				const double bestresult = double.MaxValue;
				var bestCity = i;

				for (var a = i + 1; a < _citys.Count; a++)
				{
					if (_citys[i] == _citys[a])
					{
						continue;
					}

					var energe = Math.Abs(Math.Sqrt(
						Math.Pow(_citys[i].Point.X - _citys[a].Point.X, 2)
						+
						Math.Pow(_citys[i].Point.Y - _citys[a].Point.Y, 2)));

					if (energe < bestresult)
					{
						bestCity = a;
					}
				}

				var temp = _citys[i + 1];
				_citys[i + 1] = _citys[bestCity];
				_citys[bestCity] = temp;
			}
		}

		private void bg2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var helper = Helper.GetInstance(this);

			helper.DeleteLines();
			helper.DrawLines(_citys, new SolidColorBrush(Color.FromRgb(0, 255, 0)));
		}

		#region Other
		private static double GetEnergy(IReadOnlyList<City> citys)
		{
			double energe = 0;

			for (var i = 1; i < citys.Count; i++)
			{
				energe += Math.Abs(Math.Sqrt(
					Math.Pow(citys[i].Point.X - citys[i - 1].Point.X, 2)
					+
					Math.Pow(citys[i].Point.Y - citys[i - 1].Point.Y, 2)));
			}

			//Расстояние от последний точки до первой
			energe += Math.Abs(Math.Sqrt(
					Math.Pow(citys[citys.Count - 1].Point.X - citys[0].Point.Y, 2)
					+
					Math.Pow(citys[citys.Count - 1].Point.X - citys[0].Point.Y, 2)));

			return energe;
		}
		#endregion

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (_bg.IsBusy)
			{
				return;
			}

			_temperatures.Clear();
			_energys.Clear();
			_bestEnergys.Clear();
			_bg.RunWorkerAsync();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			if (!_bg2.IsBusy)
			{
				_bg2.RunWorkerAsync();
			}
		}
	}
}
