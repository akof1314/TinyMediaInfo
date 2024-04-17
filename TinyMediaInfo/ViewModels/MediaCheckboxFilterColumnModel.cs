using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TinyMediaInfo.ViewModels
{
    public class MediaCheckboxFilterColumnModel
    {
        public ObservableCollection<MediaCheckboxFilterItemModel> Filters { get; }

        private readonly Dictionary<string, int> _dict;

        public MediaCheckboxFilterColumnModel()
        {
            Filters = new ObservableCollection<MediaCheckboxFilterItemModel>();
            _dict = new Dictionary<string, int>();
        }

        public void AddFilterCount(string filterName)
        {
            if (_dict.TryGetValue(filterName, out var idx))
            {
                Filters[idx].FilterCount++;
            }
            else
            {
                Filters.Add(new MediaCheckboxFilterItemModel
                {
                    FilterName = filterName,
                    FilterCount = 1,
                    IsChecked = true,
                    FilterChecked = true
                });
                _dict[filterName] = Filters.Count - 1;
            }
        }

        public void Clear()
        {
            Filters.Clear();
            _dict.Clear();
        }
    }
}