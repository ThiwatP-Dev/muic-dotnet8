using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Logs;

namespace KeystoneLibrary.Models
{
    public class BatchRegistrationConfirmationViewModel
    {
        public Criteria Criteria { get; set; }

        public List<BatchRegistrationConfirmJob> BatchRegistrationConfirmJobs { get; set; }

        public List<DataSyncLog> DataSyncLogs { get; set; }

        public bool IsAbleToCreateNewJob { get; set; }
    }

    public class BatchRegistrationConfirmationJobCreationViewModel
    {
        public BatchRegistrationConfirmationCriteriaViewModel Criteria { get; set; }

        public BatchRegistrationConfirmJob BatchRegistrationConfirmJob { get; set; }

        public List<BatchRegistrationConfirmJobDetail> BatchRegistrationConfirmJobDetailList { get; set; }

        public class BatchRegistrationConfirmationCriteriaViewModel
        {
            public long AcademicLevelId { get; set; }
            public long TermId { get; set; }

            public bool IsOnlyUnconfirm { get; set; } = true;
            public bool IsCheckCreditLimit { get; set; } = true;
            public bool IsExcludeScholarshipStudent { get; set; } = true;
            public bool IsNotExchangeAdmissionType { get; set; } = false;
            public bool IsNotExchangeStatus { get; set; } = true;

            public bool IsRecheckWithUSpark { get; set; } = false;

            [JsonIgnore]
            private List<string> excludeStudentCodesList;
            public string ExcludeStudentCodesCsv 
            { 
                get 
                {  
                    return ExcludeStudentCodesList == null || ExcludeStudentCodesList.Count == 0 ? "" : string.Join(",", ExcludeStudentCodesList?.Select(x => x.Trim().ToString()).ToArray()); 
                } 
                set 
                {
                    if (value != null)
                    {
                        excludeStudentCodesList = value.Split(',').Select(x => x.Trim()).ToList();
                    }
                } 
            }
            [JsonIgnore]
            public List<string> ExcludeStudentCodesList 
            {
                get
                {
                    return excludeStudentCodesList = excludeStudentCodesList ?? new List<string>();
                }
                set
                {
                    excludeStudentCodesList = value;
                }
            }

            [JsonIgnore]
            private List<long> excludeDepartmentIdList { get; set; }
            public string ExcludeDepartmentIdCsv
            {
                get
                {
                    return excludeDepartmentIdList == null || excludeDepartmentIdList.Count == 0 ? "" : string.Join(",", excludeDepartmentIdList?.Select(x => x.ToString()).ToArray());
                }
                set
                {
                    if (value != null)
                    {
                        excludeDepartmentIdList = value.Split(',').Select(x => Convert.ToInt64(x.Trim())).ToList();
                    }
                }
            }
            [JsonIgnore]
            public List<long> ExcludeDepartmentIdList
            {
                get
                {
                    return excludeDepartmentIdList = excludeDepartmentIdList ?? new List<long>() { 70 }; //70 == dentistry
                }
                set
                {
                    excludeDepartmentIdList = value;
                }
            }

            [JsonIgnore]
            private List<long> includedAdmissionTypeIdList { get; set; }
            public string IncludeAdmissionTypeIdCsv
            {
                get
                {
                    return includedAdmissionTypeIdList == null || includedAdmissionTypeIdList.Count == 0 ? "" : string.Join(",", includedAdmissionTypeIdList?.Select(x => x.ToString()).ToArray());
                }
                set
                {
                    if (value != null)
                    {
                        includedAdmissionTypeIdList = value.Split(',').Select(x => Convert.ToInt64(x.Trim())).ToList();
                    }
                }
            }
            [JsonIgnore]
            public List<long> IncludedAdmissionTypeIdList
            {
                get
                {
                    return includedAdmissionTypeIdList = includedAdmissionTypeIdList ?? new List<long>() { 1 }; //1 == Normal
                }
                set
                {
                    includedAdmissionTypeIdList = value;
                }
            }

        }
    }


    
}
