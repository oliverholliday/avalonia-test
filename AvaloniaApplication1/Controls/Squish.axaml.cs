using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using ReactiveUI;
// ReSharper disable MemberCanBePrivate.Global

namespace AvaloniaApplication1.Controls
{
    /// <summary>
    /// The interaction state of the slider.
    /// </summary>
    public enum SquishState
    {
        /// <summary>
        /// The slider is idle.
        /// </summary>
        None,

        /// <summary>
        /// The slider is being dragged.
        /// </summary>
        Dragging,

        /// <summary>
        /// The slider is awaiting user confirmation of the value change.
        /// </summary>
        Confirming
    }

    /// <summary>
    /// Configures how the Value property is set for interactions.
    /// </summary>
    public enum SquishApplyValueChange
    {
        /// <summary>
        /// The value changes constantly during drag.
        /// </summary>
        WhenDragging,

        /// <summary>
        /// The value changes only when the thumb is released.
        /// </summary>
        WhenReleased,

        /// <summary>
        /// When the thumb is released the confirmation popup is displayed before setting the value.
        /// </summary>
        WhenConfirmed
    }

    public class Squish : RangeBase
    {
        private IDisposable? _thumbChangeBinding;

        public double Density { get; protected set; } = double.NaN;

        /// <summary>
        /// Whether the control is normal, dragging or confirming.
        /// </summary>
        public SquishState State
        {
            get => GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        /// <summary>
        /// Whether the control prompts to apply the change to Value.
        /// </summary>
        public SquishApplyValueChange ApplyValueChange
        {
            get => GetValue(ApplyValueChangeProperty);
            set => SetValue(ApplyValueChangeProperty, value);
        }

        /// <summary>
        /// The control used as the draggable thumb.
        /// </summary>
        [Content]
        public IControl? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// The control used for the confirmation dialog. The button with IsDefault=True is used as confirmation, everything else cancels.
        /// </summary>
        public IControl? Popup
        {
            get => GetValue(PopupProperty);
            set => SetValue(PopupProperty, value);
        }

        /// <summary>
        /// The calculated width of the thumb, used to configure margins.
        /// </summary>
        public double ThumbWidth
        {
            get => GetValue(ThumbWidthProperty);
            protected set => SetValue(ThumbWidthProperty, value);
        }

        /// <summary>
        /// The calculated margins used to position so the thumb can overlap the control edges.
        /// </summary>
        public Thickness TrackMargin
        {
            get => GetValue(TrackMarginProperty);
            protected set => SetValue(TrackMarginProperty, value);
        }

        /// <summary>
        /// The template property for how long the track before the thumb should be.
        /// </summary>
        public double BeforeWidth
        {
            get => GetValue(BeforeWidthProperty);
            protected set => SetValue(BeforeWidthProperty, value);
        }

        /// <summary>
        /// The template property for how long the track after the thumb should be.
        /// </summary>
        public double AfterWidth
        {
            get => GetValue(AfterWidthProperty);
            protected set => SetValue(AfterWidthProperty, value);
        }

        private double _unconfirmedValue;

        /// <summary>
        /// Defines the <see cref="UnconfirmedValue"/> property.
        /// </summary>
        public static readonly DirectProperty<Squish, double> UnconfirmedValueProperty =
            AvaloniaProperty.RegisterDirect<Squish, double>(
                nameof(UnconfirmedValue),
                o => o.UnconfirmedValue,
                (o, v) => o.UnconfirmedValue = v,
                defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public double UnconfirmedValue
        {
            get => _unconfirmedValue;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value))
                {
                    return;
                }

                if (IsInitialized)
                {
                    value = MathUtilities.Clamp(value, Minimum, Maximum);
                    SetAndRaise(UnconfirmedValueProperty, ref _unconfirmedValue, value);
                }
                else
                {
                    SetAndRaise(UnconfirmedValueProperty, ref _unconfirmedValue, value);
                }
            }
        }

        public static readonly StyledProperty<SquishState> StateProperty = AvaloniaProperty.Register<Squish, SquishState>(nameof(State));
        public static readonly StyledProperty<SquishApplyValueChange> ApplyValueChangeProperty = AvaloniaProperty.Register<Squish, SquishApplyValueChange>(nameof(ApplyValueChange));
        public static readonly StyledProperty<IControl?> ContentProperty = AvaloniaProperty.Register<Squish, IControl?>(nameof(Content));
        public static readonly StyledProperty<IControl?> PopupProperty = AvaloniaProperty.Register<Squish, IControl?>(nameof(Popup));

        public static readonly StyledProperty<double> BeforeWidthProperty = AvaloniaProperty.Register<Squish, double>(nameof(BeforeWidth));
        public static readonly StyledProperty<double> AfterWidthProperty = AvaloniaProperty.Register<Squish, double>(nameof(AfterWidth));
        public static readonly StyledProperty<double> ThumbWidthProperty = AvaloniaProperty.Register<Squish, double>(nameof(ThumbWidth));
        public static readonly StyledProperty<Thickness> TrackMarginProperty = AvaloniaProperty.Register<Squish, Thickness>(nameof(TrackMargin));

        //protected Image Ghost = null!;
        protected Panel Ghost = null!;

        protected IPopupHost? PopupHost;
        protected Button Before = null!;
        protected Thumb Thumb = null!;
        protected Button After = null!;
        protected IControl? ThumbChild;

