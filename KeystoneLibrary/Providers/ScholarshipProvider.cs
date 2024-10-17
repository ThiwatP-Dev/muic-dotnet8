using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class ScholarshipProvider : IScholarshipProvider
    {
        protected readonly ApplicationDbContext _db;

        public ScholarshipProvider(ApplicationDbContext db)
        {
            _db = db;
        }

        public Scholarship GetScholarshipById(long id)
        {
            var scholarship = _db.Scholarships.Include(x => x.ScholarshipType)
                                              .IgnoreQueryFilters()
                                              .SingleOrDefault(x => x.Id == id);
            return scholarship;
        }

        public ScholarshipBudget GetScholarshipBudget(long id)
        {
            var budget = _db.ScholarshipBudgets.SingleOrDefault(x => x.Id == id);
            return budget;
        }

        public ScholarshipType GetScholarshipTypeById(long id)
        {
            var scholarshipType = _db.ScholarshipTypes.IgnoreQueryFilters()
                                                      .SingleOrDefault(x => x.Id == id);
            return scholarshipType;
        }

        public List<ScholarshipBudget> GetScholarshipBudgetByScholarshipId(long scholarshipId)
        {
            var budgets = _db.ScholarshipBudgets.Include(x => x.Faculty)
                                                .Include(x => x.Department)
                                                .Include(x => x.Curriculum)
                                                .Include(x => x.CurriculumVersion)
                                                .Where(x => x.ScholarshipId == scholarshipId)
                                                .ToList();
            return budgets;
        }

        public List<ScholarshipStudent> GetScholarshipStudents(Guid stundentId)
        {
            var scholarships = _db.ScholarshipStudents.Include(x => x.Student)
                                                      .Include(x => x.Scholarship)
                                                      .Include(x => x.EffectivedTerm)
                                                      .Include(x => x.ExpiredTerm)
                                                      .Where(x => x.StudentId == stundentId)
                                                      .ToList();
            return scholarships;
        }

        public ScholarshipStudent GetScholarshipStudentById(long id)
        {
            var student = _db.ScholarshipStudents.Include(x => x.Scholarship)
                                                      .ThenInclude(x => x.ScholarshipType)
                                                  .Include(x => x.Student)
                                                      .ThenInclude(x => x.AcademicInformation)
                                                      .ThenInclude(x => x.Department)
                                                  .Include(x => x.Student)
                                                      .ThenInclude(x => x.Title)
                                                  .Include(x => x.FinancialTransactions)
                                                      .ThenInclude(x => x.Term)
                                                  .Include(x => x.FinancialTransactions)
                                                      .ThenInclude(x => x.ReceiptItem)
                                                  .IgnoreQueryFilters()
                                                  .SingleOrDefault(x => x.Id == id);

            student.ScholarshipTypeId = student.Scholarship.ScholarshipTypeId;
            return student;
        }

        public ScholarshipStudent GetCurrentScholarshipStudent(Guid studentId)
        {
            var scholarship = _db.ScholarshipStudents.Include(x => x.Scholarship)
                                                         .ThenInclude(x => x.BudgetDetails)
                                                             .ThenInclude(x => x.FeeGroup)
                                                     .Where(x => x.StudentId == studentId)
                                                     .FirstOrDefault();
            return scholarship;
        }

        public ScholarshipStudent GetCurrentScholarshipByTerm(Guid studentId, long termId)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            if (term == null)
            {
                return null;
            }

            var scholarshipStudent = (from scholarship in _db.ScholarshipStudents.Include(x => x.Scholarship)
                                      join effectiveTerm in _db.Terms on scholarship.EffectivedTermId equals effectiveTerm.Id
                                      join expiredTerm in _db.Terms on scholarship.ExpiredTermId equals expiredTerm.Id into expiredTerms
                                      from expiredTerm in expiredTerms.DefaultIfEmpty()
                                      where scholarship.IsActive
                                            && scholarship.StudentId == studentId
                                            && (term.AcademicYear > effectiveTerm.AcademicYear
                                                || (term.AcademicYear == effectiveTerm.AcademicYear
                                                    && term.AcademicTerm >= effectiveTerm.AcademicTerm))
                                            && (scholarship.ExpiredTermId == null
                                                || term.AcademicYear < expiredTerm.AcademicYear
                                                || (term.AcademicYear == expiredTerm.AcademicYear
                                                    && term.AcademicTerm <= expiredTerm.AcademicTerm))
                                      select scholarship).FirstOrDefault();

            if (scholarshipStudent != null)
            {
                var transaction = _db.FinancialTransactions.Where(x => x.StudentScholarshipId == scholarshipStudent.Id)
                                                           .OrderByDescending(x => x.Id)
                                                           .FirstOrDefault();
                if (transaction != null)
                {
                    scholarshipStudent.LimitedAmount = transaction.Balance;
                }
                return scholarshipStudent;
            }
                                          
            return null;
        }

        public decimal GetScholarshipBalance(long studentScholarshipId)
        {
            var balance = _db.FinancialTransactions.Where(x => x.StudentScholarshipId == studentScholarshipId)
                                                   .OrderByDescending(x => x.Id)
                                                   .FirstOrDefault()
                                                   ?.Balance;
            
            return balance ?? _db.ScholarshipStudents.SingleOrDefault(x => x.Id == studentScholarshipId)
                                                     ?.LimitedAmount ?? 0;
        }

        public void CreateTransactionFromReceipt(long receiptId, DateTime paidAt, string type)
        {
            var transactions = (from receiptItem in _db.ReceiptItems.IgnoreQueryFilters()
                                join invoiceItem in _db.InvoiceItems.IgnoreQueryFilters() on receiptItem.InvoiceItemId equals invoiceItem.Id
                                join invoice in _db.Invoices.IgnoreQueryFilters() on invoiceItem.InvoiceId equals invoice.Id
                                where receiptItem.ReceiptId == receiptId
                                      && invoiceItem.ScholarshipStudentId != null
                                select new FinancialTransaction
                                           {
                                               StudentId = invoice.StudentId ?? Guid.Empty,
                                               StudentScholarshipId = invoiceItem.ScholarshipStudentId ?? 0,
                                               TermId = invoice.TermId ?? 0,
                                               ReceiptId = receiptItem.ReceiptId,
                                               ReceiptItemId = receiptItem.Id,
                                               PaidAt = paidAt,
                                               PersonalPay = receiptItem.TotalAmount,
                                               UsedScholarship = receiptItem.ScholarshipAmount,
                                               Type = type
                                           }).ToList();

            CreateTransactions(transactions);
        }

        public void CancelTransactionFromReceipt(long receiptId)
        {
            var originalTransactions = (from receiptItem in _db.ReceiptItems
                                        join transaction in _db.FinancialTransactions on receiptItem.Id equals transaction.ReceiptItemId
                                        where receiptItem.ReceiptId == receiptId
                                        select transaction).IgnoreQueryFilters().ToList();

            var transactions = originalTransactions.Select(x => new FinancialTransaction
                                                                {
                                                                    StudentId = x.StudentId,
                                                                    StudentScholarshipId = x.StudentScholarshipId,
                                                                    TermId = x.TermId,
                                                                    ReceiptId = x.ReceiptId,
                                                                    ReceiptItemId = x.ReceiptItemId,
                                                                    PaidAt = DateTime.UtcNow,
                                                                    PersonalPay = x.PersonalPay * -1,
                                                                    UsedScholarship = x.UsedScholarship * -1,
                                                                    Type = "c"
                                                                })
                                                   .ToList();
            
            originalTransactions.ForEach(x => x.IsActive = false);
            CreateTransactions(transactions);
        }

        public void CreateTransactions(List<FinancialTransaction> transactions)
        {
            var balance = GetScholarshipBalance(transactions.FirstOrDefault()?.StudentScholarshipId ?? 0);
            foreach (var transaction in transactions)
            {
                balance -= transaction.UsedScholarship;
                transaction.Balance = balance;

                _db.FinancialTransactions.Add(transaction);
            }

            _db.SaveChanges();
        }

        public bool IsScholarshipLimitExceeded(ScholarshipStudent scholarshipStudent)
        {
            // SCHOLARSHIP
            var limitAmount = _db.Scholarships.AsNoTracking()
                                              .SingleOrDefault(x => x.Id == scholarshipStudent.ScholarshipId)?.LimitedAmount ?? 0;
            if (limitAmount == 0)
            {
                return false;
            }

            // SCHOLARSHIP STUDENT
            var totalScholarShipAmount = _db.ScholarshipStudents.AsNoTracking()
                                                                .Where(x => x.IsActive
                                                                            && x.Id != scholarshipStudent.Id
                                                                            && x.ScholarshipId == scholarshipStudent.ScholarshipId)
                                                                .Sum(x => x.LimitedAmount);
            return limitAmount - totalScholarShipAmount - scholarshipStudent.LimitedAmount < 0;
        }

        public bool IsAnyScholarshipActive(ScholarshipStudent scholarshipStudent)
        {
            return _db.ScholarshipStudents.Any(x => x.Id != scholarshipStudent.Id
                                                    && x.StudentId == scholarshipStudent.StudentId
                                                    && x.IsActive);
        }

        public void CreateTransactionFromVoucher(Voucher voucher)
        {
            var scholarshipStudent = GetScholarshipStudentById(voucher.StudentScholarshipId);
            if (scholarshipStudent != null)
            {
                var balance = GetScholarshipBalance(scholarshipStudent.Id);
                var scholarshipAmount = voucher.TotalAmount;
                var transaction = new FinancialTransaction
                {
                    StudentId = voucher.StudentId,
                    StudentScholarshipId = scholarshipStudent.Id,
                    TermId = voucher.TermId,
                    VoucherId = voucher.Id,
                    UsedScholarship = scholarshipAmount,
                    PersonalPay = 0,
                    Balance = balance - scholarshipAmount,
                    Type = "a"
                };

                _db.FinancialTransactions.Add(transaction);
                _db.SaveChanges();
            }
        }

        public void InactiveTransactionFromVoucher(long voucherId)
        {
            var transaction = _db.FinancialTransactions.FirstOrDefault(x => x.VoucherId == voucherId
                                                                            && x.IsActive);
            if (transaction != null)
            {
                transaction.IsActive = false;
                transaction.Type = "c";
                _db.SaveChanges();
            }
        }

        public List<FinancialTransaction> GetTransactionsByStudent(Guid studentId)
        {
            return _db.FinancialTransactions.Include(x => x.ScholarshipStudent)
                                                .ThenInclude(x => x.Scholarship)
                                            .Include(x => x.Term)
                                            .Include(x => x.Receipt)
                                                .ThenInclude(x => x.Invoice)
                                            .Where(x => x.StudentId == studentId)
                                            .ToList();
        }
    }
}