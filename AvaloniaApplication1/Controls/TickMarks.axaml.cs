using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Utilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
// ReSharper disable MemberCanBePrivate.Global

namespace AvaloniaApplication1.Controls
{
    /// <summary>
    /// An element that is used for drawing <see cref="Slider"/>'s MajorTicks.
    /// </summary>
    public class TickMarks : RangeBase
    {
        static TickMarks()
        {
            AffectsArrange<TickMarks>(
                MaximumProperty,
                MinimumProperty,
                LargeChangeProperty,
                SmallChangeProperty
            );

            AffectsRender<TickMarks>(
                BoundsProperty,
                MajorTickPenProperty,
                MinorTickPenProperty,
                MajorTicksProperty,
                MinorTicksProperty
            );
        }

        public static readonly StyledProperty<IPen> MajorTickPenProperty = AvaloniaProperty.Register<TickMarks, IPen>(nameof(MajorTickPen), new Pen(SolidColorBrush.Parse("Black")));
        public static readonly StyledProperty<IPen> MinorTickPenProperty = AvaloniaProperty.Register<TickMarks, IPen>(nameof(MinorTickPen), new Pen(SolidColorBrush.Parse("Silver")));
        public static readonly StyledProperty<bool> RenderMinorTickAtMajorTickProperty = AvaloniaProperty.Register<TickMarks, bool>(nameof(RenderMinorTickAtMajorTick));

        public static readonly StyledProperty<double> MajorTickInnerProperty = AvaloniaProperty.Register<TickMarks, double>(nameof(MajorTickInner), 0);
        public static readonly StyledProperty<double> MajorTickOuterProperty = AvaloniaProperty.Register<TickMarks, double>(nameof(MajorTickOuter), 100);
        
        public static readonly StyledProperty<double> MinorTickInnerProperty = AvaloniaProperty.Register<TickMarks, double>(nameof(MinorTickInner), 0);
        public static readonly StyledProperty<double> MinorTickOuterProperty = AvaloniaProperty.Register<TickMarks, double>(nameof(MinorTickOuter), 75);

        public IPen MajorTickPen
        {
            get => GetValue(MajorTickPenProperty);
            set => SetValue(MajorTickPenProperty, value);
        }

        public IPen MinorTickPen
        {
            get => GetValue(MinorTickPenProperty);
            set => SetValue(MinorTickPenProperty, value);
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

            UpdateTicks();

            base.OnApplyTemplate(e);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"Control is now {finalSize.Width} x {finalSize.Height}");

            UpdateTicks();

            return base.ArrangeOverride(finalSize);
        }

        public static readonly StyledProperty<AvaloniaList<double>?> MajorTicksProperty = AvaloniaProperty.Register<TickMarks, AvaloniaList<double>?>(nameof(MajorTicks));

        public AvaloniaList<double>? MajorTicks
        {
            get => GetValue(MajorTicksProperty);
            set => SetValue(MajorTicksProperty, value);
        }

        public static readonly StyledProperty<AvaloniaList<double>?> MinorTicksProperty = AvaloniaProperty.Register<TickMarks, AvaloniaList<double>?>(nameof(MinorTicks));

        public AvaloniaList<double>? MinorTicks
        {
            get => GetValue(MinorTicksProperty);
            set => SetValue(MinorTicksProperty, value);
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
                    if (!RenderMinorTickAtMajorTick && MajorTicks.Any(x => MathUtilities.AreClose(d, x))) continue;

                    MinorTicks.Add(d);
                }
            }
        }

        public override void Render(DrawingContext dc)
        {
            var size = new Size(Bounds.Width, Bounds.Height);

            var horizontal = Bounds.Width / (Maximum - Minimum);
            var vertical = Bounds.Height / 100;

            if (MathUtilities.GreaterThanOrClose(0, size.Width) || MathUtilities.GreaterThanOrClose(0, size.Height)) return;

            if (MinorTicks != null)
            {
                foreach (var minor in MinorTicks)
                {
                    dc.DrawLine(
                        MinorTickPen,
                        new Point((minor - Minimum) * horizontal, MinorTickInner * vertical),
                        new Point((minor - Minimum) * horizontal, MinorTickOuter * vertical)
                    );
                }
            }

            if (MajorTicks != null)
            {
                foreach (var major in MajorTicks)
                {
                    dc.DrawLine(
                        MajorTickPen,
                        new Point((major - Minimum) * horizontal, MajorTickInner * vertical),
                        new Point((major - Minimum) * horizontal, MajorTickOuter * vertical)
                    );
                }
            }
        }
    }
}
