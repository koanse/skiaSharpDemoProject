using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiaSharpFormsDemos.Basics
{
    public partial class BasicsHomePage : HomeBasePage
    {
        public BasicsHomePage()
        {
            InitializeComponent();
        }

	    private void BindableObject_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	    {
		    var textSize = 35;
		    var stringsToDraw = StringsToDraw.Text;

		    if (!int.TryParse(TextSize.Text, out textSize))
		    {
			    textSize = 35;
		    }
			SimpleCirclePage.TextSize = textSize;
			SimpleCirclePage.Tolerance = textSize;
			SimpleCirclePage.StringsToDraw = stringsToDraw;
		}
    }
}
