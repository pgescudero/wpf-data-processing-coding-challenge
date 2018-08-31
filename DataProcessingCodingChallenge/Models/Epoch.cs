using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessingCodingChallenge
{
    public class Epoch
    {
        public int ActivityId { get; set; }
        public DateTime Date { get; set; }



        // Parses an epoch from a string.  Does some basic error checking, but generally assumes
        // that the incoming data is clean, and is not robust against format changes.
        public static Epoch ParseFromString(string csvString) 
        {
            Epoch epoch = new Epoch();
            string[] items = csvString.Split(',');

            if (items.Length < 2) 
            {
                Trace.TraceWarning("Epoch could not be parsed " + csvString);
                return null;
            } 
            else 
            {
                int activityId;
                if (int.TryParse(items[0], out activityId))
                    epoch.ActivityId = activityId;
                else
                {
                    Trace.TraceWarning("Epoch activity id could not be parsed " + csvString);
                    return null;
                }
                
                DateTime? date = dateFromCSV(items[1]);
                if (date != null) 
                    epoch.Date = date.Value;
                else 
                {
                    Trace.TraceWarning("Epoch date could not be parsed " + csvString);
                    return null;
                }
                return epoch;
            }
    
        }

        // Date formatter, assumes that all dates are represented in the same format as the original test data.  
        private static DateTime? dateFromCSV(string inputString) 
        {
            //string pattern = "MM/dd/YY HH:mm";

            string[] dateAndTime = inputString.Split(' ');
            if (dateAndTime.Length != 2)
                return null;

            string[] dateParts = dateAndTime[0].Split('/');
            if (dateParts.Length != 3)
                return null;

            string[] timeParts = dateAndTime[1].Split(':');
            if (timeParts.Length != 2)
                return null;

            int year, month, day, hour, minute;
            if (!int.TryParse(dateParts[0], out month) || !int.TryParse(dateParts[1], out day) || !int.TryParse(dateParts[2], out year) 
                || !int.TryParse(timeParts[0], out hour) || !int.TryParse(timeParts[1], out minute))
            {
                return null;
            }

            year += 2000;
            return new DateTime(year, month, day, hour, minute, 0);
        }

    }
}
