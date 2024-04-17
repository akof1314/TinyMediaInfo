using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Selection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using TinyMediaInfo.FFMpeg;
using TinyMediaInfo.Messenger;
using TinyMediaInfo.Models;

namespace TinyMediaInfo.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// 显示处理的进度条
        /// </summary>
        [ObservableProperty]
        private bool _showProgressBar;

        /// <summary>
        /// 进度条的值
        /// </summary>
        [ObservableProperty]
        private int _valueProgressBar;

        /// <summary>
        /// 标记当前正在运行处理线程
        /// </summary>
        private bool _runningParse;

        /// <summary>
        /// 处理的文件总数量
        /// </summary>
        private int _processCount;

        /// <summary>
        /// 处理文件的列表
        /// </summary>
        private readonly ConcurrentQueue<int> _processIndexList;

        /// <summary>
        /// 处理详情文件的列表
        /// </summary>
        private readonly ConcurrentQueue<int> _processDetailIndexList;

        /// <summary>
        /// 源数据
        /// </summary>
        private readonly List<MediaViewModel> _sourceMediaList;

        /// <summary>
        /// 显示的文件列表
        /// </summary>
        private ObservableCollection<MediaViewModel> _viewMediaList;

        /// <summary>
        /// 表格的数据源
        /// </summary>
        public FlatTreeDataGridSource<MediaViewModel> Source { get; }

        /// <summary>
        /// 筛选框
        /// </summary>
        public MediaFilterViewModel FilterViewModel { get; }

        /// <summary>
        /// 各个表头分组信息
        /// </summary>
        private readonly MediaCheckboxFilterColumnsModel _groupModel;

        /// <summary>
        /// 显示详情
        /// </summary>
        [ObservableProperty]
        private bool _showDetailPanel;

        /// <summary>
        /// 详情
        /// </summary>
        public HierarchicalTreeDataGridSource<MediaDetailModel> DetailSource { get; }

        /// <summary>
        /// 详情显示的列表
        /// </summary>
        private readonly ObservableCollection<MediaDetailModel> _detailViewList;

        public MainWindowViewModel()
        {
            _processIndexList = new ConcurrentQueue<int>();
            _processDetailIndexList = new ConcurrentQueue<int>();
            _groupModel = new MediaCheckboxFilterColumnsModel();
            FilterViewModel = new MediaFilterViewModel(this);

            _sourceMediaList = new List<MediaViewModel>();
            _viewMediaList = new ObservableCollection<MediaViewModel>();

            Source = new FlatTreeDataGridSource<MediaViewModel>(_viewMediaList)
            {
                Columns =
                {
                    new MediaColumn<string>("Local.GridFilePath", x=>x.FilePath, 100, x=>x.FilePath, null, "FilePath"),
                    new MediaColumn<string>("Local.GridFileName", x=>x.FileName, 120, x=>x.FileName, null, "FileName"),
                    new MediaColumn<string>("Local.GridFormatName", x=>x.FormatName, 95, x=>x.FormatName, _groupModel.FormatName, "FormatName"),
                    new MediaColumn<long>("Local.GridNbStreams", x=>x.NbStreams, 62, x=>x.NbStreamsLong, _groupModel.NbStreams, "NbStreams"),
                    new MediaColumn<long>("Local.GridSize", x=>x.Size, 90, x=>x.SizeLong, null, "Size"),
                    new MediaColumn<long>("Local.GridDuration", x=>x.Duration, 90, x=>x.DurationLong, null, "Duration"),
                    new MediaColumn<long>("Local.GridBitRate", x=>x.BitRate, 62, x=>x.BitRateLong, null, "BitRate"),
                    new MediaColumn<string>("Local.GridVideoCode", x=>x.VideoCode, 105, x=>x.VideoCode, _groupModel.VideoCode, "VideoCode"),
                    new MediaColumn<string>("Local.GridVideoCodeTag", x=>x.VideoCodeTag, 105, x=>x.VideoCodeTag, _groupModel.VideoCodeTag, "VideoCodeTag"),
                    new MediaColumn<long>("Local.GridVideoWidth", x=>x.VideoWidth, 60, x=>x.VideoWidthLong, _groupModel.VideoWidth, "VideoWidth"),
                    new MediaColumn<long>("Local.GridVideoHeight", x=>x.VideoHeight, 60, x=>x.VideoHeightLong, _groupModel.VideoHeight, "VideoHeight"),
                    new MediaColumn<string>("Local.GridDisplayAspectRatio", x=>x.DisplayAspectRatio, 90, x=>x.DisplayAspectRatio, _groupModel.DisplayAspectRatio, "DisplayAspectRatio"),
                    new MediaColumn<long>("Local.GridVideoBitRate", x=>x.VideoBitRate, 93, x=>x.VideoBitRateLong, null, "VideoBitRate"),
                    new MediaColumn<string>("Local.GridVideoFrameRate", x=>x.VideoFrameRate, 65, x=>x.VideoFrameRate, _groupModel.VideoFrameRate, "VideoFrameRate"),
                    new MediaColumn<long>("Local.GridVideoNbFrames", x=>x.VideoNbFrames, 65, x=>x.VideoNbFramesLong, null, "VideoNbFrames"),
                    new MediaColumn<string>("Local.GridAudioCode", x=>x.AudioCode, 105, x=>x.AudioCode, _groupModel.AudioCode, "AudioCode"),
                    new MediaColumn<string>("Local.GridAudioCodeTag", x=>x.AudioCodeTag, 105, x=>x.AudioCodeTag, _groupModel.AudioCodeTag, "AudioCodeTag"),
                    new MediaColumn<long>("Local.GridAudioSampleRate", x=>x.AudioSampleRate, 75, x=>x.AudioSampleRateLong, _groupModel.AudioSampleRate, "AudioSampleRate"),
                    new MediaColumn<long>("Local.GridAudioBitRate", x=>x.AudioBitRate, 93, x=>x.AudioBitRateLong, null, "AudioBitRate"),
                    new MediaColumn<long>("Local.GridAudioChannels", x=>x.AudioChannels, 80, x=>x.AudioChannelsLong, _groupModel.AudioChannels, "AudioChannels"),
                },
            };

            if (CultureInfo.CurrentCulture.Name != "zh-CN")
            {
                Source.Columns.SetColumnWidth(3, new GridLength(82));
                Source.Columns.SetColumnWidth(6, new GridLength(79));
                Source.Columns.SetColumnWidth(9, new GridLength(75));
                Source.Columns.SetColumnWidth(10, new GridLength(75));
                Source.Columns.SetColumnWidth(11, new GridLength(110));
                Source.Columns.SetColumnWidth(13, new GridLength(80));
                Source.Columns.SetColumnWidth(14, new GridLength(80));
                Source.Columns.SetColumnWidth(17, new GridLength(87));
                Source.Columns.SetColumnWidth(19, new GridLength(90));
            }

            _detailViewList = new ObservableCollection<MediaDetailModel>();
            DetailSource = new HierarchicalTreeDataGridSource<MediaDetailModel>(_detailViewList)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<MediaDetailModel>(
                        new TextColumn<MediaDetailModel, string>("", x => x.Desc, new GridLength(480)),
                        x => x.Children)
                }
            };

            Source.RowSelection!.SelectionChanged += OnRowSelectionChanged;
        }

        [RelayCommand]
        private void ShowAbout()
        {
            try
            {
                var url = "https://github.com/akof1314/TinyMediaInfo";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = url
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("x-www-browser", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void AddMediaFile(string fileName, string filePath)
        {
            _sourceMediaList.Add(new MediaViewModel()
            {
                FileName = fileName,
                FilePath = filePath
            });
            _viewMediaList.Add(_sourceMediaList[^1]);

            _processIndexList.Enqueue(_viewMediaList.Count - 1);
            _processCount++;
        }

        public void ClearMediaFiles()
        {
            ShowDetailPanel = false;
            _processCount = 0;
            _processIndexList.Clear();
            _processDetailIndexList.Clear();
            _sourceMediaList.Clear();
            _viewMediaList.Clear();

            foreach (var sourceColumn in Source.Columns)
            {
                if (sourceColumn is IMediaFilterColumn filterColumn)
                {
                    if (filterColumn.CheckboxFilter != null)
                    {
                        filterColumn.CheckboxFilter.Clear();
                    }
                }
            }
        }

        public void ParseMediaFiles()
        {
            if (!_runningParse && FFmpegHelper.IsEnabled() && (_processIndexList.Count > 0 || _processDetailIndexList.Count > 0))
            {
                Task.Run(ThreadParseMedia);
            }
        }

        private void ThreadParseMedia()
        {
            if (_processIndexList.Count > 0)
            {
                ValueProgressBar = 0;
                ShowProgressBar = true;
            }

            try
            {
                FilterViewModel.EnableFilterView = false;
                _runningParse = true;
                while (_processIndexList.Count > 0 && _processIndexList.TryDequeue(out var itemIndex))
                {
                    if (itemIndex < _viewMediaList.Count)
                    {
                        FFmpegHelper.ParseMedia(_viewMediaList[itemIndex], _groupModel);
                        int itemIndex2 = itemIndex;
                        Dispatcher.UIThread.Post(() =>
                        {
                            WeakReferenceMessenger.Default.Send(new MediaRowMessage(itemIndex2, true));

                            if (_processIndexList.Count == 0)
                            {
                                _processCount = 0;
                                ShowProgressBar = false;
                            }
                            else
                            {
                                ValueProgressBar = (int)((_processCount - _processIndexList.Count) * 1f / _processCount * 100);
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            _runningParse = false;
            FilterViewModel.EnableFilterView = true;
            FilterViewModel.RefreshFlyoutColumnCheckbox();

            // 详情
            try
            {
                _runningParse = true;
                while (_processDetailIndexList.Count > 0 && _processDetailIndexList.TryDequeue(out var itemIndex))
                {
                    if (itemIndex < _viewMediaList.Count)
                    {
                        FFmpegHelper.ParseDetailMedia(_viewMediaList[itemIndex]);
                        int itemIndex2 = itemIndex;
                        Dispatcher.UIThread.Post(() =>
                        {
                            WeakReferenceMessenger.Default.Send(new MediaDetailMessage(itemIndex2));
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            _runningParse = false;
        }

        public void SortMediaFilesBy(int columnIndex, ListSortDirection direction)
        {
            if (columnIndex >= 0 && columnIndex < Source.Columns.Count)
            {
                SortMediaFilesBy(Source.Columns[columnIndex], direction);
            }
        }

        private void SortMediaFilesBy(IColumn? column, ListSortDirection direction)
        {
            List<MediaViewModel>? orderList = null;
            if (column is MediaColumn<long> mediaColumn)
            {
                orderList = (direction == ListSortDirection.Ascending ?
                    _sourceMediaList.OrderBy(mediaColumn.SortSelector) :
                    _sourceMediaList.OrderByDescending(mediaColumn.SortSelector)).ToList();

            }
            else if (column is MediaColumn<string> mediaColumn2)
            {
                orderList = (direction == ListSortDirection.Ascending ?
                    _sourceMediaList.OrderBy(mediaColumn2.SortSelector) :
                    _sourceMediaList.OrderByDescending(mediaColumn2.SortSelector)).ToList();
            }

            if (orderList != null)
            {
                _sourceMediaList.Clear();
                _sourceMediaList.AddRange(orderList);
            }

            ApplyFilters();
        }

        public void ApplyFilters()
        {
            var viewMediaList = new ObservableCollection<MediaViewModel>();

            List<Func<MediaViewModel, bool>> filterFuncList = new List<Func<MediaViewModel, bool>>();
            foreach (var sourceColumn in Source.Columns)
            {
                bool isShowFilterIcon = false;
                if (sourceColumn is IMediaFilterColumn filterColumn)
                {
                    if (filterColumn.TabIndexFilter == 0)
                    {
                        if (filterColumn.CheckboxFilter != null)
                        {
                            var baseColumn = (ColumnBase<MediaViewModel, string>)filterColumn;
                            foreach (var checkboxFilterItemModel in filterColumn.CheckboxFilter.Filters)
                            {
                                if (!checkboxFilterItemModel.IsChecked)
                                {
                                    filterFuncList.Add(model => baseColumn.ValueSelector(model) == checkboxFilterItemModel.FilterName);
                                    isShowFilterIcon = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!filterColumn.AdvancedFilter.IsAllEmpty())
                        {
                            if (filterColumn is MediaColumn<long> longColumn)
                            {
                                if (longColumn.ExcelKey == "Size")
                                {
                                    filterFuncList.Add(model => !filterColumn.AdvancedFilter.IsByteFilter(longColumn.SortSelector(model)));
                                }
                                else
                                {
                                    filterFuncList.Add(model => !filterColumn.AdvancedFilter.IsLongFilter(longColumn.SortSelector(model)));
                                }
                                isShowFilterIcon = true;
                            }
                            else if (filterColumn is MediaColumn<string> strColumn)
                            {
                                filterFuncList.Add(model => !filterColumn.AdvancedFilter.IsStrFilter(strColumn.SortSelector(model)));
                                isShowFilterIcon = true;
                            }
                        }
                    }
                }

                sourceColumn.SortDirection = isShowFilterIcon ? ListSortDirection.Descending : null;
            }

            foreach (var viewModel in _sourceMediaList)
            {
                // 是否应当被过滤不显示
                bool isFiltered = false;
                foreach (var func in filterFuncList)
                {
                    // 过滤条件达到则隐藏
                    if (func(viewModel))
                    {
                        isFiltered = true;
                        break;
                    }
                }

                if (!isFiltered)
                {
                    viewMediaList.Add(viewModel);
                }
            }

            _viewMediaList = viewMediaList;
            Source.Items = _viewMediaList;
        }

        public void CancelFilters()
        {
            foreach (var sourceColumn in Source.Columns)
            {
                if (sourceColumn is IMediaFilterColumn filterColumn)
                {
                    if (filterColumn.TabIndexFilter == 0)
                    {
                        if (filterColumn.CheckboxFilter != null)
                        {
                            foreach (var checkboxFilterItemModel in filterColumn.CheckboxFilter.Filters)
                            {
                                checkboxFilterItemModel.FilterChecked = true;
                                checkboxFilterItemModel.IsChecked = true;
                            }
                        }
                    }
                    else
                    {
                        filterColumn.AdvancedFilter.FilterValue = String.Empty;
                        filterColumn.AdvancedFilter.FilterValue2 = String.Empty;
                    }
                }
            }
            ApplyFilters();
        }

        public void ExportExcel(string path)
        {
            try
            {
                var config = new OpenXmlConfiguration
                {
                    DynamicColumns = new DynamicExcelColumn[Source.Columns.Count]
                };

                for (int i = 0; i < Source.Columns.Count; i++)
                {
                    var column = Source.Columns[i];
                    string title = (column.Header != null) ? (string)column.Header : string.Empty;

                    if (column is IMediaFilterColumn filterColumn)
                    {
                        config.DynamicColumns[i] = new DynamicExcelColumn(filterColumn.ExcelKey)
                        {
                            Name = title,
                            Width = column.ActualWidth / 7
                        };
                    }
                }

                MiniExcel.SaveAs(path, _viewMediaList, configuration: config, excelType: ExcelType.XLSX);
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = path
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(
                    App.GetLocalizedString("Local.ErrorTitle"),
                    e.Message, NotificationType.Error, TimeSpan.Zero)));
            }
        }

        private void OnRowSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<MediaViewModel> e)
        {
            ShowDetailPanel = false;
        }

        public void DetailMediaFile()
        {
            ShowDetailPanel = false;
            ShowDetailPanel = true;
            _detailViewList.Clear();

            var viewModel = Source.RowSelection!.SelectedItem;
            if (viewModel != null)
            {
                if (viewModel.DetailModel == null)
                {
                    _processDetailIndexList.Enqueue(Source.RowSelection.SelectedIndex[0]);
                    ParseMediaFiles();
                }
                else
                {
                    ShowDetailModel(viewModel.DetailModel);
                }
            }
        }

        public void DetailIndexMediaFile(int itemIndex)
        {
            var selectedIndex = Source.RowSelection!.SelectedIndex;
            if (selectedIndex != IndexPath.Unselected && selectedIndex[0] == itemIndex && _detailViewList.Count == 0)
            {
                var viewModel = Source.RowSelection!.SelectedItem;
                if (viewModel != null)
                {
                    if (viewModel.DetailModel != null)
                    {
                        ShowDetailModel(viewModel.DetailModel);
                    }
                }
            }
        }

        private void ShowDetailModel(MediaDetailModel detailModel)
        {
            _detailViewList.Clear();
            _detailViewList.Add(detailModel);
            DetailSource.Expand(0);
            for (int i = 0; i < detailModel.Children?.Count; i++)
            {
                DetailSource.Expand(new IndexPath(0, i));
            }
        }

        public void OpenMediaFile()
        {
            var viewModel = Source.RowSelection!.SelectedItem;
            if (viewModel != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = viewModel.FilePath
                });
            }
        }

        public void LocateMediaFile()
        {
            var viewModel = Source.RowSelection!.SelectedItem;
            if (viewModel != null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = "explorer.exe",
                        Arguments = "/select, \"" + viewModel.FilePath + "\""
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // todo
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = viewModel.FilePath
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", "-R \"" + viewModel.FilePath + "\"");
                }
            }
        }
    }
}