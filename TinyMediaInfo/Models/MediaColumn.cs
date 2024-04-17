using System;
using System.Linq.Expressions;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using TinyMediaInfo.ViewModels;

namespace TinyMediaInfo.Models;

public class MediaColumn<TKey> : TextColumn<MediaViewModel, string>, IMediaFilterColumn
{
    public Func<MediaViewModel, TKey?> SortSelector { get; }

    public MediaCheckboxFilterColumnModel? CheckboxFilter { get; }

    public MediaAdvancedFilterColumnModel AdvancedFilter { get; }

    public int TabIndexFilter { get; set; }

    public string ExcelKey { get; set; }

    public MediaColumn(string headerKey, Expression<Func<MediaViewModel, string?>> getter, double width, 
        Func<MediaViewModel, TKey> sortSelector, MediaCheckboxFilterColumnModel? checkboxFilter, string excelKey) : 
        base(App.GetLocalizedString(headerKey), getter, new GridLength(width), null)
    {
        SortSelector = sortSelector;
        CheckboxFilter = checkboxFilter;
        AdvancedFilter = new MediaAdvancedFilterColumnModel();
        if (typeof(TKey) == typeof(long))
        {
            AdvancedFilter.FilterLongType = true;
        }

        ExcelKey = excelKey;
    }

#pragma warning disable CS8618
    public MediaColumn(object? header, Expression<Func<MediaViewModel, string?>> getter, GridLength? width = null, TextColumnOptions<MediaViewModel>? options = null) : base(header, getter, width, options)
    {
    }

    public MediaColumn(object? header, Expression<Func<MediaViewModel, string?>> getter, Action<MediaViewModel, string?> setter, GridLength? width = null, TextColumnOptions<MediaViewModel>? options = null) : base(header, getter, setter, width, options)
    {
    }
#pragma warning restore CS8618
}