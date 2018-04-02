using SkiaSharp;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Microcharts.Samples.Forms
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SampleChart : ContentPage
	{
		public SampleChart()
		{
			InitializeComponent();
		}

		private Chart _chart;
		public Chart Chart
		{
			get => _chart;
			set => _chart = value;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			//LabelOrientation = Orientation.Horizontal,
			//LabelColor = new SKColor(255, 255, 255),
			int count = 1;
			Entry CreateEntry(int value)
			{
				var e = new Entry(value)
				{
					TextColor = new SKColor(255, 255, 255, 127),
					Color = new SKColor(255, 255, 255),
					Label = count.ToString(),
					ValueLabel = " "					
				};
				count++;
				return e;
			}

			var entries = new List<Entry>()
			{
				CreateEntry(10),
				CreateEntry(14),
				CreateEntry(16),
				CreateEntry(25),
				CreateEntry(10),
				CreateEntry(15),
				CreateEntry(2),
				CreateEntry(19)
			};
			chartView.HeightRequest = 400;
			chartView.Chart = new LineChart() { Entries = entries, BackgroundColor = new SKColor(255, 255, 255, 0),  LabelTextSize = 35, PointMode = PointMode.Circle };
		}

	}
}