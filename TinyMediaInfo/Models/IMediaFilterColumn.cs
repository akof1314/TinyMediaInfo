using System;
using TinyMediaInfo.ViewModels;

namespace TinyMediaInfo.Models;

public interface IMediaFilterColumn
{
    MediaCheckboxFilterColumnModel? CheckboxFilter { get; }

    MediaAdvancedFilterColumnModel AdvancedFilter { get; }

    int TabIndexFilter { get; set; }

    string ExcelKey { get; set; }
}