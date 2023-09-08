using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeicConverter.Data.Converters
{
    public class FormatOption
    {
        static long nextId = 0;
        public FormatOption(string name, string extension) {
            Name = name;
            Extension = extension;
            ID = nextId;
            nextId++;
        }
        public long ID
        {
            get;
        }
        public string Name { get; }
        public string Extension { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
