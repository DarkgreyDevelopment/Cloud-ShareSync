using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureDatabaseTab : StackPanel, IConfigurationTab {

        public ConfigureDatabaseTab( DatabaseConfig config, ConfigureWindow parent ) {
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


        #region Sqlite

        private readonly CheckBox _useSqliteCheckBox = new( ) {
            IsChecked = true,
            Content = "Use Sqlite",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly TextBlock _sqliteDBPathHeader = new( ) {
            Text = "Sqlite DB Path: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly TextBlock _sqliteDBPathTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16
        };

        private readonly Button _updateSqliteDBPathButton = new( ) {
            Name = "UpdateSqliteDBPath",
            Margin = Thickness.Parse( "5,0,5,15" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = true,
            Content = "Change Sqlite Database Path"
        };

        #endregion Sqlite


        #region Postgres

        private readonly CheckBox _usePostgresCheckBox = new( ) {
            IsChecked = false,
            Content = "Use Postgres",
            Margin = Thickness.Parse( "5,15,5,0" ),
            ClickMode = ClickMode.Press
        };

        private readonly TextBlock _postgresConnectionStringHeader = new( ) {
            Text = "Postgres Connection String: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly TextBox _postgresConnectionString = new( ) {
            Name = "PostgresConnectionString",
            Margin = Thickness.Parse( "5,5,5,5" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = false
        };

        #endregion Postgres

        #endregion Fields


        #region Methods

        private void ConfigureWindowSettings( ) {
            Name = "ConfigureDatabaseTab";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }

        private void ConfigureWindowContent( DatabaseConfig config ) {
            ConfigureSqliteSettings( config );
            ConfigurePostgresSettings( config );

            AntiLinkSqliteCheckboxAndPostgresCheckbox( );
            AddSaveButton( );
        }

        #region Sqlite Methods

        private void ConfigureSqliteSettings( DatabaseConfig config ) {
            _updateSqliteDBPathButton.Click += ChangeSqliteDBPathHandler;
            Children.Add( _useSqliteCheckBox );
            AddWrapPanelOne( config );
            Children.Add( _updateSqliteDBPathButton );
            LinkSqliteCheckboxAndButton( );
        }

        private void AddWrapPanelOne( DatabaseConfig config ) {
            WrapPanel wrap = new( ) {
                Margin = Thickness.Parse( "5,0,5,0" )
            };
            _useSqliteCheckBox.IsChecked = config.UseSqlite;
            _sqliteDBPathTxt.Text = config.SqliteDBPath;
            wrap.Children.Add( _sqliteDBPathHeader );
            wrap.Children.Add( _sqliteDBPathTxt );
            Children.Add( wrap );
        }

        private void LinkSqliteCheckboxAndButton( ) {
            IObservable<bool?> sqliteCheckboxStatus = _useSqliteCheckBox.GetObservable( CheckBox.IsCheckedProperty );
            _ = sqliteCheckboxStatus.Subscribe( value => _updateSqliteDBPathButton.IsEnabled = value ?? false );
        }

        #endregion Sqlite Methods


        #region Postgres Methods

        private void ConfigurePostgresSettings( DatabaseConfig config ) {
            Children.Add( _usePostgresCheckBox );
            AddWrapPanelTwo( config );
            LinkPostgresCheckboxAndTextbox( );
        }

        private void AddWrapPanelTwo( DatabaseConfig config ) {
            WrapPanel wrap2 = new( ) {
                Margin = Thickness.Parse( "5,0,5,15" )
            };

            _usePostgresCheckBox.IsChecked = config.UsePostgres;
            _postgresConnectionString.Text = config.PostgresConnectionString;

            wrap2.Children.Add( _postgresConnectionStringHeader );
            wrap2.Children.Add( _postgresConnectionString );

            Children.Add( wrap2 );
        }

        private void LinkPostgresCheckboxAndTextbox( ) {
            IObservable<bool?> postgresCheckboxStatus = _usePostgresCheckBox.GetObservable( CheckBox.IsCheckedProperty );
            _ = postgresCheckboxStatus.Subscribe( value => _postgresConnectionString.IsEnabled = value ?? false );
        }

        #endregion Postgres Methods

        private void AntiLinkSqliteCheckboxAndPostgresCheckbox( ) {
            IObservable<bool?> sqliteCheckboxStatus = _useSqliteCheckBox.GetObservable( CheckBox.IsCheckedProperty );
            IObservable<bool?> postgresCheckboxStatus = _usePostgresCheckBox.GetObservable( CheckBox.IsCheckedProperty );

            _ = sqliteCheckboxStatus.Subscribe( value => _usePostgresCheckBox.IsChecked = (value ?? false) == false );
            _ = postgresCheckboxStatus.Subscribe( value => _useSqliteCheckBox.IsChecked = (value ?? false) == false );
        }

        #region SaveButton Methods

        private void AddSaveButton( ) {
            _saveButton.Click += SaveConfigHandler;
            Children.Add( _saveButton );
        }

        private DatabaseConfig GetDatabaseConfig( ) {
            DatabaseConfig config = new( );
            config.UseSqlite = _useSqliteCheckBox.IsChecked ?? false;
            config.SqliteDBPath = _sqliteDBPathTxt.Text;
            config.UsePostgres = _usePostgresCheckBox.IsChecked ?? false;
            config.PostgresConnectionString = _postgresConnectionString.Text;
            return config;
        }

        #endregion SaveButton Methods


        #region Click Methods

        public async void ChangeSqliteDBPathHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                OpenFolderDialog dialog = new( ) { Title = "Select Sqlite Database Folder", };
                string? result = await dialog.ShowAsync( _parentWindow );
                if (result?.Length > 0) { _sqliteDBPathTxt.Text = result; }
            } catch (Exception ex) {
                await new MessageBox(
                    "Failed to change sqlite db folder path.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                button.IsEnabled = true;
            }
        }

        public async void SaveConfigHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            button.IsEnabled = false;
            try {
                _parentWindow.UpdateConfigSection( GetDatabaseConfig( ) );
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

        #endregion Click Methods

        #endregion Methods

    }

}

