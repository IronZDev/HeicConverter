using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeicConverter.Data
{
    public class MainPageViewModel
    {
        public ObservableCollection<FileListElement> files = new ObservableCollection<FileListElement>();
    }
}
