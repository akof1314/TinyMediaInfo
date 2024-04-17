using System;
using System.Diagnostics;
using Avalonia.Controls;
using System.IO;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using TinyMediaInfo.ViewModels;
using TinyMediaInfo.Messenger;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Models.TreeDataGrid;

namespace TinyMediaInfo
{
    public partial class MainWindow : Window
    {
        private INotificationManager? _notificationManager;
        private readonly TreeDataGrid _treeDataGrid;
        private FlyoutBase? _flyout;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            _treeDataGrid = this.Get<TreeDataGrid>("MyTree");

            WeakReferenceMessenger.Default.Register<NotificationMessage>(this, ShowNotification);
            WeakReferenceMessenger.Default.Register<MediaRowMessage>(this, RefreshRowItem);
            WeakReferenceMessenger.Default.Register<MediaDetailMessage>(this, RefreshDetailItem);
            WeakReferenceMessenger.Default.Register<CloseFlyoutMessage>(this, CloseFlyoutPopup);
            AddHandler(DragDrop.DropEvent, Drop);
            DragDrop.DragOverEvent.AddClassHandler<TreeDataGrid>(OnTreeDragOver);
            AddHandler(TreeDataGridColumnHeader.ClickEvent, OnHeaderClick);

            DoubleTappedEvent.AddClassHandler<TreeDataGridRow>(OnDoubleTapped);
        }

