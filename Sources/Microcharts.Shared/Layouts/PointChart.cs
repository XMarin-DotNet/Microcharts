// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Microcharts.Shared.Structs;
	using SkiaSharp;

	/// <summary>
	/// ![chart](../images/Point.png)
	/// 
	/// Point chart.
	/// </summary>
	public class PointChart : Chart
	{
		#region Properties

		public float PointSize { get; set; } = 14;

		public PointMode PointMode { get; set; } = PointMode.Circle;

		public byte PointAreaAlpha { get; set; } = 100;

		private float ValueRange => this.MaxValue - this.MinValue;

		#endregion

		#region Methods

		public float CalculateYOrigin(float itemHeight, float headerHeight)
		{
			if (this.MaxValue <= 0)
			{
				return headerHeight;
			}

			if (this.MinValue > 0)
			{
				return headerHeight + itemHeight;
			}

			return headerHeight + ((this.MaxValue / this.ValueRange) * itemHeight);
		}

		public override void DrawContent(SKCanvas canvas, int width, int height)
		{
			var valueLabelSizes = this.MeasureValueLabels();
			var footerHeight = this.CalculateFooterHeight(valueLabelSizes);
			var headerHeight = this.CalculateHeaderHeight(valueLabelSizes);
			var itemSize = this.CalculateItemSize(width, height, footerHeight, headerHeight);
			var origin = this.CalculateYOrigin(itemSize.Height, headerHeight);
			var points = this.CalculatePoints(itemSize, origin, headerHeight);

			this.DrawPointAreas(canvas, points, origin);
			this.DrawPoints(canvas, points);
			this.DrawFooter(canvas, points, itemSize, height, footerHeight, headerHeight);
			this.DrawValueLabel(canvas, points, itemSize, height, valueLabelSizes);
		}

		protected SKSize CalculateItemSize(int width, int height, float footerHeight, float headerHeight)
		{
			var total = this.Entries.Count();
			var w = (width - ((total + 1) * this.Margin)) / total;
			var h = height - this.Margin - footerHeight - headerHeight;
			return new SKSize(w, h);
		}

		protected SKPoint[] CalculatePoints(SKSize itemSize, float origin, float headerHeight)
		{
			var result = new List<SKPoint>();

			for (int i = 0; i < this.Entries.Count(); i++)
			{
				var entry = this.Entries.ElementAt(i);

				var x = this.Margin + (itemSize.Width / 2) + (i * (itemSize.Width + this.Margin));
				var y = headerHeight + (((this.MaxValue - entry.Value) / this.ValueRange) * itemSize.Height);
				var point = new SKPoint(x, y);
				result.Add(point);
			}

			return result.ToArray();
		}

		protected void DrawFooter(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight, float headerHeight)
		{
			this.DrawLabels(canvas, points, itemSize, height, footerHeight, headerHeight);
		}

		protected void DrawLabels(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight, float headerHeight)
		{
			// Draw X Axis Labels
			for (int i = 0; i < this.Entries.Count(); i++)
			{
				var entry = this.Entries.ElementAt(i);
				var point = points[i];

				if (!string.IsNullOrEmpty(entry.Label))
				{
					using (var paint = new SKPaint())
					{
						paint.TextSize = this.LabelTextSize;
						paint.IsAntialias = true;
						paint.TextAlign = SKTextAlign.Center;

						if (!entry.Selected)
							paint.Color = entry.TextColor;
						else
							paint.Color = new SKColor(255, 255, 255);

						paint.IsStroke = false;

						var bounds = new SKRect();
						var text = entry.Label;
						paint.MeasureText(text, ref bounds);

						if (bounds.Width > itemSize.Width)
						{
							text = text.Substring(0, Math.Min(3, text.Length));
							paint.MeasureText(text, ref bounds);
						}

						if (bounds.Width > itemSize.Width)
						{
							text = text.Substring(0, Math.Min(1, text.Length));
							paint.MeasureText(text, ref bounds);
						}

						// Need to draw Month start and end points
						if (i == 0) // First
						{
							canvas.DrawText("Apr", point.X, height - this.Margin - this.LabelTextSize - 5, paint);
						}

						if (i == this.Entries.Count() - 1) // End
						{
							canvas.DrawText("Apr", point.X, height - this.Margin - this.LabelTextSize - 5, paint);
						}

						canvas.DrawText(text, point.X, height - this.Margin, paint);
					}
				}
			}

			// Draw Y Axis Labels
			var min = 0;
			var max = Entries.Max(x => x.Value);

			var minLabel = GetLabel(min);
			var maxLabel = GetLabel(max);

			using (var paint = new SKPaint())
			{
				paint.TextSize = this.LabelTextSize;
				paint.IsAntialias = true;
				paint.Color = new SKColor(255, 255, 255);
				paint.IsStroke = false;
				paint.TextAlign = SKTextAlign.Right;
				canvas.DrawText(minLabel, this.Margin + 30, height - this.Margin - footerHeight, paint);
			}

			using (var paint = new SKPaint())
			{
				paint.TextSize = this.LabelTextSize;
				paint.IsAntialias = true;
				paint.Color = new SKColor(255, 255, 255);
				paint.IsStroke = false;
				paint.TextAlign = SKTextAlign.Right;
				canvas.DrawText(maxLabel, this.Margin + 30, headerHeight, paint);
			}

			string GetLabel(double value)
			{
				if (value < 1000)
					return value.ToString("N0");
				else
					return (value / 1000).ToString("N1") + "K";
			}
		}

		protected void DrawPoints(SKCanvas canvas, SKPoint[] points, float origin = 0)
		{
			if (points.Length > 0 && PointMode != PointMode.None)
			{
				for (int i = 0; i < points.Length; i++)
				{
					var entry = this.Entries.ElementAt(i);
					var point = points[i];

					var size = entry.Selected ? 20 : this.PointSize;

					if (entry.Selected)
					{
						// Line from Point to bottom
						canvas.DrawLine(point.X, point.Y, point.X, origin, new SKPaint() { Color = new SKColor(255, 255, 255) });

						// Draw Annotation
						var path = new SKPath();

						if (i == 0) // Inverse
						{
							canvas.DrawRoundRect(new SKRect(point.X + 90, point.Y - 70, point.X, point.Y - 20), 5, 5, new SKPaint() { Color = new SKColor(255, 255, 255) });
							path.MoveTo(point.X, point.Y - 15);
							path.RLineTo(0, -10);
							path.RLineTo(20, 0);
							path.LineTo(point.X, point.Y - 15);
							canvas.DrawText(entry.AnnotationLabel ?? "$790", point.X + 45, point.Y - 30, new SKPaint() { IsAntialias = true, Color = new SKColor(0, 0, 0), TextAlign = SKTextAlign.Center, TextSize = 20 });
							canvas.DrawText(entry.AnnotationHeadingLabel ?? "3/14 - 7/14", point.X + 45, point.Y - 55, new SKPaint() { IsAntialias = true, Color = new SKColor(61, 61, 61), TextAlign = SKTextAlign.Center, TextSize = 11 });
							// TODO: SkTypeface.FromFile("path/to/typeface.ttf");
						}
						else
						{
							canvas.DrawRoundRect(new SKRect(point.X - 90, point.Y - 70, point.X, point.Y - 20), 5, 5, new SKPaint() { Color = new SKColor(255, 255, 255) });
							path.MoveTo(point.X, point.Y - 15);
							path.RLineTo(0, -10);
							path.RLineTo(-20, 0);
							path.LineTo(point.X, point.Y - 15);
							canvas.DrawText(entry.AnnotationLabel ?? "$790", point.X - 45, point.Y - 30, new SKPaint() { IsAntialias = true, Color = new SKColor(0, 0, 0), TextAlign = SKTextAlign.Center, TextSize = 20 });
							canvas.DrawText(entry.AnnotationHeadingLabel ?? "3/14 - 7/14", point.X - 45, point.Y - 55, new SKPaint() { IsAntialias = true, Color = new SKColor(61, 61, 61), TextAlign = SKTextAlign.Center, TextSize = 11 });
							// TODO: SkTypeface.FromFile("path/to/typeface.ttf");
						}

						canvas.DrawPath(path, new SKPaint() { IsAntialias = true, Color = new SKColor(255, 255, 255), StrokeWidth = 1, Style = SKPaintStyle.StrokeAndFill });
						
						canvas.DrawPoint(point, entry.Color, size, this.PointMode);
						canvas.DrawPoint(point, new SKColor(0, 0, 0), size - 12, this.PointMode);
					}
					else
					{
						// Line from Point to bottom
						canvas.DrawLine(point.X, point.Y, point.X, origin, new SKPaint() { Color = new SKColor(255, 255, 255, 127), PathEffect = SKPathEffect.CreateDash(new float[] { 10, 2 }, 20) });
						canvas.DrawPoint(point, entry.Color, size, this.PointMode);
					}

					AddTouchHandler(new Rectangle(new Point(point.X - 20, point.Y - 20), new Size(40, 40)), () =>
					{
						foreach (var item in Entries)
							if (item == entry)
								entry.Selected = !entry.Selected;
							else
								item.Selected = false;
					});
				}
			}
		}

		protected void DrawPointAreas(SKCanvas canvas, SKPoint[] points, float origin)
		{
			if (points.Length > 0 && this.PointAreaAlpha > 0)
			{
				for (int i = 0; i < points.Length; i++)
				{
					var entry = this.Entries.ElementAt(i);
					var point = points[i];
					var y = Math.Min(origin, point.Y);

					using (var shader = SKShader.CreateLinearGradient(new SKPoint(0, origin), new SKPoint(0, point.Y), new[] { entry.Color.WithAlpha(this.PointAreaAlpha), entry.Color.WithAlpha((byte)(this.PointAreaAlpha / 3)) }, null, SKShaderTileMode.Clamp))
					using (var paint = new SKPaint
					{
						Style = SKPaintStyle.Fill,
						Color = entry.Color.WithAlpha(this.PointAreaAlpha),
					})
					{
						paint.Shader = shader;
						var height = Math.Max(2, Math.Abs(origin - point.Y));
						canvas.DrawRect(SKRect.Create(point.X - (this.PointSize / 2), y, this.PointSize, height), paint);
					}
				}
			}
		}

		protected void DrawValueLabel(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float height, SKRect[] valueLabelSizes)
		{
			if (points.Length > 0)
			{
				for (int i = 0; i < points.Length; i++)
				{
					var entry = this.Entries.ElementAt(i);
					var point = points[i];
					var isAbove = point.Y > (this.Margin + (itemSize.Height / 2));

					if (!string.IsNullOrEmpty(entry.ValueLabel))
					{
						using (new SKAutoCanvasRestore(canvas))
						{
							using (var paint = new SKPaint())
							{
								paint.TextSize = this.LabelTextSize;
								paint.FakeBoldText = true;
								paint.IsAntialias = true;
								paint.Color = entry.Color;
								paint.IsStroke = false;

								var bounds = new SKRect();
								var text = entry.ValueLabel;
								paint.MeasureText(text, ref bounds);

								canvas.RotateDegrees(90);
								canvas.Translate(this.Margin, -point.X + (bounds.Height / 2));

								canvas.DrawText(text, 0, 0, paint);
							}
						}
					}
				}
			}
		}

		protected float CalculateFooterHeight(SKRect[] valueLabelSizes)
		{
			var result = this.Margin;

			if (this.Entries.Any(e => !string.IsNullOrEmpty(e.Label)))
			{
				result += this.LabelTextSize + this.Margin;
			}

			return result;
		}

		protected float CalculateHeaderHeight(SKRect[] valueLabelSizes)
		{
			var result = this.Margin;

			if (this.Entries.Any())
			{
				var maxValueWidth = valueLabelSizes.Max(x => x.Width);
				if (maxValueWidth > 0)
				{
					result += maxValueWidth + this.Margin;
				}
			}

			return result;
		}

		protected SKRect[] MeasureValueLabels()
		{
			using (var paint = new SKPaint())
			{
				paint.TextSize = this.LabelTextSize;
				return this.Entries.Select(e =>
				{
					if (string.IsNullOrEmpty(e.ValueLabel))
					{
						return SKRect.Empty;
					}

					var bounds = new SKRect();
					var text = e.ValueLabel;
					paint.MeasureText(text, ref bounds);
					return bounds;
				}).ToArray();
			}
		}

		#endregion
	}
}
