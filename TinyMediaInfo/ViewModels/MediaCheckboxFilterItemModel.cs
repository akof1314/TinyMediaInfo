using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TinyMediaInfo.ViewModels
{
    public partial class MediaCheckboxFilterItemModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isChecked;

        public bool FilterChecked;

        public string FilterName { get; set; } = String.Empty;

        public int FilterCount { get; set; }

        public string FilterDesc => $"{FilterName} ({FilterCount})";
    }
}