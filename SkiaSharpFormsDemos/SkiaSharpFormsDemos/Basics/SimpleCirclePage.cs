using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Basics
{
	// Главная страница приложения
    public class SimpleCirclePage : ContentPage
    {
		// Размер текста, пороговое значение для определения пересечения

		public static int Tolerance;
		public static int TextSize;
		public static string  StringsToDraw = string.Empty;
		private static Random rnd = new Random();

		// Абстрактный класс - базовый для все 2d объектов, выводимых на экран (строк, букв и кругов)
		public abstract class Drawable
	    {
			public int x, y;
		    protected SKColor color;

			// Отрисовка
			public abstract void Draw(SKCanvas canvas);

			// Установка случайного цвета
			public void SetRandomColor()
		    {
			    color = Color.FromRgb(rnd.Next(255), rnd.Next(255), rnd.Next(255)).ToSKColor();
		    }

			// Проверка пересечения с другим объектом
			public bool CollidesWith(int xOther, int yOther)
			{
				return (xOther - x)*(xOther - x) + (yOther - y)*(yOther - y) < Tolerance*Tolerance;
			}
		}

		// Строка для отрисовки
		public class StringToDraw : Drawable
		{
			// Буквы строки
			private List<LetterToDraw> letters;
			// Конструктор строки (заполнение 2d объектов - букв)
			public StringToDraw(string text, float textSize, int x, int y)
			{
				this.x = x;
				this.y = y;

				letters = new List<LetterToDraw>();

				var xCurrent = x;

				foreach (var character in text)
				{
					var letterToDraw = new LetterToDraw(character, textSize, xCurrent, y);
					xCurrent += Tolerance;
					letters.Add(letterToDraw);
				}
				
				SetRandomColor();
			}

			// Отрисовка строки (через отрисовку букв)
			public override void Draw(SKCanvas canvas)
			{
				foreach (var letter in letters)
				{
					letter.Draw(canvas);
				}
			}

			// Проверка пересечения произвольного 2d объекта с буквами строки
			public bool HitLetters(int xOther, int yOther)
			{
				var hit = false;
				foreach (var letterToDraw in letters)
				{
					if (letterToDraw.CollidesWith(xOther, yOther))
					{
						letterToDraw.SetRandomColor();
						hit = true;
					}
				}

				return hit;
			}
	    }

		// Буква
		public class LetterToDraw : Drawable
		{
			private char letterCharacter;
			private float textSize;
			
			public LetterToDraw(char letterCharacter, float textSize, int x,  int y)
			{
				this.letterCharacter = letterCharacter;
				this.textSize = textSize;
				this.x = x;
				this.y = y;

				SetRandomColor();
			}

			public override void Draw(SKCanvas canvas)
			{
				var paint = new SKPaint
				{
					Style = SKPaintStyle.Stroke,
					Color = color,
					StrokeWidth = 2,
					TextSize = textSize
				};

				canvas.DrawText(letterCharacter.ToString(), x, y, paint);
			}
		}

		// Движущиеся круги
		public class BallonToDraw : Drawable
		{
			public BallonToDraw(int x, int y)
			{
				this.x = x;
				this.y = y;

				SetRandomColor();
			}

			public override void Draw(SKCanvas canvas)
			{
				var paint = new SKPaint
				{
					Style = SKPaintStyle.Stroke,
					Color = color,
					StrokeWidth = 3
				};

				canvas.DrawCircle(x, y, Tolerance, paint);
			}
		}

		// Списки со строками для отрисовки и движущимися кругами
	    private List<StringToDraw> stringsToDraw;
		private List<BallonToDraw> baloonsToDraw;

		public SimpleCirclePage()
        {
            Title = "Кроссплатформенная анимация";

			var width = 400;
			var height = 400;
			var baloonsToCreate = 10;

			// Заполнение списка строк
			stringsToDraw = new List<StringToDraw>();

			var strings = StringsToDraw.Split(new[] {';'});

			foreach (var str in strings)
			{
				stringsToDraw.Add(new StringToDraw(str, TextSize, rnd.Next((int)width), rnd.Next((int)height)));
			}


			int maxTriesCount = 1000;
			int triesCouunt = 0;
			// Заполнение списка кругов
			baloonsToDraw = new List<BallonToDraw>();
			while (baloonsToDraw.Count < baloonsToCreate && triesCouunt++ < maxTriesCount)
			{
				var baloon = new BallonToDraw(rnd.Next((int) width), rnd.Next((int) height));

				var collision = false;

				foreach (var otherBaloon in baloonsToDraw)
				{
					if (otherBaloon != baloon && otherBaloon.CollidesWith(baloon.x, baloon.y))
					{
						collision = true;
					}
				}

				foreach (var stringToDraw in stringsToDraw)
				{
					if (stringToDraw.HitLetters(baloon.x, baloon.y))
					{
						collision = true;
					}
				}

				if (!collision)
				{
					baloonsToDraw.Add(baloon);
				}
			};

			// Включение таймера
			var canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

			Device.StartTimer(TimeSpan.FromSeconds(0.2), () =>
			{
				canvasView.InvalidateSurface();

				return true;
			});


        }

		// Оторисовка сцены по таймеру
        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
	        var coefficient = 1;

			var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();

			// Анимация кругов
			foreach (var baloon in baloonsToDraw)
			{
				var xNext = baloon.x + rnd.Next(-coefficient * Tolerance, coefficient * Tolerance);
				var yNext = baloon.y + rnd.Next(-coefficient * Tolerance, coefficient * Tolerance);

				var collision = false;

				foreach (var otherBaloon in baloonsToDraw)
				{
					if (otherBaloon != baloon && otherBaloon.CollidesWith(xNext, yNext))
					{
						collision = true;
					}
				}

				foreach (var stringToDraw in stringsToDraw)
				{
					if (stringToDraw.HitLetters(xNext, yNext))
					{
						collision = true;
					}
				}

				if (xNext < 0 || yNext < 0 || xNext > canvas.ClipBounds.Width || yNext > canvas.ClipBounds.Height)
				{
					collision = true;
				}

				if (!collision)
				{
					baloon.x = xNext;
					baloon.y = yNext;
				}
			}

			// Отрисовка сцены
			foreach (var baloon in baloonsToDraw)
			{
				baloon.Draw(canvas);
			}

			foreach (var stringToDraw in stringsToDraw)
			{
				stringToDraw.Draw(canvas);
			}
		}
    }
}
