using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel
{
    public class Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public long Date { get; set; }
        public string Body { get; set; }
    }
}
