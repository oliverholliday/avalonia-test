using ReactiveUI;

namespace AvaloniaApplication1.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        protected double ValueProperty = 30;

        public double Value
        {
            get => ValueProperty;
            set => this.RaiseAndSetIfChanged(ref ValueProperty, value);
        }
    }
}
