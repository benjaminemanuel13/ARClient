using ARClient.Services;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ARClient
{
    public partial class MainPage : ContentPage
    {
        private BlueToothService svc = new BlueToothService();

        private int LeftOffset = 0;
        private int RightOffset = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            svc.MessageRecieved += Svc_MessageRecieved;
            svc.Start();

            base.OnAppearing();
        }

        private void Svc_MessageRecieved(object sender, EventArguments.MessageRecievedEventArgs e)
        {
            if (e.Message == "LeftLeft")
            {
                LeftOffset++;
            }
            else if (e.Message == "LeftRight")
            {
                LeftOffset--;
            }

            CanvasViewLeft.InvalidateSurface();
        }

        private void OnCanvasViewPaintSurfaceLeft(object sender, SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();
            canvas.Scale(-1, 1, info.Width / 2, 0);

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Color.Red.ToSKColor()
            };
            canvas.DrawCircle((info.Width / 2) + LeftOffset, info.Height / 2, 100, paint);
        }

        private void OnCanvasViewPaintSurfaceRight(object sender, SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();
            canvas.Scale(-1, 1, info.Width / 2, 0);

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Color.Blue.ToSKColor()
            };
            canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
        }
    }
}
