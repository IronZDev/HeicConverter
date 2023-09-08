﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeicConverter.Data.Converters
{
    public class FormatOption
    {
        public FormatOption(string name, string extension) {
            Name = name;
            Extension = extension;
        }
        public string Name { get; }
        public string Extension { get; }
    }
}