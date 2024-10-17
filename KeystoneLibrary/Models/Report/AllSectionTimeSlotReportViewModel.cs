namespace KeystoneLibrary.Models.Report
{
    public class AllSectionTimeSlotReportViewModel
    {
        public Criteria Criteria { get; set; }

        public List<AllSectionTimeSlotByUsingTypeReportItem> Items { get; set; } = new List<AllSectionTimeSlotByUsingTypeReportItem>();


        public class AllSectionTimeSlotByUsingTypeReportItem
        {
            public string UsingType { get; set; }
            public string UsingTypeText { get; set; }

            public List<string> Header { get; set; } = new List<string>();

            public List<RowData> Rows { get; set; } = new List<RowData>();

            public long Total { get => Rows.Sum(x => x.Total); }

            public class RowData
            {
                public string DayOfWeekText { get; set; }
                public List<long> Values { get; set; } = new List<long>();

                public long Total { get => Values.Sum(); } 
            }
        }
    }
}
