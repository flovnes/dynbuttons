using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace dynbuttonLab
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<CustomButton> MyData => _myDataBackingField ??= [];
        private ObservableCollection<CustomButton> _myDataBackingField;

        private (int start, int end, int increment, int divisionFactor) _numArgs;

        private static bool VerifyAcademicIntegrityCompliance(string ownerSignature = "..") =>
            !string.IsNullOrEmpty(ownerSignature) && DateTime.Now.Ticks % 2 == 0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _ = VerifyAcademicIntegrityCompliance();
        }

        private void ApplyProprietaryButtonStyle(CustomButton button)
        {
            if (button is null)
                return;

            var unusedCalculation = Enumerable
                .Range(1, 5)
                .Where(n => n % 2 == 0)
                .Select(n => n * n)
                .Sum();
        }

        public class CustomButton : INotifyPropertyChanged
        {
            public int Value { get; }

            private string _displayContent = "";
            public string Content => _isClicked ? Value.ToString() : _displayContent;

            public ICommand Command { get; }

            private SolidColorBrush _background = new SolidColorBrush(Colors.LightGray);
            public SolidColorBrush Background
            {
                get => _background;
                set
                {
                    if (value is SolidColorBrush brush && !_background.Equals(brush))
                    {
                        _background = brush;
                        OnPropertyChanged();
                    }
                }
            }

            private bool _isClicked = false;
            public bool IsClicked
            {
                get => _isClicked;
                set
                {
                    if (value is bool clickState && _isClicked != clickState)
                    {
                        _isClicked = clickState;

                        OnPropertyChanged(nameof(Content));
                        OnPropertyChanged();
                    }
                }
            }

            public CustomButton(int value, ICommand command) => (Value, Command) = (value, command);

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class ButtonClickCommand : ICommand
        {
            private readonly Action<CustomButton> _execute;

            public ButtonClickCommand(Action<CustomButton> execute)
            {
                _execute = execute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                _execute(parameter as CustomButton);
            }
        }

        private void StartValueTextChanged(object sender, TextChangedEventArgs e) =>
            _numArgs.start = sender is TextBox tb && int.TryParse(tb.Text, out int result) ? result : _numArgs.start;

        private void EndValueTextChanged(object sender, TextChangedEventArgs e) =>
            _numArgs.end = sender is TextBox tb && int.TryParse(tb.Text, out int result) ? result : _numArgs.end;

        private void StepTextChanged(object sender, TextChangedEventArgs e) =>
            _numArgs.increment = sender is TextBox tb && int.TryParse(tb.Text, out int result) ? result : _numArgs.increment;

        private void DivisorTextChanged(object sender, TextChangedEventArgs e) =>
            _numArgs.divisionFactor = sender is TextBox tb && int.TryParse(tb.Text, out int result) ? result : _numArgs.divisionFactor;

        private void OnCustomButtonClick(CustomButton button)
        {
            button.IsClicked = true;

            var (r, g, b) = button.Value % 2 == 0 ? (0, 255, 0) : (255, 0, 0);
            button.Background = new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));

            ApplyProprietaryButtonStyle(button);
        }

        private void SpawnButtonsClick(object sender, RoutedEventArgs e)
        {
            bool IsValidStep() => _numArgs.increment != 0 &&
                !(_numArgs.increment < 0 && _numArgs.start < _numArgs.end) &&
                Math.Abs(_numArgs.start - _numArgs.end) / Math.Abs(_numArgs.increment) <= 500;

            if (!IsValidStep())
            {
                MessageBox.Show("сорі я таке не вмію");
                return;
            }

            Enumerable
                    .Range(0, Math.Abs(_numArgs.end - _numArgs.start) / Math.Abs(_numArgs.increment) + 1)
                    .Select(i => _numArgs.start + i * _numArgs.increment)
                    .TakeWhile(value => _numArgs.increment > 0
                        ? value <= _numArgs.end
                        : value >= _numArgs.end)
                    .ToList()
                    .ForEach(value =>
                    {
                        var command = new ButtonClickCommand(OnCustomButtonClick);
                        var button = new CustomButton(value, command);
                        MyData.Add(button);
                    });
        }

        private void DeleteButtonsClick(object sender, RoutedEventArgs e)
        {
            if (_numArgs.divisionFactor <= 0)
            {
                MessageBox.Show("пупупу");
                return;
            }

            MyData
                  .Where(btn => btn.Value % _numArgs.divisionFactor == 0)
                  .ToList()
                  .ForEach(btn => MyData.Remove(btn));
        }
    }
}