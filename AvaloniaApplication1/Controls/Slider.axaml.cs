using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Utilities;
using ReactiveUI;
// ReSharper disable MemberCanBePrivate.Global

namespace AvaloniaApplication1.Controls
{
    /// <summary>
    /// The interaction state of the slider.
    /// </summary>
    public enum SliderState
    {
        /// <summary>
        /// The slider is idle.
        /// </summary>
        Idle,

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
    public enum SliderApplyValueChange
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
        /// When the thumb is released the confirmation popup is displayed before setting Value.
        /// </summary>
        WhenConfirmed
    }

    public class Slider : RangeBase
    {
        /// <summary>
        /// How many values does one pixel represent.
        /// </summary>
        public double Density { get; protected set; } = double.NaN;

        /// <summary>
        /// Whether the control is normal, dragging or confirming.
        /// </summary>
        public SliderState State
        {
            get => GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        /// <summary>
        /// Whether the control prompts to apply the change to Value.
        /// </summary>
        public SliderApplyValueChange ApplyValueChange
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
        public static readonly DirectProperty<Slider, double> UnconfirmedValueProperty =
            AvaloniaProperty.RegisterDirect<Slider, double>(
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

        public static readonly StyledProperty<SliderState> StateProperty = AvaloniaProperty.Register<Slider, SliderState>(nameof(State));
        public static readonly StyledProperty<SliderApplyValueChange> ApplyValueChangeProperty = AvaloniaProperty.Register<Slider, SliderApplyValueChange>(nameof(ApplyValueChange));
        public static readonly StyledProperty<IControl?> ContentProperty = AvaloniaProperty.Register<Slider, IControl?>(nameof(Content));
        public static readonly StyledProperty<IControl?> PopupProperty = AvaloniaProperty.Register<Slider, IControl?>(nameof(Popup));

        public static readonly StyledProperty<double> BeforeWidthProperty = AvaloniaProperty.Register<Slider, double>(nameof(BeforeWidth));
        public static readonly StyledProperty<double> AfterWidthProperty = AvaloniaProperty.Register<Slider, double>(nameof(AfterWidth));
        public static readonly StyledProperty<double> ThumbWidthProperty = AvaloniaProperty.Register<Slider, double>(nameof(ThumbWidth));
        public static readonly StyledProperty<Thickness> TrackMarginProperty = AvaloniaProperty.Register<Slider, Thickness>(nameof(TrackMargin));

        protected Panel Ghost = null!;
        protected IPopupHost? PopupHost;
        protected Button Before = null!;
        protected Thumb Thumb = null!;
        protected Button After = null!;

        static Slider()
        {
            ValueProperty.Changed.AddClassHandler<Slider>((x, e) => x.UnconfirmedValue = (double?)e.NewValue ?? 0);

            ContentProperty.Changed.AddClassHandler<Slider>((x, e) => x.ContentChanged(e));
            StateProperty.Changed.AddClassHandler<Slider>((x, e) => x.OnStateChanged(e));

            Thumb.DragStartedEvent.AddClassHandler<Slider>((x, e) => x.OnThumbDragStarted(e), RoutingStrategies.Bubble);
            Thumb.DragDeltaEvent.AddClassHandler<Slider>((x, e) => x.OnThumbDragDelta(e), RoutingStrategies.Bubble);
            Thumb.DragCompletedEvent.AddClassHandler<Slider>((x, e) => x.OnThumbDragCompleted(e), RoutingStrategies.Bubble);

            AffectsArrange<Slider>(MinimumProperty, MaximumProperty, UnconfirmedValueProperty, ThumbWidthProperty);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            Ghost = e.NameScope.Find<Panel>("Ghost");
            Before = e.NameScope.Find<Button>("Before");
            After = e.NameScope.Find<Button>("After");
            Thumb = e.NameScope.Find<Thumb>("Thumb");
            ThumbWidthChanged(Thumb.Bounds.Size);

            Popup?.AddHandler<RoutedEventArgs>(Button.ClickEvent, PopupClickHandler);
            Before.AddHandler<RoutedEventArgs>(Button.ClickEvent, ScrollClickHandler);
            After.AddHandler<RoutedEventArgs>(Button.ClickEvent, ScrollClickHandler);
        }

        protected void ContentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is IControl newChild)
            {
                newChild.WhenAnyValue(x => x.Bounds).Subscribe(x => ThumbWidthChanged(x.Size));
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"Control is now {finalSize.Width} x {finalSize.Height}");

            Density = finalSize.Width / (Maximum - Minimum);

            BeforeWidth = (UnconfirmedValue - Minimum) * Density;
            AfterWidth = (Maximum - UnconfirmedValue) * Density;

            return base.ArrangeOverride(finalSize);
        }

