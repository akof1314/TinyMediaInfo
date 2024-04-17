using System;
using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TinyMediaInfo.ViewModels;

public partial class MediaAdvancedFilterColumnModel : ViewModelBase
{
    [ObservableProperty]
    private bool _filterLongType;

    [ObservableProperty]
    private int _filterType;

    [ObservableProperty]
    private string _filterValue = String.Empty;

    [ObservableProperty]
    private bool _predicateAnd = true;

    [ObservableProperty]
    private int _filterType2;

    [ObservableProperty]
    private string _filterValue2 = String.Empty;

    public void CopyFrom(MediaAdvancedFilterColumnModel model)
    {
        FilterLongType = model.FilterLongType;
        FilterType = model.FilterType;
        FilterValue = model.FilterValue;
        PredicateAnd = model.PredicateAnd;
        FilterType2 = model.FilterType2;
        FilterValue2 = model.FilterValue2;
    }

    public bool IsAllEmpty()
    {
        return string.IsNullOrEmpty(FilterValue) && string.IsNullOrEmpty(FilterValue2);
    }

    public bool IsLongFilter(long longValue)
    {
        if (!FilterLongType)
        {
            return false;
        }

        if (PredicateAnd)
        {
            return CompareLong(longValue, FilterValue, FilterType) &&
                   CompareLong(longValue, FilterValue2, FilterType2);
        }
        return CompareLong(longValue, FilterValue, FilterType) ||
               CompareLong(longValue, FilterValue2, FilterType2);
    }

    public bool IsByteFilter(long longValue)
    {
        if (!FilterLongType)
        {
            return false;
        }

        if (PredicateAnd)
        {
            return CompareByte(longValue, FilterValue, FilterType) &&
                   CompareByte(longValue, FilterValue2, FilterType2);
        }
        return CompareByte(longValue, FilterValue, FilterType) ||
               CompareByte(longValue, FilterValue2, FilterType2);
    }

    public bool IsStrFilter(string? strValue)
    {
        if (FilterLongType || strValue == null)
        {
            return false;
        }

        if (PredicateAnd)
        {
            return CompareStr(strValue, FilterValue, FilterType) &&
                   CompareStr(strValue, FilterValue2, FilterType2);
        }
        return CompareStr(strValue, FilterValue, FilterType) ||
               CompareStr(strValue, FilterValue2, FilterType2);
    }

    private bool CompareLong(long value1, string value2, int compareType)
    {
        if (!string.IsNullOrEmpty(value2))
        {
            if (long.TryParse(value2, out var val1))
            {
                return CompareLong(value1, val1, compareType);
            }
        }
        // 空也表示匹配成功
        return true;
    }

    private bool CompareByte(long value1, string value2, int compareType)
    {
        if (!string.IsNullOrEmpty(value2))
        {
            if (ByteSize.TryParse(value2, out var val1))
            {
                return CompareLong(value1, (long)val1.Bytes, compareType);
            }
        }
        // 空也表示匹配成功
        return true;
    }

    private bool CompareLong(long value1, long value2, int compareType)
    {
        switch (compareType)
        {
            case 0: return value1 == value2;
            case 1: return value1 != value2;
            case 2: return value1 > value2;
            case 3: return value1 >= value2;
            case 4: return value1 < value2;
            case 5: return value1 <= value2;
        }
        return false;
    }

    private bool CompareStr(string value1, string value2, int compareType)
    {
        if (string.IsNullOrEmpty(value2))
        {
            return true;
        }
        switch (compareType)
        {
            case 0: return value1 == value2;
            case 1: return value1 != value2;
            case 2: return value1.StartsWith(value2, StringComparison.Ordinal);
            case 3: return value1.EndsWith(value2, StringComparison.Ordinal);
            case 4: return value1.Contains(value2);
            case 5: return !value1.Contains(value2);
        }
        return false;
    }
}