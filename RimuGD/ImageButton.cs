using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RimuGD
{
    public class ImageButton : Button
    {
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }

        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));

        public ImageSource HoveredImage
        {
            get { return (ImageSource)GetValue(HoveredImageProperty); }
            set { SetValue(HoveredImageProperty, value); }
        }

        public static readonly DependencyProperty HoveredImageProperty =
            DependencyProperty.Register("HoveredImage", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));

    } 
}