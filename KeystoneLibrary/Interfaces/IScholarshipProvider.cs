using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Scholarship;

namespace KeystoneLibrary.Interfaces
{
    public interface IScholarshipProvider
    {
        Scholarship GetScholarshipById(long id);
        ScholarshipBudget GetScholarshipBudget(long id);
        ScholarshipType GetScholarshipTypeById(long id);
        List<ScholarshipBudget> GetScholarshipBudgetByScholarshipId(long scholarshipId);
        List<ScholarshipStudent> GetScholarshipStudents(Guid stundentId);
        ScholarshipStudent GetScholarshipStudentById(long id);
        ScholarshipStudent GetCurrentScholarshipStudent(Guid studentId);
        bool IsScholarshipLimitExceeded(ScholarshipStudent scholarshipStudent);
        bool IsAnyScholarshipActive(ScholarshipStudent scholarshipStudent);
        ScholarshipStudent GetCurrentScholarshipByTerm(Guid studentId, long termId);
        decimal GetScholarshipBalance(long scholarshipStudentId);
        void CreateTransactionFromReceipt(long receiptId, DateTime paidAt, string type);
        List<FinancialTransaction> GetTransactionsByStudent(Guid studentId);
        
        // Vourcher
        void CreateTransactionFromVoucher(Voucher voucher);
        void InactiveTransactionFromVoucher(long voucherId);
        void CancelTransactionFromReceipt(long receiptId);
        void CreateTransactions(List<FinancialTransaction> transactions);
    }
}