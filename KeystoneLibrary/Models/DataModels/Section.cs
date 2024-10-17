using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.MasterTables;

namespace KeystoneLibrary.Models.DataModels
{
    public class Section : UserTimeStamp
    {
        public long Id { get; set; }

        [StringLength(30)]
        public string? Number { get; set; }
        public long CourseId { get; set; }
        public long TermId { get; set; }
        public int SeatAvailable { get; set; }
        public int SeatLimit { get; set; }
        public int SeatUsed { get; set; }
        public int PlanningSeat { get; set; }
        public int ExtraSeat { get; set; }
        public int MinimumSeat { get; set; }
        public int TotalWeeks { get; set; }
        public long? USparkId { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }
        public TimeSpan? MidtermStart { get; set; }
        public TimeSpan? MidtermEnd { get; set; }
        public bool IsDisabledMidterm { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? MidtermDate { get; set; }
        public long? MidtermRoomId { get; set; }
        public TimeSpan? FinalStart { get; set; }
        public TimeSpan? FinalEnd { get; set; }
        public bool IsDisabledFinal { get; set; }

        [TypeConverter(typeof(DefaultDateTimeFormat))]
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? FinalDate { get; set; }
        public long? FinalRoomId { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime OpenedAt { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ClosedAt { get; set; }
        public bool IsSpecialCase { get; set; } // Ghost Section
        public bool IsOutbound { get; set; } // Outbound Section
        public bool IsClosed { get; set; }

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? OpenedSectionAt { get; set; } // update when open section or re-open section after close

        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ClosedSectionAt { get; set; } // if is close is true, save today.

        public bool IsEvening { get; set; }
        public bool IsParent { get; set; }
        public bool IsMasterSection { get; set; }
        public bool IsWithdrawable { get; set; } = true;
        public long? ParentSectionId { get; set; } //use for Section Group case
        public long? MainInstructorId { get; set; }

        [StringLength(5)]
        public string? Status { get; set; } // a = approve, c = Confirm, w = waiting, r = reject

        // Condition
        public int? BatchFrom { get; set; } // remove after edit
        public int? BatchTo { get; set; } // remove after edit

        [StringLength(500)]
        public string? Batches { get; set; } // JsonString [561, 562, 563]

        [StringLength(1000)]
        public string? StudentIds { get; set; } // JsonString ['GUID1', 'GUID2', 'GUID3']

        [StringLength(500)]
        public string? FacultyIds { get; set; } // JsonString [1, 2, 3]

        [StringLength(500)]
        public string? DepartmentIds { get; set; } // JsonString [1, 2, 3]

        [StringLength(500)]
        public string? CurriculumIds { get; set; } // JsonString [1, 2, 3]

        [StringLength(500)]
        public string? CurriculumVersionIds { get; set; } // JsonString [1, 2, 3]

        [StringLength(500)]
        public string? MinorIds { get; set; } // JsonString [1, 2, 3]
        
        [DisplayFormat(DataFormatString="{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode=true)]
        public DateTime? ApprovedAt { get; set; }

        [StringLength(200)]
        public string? ApprovedBy { get; set; }
        public long? MUICId { get; set; }

        public virtual List<SectionDetail> SectionDetails { get; set; }
        public virtual List<SectionSlot> SectionSlots { get; set; }
        public virtual List<Section> ChildrenSections { get; set; }
        
        [JsonIgnore]
        public virtual List<RegistrationCourse> RegistrationCourses { get; set; }
        
        [JsonIgnore]
        public virtual List<SharedSection> SharedSections { get; set; }

        // Notmapped

        [NotMapped]
        public long AcademicLevelId { get; set; }

        [NotMapped]
        public decimal AvailabilityPercentage => (SeatLimit <= 0) ? 0 : (SeatAvailable * 100) / SeatLimit;

        [JsonIgnore]
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [JsonIgnore]
        [ForeignKey("ParentSectionId")]
        public virtual Section? ParentSection { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [JsonIgnore]
        [ForeignKey("MidtermRoomId")]
        public virtual Room? MidtermRoom { get; set; }

        [JsonIgnore]
        [ForeignKey("FinalRoomId")]
        public virtual Room? FinalRoom { get; set; }

        [JsonIgnore]
        [ForeignKey("MainInstructorId")]
        public virtual Instructor? MainInstructor { get; set; }

        #region Course to be Offered
        [NotMapped]
        public List<string> Students { get; set; } = new List<string>();
        
        [NotMapped]
        public string StudentCodes { get; set; }

        [NotMapped]
        public List<long> Faculties { get; set; } = new List<long>();

        [NotMapped]
        public List<long> Minors { get; set; } = new List<long>();

        [NotMapped]
        public List<long> Departments { get; set; } = new List<long>();

        [NotMapped]
        public List<long> Curriculums { get; set; } = new List<long>();

        [NotMapped]
        public List<long> CurriculumVersions { get; set; } = new List<long>();

        [NotMapped]
        public List<int> BatchesInt { get; set; } = new List<int>();

        [NotMapped]
        public List<string> FacultiesCodeAndName { get; set; }

        [NotMapped]
        public List<string> MinorsCodeAndName { get; set; }

        [NotMapped]
        public List<string> DepartmentsCodeAndName { get; set; }

        [NotMapped]
        public List<string> CurriculumsCodeAndName { get; set; }

        [NotMapped]
        public List<string> CurriculumVersionsCodeAndName { get; set; }

        #endregion


        [NotMapped]
        public string FinalString => $"{ FinalDate?.ToString(StringFormat.ShortDate) }";

        [NotMapped]
        public string FinalTime => $"({ FinalStart?.ToString(StringFormat.TimeSpan) } - { FinalEnd?.ToString(StringFormat.TimeSpan) })";

        [NotMapped]
        public string MidtermString => $"{ MidtermDate?.ToString(StringFormat.ShortDate) }";

        [NotMapped]
        public string MidtermTime => $"({ MidtermStart?.ToString(StringFormat.TimeSpan) } - { MidtermEnd?.ToString(StringFormat.TimeSpan) })";
        
        [NotMapped]
        public bool IsSeatOver { get; set; }

        [NotMapped]
        public int TotalSeatUsed { get; set; }// master seat used + joints seat used

        [NotMapped]
        public string Final => $"{ FinalString } \n { FinalTime }";

        [NotMapped]
        public string Midterm => $"{ MidtermString } \n { MidtermTime }";

        [NotMapped]
        public string OpenedAtText => OpenedAt.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string ClosedAtText => ClosedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string SectionDateAndTime
        {
            get
            {
                if (SectionDetails != null && SectionDetails.Any())
                {
                    var sectionDateAndTime = string.Join("\n", SectionDetails.Select(x => x.DayofweekAndTime));
                    return sectionDateAndTime;
                }

                return "N/A";
            }
        }

        [NotMapped]
        public string SectionRoom
        {
            get
            {
                if (SectionDetails != null && SectionDetails.Any())
                {
                    var rooms = SectionDetails.Where(x => x.Room != null)
                                              .Select(x => x.Room);

                    if (rooms != null && rooms.Any())
                    {
                        var sectionRoom = string.Join(",", rooms.Select(x => x.NameEn));
                        return sectionRoom;
                    }
                }
                
                return "N/A";
            }
        }

        [NotMapped]
        public int NumberValue
        {
            get
            {
                int number;
                bool success = Int32.TryParse(Number, out number);
                return success ? number : 0;
            }
        }

        // Use for generate schedule
        [NotMapped]
        public int MidtermDay => MidtermDate.HasValue ? (int)MidtermDate.Value.DayOfWeek : 0;

        [NotMapped]
        public int FinalDay => FinalDate.HasValue ? (int)FinalDate.Value.DayOfWeek : 0;

        [NotMapped]
        public string DayOfWeekMidterm => Enum.GetName(typeof(DayOfWeek),MidtermDay).Substring(0,3).ToUpper();

        [NotMapped]
        public string DayOfWeekFinal => Enum.GetName(typeof(DayOfWeek),FinalDay).Substring(0,3).ToUpper();

        [NotMapped]
        public TimeSpan MidtermStartTime => MidtermStart.HasValue ? MidtermStart.Value : TimeSpan.FromMilliseconds(2000);

        [NotMapped]
        public TimeSpan MidtermEndTime => MidtermEnd.HasValue ? MidtermEnd.Value : TimeSpan.FromMilliseconds(2000);

        [NotMapped]
        public TimeSpan FinalStartTime => FinalStart.HasValue ? FinalStart.Value : TimeSpan.FromMilliseconds(2000);

        [NotMapped]
        public TimeSpan FinalEndTime => FinalEnd.HasValue ? FinalEnd.Value : TimeSpan.FromMilliseconds(2000);

        [NotMapped]
        public string MidtermText => $"{ MidtermStart?.ToString(StringFormat.TimeSpan) } - { MidtermEnd?.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public string FinalText => $"{ FinalStart?.ToString(StringFormat.TimeSpan) } - { FinalEnd?.ToString(StringFormat.TimeSpan) }";

        [NotMapped]
        public DateTime MidtermDateValue => MidtermDate.HasValue ? MidtermDate.Value.Date : default(DateTime);

        [NotMapped]
        public DateTime FinalDateValue => FinalDate.HasValue ? FinalDate.Value.Date : default(DateTime);

        [NotMapped]
        public string MidtermDateTime => $"{ MidtermString } { MidtermText }";

        [NotMapped]
        public string FinalDateTime => $"{ FinalString } { FinalText }";

        [NotMapped]
        public string MidtermDateTimeText => (MidtermDate == new DateTime() || MidtermDate == null) ?  "-" : MidtermDateTime;
    
        [NotMapped]
        public string FinalDateTimeText => (FinalDate == new DateTime() || FinalDate == null) ?  "-" : FinalDateTime;

        [NotMapped]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "w":
                        return "Waiting";
                    case "c":
                        return "Confirm";
                    case "a":
                        return "Approved";
                    case "r":
                        return "Reject";
                    default:
                        return "N/A";
                }
            }
        }
        
        [NotMapped]
        public string OpenedSectionAtText => OpenedSectionAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string ClosedSectionAtText => ClosedSectionAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public int TotalStudent => RegistrationCourses == null ? 0
                                                               : RegistrationCourses.Count(x => x.Status == "a" || x.Status == "r");

        [NotMapped]
        public string TotalStudentText => TotalStudent.ToString(StringFormat.GeneralDecimal);

        [NotMapped]
        public string ApprovedAtText => ApprovedAt?.ToString(StringFormat.ShortDate);

        [NotMapped]
        public string SeatUsedText => SeatUsed.ToString(StringFormat.GeneralDecimal);

        [NotMapped]
        public List<Section> JointSections { get; set; }

        [NotMapped]
        public string JointSectionCodes { get; set; }

        [NotMapped]
        public bool IsOldJointSection { get; set; }

        [NotMapped]
        public string CourseCode { get; set; }

        [NotMapped]
        public string CourseNameEnAndCredit { get; set; }

        [NotMapped]
        public string ParentSectionNumber { get; set; }

        [NotMapped]
        public long KSSectionId { get; set; }

        [NotMapped]
        public long KSCourseId { get; set; }

        [NotMapped]
        public long KSSemesterId { get; set; }

        [NotMapped]
        public string MidtermRoomName { get; internal set; }

        [NotMapped]
        public string FinalRoomName { get; internal set; }

        [NotMapped]
        public long FirstTeachingTypeId { get; set; }

        [NotMapped]
        public string SectionTypes 
        { 
            get 
            {
                string result = "( ";
                if(ParentSectionId == null || ParentSectionId == 0)
                {
                    result += "Master";
                }
                else
                {
                    result += "Joint";
                }

                if(IsSpecialCase)
                {
                    result += ", Ghost";
                }

                if(IsOutbound)
                {
                    result += ", Outbound ";
                }

                return result + " )";
            }
         }

        [NotMapped]
        public bool HaveSectionSlotRoom { get; set; }
        [NotMapped]
        public List<ExaminationReservation> ExaminationReservations { get; set; } = new List<ExaminationReservation>();
        [NotMapped]
        public bool IsFinalApproval { get; set; }
        [NotMapped]
        public bool IsMidtermApproval { get; set; }
        [NotMapped]
        public bool IsStatusApproval => Status == "a";
    }
}