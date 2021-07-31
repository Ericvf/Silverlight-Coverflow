using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightCoverFlow
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();


            int maxItems = 50;

            for (int i = 0; i < maxItems; i++)
            {
                var b = new Button()
                {
                    Width = 500,
                    Height = 500,
                    Content = "Button " + i,
                    //StrokeThickness = 1,
                    //Stroke = new SolidColorBrush(Colors.Black),
                    //Fill = new SolidColorBrush(Colors.White)
                };

                this.coverFlow.AddChild(b);

            }



            this.maxVisibleSlider.Maximum = maxItems  - 1;
            this.slider1.Maximum = maxItems - 1;


            this.coverFlow.KeyUp += this.coverFlow_KeyUp;
        }

        void coverFlow_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    coverFlow.Prev();
                    break;

                case Key.Right:
                    coverFlow.Next();
                    break;

                case Key.PageUp:
                    coverFlow.First();
                    break;

                case Key.PageDown:
                    coverFlow.Last();
                    break;
            }
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var index = (int)Math.Round(this.slider1.Value);
            this.coverFlow.selectedChild = index;
        }
    }
}
