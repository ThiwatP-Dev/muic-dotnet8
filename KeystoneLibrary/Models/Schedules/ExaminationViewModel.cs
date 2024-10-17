namespace KeystoneLibrary.Models
{
    public class ExaminationViewModel
    {
        public long Id { get; set; }
        public string NameEn { get; set; }
        public int Floor { get; set; }
        public int Capacity { get; set; }
        public long BuildingId { get; set; }
        public long RoomTypeId { get; set; }
        public string RoomTypeName { get; set; }
        public string BuildingName { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string DateString => ExaminationDate.ToString(StringFormat.ShortDate);
    }

    public class RoomDetail
    {
        public long Id { get; set; }
        public string NameEn { get; set; }
        public int Floor { get; set; }
        public int Capacity { get; set; }
        public long BuildingId { get; set; }
        public long RoomTypeId { get; set; }
        public string RoomTypeName { get; set; }
        public string BuildingName { get; set; }
    }
}