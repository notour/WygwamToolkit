﻿//-----------------------------------------------------------------------
// <copyright file="ExtendedSplashScreen.cs" company="Wygwam">
//     Copyright (c) 2013 Wygwam.
//     Licensed under the Microsoft Public License (Ms-PL) (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
//         http://opensource.org/licenses/Ms-PL.html
//
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

namespace Wygwam.Windows.Controls
{
    using global::Windows.ApplicationModel.Activation;
    using global::Windows.Foundation;
    using global::Windows.UI.Core;
    using global::Windows.UI.Xaml;
    using global::Windows.UI.Xaml.Controls;
    using global::Windows.UI.Xaml.Media;
    using global::Windows.UI.Xaml.Media.Imaging;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Provides a basic extended splash screen that follows Microsoft guidelines.
    /// </summary>
    public class ExtendedSplashScreen : Control
    {
        /// <summary>
        /// Identifies the <see cref="P:Message"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ExtendedSplashScreen), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="P:IsWaiting"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsWaitingProperty =
            DependencyProperty.Register("IsWaiting", typeof(bool), typeof(ExtendedSplashScreen), new PropertyMetadata(true));

        private SplashScreen _splashScreen;

        private Image _splashScreenImage;
        private Uri _splashScreenImagePath;

        private Panel _panel;

        private Brush _backgroundBrush;

        private Type _nextPage;
        private object _navParameter;

        private SynchronizationContext _uiContext;

        private bool _mustLoadState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedSplashScreen"/> class.
        /// </summary>
        /// <param name="args">The <see cref="LaunchActivatedEventArgs"/> instance provided by the
        /// <see cref="Windows.UI.Xaml.Application"/> when it was launched.</param>
        /// <param name="nextPage">The type of the page to which the application will navigate when loading is done.</param>
        /// <param name="parameter">An optional navigation parameter.</param>
        public ExtendedSplashScreen(LaunchActivatedEventArgs args, Type nextPage, object parameter = null)
        {
            this.DefaultStyleKey = typeof(ExtendedSplashScreen);

            Window.Current.SizeChanged += OnResize;

            _nextPage = nextPage;
            _navParameter = parameter;

            _uiContext = SynchronizationContext.Current;

            _mustLoadState = args.PreviousExecutionState == ApplicationExecutionState.Terminated;

            _splashScreen = args.SplashScreen;

            if (_splashScreen != null)
            {
                // Register an event handler to be executed when the splash screen has been dismissed.
                _splashScreen.Dismissed += new TypedEventHandler<SplashScreen, Object>(DismissedEventHandler);
            }

            this.LoadManifestData();
        }

        /// <summary>
        /// Gets or sets a message.
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the splash screen is waiting.
        /// </summary>
        public bool IsWaiting
        {
            get { return (bool)GetValue(IsWaitingProperty); }
            set { SetValue(IsWaitingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the function that will be called asynchronously to perform loading operations
        /// while the splash screen is visible.
        /// </summary>
        public Func<ExtendedSplashScreen, bool, Task> LoadAction
        {
            get;
            set;
        }

        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) 
        /// call ApplyTemplate. In simplest terms, this means the method is called just before a UI element 
        /// displays in your app. Override this method to influence the default post-template logic of a class.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _splashScreenImage = this.GetTemplateChild("SplashScreenImage") as Image;

            if (_splashScreenImage != null)
            {
                _splashScreenImage.ImageOpened += this.OnImageOpened;
                _splashScreenImage.Source = new BitmapImage(_splashScreenImagePath);
            }

            _panel = this.GetTemplateChild("RootPanel") as Panel;

            if (_panel != null)
            {
                _panel.Background = _backgroundBrush;
            }

            this.PositionImage();
        }

        /// <summary>
        /// Loads the path to the splash screen and the background color from the application manifest.
        /// </summary>
        private void LoadManifestData()
        {
            var doc = XDocument.Load("AppxManifest.xml", LoadOptions.None);
            var xname = XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");

            var splashScreenElement = doc.Descendants(xname + "SplashScreen").First();

            var splashScreenPath = splashScreenElement.Attribute("Image").Value;

            _splashScreenImagePath = new Uri(new Uri("ms-appx:///"), splashScreenPath);

            XAttribute bgColor = null;

            if ((bgColor = splashScreenElement.Attribute("BackgroundColor")) == null)
            {
                bgColor = doc.Descendants(xname + "VisualElements").First().Attribute("BackgroundColor");
            }

            _backgroundBrush = new SolidColorBrush(bgColor.Value.AsColor());
        }

        /// <summary>
        /// Positions the extended splash screen image in the same location as the system splash screen image.
        /// </summary>
        private void PositionImage()
        {
            if (_splashScreenImage != null)
            {
                var imgPos = _splashScreen.ImageLocation;

                _splashScreenImage.SetValue(Canvas.LeftProperty, imgPos.X);
                _splashScreenImage.SetValue(Canvas.TopProperty, imgPos.Y);
                _splashScreenImage.Height = imgPos.Height;
                _splashScreenImage.Width = imgPos.Width;
            }
        }

        /// <summary>
        /// Called when the splash screen image has been opened. The system splash screen will be dismissed
        /// a few milliseconds afterwards to avoid flickering.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void OnImageOpened(object sender, RoutedEventArgs e)
        {
            await Task.Delay(50);

            Window.Current.Activate();
        }

        /// <summary>
        /// Called when the extended splash screen is resized. This is important to ensure that the extended splash 
        /// screen is formatted properly in response to snapping, unsnapping, rotation, etc...
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="WindowSizeChangedEventArgs"/> instance containing the event data.</param>
        private void OnResize(object sender, WindowSizeChangedEventArgs e)
        {
            this.PositionImage();
        }

        // Include code to be executed when the system has transitioned from the splash screen to the extended splash screen (application's first view).
        private void DismissedEventHandler(SplashScreen sender, object e)
        {
            _uiContext.Post(this.PerformLoadingAction, null);
        }

        private async void PerformLoadingAction(object state)
        {
            if (this.LoadAction != null)
            {
                await this.LoadAction(this, _mustLoadState);
            }

            var rootFrame = new Frame();

            rootFrame.Navigate(_nextPage, _navParameter);

            Window.Current.Content = rootFrame;
        }
    }
}
