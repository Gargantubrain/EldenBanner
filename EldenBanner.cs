using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace EldenRingBanner
{
    public class Program : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            string bannerText = null;
            bool noSound = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "--help" || arg == "-h" || arg == "/?") { ShowUsage(null); return; }
                if (arg == "--nosound" || arg == "--quiet" || arg == "--silent") { noSound = true; continue; }
                if (arg.StartsWith("-") || arg.StartsWith("/")) { ShowUsage($"Error: Unrecognized option '{arg}'"); return; }
                if (bannerText != null) { ShowUsage("Error: Too many arguments.\nDid you forget to wrap your message in quotes?"); return; }
                bannerText = arg;
            }

            // Default message if none provided
            if (bannerText == null)
                bannerText = "Run EldenBanner.exe --help";

            var app = new Program();
            app.Run(new BannerWindow(bannerText, noSound));
        }

        private static void ShowUsage(string errorMsg)
        {
            string usage = string.IsNullOrEmpty(errorMsg) ? "" : errorMsg + "\n\n";
            usage +=
                "Elden Banner v1.0.2 - github.com/Gargantubrain/EldenBanner\n" +
                "------------------------------------------------\n" +
                "Displays a transparent, click-through overlay message.\n\n" +
                "Usage:\n" +
                "  EldenBanner.exe [\"Your Custom Message\"]\n\n" +
                "Examples:\n" +
                "  EldenBanner.exe\n" +
                "      -> Displays --help banner message\n\n" +
                "  EldenBanner.exe \"YOU DIED\"\n" +
                "      -> Displays: \"YOU DIED\"\n\n" +
                "Options:\n" +
                "  --help, -h, /?              Show this help message.\n" +
                "  --nosound, --quiet, --silent  Disable sound playback.\n\n" +
                "Note: Always wrap multi-word messages in quotes.";

            MessageBox.Show(usage, "Elden Banner Usage", MessageBoxButton.OK,
                string.IsNullOrEmpty(errorMsg) ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
    }

    public class BannerWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private TextBlock _backText;
        private TextBlock _frontText;
        private bool _noSound;
        private MediaPlayer _player;

        public BannerWindow(string bannerText, bool noSound)
        {
            _noSound = noSound;

            Title = "Elden Banner";
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Load sound
            if (!_noSound)
            {
                string soundPath = Path.Combine(baseDir, "elden_ring_sound.mp3");
                if (File.Exists(soundPath))
                {
                    _player = new MediaPlayer();
                    _player.Open(new Uri(soundPath));
                    _player.Volume = 0.5;
                }
            }

            // Load font
            FontFamily font = new FontFamily("Times New Roman");
            string fontPath = Path.Combine(baseDir, "Mantinia.otf");
            if (File.Exists(fontPath))
            {
                string dir = baseDir.Replace("\\", "/");
                font = new FontFamily(new Uri("file:///" + dir), "./#Mantinia");
            }

            // Build UI
            var grid = new Grid();

            // Dark gradient strip
            var strip = new Border
            {
                Height = 120,
                VerticalAlignment = VerticalAlignment.Center,
                Background = CreateGradientBrush()
            };
            grid.Children.Add(strip);

            string text = bannerText.ToUpper();

            // Back text (glow layer)
            _backText = new TextBlock
            {
                Text = text,
                FontSize = 78,
                FontFamily = font,
                Foreground = new SolidColorBrush(Color.FromArgb(100, 218, 165, 32)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Effect = new BlurEffect { Radius = 15 },
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0.9, 0.9)
            };
            grid.Children.Add(_backText);

            // Front text (sharp layer)
            _frontText = new TextBlock
            {
                Text = text,
                FontSize = 72,
                FontFamily = font,
                Foreground = new SolidColorBrush(Color.FromRgb(218, 165, 32)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 8, ShadowDepth = 0 },
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0.85, 0.85)
            };
            grid.Children.Add(_frontText);

            Content = grid;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MakeClickThrough();
            _player?.Play();
            StartAnimations();
        }

        private void MakeClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        private LinearGradientBrush CreateGradientBrush()
        {
            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(180, 0, 0, 0), 0.15));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(180, 0, 0, 0), 0.85));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1.0));
            return brush;
        }

        private void StartAnimations()
        {
            double duration = 3.0;
            var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            // Back layer zoom
            var backScale = (ScaleTransform)_backText.RenderTransform;
            backScale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0.9, 1.2, TimeSpan.FromSeconds(duration)) { EasingFunction = ease });
            backScale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0.9, 1.2, TimeSpan.FromSeconds(duration)) { EasingFunction = ease });

            // Front layer zoom
            var frontScale = (ScaleTransform)_frontText.RenderTransform;
            frontScale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0.85, 1.0, TimeSpan.FromSeconds(duration)) { EasingFunction = ease });
            frontScale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0.85, 1.0, TimeSpan.FromSeconds(duration)) { EasingFunction = ease });

            // Fade out
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5))
            {
                BeginTime = TimeSpan.FromSeconds(duration - 0.5)
            };
            fadeOut.Completed += (s, ev) => Close();
            _backText.BeginAnimation(OpacityProperty, fadeOut);
            _frontText.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
