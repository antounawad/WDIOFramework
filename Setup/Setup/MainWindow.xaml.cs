using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Eulg.Setup.Pages;
using Size = System.Windows.Size;

namespace Eulg.Setup
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public Stack<Type> PreviousPages = new Stack<Type>();
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public MainWindow()
        {
            Instance = this;
            SetupHelper.ProgressPage = new InstProgress();
            InitializeComponent();

            EventHandler hideDialog = delegate
            {
                DialogContainer.Visibility = Visibility.Collapsed;
                DialogContent.Content = null;
                PageContainer.IsEnabled = true;
            };
            (FindResource("StoryEndDialog") as Storyboard).Completed += hideDialog;
        }

        public ISetupPageBase CurrentPage { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckInst();
        }

        private void CheckInst()
        {
            if (Environment.GetCommandLineArgs().Any(a => a.Equals("/D", StringComparison.CurrentCultureIgnoreCase)))
            {
                NavigateToPage(new InstDependencies(), false);
                return;
            }
            if (Environment.GetCommandLineArgs().Any(a => a.Equals("/C", StringComparison.CurrentCultureIgnoreCase)))
            {
                NavigateToPage(new MaintainUninstall(), false);
                return;
            }
            if (!SetupHelper.CheckInstallation())
            {
                NavigateToPage(new InstWelcome(), false);
                return;
            }
            else
            {
                NavigateToPage(new MaintainChoose(), false);
                return;
            }
        }

        public void NavigateToPage(ISetupPageBase page, bool animation = true, bool noStack = false)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                BtnPrev.IsEnabled = true;
                BtnNext.IsEnabled = true;

                if (!noStack && CurrentPage != null)
                {
                    PreviousPages.Push(CurrentPage.GetType());
                }
                if (!animation)
                {
                    CurrentPage = page;
                    ContentFrame.Content = CurrentPage;
                    LabelTitle.Content = CurrentPage.PageTitle;
                    BtnNext.Content = String.IsNullOrEmpty(CurrentPage.NextButtonText) ? "Weiter" : CurrentPage.NextButtonText;
                    BtnPrev.Visibility = CurrentPage.HasPrev ? Visibility.Visible : Visibility.Hidden;
                    BtnNext.Visibility = CurrentPage.HasNext ? Visibility.Visible : Visibility.Hidden;
                    CurrentPage.PrevPage = (PreviousPages.Count > 0 ? PreviousPages.Peek() : null);
                    CurrentPage.OnLoad();
                    CurrentPage.OnLoadComplete();
                    return;
                }

                var fadeOutCtls = new List<FrameworkElement> { LabelTitle, ContentFrame };
                if (CurrentPage.HasPrev && !page.HasPrev && BtnPrev.IsVisible)
                {
                    fadeOutCtls.Add(BtnPrev);
                }
                if (CurrentPage.HasNext && !page.HasNext && BtnNext.IsVisible)
                {
                    fadeOutCtls.Add(BtnNext);
                }

                var fadeInCtls = new List<FrameworkElement> { LabelFadeInTitle, ContentFadeInFrame };
                if (!CurrentPage.HasPrev && page.HasPrev)
                {
                    fadeInCtls.Add(BtnPrev);
                }
                if (!CurrentPage.HasNext && page.HasNext)
                {
                    fadeInCtls.Add(BtnNext);
                }

                CurrentPage = page;
                ContentFadeInFrame.Content = CurrentPage;
                CloseIcon.Visibility = CurrentPage.OnClose() ? Visibility.Visible : Visibility.Hidden;
                LabelFadeInTitle.Content = CurrentPage.PageTitle;
                CurrentPage.PrevPage = PreviousPages.Count > 0 ? PreviousPages.Peek() : null;
                CurrentPage.OnLoad();

                (ContentFadeInFrame.Content as UIElement).Measure(new Size(ContentAnimationPanel.ActualWidth, 1000));

                var delta = (ContentFadeInFrame.Content as UIElement).DesiredSize.Height - ContentFrame.ActualHeight;
                RunCrossfadeAnimation(delta, fadeOutCtls, fadeInCtls,
                                      delegate
                                      {
                                          LabelTitle.Content = CurrentPage.PageTitle;
                                          LabelTitle.Opacity = 1;
                                          LabelTitle.Visibility = Visibility.Visible;
                                          LabelFadeInTitle.Visibility = Visibility.Collapsed;
                                          ContentFadeInFrame.Content = null;
                                          ContentFrame.Content = CurrentPage;
                                          ContentFrame.Opacity = 1;
                                          ContentFrame.Visibility = Visibility.Visible;
                                          ContentFadeInFrame.Visibility = Visibility.Collapsed;

                                          BtnNext.Content = String.IsNullOrEmpty(CurrentPage.NextButtonText) ? "Weiter" : CurrentPage.NextButtonText;
                                          BtnPrev.Visibility = CurrentPage.HasPrev ? Visibility.Visible : Visibility.Hidden;
                                          BtnNext.Visibility = CurrentPage.HasNext ? Visibility.Visible : Visibility.Hidden;
                                          CurrentPage.OnLoadComplete();
                                      });
            }));
        }

        public void SetCurrentPageProperties()
        {
        }

        private void BtnPrev_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage != null && PreviousPages.Count > 0)
            {
                if (CurrentPage.OnPrev())
                {
                    NavigateToPage((ISetupPageBase)Activator.CreateInstance(PreviousPages.Pop()), true, true);
                }
            }
        }

        private void BtnNext_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage != null)
            {
                CurrentPage.OnNext();
            }
        }

        private void CloseIcon_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentPage != null)
            {
                if (!CurrentPage.OnClose())
                {
                    return;
                }
            }
            ShowDialog("Setup beenden?", null, new Tuple<string, Task>("Ja", new Task(() => Dispatcher.Invoke(new Action(Close)))), new Tuple<string, Task>("Nein", new Task(() => { })));
        }

        #region Integrated Message Box

        public void ShowDialog(string message, Icon icon, params Tuple<string, Task>[] buttons)
        {
            ShowDialog(new[] { new Run(message) }, icon, buttons);
        }

        public void ShowDialog(IEnumerable<Inline> message, Icon icon, params Tuple<string, Task>[] buttons)
        {
            DialogContent.Visibility = Visibility.Collapsed;

            if(icon == null)
            {
                DialogIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                DialogIcon.Visibility = Visibility.Visible;

                var src = new BitmapImage();
                src.BeginInit();
                src.CreateOptions = BitmapCreateOptions.PreservePixelFormat;

                var streamBuffer = new MemoryStream();
                icon.Save(streamBuffer);
                streamBuffer.Position = 0;
                src.StreamSource = streamBuffer;
                src.EndInit();
                DialogIcon.Source = src;
            }

            DialogMessage.Inlines.Clear();
            DialogMessage.Inlines.AddRange(message);
            DialogMessage.Visibility = Visibility.Visible;

            var funkyButtons = buttons.Select(b => Tuple.Create<string, Func<bool>>(b.Item1, () =>
            {
                b.Item2.Start();
                return true;
            })).ToArray();

            CreateDialogButtons(HorizontalAlignment.Center, funkyButtons);

            var storyBeginDialog = FindResource("StoryBeginDialog") as Storyboard;
            storyBeginDialog.Begin();

            DialogContainer.Visibility = Visibility.Visible;
            PageContainer.IsEnabled = false;
        }

        public void ShowCustomDialog(object content, HorizontalAlignment buttonsAlignment, params Tuple<string, Func<bool>>[] buttons)
        {
            DialogIcon.Visibility = Visibility.Collapsed;
            DialogMessage.Visibility = Visibility.Collapsed;
            DialogContent.Visibility = Visibility.Visible;

            DialogContent.Content = content;

            CreateDialogButtons(buttonsAlignment, buttons);

            var storyBeginDialog = FindResource("StoryBeginDialog") as Storyboard;
            storyBeginDialog.Begin();

            DialogContainer.Visibility = Visibility.Visible;
            PageContainer.IsEnabled = false;
        }

        private void CreateDialogButtons(HorizontalAlignment buttonsAlignment, params Tuple<string, Func<bool>>[] buttons)
        {
            DialogPanel.Children.Clear();
            DialogPanel.HorizontalAlignment = buttonsAlignment;

            var style = FindResource("BorderButton") as Style;
            var controls = buttons.Select(b =>
            {
                var c = new Button
                {
                    Content = b.Item1,
                    Style = style,
                    Width = 80,
                    Height = 26,
                    Margin = new Thickness(6, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                return c;
            }).ToArray();

            Action<bool> setStateAll = delegate(bool enabled)
            {
                foreach(var c in controls)
                {
                    c.IsEnabled = enabled;
                }
            };

            for(var n = 0; n < buttons.Length; ++n)
            {
                var index = n;
                controls[n].Click += delegate
                {
                    setStateAll(false);
                    if (buttons[index].Item2())
                    {
                        var storyEndDialog = FindResource("StoryEndDialog") as Storyboard;
                        storyEndDialog.Begin();
                    }
                    else
                    {
                        setStateAll(true);
                    }
                };

                DialogPanel.Children.Add(controls[n]);
            }
        }

        #endregion

        #region Animation

        private void RunCrossfadeAnimation(double deltaHeight, IEnumerable<FrameworkElement> fadeOut, IEnumerable<FrameworkElement> fadeIn, Action onComplete)
        {
            var easeIn = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 9 };
            var easeOut = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 12 };
            var fadeInEaseIn = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 4 };
            var fadeInEaseOut = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 9 };
            var fadeOutEaseIn = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 4 };
            var fadeOutEaseOut = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 9 };

            var duration = 0.35 + Math.Abs(deltaHeight) / 400;

            var contentFromHeight = ContentAnimationPanel.ActualHeight;
            var contentToHeight = contentFromHeight + deltaHeight;

            var startKeyFrameDeltaContent = new EasingDoubleKeyFrame(contentFromHeight,
                                                                     KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)), easeIn);
            var endKeyFrameDeltaContent = new EasingDoubleKeyFrame(contentToHeight,
                                                                   KeyTime.FromTimeSpan(TimeSpan.FromSeconds(duration)), easeOut);

            var startKeyFrameFadeIn = new EasingDoubleKeyFrame(0,
                                                               KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)), fadeInEaseIn);
            var endKeyFrameFadeIn = new EasingDoubleKeyFrame(1,
                                                             KeyTime.FromTimeSpan(TimeSpan.FromSeconds(duration)), fadeInEaseOut);

            var startKeyFrameFadeOut = new EasingDoubleKeyFrame(1,
                                                                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)), fadeOutEaseIn);
            var endKeyFrameFadeOut = new EasingDoubleKeyFrame(0,
                                                              KeyTime.FromTimeSpan(TimeSpan.FromSeconds(duration)), fadeOutEaseOut);
            var resetKeyFrameFadeOut = new DiscreteDoubleKeyFrame(1,
                                                                  KeyTime.FromTimeSpan(TimeSpan.FromSeconds(duration * 1.0001)));

            var animContent = new DoubleAnimationUsingKeyFrames();
            animContent.KeyFrames.Add(startKeyFrameDeltaContent);
            animContent.KeyFrames.Add(endKeyFrameDeltaContent);
            animContent.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("Height"));
            animContent.FillBehavior = FillBehavior.Stop;

            var animFadeIn = new DoubleAnimationUsingKeyFrames();
            animFadeIn.KeyFrames.Add(startKeyFrameFadeIn);
            animFadeIn.KeyFrames.Add(endKeyFrameFadeIn);
            animFadeIn.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("Opacity"));
            animFadeIn.FillBehavior = FillBehavior.Stop;

            var animFadeOut = new DoubleAnimationUsingKeyFrames();
            animFadeOut.KeyFrames.Add(startKeyFrameFadeOut);
            animFadeOut.KeyFrames.Add(endKeyFrameFadeOut);
            animFadeOut.KeyFrames.Add(resetKeyFrameFadeOut);
            animFadeOut.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("Opacity"));
            animFadeOut.FillBehavior = FillBehavior.Stop;

            var contentDeltaStory = new Storyboard();
            contentDeltaStory.Children.Add(animContent);

            var fadeOutStory = new Storyboard();
            fadeOutStory.Children.Add(animFadeOut);

            var fadeInStory = new Storyboard();
            fadeInStory.Children.Add(animFadeIn);

            contentDeltaStory.Completed += delegate
            {
                Action completeHandler = delegate
                {
                    foreach (var element in fadeOut)
                    {
                        element.Visibility = Visibility.Hidden;
                    }

                    onComplete();
                };

                Dispatcher.Invoke(completeHandler);
            };

            foreach (var element in fadeOut)
            {
                fadeOutStory.Begin(element);
            }
            foreach (var element in fadeIn)
            {
                fadeInStory.Begin(element);
                element.Visibility = Visibility.Visible;
            }

            contentDeltaStory.Begin(ContentAnimationPanel);
        }

        #endregion

        #region Window Move
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        private void OnMouseButtonDown(object sender, RoutedEventArgs e)
        {
            ReleaseCapture();
            SendMessage(new WindowInteropHelper(this).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
        #endregion

    }
}
