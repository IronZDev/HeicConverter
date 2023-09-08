using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HeicConverter.Data
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private const string CONVERT_BTN_PENDING_TEXT = "Convert";
        private const string CONVERT_BTN_CONVERTING_TEXT = "Converting...";
        public ObservableCollection<FileListElement> files = new ObservableCollection<FileListElement>();
        public bool _isConversionInProgress = false;
        public long _convertedFilesCounter = 0;

        public bool IsConversionInProgress {  
            get { return _isConversionInProgress; }
            set { _isConversionInProgress = value; OnPropertyChanged("IsConversionInProgress"); }
        }

        public long ConvertedFilesCounter
        {
            get { return _convertedFilesCounter; }
            set { _convertedFilesCounter = value; OnPropertyChanged("ConvertedFilesCounter"); }
        }

        public bool isConvertButtonEnabled(bool isConversionInProgress, ObservableCollection<FileListElement> files)
        {
            return !isConversionInProgress && files.Count > 0;
        }

        public string getConvertBtnText(bool isConversionInProgress, long convertedFilesCounter, ObservableCollection<FileListElement> files)
        {
            return isConversionInProgress ? $"{CONVERT_BTN_CONVERTING_TEXT} ({convertedFilesCounter}/{files.Count})" : CONVERT_BTN_PENDING_TEXT;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
