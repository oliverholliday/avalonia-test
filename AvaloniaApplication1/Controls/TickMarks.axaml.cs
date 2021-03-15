using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Utilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;

// ReSharper disable MemberCanBePrivate.Global

namespace AvaloniaApplication1.Controls
{
    /// <summary>
    /// An element that is used for drawing <see cref="Slider"/>'s MajorTicks.
    /// </summary>
    public class TickMarks : RangeBase
    {
        /// <summary>
        /// How many values does one pixel represent.
        /// </summary>
        public double Density { get; protected set; }

        protected Canvas? Canvas;

        static TickMarks()
        {
            AffectsRender<TickBar>(
                BoundsProperty,
                MajorTickBrushProperty,
                MinorTickBrushProperty,
                MaximumProperty,
                MinimumProperty,
                LargeChangeProperty,
                SmallChangeProperty,
                MajorTicksProperty,
                MinorTicksProperty
            );
        }


        public static readonly StyledProperty<decimal> MajorTickStartProperty = AvaloniaProperty.Register<TickBar, decimal>(nameof(MajorTickStart));
        //        public static readonly StyledProperty<decimal> MajorTickSpacingProperty = AvaloniaProperty.Register<TickBar, decimal>(nameof(MajorTickSpacing), 5);
        public static readonly StyledProperty<IBrush> MajorTickBrushProperty = AvaloniaProperty.Register<TickBar, IBrush>(nameof(MajorTickBrush), SolidColorBrush.Parse("Black"));
        public static readonly StyledProperty<double> MajorTickWidthProperty = AvaloniaProperty.Register<TickBar, double>(nameof(MajorTickWidth), 2);
        public static readonly StyledProperty<double> MajorTickInnerProperty = AvaloniaProperty.Register<TickBar, double>(nameof(MajorTickInner), 0);
        public static readonly StyledProperty<double> MajorTickOuterProperty = AvaloniaProperty.Register<TickBar, double>(nameof(MajorTickOuter), 100);

        public static readonly StyledProperty<decimal> MinorTickStartProperty = AvaloniaProperty.Register<TickBar, decimal>(nameof(MinorTickStart));
        //        public static readonly StyledProperty<decimal> MinorTickSpacingProperty = AvaloniaProperty.Register<TickBar, decimal>(nameof(MinorTickSpacing), 1);
        public static readonly StyledProperty<IBrush> MinorTickBrushProperty = AvaloniaProperty.Register<TickBar, IBrush>(nameof(MinorTickBrush), SolidColorBrush.Parse("Black"));
        public static readonly StyledProperty<double> MinorTickWidthProperty = AvaloniaProperty.Register<TickBar, double>(nameof(MinorTickWidth), 1);
        public static readonly StyledProperty<double> MinorTickInnerProperty = AvaloniaProperty.Register<TickBar, double>(nameof(MinorTickInner), 0);
        public static readonly StyledProperty<double> MinorTickOuterProperty = AvaloniaProperty.Register<TickBar, double>(nameof(MinorTickOuter), 75);
        public static readonly StyledProperty<bool> RenderMinorTickAtMajorTickProperty = AvaloniaProperty.Register<TickBar, bool>(nameof(RenderMinorTickAtMajorTick));

        public decimal MajorTickStart
        {
            get => GetValue(MajorTickStartProperty);
            set => SetValue(MajorTickStartProperty, value);
        }

        //public decimal MajorTickSpacing
        //{
        //    get => GetValue(MajorTickSpacingProperty);
        //    set => SetValue(MajorTickSpacingProperty, value);
        //}

        public IBrush MajorTickBrush
        {
            get => GetValue(MajorTickBrushProperty);
            set => SetValue(MajorTickBrushProperty, value);
        }

        public double MajorTickWidth
        {
            get => GetValue(MajorTickWidthProperty);
            set => SetValue(MajorTickWidthProperty, value);
        }

