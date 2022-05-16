using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace Cloud_ShareSync.GUI.Types {
    internal class PathStack : StackPanel {

        public PathStack(
            string headerTxt,
            string primaryTxt,
            string buttonTxt,
            EventHandler<RoutedEventArgs> lambda,
            string? missingPathTxt = null
        ) {
            _header.Text = headerTxt;
            _mainTxt.Text = primaryTxt;
            _updateButton.Content = buttonTxt;
            if (missingPathTxt != null) {
                _missingPathTxt.Text = missingPathTxt;
            } else {
                SetMissingPathWarningVisibility( false );
            }
            _updateButton.Click += lambda;
            Configure( );
        }

        public string PathText {
            get { return _mainTxt.Text; }
            set { _mainTxt.Text = value; }
        }

        public readonly WrapPanel InnerWrap1 = new( ) { Margin = Thickness.Parse( "5,5,5,0" ) };
        public readonly WrapPanel InnerWrap2 = new( ) { Margin = Thickness.Parse( "5,0,5,5" ) };

        private readonly TextBlock _header = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly TextBlock _mainTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 16
        };

        private readonly Button _updateButton = new( ) {
            Margin = Thickness.Parse( "5,5,5,5" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = true,
        };

        private readonly TextBlock _missingPathTxt = new( ) {
            Foreground = Brushes.White,
            Background = Brushes.Red,
            VerticalAlignment = VerticalAlignment.Center
        };

        internal void WarnOnMissingPath( string path, bool file ) {
            bool state = (file ?
                    File.Exists( path ) :
                    Directory.Exists( path )) == false;
            SetMissingPathWarningVisibility( state );
            SetMainTxtColors( state );
        }

        internal void SetMissingPathWarningVisibility( bool status ) {
            _missingPathTxt.IsVisible = status;
        }

        private void SetMainTxtColors( bool errColors ) {
            if (errColors) {
                _mainTxt.Foreground = Brushes.White;
                _mainTxt.Background = Brushes.Red;
            } else {
                _mainTxt.Foreground = Brushes.Black;
                _mainTxt.Background = Brushes.White;
            }
        }

        private void Configure( ) {
            ConfigureWrapPanelOne( );
            ConfigureWrapPanelTwo( );
        }

        private void ConfigureWrapPanelOne( ) {
            InnerWrap1.Children.Add( _header );
            InnerWrap1.Children.Add( _mainTxt );
            Children.Add( InnerWrap1 );
        }

        private void ConfigureWrapPanelTwo( ) {
            InnerWrap2.Children.Add( _updateButton );
            InnerWrap2.Children.Add( _missingPathTxt );
            Children.Add( InnerWrap2 );
        }

    }
}
