using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HeicConverter.Data
{
    public enum FileStatus
    {
        [Description("Pending")]
        PENDING,
        [Description("In progress")]
        IN_PROGRESS,
        [Description("Completed")]
        COMPLETED,
        [Description("Invalid")]
        INVALID,
        [Description("Error")]
        ERROR
    }
    
    // Only FileStatus might change, so no need for triggering ProprtyChanged Event for others;
    public class FileListElement : INotifyPropertyChanged
    {
        private FileStatus _status;
        private string _tooltipMsg;

        public string Name { get; }
        public string Path { get; }
        public string Token { get; }
        public FileStatus Status { 
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); } 
        } 

        public string TooltipMsg
        {
            get { return _tooltipMsg; }
            set { _tooltipMsg = value; OnPropertyChanged("TooltipMsg"); }
        }

        public FileListElement (string name, string path, FileStatus status, string token)
        {
            Name = name;
            Path = path;
            Status = status;
            Token = token;
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
