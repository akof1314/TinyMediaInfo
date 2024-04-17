using System;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TinyMediaInfo.Messenger;
using TinyMediaInfo.Models;

namespace TinyMediaInfo.ViewModels;

public partial class MediaFilterViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    /// <summary>
    /// 点击表头的列索引
    /// </summary>
    private int _flyoutColumnIndex = -1;

    [ObservableProperty]
    private bool _enableFilterView = true;

    [ObservableProperty]
    private int _showFilterTabIndex;

    [ObservableProperty]
    private bool _showFilterByItems;

    /// <summary>
    /// 筛选框数据源
    /// </summary>
    public ObservableCollection<MediaCheckboxFilterItemModel> CheckboxFilters { get; }

    /// <summary>
    /// 按条件
    /// </summary>
    public MediaAdvancedFilterColumnModel AdvancedFilter { get; }

    public MediaFilterViewModel()
    {
        _mainWindowViewModel = new MainWindowViewModel();
        CheckboxFilters = new ObservableCollection<MediaCheckboxFilterItemModel>();
        AdvancedFilter = new MediaAdvancedFilterColumnModel();
    }

    public MediaFilterViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        CheckboxFilters = new ObservableCollection<MediaCheckboxFilterItemModel>();
        AdvancedFilter = new MediaAdvancedFilterColumnModel();
    }

    public void SetFlyoutColumnIndex(int columnIndex)
    {
        _flyoutColumnIndex = columnIndex;
        CheckboxFilters.Clear();
        ShowFilterByItems = false;
        ShowFilterTabIndex = 1;

        if (_flyoutColumnIndex >= 0 && _flyoutColumnIndex < _mainWindowViewModel.Source.Columns.Count)
        {
            if (_mainWindowViewModel.Source.Columns[_flyoutColumnIndex] is IMediaFilterColumn filterColumn)
            {
                if (filterColumn.CheckboxFilter != null)
                {
                    ShowFilterByItems = true;
                    foreach (var filter in filterColumn.CheckboxFilter.Filters)
                    {
                        filter.IsChecked = filter.FilterChecked;
                        CheckboxFilters.Add(filter);
                    }
                }
                else
                {
                    filterColumn.TabIndexFilter = 1;
                }
                AdvancedFilter.CopyFrom(filterColumn.AdvancedFilter);
                ShowFilterTabIndex = filterColumn.TabIndexFilter;
            }
        }
    }

    public void RefreshFlyoutColumnCheckbox()
    {
        CheckboxFilters.Clear();
        if (_flyoutColumnIndex >= 0 && _flyoutColumnIndex < _mainWindowViewModel.Source.Columns.Count)
        {
            if (_mainWindowViewModel.Source.Columns[_flyoutColumnIndex] is IMediaFilterColumn filterColumn)
            {
                if (filterColumn.CheckboxFilter != null)
                {
                    foreach (var filter in filterColumn.CheckboxFilter.Filters)
                    {
                        filter.IsChecked = filter.FilterChecked;
                        CheckboxFilters.Add(filter);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 升序按钮
    /// </summary>
    [RelayCommand]
    private void SortMediaFilesAscending()
    {
        _mainWindowViewModel.SortMediaFilesBy(_flyoutColumnIndex, ListSortDirection.Ascending);
        WeakReferenceMessenger.Default.Send(new CloseFlyoutMessage());
    }

    /// <summary>
    /// 降序按钮
    /// </summary>
    [RelayCommand]
    private void SortMediaFilesDescending()
    {
        _mainWindowViewModel.SortMediaFilesBy(_flyoutColumnIndex, ListSortDirection.Descending);
        WeakReferenceMessenger.Default.Send(new CloseFlyoutMessage());
    }

    /// <summary>
    /// 全选按钮
    /// </summary>
    [RelayCommand]
    private void SelectAllFilterItems()
    {
        foreach (var filter in CheckboxFilters)
        {
            filter.IsChecked = true;
        }
    }

    /// <summary>
    /// 反选按钮
    /// </summary>
    [RelayCommand]
    private void InvertFilterItems()
    {
        foreach (var filter in CheckboxFilters)
        {
            filter.IsChecked = !filter.IsChecked;
        }
    }

    /// <summary>
    /// 重置按钮
    /// </summary>
    [RelayCommand]
    private void ResetAdvancedFilter()
    {
        AdvancedFilter.FilterValue = String.Empty;
        AdvancedFilter.FilterValue2 = String.Empty;
    }

    /// <summary>
    /// 确定按钮
    /// </summary>
    [RelayCommand]
    private void StartFilterItems()
    {
        if (_flyoutColumnIndex < _mainWindowViewModel.Source.Columns.Count)
        {
            if (_mainWindowViewModel.Source.Columns[_flyoutColumnIndex] is IMediaFilterColumn filterColumn)
            {
                filterColumn.TabIndexFilter = ShowFilterTabIndex;
                if (ShowFilterTabIndex == 0)
                {
                    foreach (var filter in CheckboxFilters)
                    {
                        filter.FilterChecked = filter.IsChecked;
                    }
                    filterColumn.AdvancedFilter.FilterValue = String.Empty;
                    filterColumn.AdvancedFilter.FilterValue2 = String.Empty;
                }
                else
                {
                    filterColumn.AdvancedFilter.CopyFrom(AdvancedFilter);
                    foreach (var filter in CheckboxFilters)
                    {
                        filter.FilterChecked = true;
                    }
                }

                _mainWindowViewModel.ApplyFilters();
            }
        }
        WeakReferenceMessenger.Default.Send(new CloseFlyoutMessage());
    }

    /// <summary>
    /// 取消按钮
    /// </summary>
    [RelayCommand]
    private void CloseFilterWindow()
    {
        WeakReferenceMessenger.Default.Send(new CloseFlyoutMessage());
        _flyoutColumnIndex = -1;
    }
}