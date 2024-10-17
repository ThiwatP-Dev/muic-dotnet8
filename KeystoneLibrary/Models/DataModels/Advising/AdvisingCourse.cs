using System.ComponentModel.DataAnnotations.Schema;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Models.DataModels.Advising
{
    public class AdvisingCourse : UserTimeStamp
    {
        public long Id { get; set; }
        public long TermId { get; set; }
        public Guid StudentId { get; set; }
        public long InstructorId { get; set; }
        public long CourseId { get; set; }
        public long? SectionId { get; set; }
        public bool IsRequired { get; set; }

        [ForeignKey("TermId")]
        public virtual Term Term { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }
        
        [NotMapped]
        public List<Section> Sections { get; set; }

        [NotMapped]
        public SelectList SectionSelectList
        {
            get
            {
                if (Sections != null && Sections.Any())
                {
                    var sections = Sections.Select(x => new SelectListItem
                                                        {
                                                            Text = x.Number,
                                                            Value = x.Id.ToString()
                                                        });
                                                        
                    return new SelectList(sections, "Value", "Text");
                }

                return new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }
}