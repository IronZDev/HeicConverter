namespace HeicConverter.Data
{
    public enum FileStatus
    {
        PENDING,
        IN_PROGRESS,
        COMPLETED,
        INVALID,
        ERROR
    }
    public class FileListElement
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public FileStatus Status { get; set; } 

        public FileListElement (string name, string path, FileStatus status)
        {
            Name = name;
            Path = path;
            Status = status;
        }
    }
}
