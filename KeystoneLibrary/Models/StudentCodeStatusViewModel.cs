namespace KeystoneLibrary.Models
{
    public class StudentCodeStatusViewModel 
    {
        public int Code { get; set; }
        public bool IsUsed { get; set; }
    }

    public class StudentCodeStatusRange
    {
        public string StartedCode { get; set; }
        public string EndedCode { get; set; }
    }
}