using System;
using System.Collections.Generic;
using System.Text;

namespace Log.Interface
{
    public class LogModel
    {
        public enum LogTypes
        {
            NotSet = 0,
            Debug = 1,
            Information = 2,
            Warning = 4,
            Error = 8
        }

        public LogModel()
        {
            this.NotesA = string.Empty;
            this.NotesB = string.Empty;
            this.ReferenceName = string.Empty;
            this.ReferenceValue = string.Empty;
        }
        public long Id { get; set; }
        public LogTypes LogType { get; set; }
        public string Owner { get; set; }
        public string Machine { get; set; }
        public string Location { get; set; }
        public string Operation { get; set; }
        public string Step { get; set; }
        public int Counter { get; set; }
        public Guid Correlation { get; set; }
        public string CodeReference { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceValue { get; set; }
        public string NotesA { get; set; }
        public string NotesB { get; set; }
        //duration between start and end in seconds
        public double? Duration { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
