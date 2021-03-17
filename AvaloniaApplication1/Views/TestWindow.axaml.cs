using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Visuals.Media.Imaging;

namespace AvaloniaApplication1.Views
{
    public class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.Find<Button>("Source1").Click += (sender, args) => RenderVisualBrush((IControl)sender, this.Find<Panel>("Target1"));
            this.Find<Button>("Source2").Click += (sender, args) => RenderVisualBrush((IControl)sender, this.Find<Panel>("Target2"));
            this.Find<Button>("Source3").Click += (sender, args) => RenderVisualBrush((IControl)sender, this.Find<Panel>("Target3"));
        }

        private void RenderVisualBrush(IControl source, Panel target)
        {
            target.Width = source.Bounds.Width;
            target.Height = source.Bounds.Height;

            var vb = new VisualBrush(source)
            {
                BitmapInterpolationMode = BitmapInterpolationMode.HighQuality,
                TileMode = TileMode.Tile,
                //SourceRect = new RelativeRect(source.Bounds, RelativeUnit.Absolute),
//                DestinationRect = new RelativeRect(source.Bounds, RelativeUnit.Absolute),
                //DestinationRect = new RelativeRect(target.Bounds, RelativeUnit.Absolute)
            };
            //var bitmap = new RenderTargetBitmap()
            //new ImageBrush()

            target.Background = vb;

            target.IsVisible = true;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