        public double MajorTickInner
        {
            get => GetValue(MajorTickInnerProperty);
            set => SetValue(MajorTickInnerProperty, value);
        }

        public double MajorTickOuter
        {
            get => GetValue(MajorTickOuterProperty);
            set => SetValue(MajorTickOuterProperty, value);
        }

        public decimal MinorTickStart
        {
            get => GetValue(MinorTickStartProperty);
            set => SetValue(MinorTickStartProperty, value);
        }

        //public decimal MinorTickSpacing
        //{
        //    get => GetValue(MinorTickSpacingProperty);
        //    set => SetValue(MinorTickSpacingProperty, value);
        //}

        public IBrush MinorTickBrush
        {
            get => GetValue(MinorTickBrushProperty);
            set => SetValue(MinorTickBrushProperty, value);
        }

        public double MinorTickWidth
        {
            get => GetValue(MinorTickWidthProperty);
            set => SetValue(MinorTickWidthProperty, value);
        }

        public double MinorTickInner
        {
            get => GetValue(MinorTickInnerProperty);
            set => SetValue(MinorTickInnerProperty, value);
        }

        public double MinorTickOuter
        {
            get => GetValue(MinorTickOuterProperty);
            set => SetValue(MinorTickOuterProperty, value);
        }

        /// <summary>
        /// When true, a Minor tick point will be rendered even if a Major tick was rendered at this point.
        /// </summary>
        public bool RenderMinorTickAtMajorTick
        {
            get => GetValue(RenderMinorTickAtMajorTickProperty);
            set => SetValue(RenderMinorTickAtMajorTickProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            Debug.WriteLine("Control is applying template.");

            Canvas = e.NameScope.Find<Canvas>("PART_Canvas");

            UpdateTicks();

            base.OnApplyTemplate(e);
        }





        ///// <summary>
        ///// Defines the <see cref="SmallTickFill"/> property.
        ///// </summary>
        //public static readonly StyledProperty<IBrush> SmallTickFillProperty = AvaloniaProperty.Register<TickBar, IBrush>(nameof(SmallTickFill));

        ///// <summary>
        ///// Brush used to fill the TickBar's MajorTicks.
        ///// </summary>
        //public IBrush SmallTickFill
        //{
        //    get => GetValue(SmallTickFillProperty);
        //    set => SetValue(SmallTickFillProperty, value);
        //}

        ///// <summary>
        ///// Defines the <see cref="LargeTickFill"/> property.
        ///// </summary>
        //public static readonly StyledProperty<IBrush> LargeTickFillProperty = AvaloniaProperty.Register<TickBar, IBrush>(nameof(LargeTickFill));

        ///// <summary>
        ///// Brush used to fill the TickBar's MajorTicks.
        ///// </summary>
        //public IBrush LargeTickFill
        //{
        //    get => GetValue(LargeTickFillProperty);
        //    set => SetValue(LargeTickFillProperty, value);
        //}

        ///// <summary>
        ///// Defines the <see cref="TickFrequency"/> property.
        ///// </summary>
        //public static readonly StyledProperty<double> TickFrequencyProperty = AvaloniaProperty.Register<TickBar, double>(nameof(TickFrequency));

        ///// <summary>
        ///// TickFrequency property defines how the tick will be drawn.
        ///// </summary>
        //public double TickFrequency
        //{
        //    get => GetValue(TickFrequencyProperty);
        //    set => SetValue(TickFrequencyProperty, value);
        //}

        public static readonly StyledProperty<AvaloniaList<double>?> MajorTicksProperty = AvaloniaProperty.Register<TickBar, AvaloniaList<double>?>(nameof(MajorTicks));

        public AvaloniaList<double>? MajorTicks
        {
            get => GetValue(MajorTicksProperty);
            set => SetValue(MajorTicksProperty, value);
        }

        public static readonly StyledProperty<AvaloniaList<double>?> MinorTicksProperty = AvaloniaProperty.Register<TickBar, AvaloniaList<double>?>(nameof(MinorTicks));

        public AvaloniaList<double>? MinorTicks
        {
            get => GetValue(MinorTicksProperty);
            set => SetValue(MinorTicksProperty, value);
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"Control is now {finalSize.Width} x {finalSize.Height}");

            Density = finalSize.Width / (Maximum - Minimum);

            UpdateTicks();
            UpdateFace();

            return base.ArrangeOverride(finalSize);
        }

