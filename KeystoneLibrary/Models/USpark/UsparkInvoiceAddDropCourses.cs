namespace KeystoneLibrary.Models.USpark
{
    public class UsparkInvoiceAddDropCourses
	{
		[JsonProperty("addCourses")]
		public IEnumerable<Guid> AddCourses { get; set; }

		[JsonProperty("dropCourses")]
		public IEnumerable<Guid> DropCourses { get; set; }
	}
}

