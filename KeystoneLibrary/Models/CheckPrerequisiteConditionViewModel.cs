namespace KeystoneLibrary.Models
{
    public class CheckPrerequisiteConditionViewModel
    {   
        public bool IsAlreadyExist { get; set; } = false;
        public string Type { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public string MessageString => Messages.Any() ? string.Join(", ", Messages ) : "";
        public string Message => $"This { TypeText } already exist in { MessageString }";
        public string TypeText 
        {
            get
            {
                switch (Type)
                {
                    case "and":
                        return "And Condition";
                    case "or":
                        return "Or Condition";
                    case "coursegroup":
                        return "Course Group Condition";
                    case "credit":
                        return "Credit Condition";
                    case "gpa":
                        return "GPA Condition";
                    case "grade":
                        return "Grade Condition";
                    case "term":
                        return "Term Condition";
                    case "totalcoursegroup":
                        return "Total Course Group Condition";
                    case "batch":
                        return "Batch Condition";
                    case "ability":
                        return "Ability Condition";
                    default:
                        return "N/A";
                }
            }
        }
    }
}