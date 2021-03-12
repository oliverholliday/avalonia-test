using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AvaloniaApplication1.Controls
{
    [PseudoClasses(":pressed")]
    public class Thumb : TemplatedControl
    {
        public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent = RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent = RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<VectorEventArgs> DragCompletedEvent = RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

        [Content]
        public IControl? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Content"/> property.
        /// </summary>
        public static readonly StyledProperty<IControl?> ContentProperty = AvaloniaProperty.Register<Thumb, IControl?>(nameof(Content));

        /// <summary>
        /// The position on the Thumb that the mouse was pressed. Subtracted from the mouse position to get a delta.
        /// </summary>
        public Point? MouseDownPoint { get; protected set; }

        static Thumb()
        {
            DragStartedEvent.AddClassHandler<Thumb>((x, e) => x.OnDragStarted(e), RoutingStrategies.Bubble);
            DragDeltaEvent.AddClassHandler<Thumb>((x, e) => x.OnDragDelta(e), RoutingStrategies.Bubble);
            DragCompletedEvent.AddClassHandler<Thumb>((x, e) => x.OnDragCompleted(e), RoutingStrategies.Bubble);
        }

        public event EventHandler<VectorEventArgs> DragStarted
        {
            add => AddHandler(DragStartedEvent, value);
            remove => RemoveHandler(DragStartedEvent, value);
        }

        public event EventHandler<VectorEventArgs> DragDelta
        {
            add => AddHandler(DragDeltaEvent, value);
            remove => RemoveHandler(DragDeltaEvent, value);
        }

        public event EventHandler<VectorEventArgs> DragCompleted
        {
            add => AddHandler(DragCompletedEvent, value);
            remove => RemoveHandler(DragCompletedEvent, value);
        }

        protected virtual void OnDragStarted(VectorEventArgs e)
        {
        }

        protected virtual void OnDragDelta(VectorEventArgs e)
        {
        }

        protected virtual void OnDragCompleted(VectorEventArgs e)
        {
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (MouseDownPoint.HasValue)
            {
                var ev = new VectorEventArgs
                {
                    RoutedEvent = DragDeltaEvent,
                    Vector = e.GetPosition(Parent) - MouseDownPoint!.Value
                };

                RaiseEvent(ev);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            e.Handled = true;

            MouseDownPoint = e.GetPosition(this);

            var ev = new VectorEventArgs
            {
                RoutedEvent = DragStartedEvent,
                Vector = e.GetPosition(Parent) - MouseDownPoint!.Value
            };

            PseudoClasses.Add(":pressed");

            RaiseEvent(ev);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (MouseDownPoint.HasValue)
            {
                e.Handled = true;

                var ev = new VectorEventArgs
                {
                    RoutedEvent = DragCompletedEvent,
                    Vector = e.GetPosition(Parent) - MouseDownPoint!.Value
                };

                MouseDownPoint = null;

                RaiseEvent(ev);
            }

            PseudoClasses.Remove(":pressed");
        }
    }
}