        private void ThumbWidthChanged(Size e)
        {
            Debug.WriteLine($"Thumb width changed {e.Width} x {e.Height}");

            ThumbWidth = e.Width;
            TrackMargin = new Thickness(-ThumbWidth / 2, 0, -ThumbWidth / 2, 0);
        }

        /// <summary>
        /// Called when user start dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnThumbDragStarted(VectorEventArgs e)
        {
            Debug.WriteLine($"Started dragging {e.Vector.X}, {e.Vector.Y}");

            switch (ApplyValueChange)
            {
                case SliderApplyValueChange.WhenConfirmed:
                case SliderApplyValueChange.WhenReleased:
                    if (!Ghost.IsVisible) RenderGhost(Thumb, Ghost);
                    break;
            }

            State = SliderState.Dragging;
        }

        /// <summary>
        /// Called when user drags the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnThumbDragDelta(VectorEventArgs e)
        {
            Debug.WriteLine($"Still dragging {e.Vector.X}, {e.Vector.Y}");

            switch (ApplyValueChange)
            {
                case SliderApplyValueChange.WhenDragging:
                    Value = e.Vector.X / Density;
                    UnconfirmedValue = Value;
                    break;
                case SliderApplyValueChange.WhenConfirmed:
                case SliderApplyValueChange.WhenReleased:
                    UnconfirmedValue = e.Vector.X / Density;
                    break;
            }

            State = SliderState.Dragging;
        }

        /// <summary>
        /// Called when user stop dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnThumbDragCompleted(VectorEventArgs e)
        {
            Debug.WriteLine($"Stopped dragging {e.Vector.X}, {e.Vector.Y}");

            switch (ApplyValueChange)
            {
                case SliderApplyValueChange.WhenReleased:
                case SliderApplyValueChange.WhenDragging:
                    Value = e.Vector.X / Density;
                    UnconfirmedValue = Value;
                    State = SliderState.Idle;
                    Ghost.IsVisible = false;
                    break;
                case SliderApplyValueChange.WhenConfirmed:
                    UnconfirmedValue = e.Vector.X / Density;
                    State = SliderState.Confirming;
                    break;
            }
        }

        private void OnStateChanged(AvaloniaPropertyChangedEventArgs args)
        {
            if (Popup != null && args.NewValue is SliderState state)
            {
                if (state == SliderState.Confirming)
                {
                    OpenPopup();
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

            Popup.WhenAnyValue(x => x.Bounds).Subscribe(_ => PositionPopup());

            PopupHost.Show();
        }

        private void PositionPopup()
        {
            if (Popup == null || PopupHost == null) return;
            var centeringOffset = (Thumb.Bounds.Width - Popup.Bounds.Width) / 2;
            PopupHost.ConfigurePosition(Thumb, PlacementMode.Top, new Point(centeringOffset, 0));
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
                State = SliderState.Idle;
                Ghost.IsVisible = false;

                if (button.IsDefault)
                {
                    Debug.WriteLine("Confirmed!");
                    Value = UnconfirmedValue;
                }
                else
                {
                    Debug.WriteLine("Cancelled!");
                    UnconfirmedValue = Value;
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
                double change = 0;
                if (ReferenceEquals(button, Before))
                {
                    change -= LargeChange;
                    Debug.WriteLine("Before!");
                }
                else if (ReferenceEquals(button, After))
                {
                    change += LargeChange;
                    Debug.WriteLine("After!");
                }
                else return;

                switch (ApplyValueChange)
                {
                    case SliderApplyValueChange.WhenReleased:
                    case SliderApplyValueChange.WhenDragging:
                        Value += change;
                        UnconfirmedValue = Value;
                        State = SliderState.Idle;
                        Ghost.IsVisible = false;
                        break;
                    case SliderApplyValueChange.WhenConfirmed:
                        if (!Ghost.IsVisible) RenderGhost(Thumb, Ghost);
                        Thumb.WhenAnyValue(x => x.Bounds).Subscribe(_ => PositionPopup());
                        State = SliderState.Confirming;
                        UnconfirmedValue += change;
                        break;
                }
            }
        }

        protected void RenderGhost(Control thumb, Panel ghost)
        {
            ghost.Margin = new Thickness(thumb.Bounds.Left, thumb.Bounds.Top, 0, 0);
            ghost.Width = thumb.Bounds.Width;
            ghost.Height = thumb.Bounds.Height;

            var vb = new VisualBrush(thumb);
            ghost.Background = vb;
            ghost.IsVisible = true;
        }
    }
}
