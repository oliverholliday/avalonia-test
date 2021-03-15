using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
// ReSharper disable MemberCanBePrivate.Global

namespace AvaloniaApplication1.Controls
{
    public class NumberedTicks : RangeBase
    {
        /// <summary>
        /// How many values does one pixel represent.
        /// </summary>
        public double Density { get; protected set; } = double.NaN;

        protected Canvas? Canvas;

        public AvaloniaList<double> Ticks
        {
            get => GetValue(TicksProperty);
            set => SetValue(TicksProperty, value);
        }

        public string TickLabelFormat
        {
            get => GetValue(TickLabelFormatProperty);
            set => SetValue(TickLabelFormatProperty, value);
        }
        
        public static readonly StyledProperty<AvaloniaList<double>> TicksProperty = AvaloniaProperty.Register<NumberedTicks, AvaloniaList<double>>(nameof(Ticks));
        public static readonly StyledProperty<string> TickLabelFormatProperty = AvaloniaProperty.Register<NumberedTicks, string>(nameof(TickLabelFormat), "N0");

        public AvaloniaList<TextBlock> Labels { get; protected set; } = new AvaloniaList<TextBlock>();



        static NumberedTicks()
        {
            AffectsArrange<NumberedTicks>(MinimumProperty, MaximumProperty, TicksProperty, BoundsProperty);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            Debug.WriteLine("Control is applying template.");

            Canvas = e.NameScope.Find<Canvas>("PART_Canvas");

            UpdateLabels();

            base.OnApplyTemplate(e);
        }

        //protected void AddTextBlock()
        //{
        //    var tb = new TextBlock();

        //    Labels.Add(tb);
        //    LogicalChildren.Add(tb);
        //}

        //protected void FixTextBlockCounts()
        //{
        //    while (Ticks.Count < Labels.Count)
        //    {
        //        var tb = Labels[Labels.Count];
        //        Labels.Remove(tb);
        //        Canvas.Children.Remove(tb);
        //        LogicalChildren.Remove(tb);
        //    }
        //    while (Ticks.Count > Labels.Count)
        //    {
        //        var tb = new TextBlock();
        //        Labels.Add(tb);
        //        Canvas.Children.Add(tb);
        //        LogicalChildren.Add(tb);
        //    }
        //}

        protected void UpdateLabels()
        {
            if (Canvas == null) return;

            //if (Ticks.Count != Labels.Count) FixTextBlockCounts();

            if (Ticks == null)
            {
                Ticks = new AvaloniaList<double>((int)Math.Floor((Maximum - Minimum) / LargeChange) + 1);
                for (double d = Minimum; d <= Maximum; d += LargeChange)
                {
                    Ticks.Add(d);
                }
            }

            while (Ticks.Count < Labels.Count)
            {
                var tb = Labels[Labels.Count];
                Labels.Remove(tb);
                Canvas.Children.Remove(tb);
                LogicalChildren.Remove(tb);
            }
            while (Ticks.Count > Labels.Count)
            {
                var tb = new TextBlock
                {
                    Foreground = Foreground, 
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    FontSize = FontSize,
                    FontFamily = FontFamily,
                    FontStyle = FontStyle,
                    FontWeight = FontWeight,
                    Width = 10,
                    Margin = new Thickness(-5, 0,0,0),
                    ClipToBounds = false
                };
                Labels.Add(tb);
                Canvas.Children.Add(tb);
                LogicalChildren.Add(tb);
            }

            for (int i = 0; i < Ticks.Count; i++)
            {
                var tb = Labels[i];
                var val = Ticks[i];
                tb.Text = val.ToString(TickLabelFormat);
                Canvas.SetLeft(tb, Density * (val - Minimum));
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"Control is now {finalSize.Width} x {finalSize.Height}");

            Density = finalSize.Width / (Maximum - Minimum);

            UpdateLabels();

            return base.ArrangeOverride(finalSize);
        }
    }
}
