using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewRetriever.Model
{
    class Review
    {
        public string Store { get; set; }
        public DateTime Date { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}

