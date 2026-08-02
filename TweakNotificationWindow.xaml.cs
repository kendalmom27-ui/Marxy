using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace RasTweaksCS
{
    public partial class TweakNotificationWindow : Window
    {
        private DispatcherTimer? _spinnerTimer;
        private DispatcherTimer? _autoCloseTimer;

        public TweakNotificationWindow()
        {
            InitializeComponent();

            this.Closing += (s, e) =>
            {
                StopSpinnerAnimation();
                StopAutoCloseTimer();
            };
        }

        private void OnDismissClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }

        private void StartAutoCloseTimer(TimeSpan delay)
        {
            StopAutoCloseTimer();

            _autoCloseTimer = new DispatcherTimer { Interval = delay };
            _autoCloseTimer.Tick += (s, e) =>
            {
                StopAutoCloseTimer();
                Close();
            };
            _autoCloseTimer.Start();
        }

        private void StopAutoCloseTimer()
        {
            if (_autoCloseTimer != null)
            {
                _autoCloseTimer.Stop();
                _autoCloseTimer = null;
            }
        }

        public void SetLoadingState(string title, string status)
        {
            try
            {
                TitleText.Text = title;
                StatusText.Text = status;
                LoadingSpinner.Visibility = Visibility.Visible;
                SuccessCheckmark.Visibility = Visibility.Collapsed;
                ErrorMark.Visibility = Visibility.Collapsed;
                SetGlowColor("#4CAF50");
                StartSpinnerAnimation();
            }
            catch { }
        }

        public void SetSuccessState(string title, string status)
        {
            try
            {
                StopSpinnerAnimation();

                TitleText.Text = title;
                StatusText.Text = status;
                LoadingSpinner.Visibility = Visibility.Collapsed;
                SuccessCheckmark.Visibility = Visibility.Visible;
                ErrorMark.Visibility = Visibility.Collapsed;
                SetGlowColor("#4CAF50");

                StartAutoCloseTimer(TimeSpan.FromSeconds(1.5));
            }
            catch { }
        }

        public void SetErrorState(string title, string status)
        {
            try
            {
                StopSpinnerAnimation();

                TitleText.Text = title;
                StatusText.Text = status;
                LoadingSpinner.Visibility = Visibility.Collapsed;
                SuccessCheckmark.Visibility = Visibility.Collapsed;
                ErrorMark.Visibility = Visibility.Visible;
                SetGlowColor("#FF5C5C");

                StartAutoCloseTimer(TimeSpan.FromSeconds(2.5));
            }
            catch { }
        }

        private void SetGlowColor(string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            GlowEffect.Color = color;
            RootBorder.BorderBrush = new SolidColorBrush(color) { Opacity = 0.4 };
        }

        public void ShowSuccess()
        {
            SetSuccessState("Success", "Tweak applied successfully!");
        }

        private void StartSpinnerAnimation()
        {
            StopSpinnerAnimation();
            
            _spinnerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            
            _spinnerTimer.Tick += (s, e) =>
            {
                try
                {
                    if (SpinnerRotate != null)
                    {
                        SpinnerRotate.Angle = (SpinnerRotate.Angle + 6) % 360;
                    }
                }
                catch
                {
                    StopSpinnerAnimation();
                }
            };
            
            _spinnerTimer.Start();
        }

        private void StopSpinnerAnimation()
        {
            if (_spinnerTimer != null)
            {
                _spinnerTimer.Stop();
                _spinnerTimer = null;
            }
        }
    }
}