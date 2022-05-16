using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureSyncTab : StackPanel, IConfigurationTab {

        public ConfigureSyncTab( SyncConfig config, ConfigureWindow parent ) {
            _parentWindow = parent;

            _syncFolderPath = new(
                "Sync Folder: ",
                config.SyncFolder,
                "Change Sync Folder",
                ChangeSyncFolderHandler,
                "Sync folder doesn't exist!"
            );
            _syncFolderPath.WarnOnMissingPath( config.SyncFolder, false );

            _workDirPath = new(
                "Working Directory: ",
                config.WorkingDirectory,
                "Change Working Directory",
                ChangeWorkDirHandler
            );

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

        private readonly PathStack _syncFolderPath;

        private readonly CheckBox _recurseSyncFolderCheckBox = new( ) {
            IsChecked = true,
            Content = "Recurse",
            Margin = Thickness.Parse( "5,0,5,5" )
        };

        private readonly PathStack _workDirPath;


        #region ExcludePath

        private readonly TextBlock _excludePathsHeader = new( ) {
            Text = "Excluded Paths:",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16,
            Margin = Thickness.Parse( "0,5,5,5" )
        };

        private readonly TextBox _excludePathsTxtBox = new( ) {
            Name = "ExcludePathsTxtBox",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.WrapWithOverflow,
            IsUndoEnabled = true,
            AcceptsTab = false,
            TextAlignment = TextAlignment.Left,
            Margin = Thickness.Parse( "5,0,5,5" ),
            Padding = Thickness.Parse( "5,5,5,5" ),
            NewLine = "\n"
        };

        #endregion ExcludePath


        #region Options CheckBoxes

        private readonly TextBlock _optionsHeader = new( ) {
            Text = "Options: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly CheckBox _compressBeforeUploadCheckBox = new( ) {
            IsChecked = false,
            Content = "Compress Before Upload",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _uniqueCompressionPasswordsCheckBox = new( ) {
            IsChecked = false,
            Content = "Unique Compression Passwords",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _encryptBeforeUploadCheckBox = new( ) {
            IsChecked = false,
            Content = "Encrypt Before Upload",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _obfuscateUploadedFileNamesCheckBox = new( ) {
            IsChecked = false,
            Content = "Obfuscate Uploaded FileNames",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        #endregion Options CheckBoxes


        #region EnabledFeatures

        private readonly TextBlock _enabledFeaturesHeader = new( ) {
            Text = "Enabled Features: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly CheckBox _log4NetEnabledFeatureCheckbox = new( ) {
            IsChecked = true,
            Content = "Log4Net",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _encryptionEnabledFeatureCheckbox = new( ) {
            IsChecked = true,
            Content = "Encryption",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _compressionEnabledFeatureCheckbox = new( ) {
            IsChecked = true,
            Content = "Compression",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _sqliteEnabledFeatureCheckbox = new( ) {
            IsChecked = true,
            Content = "Sqlite",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _backBlazeB2EnabledFeatureCheckbox = new( ) {
            IsChecked = true,
            Content = "BackBlazeB2",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _backupEnabledFeatureCheckbox = new( ) {
            IsChecked = true,
            Content = "Backup",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _restoreEnabledFeatureCheckbox = new( ) {
            IsChecked = true,
            Content = "Restore",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        private readonly CheckBox _postgresEnabledFeatureCheckbox = new( ) {
            IsChecked = true,
            Content = "Postgres",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        #endregion EnabledFeatures


        #region CloudProviders

        private readonly TextBlock _cloudProvidersHeader = new( ) {
            Text = "Enabled CloudProviders: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly CheckBox _backBlazeCloudProviderCheckbox = new( ) {
            IsChecked = true,
            Content = "BackBlazeB2",
            Margin = Thickness.Parse( "5,0,5,0" )
        };

        #endregion CloudProviders

        #endregion Fields


        #region Methods

        private void ConfigureWindowSettings( ) {
            Name = "ConfigureSyncTab";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }

        private void ConfigureWindowContent( SyncConfig config ) {
            _syncFolderPath.InnerWrap2.Children.Add( _recurseSyncFolderCheckBox );
            Children.Add( _syncFolderPath );
            AddExcludePathsSection( config.ExcludePaths );     // _excludePathsHeader, _excludePathsTxtBox
            Children.Add( _workDirPath );
            AddOptionsCheckBoxSection(                         // _compressBeforeUploadCheckBox, _uniqueCompressionPasswordsCheckBox, _encryptBeforeUploadCheckBox, _obfuscateUploadedFileNamesCheckBox
                config.CompressBeforeUpload,
                config.UniqueCompressionPasswords,
                config.EncryptBeforeUpload,
                config.ObfuscateUploadedFileNames
            );
            AddEnabledFeatures( config.EnabledFeatures );      // _enabledFeaturesHeader, _log4NetEnabledFeatureCheckbox, _encryptionEnabledFeatureCheckbox, _compressionEnabledFeatureCheckbox, _sqliteEnabledFeatureCheckbox, _backBlazeB2EnabledFeatureCheckbox, _backupEnabledFeatureCheckbox, _restoreEnabledFeatureCheckbox, _postgresEnabledFeatureCheckbox
            AddCloudProviders( config.EnabledCloudProviders ); // _cloudProvidersHeader, _backBlazeCloudProviderCheckbox
            AddSaveButton( );
        }


        #region ExcludePaths Methods

        private void AddExcludePathsSection( string[] excludedPaths ) {
            StackPanel stack = new( ) {
                Margin = Thickness.Parse( "5,15,5,15" )
            };

            SetExcludePathsTextBoxMsg( excludedPaths );

            stack.Children.Add( _excludePathsHeader );
            stack.Children.Add( _excludePathsTxtBox );
            Children.Add( stack );
        }

        private void SetExcludePathsTextBoxMsg( string[] excludedPaths ) {
            string excludePathsTxt = "";
            if (excludedPaths.Length > 0) {
                excludePathsTxt = string.Join( "\n", excludedPaths );
            }
            _excludePathsTxtBox.Text = excludePathsTxt;
        }

        #endregion ExcludePaths Methods


        #region Options CheckBoxes

        private void AddOptionsCheckBoxSection(
            bool compress,
            bool uniquePW,
            bool encrypt,
            bool obfuscate
        ) {
            StackPanel stack2 = new( ) {
                Margin = Thickness.Parse( "5,15,5,15" )
            };
            stack2.Children.Add( _optionsHeader );
            Grid grid = NewCheckboxGrid( );
            AddCompressBeforeUploadToGrid( grid, compress );
            AddUniqueCompressionPasswordsToGrid( grid, uniquePW );
            AddEncryptBeforeUploadToGrid( grid, encrypt );
            AddObfuscateUploadedFileNamesToGrid( grid, obfuscate );
            stack2.Children.Add( grid );
            Children.Add( stack2 );
        }

        private static Grid NewCheckboxGrid( ) => new( ) {
            Margin = Thickness.Parse( "5,0,5,0" ),
            RowDefinitions = new RowDefinitions( ) {
                new RowDefinition(){ Height = GridLength.Star },
                new RowDefinition(){ Height = GridLength.Star }
            },
            ColumnDefinitions = new ColumnDefinitions( ) {
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star }
            }
        };

        private void AddCompressBeforeUploadToGrid( Grid grid, bool compress ) {
            Grid.SetColumn( _compressBeforeUploadCheckBox, 0 );
            Grid.SetRow( _compressBeforeUploadCheckBox, 0 );
            _compressBeforeUploadCheckBox.IsChecked = compress;
            grid.Children.Add( _compressBeforeUploadCheckBox );
        }

        private void AddUniqueCompressionPasswordsToGrid( Grid grid, bool uniquePW ) {
            Grid.SetColumn( _uniqueCompressionPasswordsCheckBox, 1 );
            Grid.SetRow( _uniqueCompressionPasswordsCheckBox, 0 );
            _uniqueCompressionPasswordsCheckBox.IsChecked = uniquePW;
            grid.Children.Add( _uniqueCompressionPasswordsCheckBox );
        }

        private void AddEncryptBeforeUploadToGrid( Grid grid, bool encrypt ) {
            Grid.SetColumn( _encryptBeforeUploadCheckBox, 0 );
            Grid.SetRow( _encryptBeforeUploadCheckBox, 1 );
            _encryptBeforeUploadCheckBox.IsChecked = encrypt;
            grid.Children.Add( _encryptBeforeUploadCheckBox );
        }

        private void AddObfuscateUploadedFileNamesToGrid( Grid grid, bool obfuscate ) {
            Grid.SetColumn( _obfuscateUploadedFileNamesCheckBox, 1 );
            Grid.SetRow( _obfuscateUploadedFileNamesCheckBox, 1 );
            _obfuscateUploadedFileNamesCheckBox.IsChecked = obfuscate;
            grid.Children.Add( _obfuscateUploadedFileNamesCheckBox );
        }

        #endregion Options CheckBoxes


        #region EnabledFeatures Methods

        private void AddEnabledFeatures( Cloud_ShareSync_Features features ) {
            StackPanel stack3 = new( ) {
                Margin = Thickness.Parse( "5,15,5,15" )
            };
            stack3.Children.Add( _enabledFeaturesHeader );
            stack3.Children.Add( NewEnabledFeaturesCheckboxGrid( ) );
            SetEnabledFeaturesStatus( features );
            Children.Add( stack3 );
        }

        private static Grid NewEnabledFeaturesGrid( ) => new( ) {
            Margin = Thickness.Parse( "5,0,5,0" ),
            RowDefinitions = new RowDefinitions( ) {
                new RowDefinition(){ Height = GridLength.Star },
                new RowDefinition(){ Height = GridLength.Star }
            },
            ColumnDefinitions = new ColumnDefinitions( ) {
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star },
            }
        };

        private Grid NewEnabledFeaturesCheckboxGrid( ) {
            Grid grid = NewEnabledFeaturesGrid( );
            AddLog4NetEnabledFeatureCheckboxToGrid( grid );
            AddEncryptionEnabledFeatureCheckboxToGrid( grid );
            AddCompressionEnabledFeatureCheckboxToGrid( grid );
            AddSqliteEnabledFeatureCheckboxToGrid( grid );
            AddBackBlazeB2EnabledFeatureCheckboxToGrid( grid );
            AddBackupEnabledFeatureCheckboxToGrid( grid );
            AddRestoreEnabledFeatureCheckboxToGrid( grid );
            AddPostgresEnabledFeatureCheckboxToGrid( grid );
            return grid;
        }

        private void AddLog4NetEnabledFeatureCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _log4NetEnabledFeatureCheckbox, 0 );
            Grid.SetRow( _log4NetEnabledFeatureCheckbox, 1 );
            grid.Children.Add( _log4NetEnabledFeatureCheckbox );
        }

        private void AddEncryptionEnabledFeatureCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _encryptionEnabledFeatureCheckbox, 3 );
            Grid.SetRow( _encryptionEnabledFeatureCheckbox, 0 );
            grid.Children.Add( _encryptionEnabledFeatureCheckbox );
        }

        private void AddCompressionEnabledFeatureCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _compressionEnabledFeatureCheckbox, 2 );
            Grid.SetRow( _compressionEnabledFeatureCheckbox, 0 );
            grid.Children.Add( _compressionEnabledFeatureCheckbox );
        }

        private void AddSqliteEnabledFeatureCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _sqliteEnabledFeatureCheckbox, 2 );
            Grid.SetRow( _sqliteEnabledFeatureCheckbox, 1 );
            grid.Children.Add( _sqliteEnabledFeatureCheckbox );
        }

        private void AddBackBlazeB2EnabledFeatureCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _backBlazeB2EnabledFeatureCheckbox, 1 );
            Grid.SetRow( _backBlazeB2EnabledFeatureCheckbox, 1 );
            grid.Children.Add( _backBlazeB2EnabledFeatureCheckbox );
        }

        private void AddBackupEnabledFeatureCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _backupEnabledFeatureCheckbox, 0 );
            Grid.SetRow( _backupEnabledFeatureCheckbox, 0 );
            grid.Children.Add( _backupEnabledFeatureCheckbox );
        }

        private void AddRestoreEnabledFeatureCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _restoreEnabledFeatureCheckbox, 1 );
            Grid.SetRow( _restoreEnabledFeatureCheckbox, 0 );
            grid.Children.Add( _restoreEnabledFeatureCheckbox );
        }

        private void AddPostgresEnabledFeatureCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _postgresEnabledFeatureCheckbox, 3 );
            Grid.SetRow( _postgresEnabledFeatureCheckbox, 1 );
            grid.Children.Add( _postgresEnabledFeatureCheckbox );
        }

        private void SetEnabledFeaturesStatus( Cloud_ShareSync_Features features ) {
            _log4NetEnabledFeatureCheckbox.IsChecked = features.HasFlag( Cloud_ShareSync_Features.Log4Net );
            _encryptionEnabledFeatureCheckbox.IsChecked = features.HasFlag( Cloud_ShareSync_Features.Encryption );
            _compressionEnabledFeatureCheckbox.IsChecked = features.HasFlag( Cloud_ShareSync_Features.Compression );
            _sqliteEnabledFeatureCheckbox.IsChecked = features.HasFlag( Cloud_ShareSync_Features.Sqlite );
            _backBlazeB2EnabledFeatureCheckbox.IsChecked = features.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 );
            _backupEnabledFeatureCheckbox.IsChecked = features.HasFlag( Cloud_ShareSync_Features.Backup );
            _restoreEnabledFeatureCheckbox.IsChecked = features.HasFlag( Cloud_ShareSync_Features.Restore );
            _postgresEnabledFeatureCheckbox.IsChecked = features.HasFlag( Cloud_ShareSync_Features.Postgres );
        }

        #endregion EnabledFeatures Methods


        #region CloudProvider Methods

        private void AddCloudProviders( CloudProviders providers ) {
            StackPanel stack4 = new( ) {
                Margin = Thickness.Parse( "5,15,5,15" )
            };
            stack4.Children.Add( _cloudProvidersHeader );
            stack4.Children.Add( NewCloudProvidersCheckboxGrid( ) );
            SetCloudProviderStatus( providers );
            Children.Add( stack4 );
        }

        private Grid NewCloudProvidersCheckboxGrid( ) {
            Grid grid = new( ) {
                Margin = Thickness.Parse( "5,0,5,0" ),
                RowDefinitions = new RowDefinitions( ) { new RowDefinition( ) { Height = GridLength.Star } },
                ColumnDefinitions = new ColumnDefinitions( ) { new ColumnDefinition( ) { Width = GridLength.Star } }
            };
            AddBackBlazeB2CloudProviderCheckboxToGrid( grid );
            return grid;
        }

        private void AddBackBlazeB2CloudProviderCheckboxToGrid( Grid grid ) {
            Grid.SetColumn( _backBlazeCloudProviderCheckbox, 0 );
            Grid.SetRow( _backBlazeCloudProviderCheckbox, 0 );
            grid.Children.Add( _backBlazeCloudProviderCheckbox );
        }

        private void SetCloudProviderStatus( CloudProviders providers ) {
            _backBlazeCloudProviderCheckbox.IsChecked = providers.HasFlag( CloudProviders.BackBlazeB2 );
        }

        #endregion CloudProvider Methods


        #region SaveButton Methods

        private void AddSaveButton( ) {
            _saveButton.Click += SaveConfigHandler;
            Children.Add( _saveButton );
        }

        private SyncConfig GetSyncConfig( ) {
            SyncConfig config = new( );
            config.SyncFolder = _syncFolderPath.PathText;
            config.ExcludePaths = _excludePathsTxtBox.Text.Split( '\n' );
            config.WorkingDirectory = _workDirPath.PathText;
            config.Recurse = _recurseSyncFolderCheckBox.IsChecked ?? false;
            config.CompressBeforeUpload = _compressBeforeUploadCheckBox.IsChecked ?? false;
            config.UniqueCompressionPasswords = _uniqueCompressionPasswordsCheckBox.IsChecked ?? false;
            config.EncryptBeforeUpload = _encryptBeforeUploadCheckBox.IsChecked ?? false;
            config.ObfuscateUploadedFileNames = _obfuscateUploadedFileNamesCheckBox.IsChecked ?? false;
            config.EnabledFeatures = ConvertCheckBoxToFeatures( );
            config.EnabledCloudProviders = ConvertCheckBoxToProviders( );
            return config;
        }

        private CloudProviders ConvertCheckBoxToProviders( ) {
            CloudProviders providers = 0;
            if (_log4NetEnabledFeatureCheckbox.IsChecked == true) {
                providers |= CloudProviders.BackBlazeB2;
            };
            return providers;
        }

        private Cloud_ShareSync_Features ConvertCheckBoxToFeatures( ) {
            Cloud_ShareSync_Features features = 0;
            if (_log4NetEnabledFeatureCheckbox.IsChecked == true) {
                features |= Cloud_ShareSync_Features.Log4Net;
            };
            if (_encryptionEnabledFeatureCheckbox.IsChecked == true) {
                features |= Cloud_ShareSync_Features.Encryption;
            };
            if (_compressionEnabledFeatureCheckbox.IsChecked == true) {
                features |= Cloud_ShareSync_Features.Compression;
            };
            if (_sqliteEnabledFeatureCheckbox.IsChecked == true) {
                features |= Cloud_ShareSync_Features.Sqlite;
            };
            if (_backBlazeB2EnabledFeatureCheckbox.IsChecked == true) {
                features |= Cloud_ShareSync_Features.BackBlazeB2;
            };
            if (_backupEnabledFeatureCheckbox.IsChecked == true) {
                features |= Cloud_ShareSync_Features.Backup;
            };
            if (_restoreEnabledFeatureCheckbox.IsChecked == true) {
                features |= Cloud_ShareSync_Features.Restore;
            };
            if (_postgresEnabledFeatureCheckbox.IsChecked == true) {
                features |= Cloud_ShareSync_Features.Postgres;
            };
            return features;
        }

        #endregion SaveButton Methods


        #region Click Methods

        public async void ChangeSyncFolderHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                OpenFolderDialog dialog = new( ) { Title = "Select Sync Folder", };
                string? result = await dialog.ShowAsync( _parentWindow );
                if (result?.Length > 0) {
                    _syncFolderPath.PathText = result;
                    _syncFolderPath.WarnOnMissingPath( result, false );
                }
            } catch (Exception ex) {
                await new MessageBox(
                    "Failed to change sync folder path.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                button.IsEnabled = true;
            }
        }

        public async void ChangeWorkDirHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                OpenFolderDialog dialog = new( ) { Title = "Select Working Directory", };
                string? result = await dialog.ShowAsync( _parentWindow );
                if (result?.Length > 0) { _workDirPath.PathText = result; }
            } catch (Exception ex) {
                await new MessageBox(
                    "Failed to change working directory path.",
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
                _parentWindow.UpdateConfigSection( GetSyncConfig( ) );
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
