using HeicConverter.Data.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HeicConverter.Data
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private const string CONVERT_BTN_PENDING_TEXT = "Convert";
        private const string CONVERT_BTN_CONVERTING_TEXT = "Converting...";
        public ObservableCollection<FileListElement> files = new ObservableCollection<FileListElement>();
        private bool _isConversionInProgress = false;
        private long _convertedFilesCounter = 0;
        private FormatOption _selectedItem = null;

        public List<FormatOption> formatOptions = new List<FormatOption> {
            new FormatOption("Joint Photographic Experts Group JFIF format (.jpg)", "jpg"),
            new FormatOption("Joint Photographic Experts Group JFIF format (.jpeg)", "jpeg"),
            new FormatOption("Portable Network Graphics (.png)", "png"),
            new FormatOption("Tagged image file multispectral format (.tiff)", "tiff"),
            new FormatOption("Microsoft Windows bitmap (.bmp)", "bmp"),
            new FormatOption("Portable Document Format (.pdf)", "pdf"),
            new FormatOption("Scalable Vector Graphics (.svg)", "svg"),
            new FormatOption("Weppy image format (.webp)", "webp")
        };

        public FormatOption SelectedItem
        {
            get {  return _selectedItem; }
            set { if (value == _selectedItem) return; _selectedItem = value; OnPropertyChanged("SelectedItem"); }
        }

        public bool IsConversionInProgress {  
            get { return _isConversionInProgress; }
            set { _isConversionInProgress = value; OnPropertyChanged("IsConversionInProgress"); }
        }

        public long ConvertedFilesCounter
        {
            get { return _convertedFilesCounter; }
            set { _convertedFilesCounter = value; OnPropertyChanged("ConvertedFilesCounter"); }
        }

        public MainPageViewModel()
        {
            SelectedItem = formatOptions[0];
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
