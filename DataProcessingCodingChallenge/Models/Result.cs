using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessingCodingChallenge
{
    public class Result
    {
        public DateTime? StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string[] OutputCsvArray { get; set; }
    }
}
