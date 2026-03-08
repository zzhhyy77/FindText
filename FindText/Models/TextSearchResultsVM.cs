using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

namespace FindText.Models
{

    internal class TextSearchResultsVM : VModelsBase
    {
        private static object _lock = new object();
        private ObservableCollection<TextSearchResult> _results;

        public ICommand DeleteCommand { get; }

        TextSearchResultsVM()
        {
            DeleteCommand = new RelayCommand(OnDelete, CanDelete);
        }

        public ObservableCollection<TextSearchResult> Results
        {
            get
            {
                return _results;
            }
            set
            {
                _results = value;
                BindingOperations.EnableCollectionSynchronization(_results, _lock);
                RaisePropertyChanged("Results");
            }
        }


        private void OnDelete(object args)
        {
            RaisePropertyChanged("Results");
        }

        private bool CanDelete(object args)
        {
            return true;
        }

    }
}