        protected void UpdateTicks()
        {
            if (MajorTicks == null)
            {
                MajorTicks = new AvaloniaList<double>((int)Math.Floor((Maximum - Minimum) / LargeChange) + 1);
                for (var d = Minimum; d <= Maximum; d += LargeChange)
                {
                    MajorTicks.Add(d);
                }
            }

            if (MinorTicks == null)
            {
                MinorTicks = new AvaloniaList<double>((int)Math.Floor((Maximum - Minimum) / SmallChange) + 1);
                for (var d = Minimum; d <= Maximum; d += SmallChange)
                {
                    if (!RenderMinorTickAtMajorTick && (d - Minimum) % LargeChange <= (double)0.01m) continue;

                    if (!RenderMinorTickAtMajorTick && MajorTicks.Any(x => MathUtilities.AreClose(d, x))) continue;

                    MinorTicks.Add(d);
                }
            }
        }



        private void UpdateFace()
        {
            //if (Canvas == null) return;

            //var major = new PathGeometry();

            //for (var i = MajorTickStart; i <= (decimal)Maximum; i += MajorTickSpacing)
            //{
            //    var pf = new PathFigure
            //    {
            //        IsClosed = false,
            //        StartPoint = new Point(Math.Min((double)ValueToDistance(i), MaxDistance), MajorTickInner * HeightScaleFactor),
            //        Segments = new PathSegments()
            //    };

            //    var seg = new LineSegment
            //    {
            //        Point = new Point(Math.Min((double)ValueToDistance(i), MaxDistance), MajorTickOuter * HeightScaleFactor)
            //    };

            //    pf.Segments.Add(seg);
            //    major.Figures.Add(pf);
            //}

            //Canvas.Children.Add(major);


            //var minor = new PathGeometry();

            //for (var i = MinorTickStart; i <= (decimal)Maximum; i += MinorTickSpacing)
            //{
            //    if (!RenderMinorTickAtMajorTick && (i - MajorTickStart) % MajorTickSpacing <= 0.01m) continue;

            //    var pf = new PathFigure
            //    {
            //        IsClosed = false,
            //        StartPoint = new Point(Math.Min((double)ValueToDistance(i), MaxDistance), MinorTickInner * HeightScaleFactor),
            //        Segments = new PathSegments()
            //    };

            //    var seg = new LineSegment
            //    {
            //        Point = new Point(Math.Min((double)ValueToDistance(i), MaxDistance), MinorTickOuter * HeightScaleFactor)
            //    };

            //    pf.Segments.Add(seg);
            //    minor.Figures.Add(pf);
            //}

            //MinorTicksElement.Data = minor;
        }

        public override void Render(DrawingContext dc)
        {
            var size = new Size(Bounds.Width, Bounds.Height);

            if (MathUtilities.GreaterThanOrClose(0, size.Width)) return;

            var smallPen = new Pen(MinorTickBrush);
            if (MinorTicks != null) foreach (var minor in MinorTicks)
                {
                    dc.DrawLine(
                        smallPen,
                        new Point(minor * Density, MinorTickInner),
                        new Point(minor * Density, MinorTickOuter)
                    );
                }

            var largePen = new Pen(MajorTickBrush);
            if (MajorTicks != null) foreach (var major in MajorTicks)
                {
                    dc.DrawLine(
                        largePen,
                        new Point(major * Density, MajorTickInner),
                        new Point(major * Density, MajorTickOuter)
                    );
                }
        }
    }
}
