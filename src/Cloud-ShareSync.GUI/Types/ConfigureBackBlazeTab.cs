using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureBackBlazeTab : StackPanel, IConfigurationTab {

        public ConfigureBackBlazeTab( B2Config config, ConfigureWindow parent ) {
            _parentWindow = parent;
            ConfigureWindowSettings( );
            ConfigureWindowContent( config );
        }


        #region Fields

        private readonly ConfigureWindow _parentWindow;

        private readonly Button _saveButton = new( ) {
            Name = "SaveSyncConfig",
            Margin = Thickness.Parse( "5,15,5,15" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Save"
        };


        #region ApplicationKeyId

        private readonly TextBlock _applicationKeyIdHeader = new( ) {
            Text = "Application Key Id: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };


        private readonly TextBox _applicationKeyIdTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16
        };

        #endregion ApplicationKeyId


        #region ApplicationKey

        private readonly TextBlock _applicationKeyHeader = new( ) {
            Text = "Application Key: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };


        private readonly TextBox _applicationKeyTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16
        };

        #endregion ApplicationKey


        #region BucketName

        private readonly TextBlock _bucketNameHeader = new( ) {
            Text = "Bucket Name: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };


        private readonly TextBox _bucketNameTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16
        };

        #endregion BucketName


        #region BucketId

        private readonly TextBlock _bucketIdHeader = new( ) {
            Text = "Bucket Id: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };


        private readonly TextBox _bucketIdTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16
        };

        #endregion BucketId


        #region MaxConsecutiveErrors

        private readonly TextBlock _maxConsecutiveErrorsHeader = new( ) {
            Text = "Max Consecutive Errors: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly NumericUpDown _maxConsecutiveErrors = new( ) {
            Minimum = 1,
            Increment = 1
        };

        #endregion MaxConsecutiveErrors


        #region ProcessThreads

        private readonly TextBlock _processThreadsHeader = new( ) {
            Text = "Upload/Download Threads: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly NumericUpDown _processThreads = new( ) {
            Minimum = 1,
            Increment = 1
        };

        #endregion ProcessThreads

        #endregion Fields


        #region Methods

        private void ConfigureWindowSettings( ) {
            Name = "ConfigureBackBlazeTab";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }

        private void ConfigureWindowContent( B2Config config ) {
            AddWrapPanelOne( config.ApplicationKeyId );
            AddWrapPanelTwo( config.ApplicationKey );
            AddWrapPanelThree( config.BucketName );
            AddWrapPanelFour( config.BucketId );
            AddWrapPanelFive( config.MaxConsecutiveErrors );
            AddWrapPanelSix( config.ProcessThreads );
            AddSaveButton( );
        }

        #region ApplicationKeyId Methods

        private void AddWrapPanelOne( string applicationKeyId ) {
            WrapPanel wrap1 = new( ) {
                Margin = Thickness.Parse( "5,15,5,5" )
            };
            wrap1.Children.Add( _applicationKeyIdHeader );
            wrap1.Children.Add( _applicationKeyIdTxt );
            _applicationKeyIdTxt.Text = applicationKeyId;
            Children.Add( wrap1 );
        }

        #endregion ApplicationKeyId Methods


        #region ApplicationKey Methods

        private void AddWrapPanelTwo( string applicationKey ) {
            WrapPanel wrap2 = new( ) {
                Margin = Thickness.Parse( "5,15,5,5" )
            };
            wrap2.Children.Add( _applicationKeyHeader );
            wrap2.Children.Add( _applicationKeyTxt );
            _applicationKeyTxt.Text = applicationKey;
            Children.Add( wrap2 );
        }

        #endregion ApplicationKey Methods


        #region BucketName Methods

        private void AddWrapPanelThree( string bucketName ) {
            WrapPanel wrap3 = new( ) {
                Margin = Thickness.Parse( "5,15,5,5" )
            };
            wrap3.Children.Add( _bucketNameHeader );
            wrap3.Children.Add( _bucketNameTxt );
            _bucketNameTxt.Text = bucketName;
            Children.Add( wrap3 );
        }

        #endregion BucketName Methods


        #region BucketId Methods

        private void AddWrapPanelFour( string bucketId ) {
            WrapPanel wrap4 = new( ) {
                Margin = Thickness.Parse( "5,15,5,5" )
            };
            wrap4.Children.Add( _bucketIdHeader );
            wrap4.Children.Add( _bucketIdTxt );
            _bucketIdTxt.Text = bucketId;
            Children.Add( wrap4 );
        }

        #endregion BucketId Methods


        #region MaxConsecutiveErrors Methods

        private void AddWrapPanelFive( int maxConsecutiveErrors ) {
            WrapPanel wrap5 = new( ) {
                Margin = Thickness.Parse( "5,15,5,5" )
            };
            wrap5.Children.Add( _maxConsecutiveErrorsHeader );
            wrap5.Children.Add( _maxConsecutiveErrors );
            _maxConsecutiveErrors.Value = maxConsecutiveErrors;
            Children.Add( wrap5 );
        }

        #endregion MaxConsecutiveErrors Methods


        #region ProcessThreads Methods

        private void AddWrapPanelSix( int processThreads ) {
            WrapPanel wrap6 = new( ) {
                Margin = Thickness.Parse( "5,15,5,5" )
            };
            wrap6.Children.Add( _processThreadsHeader );
            wrap6.Children.Add( _processThreads );
            _processThreads.Value = processThreads;
            Children.Add( wrap6 );
        }

        #endregion ProcessThreads Methods


        #region SaveButton Methods

        private void AddSaveButton( ) {
            _saveButton.Click += SaveConfigHandler;
            Children.Add( _saveButton );
        }

        private B2Config GetB2Config( ) {
            B2Config config = new( );
            config.ApplicationKeyId = _applicationKeyIdTxt.Text;
            config.ApplicationKey = _applicationKeyTxt.Text;
            config.BucketName = _bucketNameTxt.Text;
            config.BucketId = _bucketIdTxt.Text;
            config.MaxConsecutiveErrors = (int)_maxConsecutiveErrors.Value;
            config.ProcessThreads = (int)_processThreads.Value;
            return config;
        }

        public async void SaveConfigHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            button.IsEnabled = false;
            try {
                _parentWindow.UpdateConfigSection( GetB2Config( ) );
                _parentWindow.SetTabContent( this );
                await Task.Delay( 250 );
            } catch (Exception ex) {
                await new MessageBox(
                    "Unable to save sync config.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                button.IsEnabled = true;
            }
        }

        #endregion SaveButton Methods

        #endregion Methods

    }

}
