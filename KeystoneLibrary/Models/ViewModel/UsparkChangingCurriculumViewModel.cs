using KeystoneLibrary.Models.Enums;

namespace KeystoneLibrary.Models.ViewModel
{
    public class CreateUsparkChangingCurriculumPetitionViewModel
	{
		[JsonProperty("studentCode")]
		public string StudentCode { get; set; }

		[JsonProperty("KSTermId")]
		public long KSTermId { get; set; }

		[JsonProperty("changeToMajorId")]
		public long MoveToDepartmentId { get; set; }

		[JsonProperty("remark")]
		public string StudentRemark { get; set; }

		[JsonProperty("channel")]
		public string Channel { get; set; }
	}

	public class UsparkChangingCurriculumPetitionViewModel : CreateUsparkChangingCurriculumPetitionViewModel
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("currentDepartmentId")]
		public long DepartmentIdOnRequest { get; set; }

		[JsonProperty("status")]
		[JsonConverter(typeof(StringEnumConverter))]
		public PetitionStatusEnum Status { get; set; }

		[JsonProperty("approverRemark")]
		public string ApproverRemark { get; set; }

		[JsonProperty("createdAt")]
		public DateTime CreatedAt { get; set; }

		[JsonProperty("updatedAt")]
		public DateTime UpdatedAt { get; set; }
    }
}