        private void OnHeaderClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is TreeDataGridColumnHeader columnHeader && DataContext is MainWindowViewModel model &&
                columnHeader.ColumnIndex >= 0 && columnHeader.ColumnIndex < model.Source.Columns.Count)
            {
                if (columnHeader.Parent != null && columnHeader.Parent.TemplatedParent is Control flyoutOwner)
                {
                    var flyout = FlyoutBase.GetAttachedFlyout(flyoutOwner);
                    if (flyout != null)
                    {
                        if (flyout is PopupFlyoutBase popupFlyout)
                        {
                            popupFlyout.HorizontalOffset =
                                model.Source.Columns[columnHeader.ColumnIndex].ActualWidth / 2;
                        }
                        model.FilterViewModel.SetFlyoutColumnIndex(columnHeader.ColumnIndex);
                        if (_flyout == null)
                        {
                            flyout.Closed += FlyoutOnClosed;
                        }
                        _flyout = flyout;
                        flyout.ShowAt(columnHeader);
                    }
                }
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _notificationManager ??= new WindowNotificationManager(GetTopLevel(this));
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (_treeDataGrid.ColumnHeadersPresenter != null)
            {
                _treeDataGrid.ColumnHeadersPresenter.Height = 50;
                _treeDataGrid.ColumnHeadersPresenter.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            }
        }

        private void FlyoutOnClosed(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel model)
            {
                model.FilterViewModel.SetFlyoutColumnIndex(-1);
            }
        }

        private async void OpenFileDialog_OnClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null)
            {
                return;
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = App.GetLocalizedString("Local.OpenFileTitle"),
                AllowMultiple = true
            });

            if (files.Count >= 1 && DataContext is MainWindowViewModel model)
            {
                foreach (var storageFile in files)
                {
                    model.AddMediaFile(storageFile.Name, storageFile.Path.LocalPath);
                }
                model.ParseMediaFiles();
            }
        }

        private async void OpenFolderDialog_OnClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null)
            {
                return;
            }

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = App.GetLocalizedString("Local.OpenFolderTitle"),
                AllowMultiple = true
            });

            if (folders.Count >= 1 && DataContext is MainWindowViewModel model)
            {
                foreach (var folder in folders)
                {
                    foreach (var enumerateFile in Directory.EnumerateFiles(folder.Path.LocalPath, "*", SearchOption.AllDirectories))
                    {
                        if (!IsUnsupportedFileFormat(enumerateFile))
                        {
                            model.AddMediaFile(Path.GetFileName(enumerateFile), enumerateFile);
                        }
                    }
                }

                model.ParseMediaFiles();
            }
        }

        private void ClearItems_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel model)
            {
                model.ClearMediaFiles();
            }
        }

        private void CancelFilter_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel model)
            {
                model.CancelFilters();
            }
        }

        private async void ExportExcel_OnClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null)
            {
                return;
            }

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = App.GetLocalizedString("Local.SaveExcelTitle"),
                SuggestedFileName = "MediaInfo",
                DefaultExtension = "xlsx",
                ShowOverwritePrompt = true,
                FileTypeChoices = new[]{new FilePickerFileType("Excel document")
                {
                    Patterns = new[] { "*.xlsx" },
                    AppleUniformTypeIdentifiers = new[] { "org.openxmlformats.spreadsheetml.sheet" },
                    MimeTypes = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
                }}
            });

            if (file != null && DataContext is MainWindowViewModel model)
            {
                string path = file.Path.LocalPath;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                model.ExportExcel(path);
            }
        }

        private void OnTreeDragOver(TreeDataGrid arg1, DragEventArgs arg2)
        {
            arg2.DragEffects = DragDropEffects.Link;
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files) && DataContext is MainWindowViewModel model)
            {
                var files = e.Data.GetFiles() ?? Array.Empty<IStorageItem>();

                foreach (var storageFile in files)
                {
                    if (storageFile is IStorageFile)
                    {
                        model.AddMediaFile(storageFile.Name, storageFile.Path.LocalPath);
                    }
                    else if (storageFile is IStorageFolder folder)
                    {
                        foreach (var enumerateFile in Directory.EnumerateFiles(folder.Path.LocalPath, "*", SearchOption.AllDirectories))
                        {
                            if (!IsUnsupportedFileFormat(enumerateFile))
                            {
                                model.AddMediaFile(Path.GetFileName(enumerateFile), enumerateFile);
                            }
                        }
                    }
                }
                model.ParseMediaFiles();
            }
        }

        private bool IsUnsupportedFileFormat(string filePath)
        {
            if (Path.GetExtension(filePath) == ".meta")
            {
                return true;
            }
            return false;
        }

        private void ShowNotification(object recipient, NotificationMessage message)
        {
            _notificationManager?.Show(message.Notification);
        }

        private void RefreshRowItem(object recipient, MediaRowMessage message)
        {
            if (DataContext is MainWindowViewModel model)
            {
                var row = _treeDataGrid.TryGetRow(message.ItemIndex);
                if (row != null)
                {
                    int startIndex = 2;
                    int endIndex = message.IsSuccess ? model.Source.Columns.Count : 3;
                    var rows = model.Source.Rows;

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        var b = row.TryGetCell(i);
                        if (b is TreeDataGridTextCell c)
                        {
                            var baseColumn = (ColumnBase<MediaViewModel, string>)model.Source.Columns[i];
                            if (rows[message.ItemIndex].Model is MediaViewModel itemModel)
                            {
                                c.Value = baseColumn.ValueSelector(itemModel);
                            }
                        }
                    }
                }
            }
        }

        private void RefreshDetailItem(object recipient, MediaDetailMessage message)
        {
            if (DataContext is MainWindowViewModel model)
            {
                model.DetailIndexMediaFile(message.ItemIndex);
            }
        }

        private void CloseFlyoutPopup(object recipient, CloseFlyoutMessage message)
        {
            _flyout?.Hide();
        }

        private void ItemDetailFile_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel model)
            {
                model.DetailMediaFile();
            }
        }

        private void ItemOpenFile_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel model)
            {
                model.OpenMediaFile();
            }
        }

        private void ItemLocateFile_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel model)
            {
                model.LocateMediaFile();
            }
        }

        private void OnDoubleTapped(TreeDataGridRow arg1,  TappedEventArgs e)
        {
            if (DataContext is MainWindowViewModel model && arg1.DataContext is MediaViewModel)
            {
                model.DetailMediaFile();
            }
        }
    }
}