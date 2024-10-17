using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class VoucherProvider : BaseProvider, IVoucherProvider
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IScholarshipProvider _scholarshipProvider;
        
        public VoucherProvider(ApplicationDbContext db,
                               IStudentProvider studentProvider,
                               IScholarshipProvider scholarshipProvider) : base(db)
        {
            _studentProvider = studentProvider;
            _scholarshipProvider = scholarshipProvider;
        }

        public Voucher GetVoucherById(long id)
        {
            return _db.Vouchers.Include(x => x.Student)
                                    .ThenInclude(x => x.AcademicInformation)
                                .Include(x => x.ScholarshipStudent)
                                    .ThenInclude(x => x.Scholarship)
                                    .ThenInclude(x =>x .BudgetDetails)
                                    .ThenInclude(x => x.FeeGroup)
                                .Include(x => x.VoucherLogs)
                                .SingleOrDefault(x => x.Id == id);
        }

        public bool UpdateVoucherModel(Voucher voucher)
        {
            if (!string.IsNullOrEmpty(voucher.Code))
            {
                var student = _studentProvider.GetStudentByCode(voucher.Code);
                if (student == null)
                {
                    return false;
                }

                var scholarshipStudent = _scholarshipProvider.GetCurrentScholarshipStudent(student.Id);
                if (scholarshipStudent == null)
                {
                    return false;
                }

                voucher.StudentId = student.Id;
                voucher.Student = student;
                voucher.StudentScholarshipId = scholarshipStudent.Id;
                voucher.ScholarshipStudent = scholarshipStudent;
            }
            else
            {
                var student = _studentProvider.GetStudentById(voucher.StudentId);
                var scholarshipStudent = _scholarshipProvider.GetScholarshipStudentById(voucher.StudentScholarshipId);
                voucher.Student = student;
                voucher.ScholarshipStudent = scholarshipStudent;
            }

            return true;
        }

        public List<VoucherLog> GetVoucherLogs(long id)
        {
            return _db.VoucherLogs.Include(x => x.FeeItem)
                                  .Where(x => x.VoucherId == id)
                                  .ToList();
        }

        public List<Voucher> GetVoucherByStudentId(Guid studentId)
        {
            return _db.Vouchers.Include(x => x.Term)
                               .Where(x => x.StudentId == studentId)
                               .ToList();
        }
    }
}