        static Squish()
        {
            ContentProperty.Changed.AddClassHandler<Squish>((squish, args) => squish.ContentChanged(args));
            StateProperty.Changed.AddClassHandler<Squish>((squish, args) => squish.OnStateChanged(args));

            Thumb.DragStartedEvent.AddClassHandler<Squish>((x, e) => x.OnThumbDragStarted(e), RoutingStrategies.Bubble);
            Thumb.DragDeltaEvent.AddClassHandler<Squish>((x, e) => x.OnThumbDragDelta(e), RoutingStrategies.Bubble);
            Thumb.DragCompletedEvent.AddClassHandler<Squish>((x, e) => x.OnThumbDragCompleted(e), RoutingStrategies.Bubble);

            AffectsArrange<Squish>(MinimumProperty, MaximumProperty, ValueProperty, ThumbWidthProperty);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            //Ghost = e.NameScope.Find<Image>("Ghost");
            Ghost = e.NameScope.Find<Panel>("Ghost");
            Before = e.NameScope.Find<Button>("Before");
            After = e.NameScope.Find<Button>("After");
            Thumb = e.NameScope.Find<Thumb>("Thumb");
            Thumb.Content = ThumbChild;


            Popup?.AddHandler<RoutedEventArgs>(Button.ClickEvent, PopupClickHandler);
            Before.AddHandler<RoutedEventArgs>(Button.ClickEvent, ScrollClickHandler);
            After.AddHandler<RoutedEventArgs>(Button.ClickEvent, ScrollClickHandler);
        }

        protected void ContentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is IControl oldChild)
            {
                _thumbChangeBinding?.Dispose();
                LogicalChildren.Remove(oldChild);
                ThumbChild = null;
            }

            if (e.NewValue is IControl newChild)
            {
                LogicalChildren.Add(newChild);
                ThumbChild = newChild;
                _thumbChangeBinding = newChild.WhenAnyValue(x => x.Bounds).Subscribe(x => ThumbWidthChanged(x.Size));
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"Control is now {finalSize.Width} x {finalSize.Height}");

            Density = finalSize.Width / (Maximum - Minimum);

            BeforeWidth = (Value - Minimum) * Density;
            AfterWidth = (Maximum - Value) * Density;

            return base.ArrangeOverride(finalSize);
        }

        private void ThumbWidthChanged(Size e)
        {
            ThumbWidth = e.Width / 2;
            TrackMargin = new Thickness(-ThumbWidth, 0, -ThumbWidth, 0);
        }

        /// <summary>
        /// Called when user start dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnThumbDragStarted(VectorEventArgs e)
        {
            Debug.WriteLine($"Started dragging {e.Vector.X}, {e.Vector.Y}");

            State = SquishState.Dragging;
        }

        /// <summary>
        /// Called when user drags the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnThumbDragDelta(VectorEventArgs e)
        {
            Debug.WriteLine($"Still dragging {e.Vector.X}, {e.Vector.Y}");

            Value = e.Vector.X / Density;
            State = SquishState.Dragging;
        }

        /// <summary>
        /// Called when user stop dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnThumbDragCompleted(VectorEventArgs e)
        {
            Debug.WriteLine($"Stopped dragging {e.Vector.X}, {e.Vector.Y}");

            State = SquishState.Confirming;
        }

        private void OnStateChanged(AvaloniaPropertyChangedEventArgs args)
        {
            if (Popup != null && args.NewValue is SquishState state)
            {
                if (state == SquishState.Confirming)
                {
                    OpenPopup();
                    RenderToGhost(Thumb, Ghost);
                }
                else
                {
                    ClosePopup();
                }
            }
        }

        private void OpenPopup()
        {
            if (Popup == null) return;

            ClosePopup();

            Popup.DataContext = this;

            PopupHost = OverlayPopupHost.CreatePopupHost(this, null);
            PopupHost.SetChild(Popup);
            ((ISetLogicalParent)PopupHost).SetParent(this);

            Popup.WhenAnyValue(x => x.Bounds).Subscribe(bounds =>
            {
                var centeringOffset = (Thumb.Bounds.Width - bounds.Width) / 2;
                PopupHost.ConfigurePosition(Thumb, PlacementMode.Top, new Point(centeringOffset, 0));
            });

            PopupHost.Show();
        }

        private void ClosePopup()
        {
            if (PopupHost == null) return;
            PopupHost.SetChild(null);
            PopupHost.Dispose();
            PopupHost = null;
        }

        /// <summary>
        /// Handles events fired by buttons in the popup, discriminates between buttons using IsDefault and IsCancel.
        /// </summary>
        public void PopupClickHandler(object? sender, RoutedEventArgs e)
        {
            if (e.Source is Button button)
            {
                State = SquishState.None;

                if (button.IsDefault)
                {
                    Debug.WriteLine("Confirmed!");
                }
                else
                {
                    Debug.WriteLine("Cancelled!");
                }
            }
        }

        /// <summary>
        /// Handles events fired by the track bar buttons.
        /// </summary>
        public void ScrollClickHandler(object? sender, RoutedEventArgs e)
        {
            if (e.Source is Button button)
            {
                if (ReferenceEquals(button, Before))
                {
                    Value -= LargeChange;
                    Debug.WriteLine("Before!");
                }
                else if (ReferenceEquals(button, After))
                {
                    Value += LargeChange;
                    Debug.WriteLine("After!");
                }
            }
        }

        protected void RenderToGhost(Control source, Panel target, double dpi = 96)
        {
            target.Width = source.Bounds.Width;
            target.Height = source.Bounds.Height;

            var vb = new VisualBrush(source);
            target.Background = vb;
        }
    }
}
