using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.ConcurrencyVisualizer.Instrumentation;
using WpfExamples.Annotations;

namespace WpfExamples
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private const int ElementsToAddCount = 10;

        private bool _heavyLoadStarted;
        private Color _previousColor;
        private readonly int _delay = 50;
        private readonly int _maxElements = 306;
        private readonly Random _random = new Random();
        private int _elementsPushedToDispatcher;
        private int _elementsDisplayed;
        private int _elementsInDispacherQueue;
        private DispatcherPriority _selectedDispatcherPriority = DispatcherPriority.Background;
        private DispatcherPriority _selectedAddNewItemPriority = DispatcherPriority.Background;

        public ObservableCollection<Brush> Items { get; }
        public ObservableCollection<DispatcherPriority> DispatcherPriorities { get; }


        public int ElementsInDispacherQueue
        {
            get => _elementsInDispacherQueue;
            set
            {
                _elementsInDispacherQueue = value;
                OnPropertyChanged();
            }
        }

        public DispatcherPriority SelectedDispatcherPriority
        {
            get => _selectedDispatcherPriority;
            set
            {
                _selectedDispatcherPriority = value;
                OnPropertyChanged();
            }
        }

        public DispatcherPriority SelectedAddNewItemPriority
        {
            get => _selectedAddNewItemPriority;
            set
            {
                _selectedAddNewItemPriority = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            Items = new ObservableCollection<Brush>();
            var dispatcherPriorities = Enum.GetValues(typeof(DispatcherPriority)).Cast<DispatcherPriority>();
            DispatcherPriorities = new ObservableCollection<DispatcherPriority>(dispatcherPriorities);

            SelectedDispatcherPriority = DispatcherPriority.Background;

            DataContext = this;
            
            InitializeComponent();
            
            Enumerable.Range(0, _maxElements).ToList().ForEach(_ => AddNewNormalItem());
            _elementsPushedToDispatcher = 0;
            _elementsDisplayed = 0;
            ElementsInDispacherQueue = 0;
        }

        // TODO: Add switch for DispatcherPriority to show that application can be hanged
        // TODO: and separate for special operations

        private async void OnCreateHeavyLoad(object sender, RoutedEventArgs e)
        {
            _heavyLoadStarted = true;
            
            _previousColor = new Color();
            
            while (_heavyLoadStarted)
            {
                Debug.WriteLine($"ThreadId: {Thread.CurrentThread.ManagedThreadId}");
                
                _elementsPushedToDispatcher++;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(AddNewNormalItem), SelectedDispatcherPriority);
                }
#pragma warning restore CS4014

                await Task.Delay(_delay - 5).ConfigureAwait(false);
               
            }
        }

        private void AddNewNormalItem()
        {
            var newColor = new Color
            {
                A = 150,
                R = GetNewColor(_previousColor.R),
                G = GetNewColor(_previousColor.G),
                B = GetNewColor(_previousColor.B),
            };

            _previousColor = newColor;

            AddItem(newColor, _delay);
        }

        private byte GetNewColor(byte previousColor)
        {
            return (byte) Math.Abs(previousColor + _random.Next(50) - 25);
        }

        private void OnStopHeavyLoad(object sender, RoutedEventArgs e)
        {
            _heavyLoadStarted = false;
        }

        private void AddNewSpecialItem()
        {
            var newColor = new Color
            {
                A = 255,
                R = 255,
                G = 0,
                B = 0,
            };

            AddItem(newColor, _delay * 6);
        }

        private void AddItem(Color colour, int delay)
        {
            Items.Add(new SolidColorBrush(colour));

            ElementsInDispacherQueue = _elementsPushedToDispatcher - ++_elementsDisplayed;
                
            if (Items.Count > _maxElements)
            {
                Items.RemoveAt(0);
            }
            
            if (Items.Count == _maxElements)
            {
                var span = Markers.EnterSpan($"Starting delay: {delay}");
                Thread.Sleep(delay);
                // SpinWait.SpinUntil(() => false, delay);
                // Thread.SpinWait(delay);
                span.Leave();
            }
        }

        private void OnAddWithInvoke(object sender, RoutedEventArgs e)
        {
            InvokeButton.IsEnabled = false;

            for (var i = 0; i < ElementsToAddCount; i++)
            {
                _elementsPushedToDispatcher++;
                Application.Current.Dispatcher.Invoke(AddNewSpecialItem, SelectedAddNewItemPriority);
            }
            InvokeButton.IsEnabled = true;
        }

        private void OnAddWithBeginInvoke(object sender, RoutedEventArgs e)
        {
            BeginInvokeButton.IsEnabled = false;

            for (var i = 0; i < ElementsToAddCount; i++)
            {
                _elementsPushedToDispatcher++;
                Application.Current.Dispatcher.BeginInvoke(new Action(AddNewSpecialItem), SelectedAddNewItemPriority);
            }

            BeginInvokeButton.IsEnabled = true;
        }

        private async void OnAddWithBeginInvokeAsync(object sender, RoutedEventArgs e)
        {
            InvokeAsyncButton.IsEnabled = false;

            _elementsPushedToDispatcher += ElementsToAddCount;

            for (var i = 0; i < ElementsToAddCount; i++)
            {
                await Application.Current.Dispatcher.InvokeAsync(AddNewSpecialItem, SelectedAddNewItemPriority);
            }

            InvokeAsyncButton.IsEnabled = true;
        }

        private async void OnAddWithBeginInvokeAsyncAndWhenAll(object sender, RoutedEventArgs e)
        {
            WhenAllButton.IsEnabled = false;

            _elementsPushedToDispatcher += ElementsToAddCount;
            
            var tasks = Enumerable.Range(0, ElementsToAddCount)
                .Select(_ => Application.Current.Dispatcher.InvokeAsync(AddNewSpecialItem, SelectedAddNewItemPriority).Task);

            await Task.WhenAll(tasks);

            WhenAllButton.IsEnabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}