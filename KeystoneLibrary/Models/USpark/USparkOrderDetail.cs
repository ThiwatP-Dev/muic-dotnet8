namespace KeystoneLibrary.Models.USpark
{
    public class USparkOrderDetail
   {
        //public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public string ItemCode { get; set; }
        public string ItemNameEn { get; set; }
        public string ItemNameTh { get; set; }
        public long? KSRegistrationCourseId { get; set; }
        public long? KSCourseId { get; set; }
        public long? KSSectionId { get; set; }
        public string LumpSumAddDrop { get; internal set; }
    }
}