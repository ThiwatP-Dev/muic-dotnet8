namespace KeystoneLibrary.Models
{
    public class GradeMemberViewModel
    {
        public Criteria Criteria { get; set; }
        public List<GradeMemberDetailViewModel> GradeMembers { get; set; }
    }
    public class GradeMemberDetailViewModel
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IsChecked { get; set; }
        public string FullNameEn { get; set; }
    }
}