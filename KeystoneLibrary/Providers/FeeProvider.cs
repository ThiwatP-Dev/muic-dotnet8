using AutoMapper;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Scholarship;
using KeystoneLibrary.Models.Report;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Extensions;

namespace KeystoneLibrary.Providers
{
    public class FeeProvider : IFeeProvider
    {
        protected readonly ApplicationDbContext _db;
        protected IRegistrationProvider _registrationProvider { get; }
        protected IAcademicProvider _academicProvider;
        protected ICacheProvider _cacheProvider;
        protected IMasterProvider _masterProvider;
        protected IStudentProvider _studentProvider;
        protected IScholarshipProvider _scholarshipProvider;
        protected IMapper _mapper;

        public FeeProvider(ApplicationDbContext db,
                           IRegistrationProvider registrationProvider,
                           IAcademicProvider academicProvider,
                           ICacheProvider cacheProvider,
                           IMasterProvider masterProvider,
                           IStudentProvider studentProvider,
                           IScholarshipProvider scholarshipProvider,
                           IMapper mapper)
        {
            _db = db;
            _registrationProvider = registrationProvider;
            _academicProvider = academicProvider;
            _cacheProvider = cacheProvider;
            _masterProvider = masterProvider;
            _studentProvider = studentProvider;
            _scholarshipProvider = scholarshipProvider;
            _mapper = mapper;
        }

        public List<TermFee> GetTermFees()
        {
            var termFees = (from termFee in _db.TermFees
                            join termType in _db.TermTypes on termFee.TermTypeId equals termType.Id into termTypes
                            from termType in termTypes.DefaultIfEmpty()
                            select new TermFee
                            {
                                Id = termFee.Id,
                                StartedBatch = termFee.StartedBatch,
                                EndedBatch = termFee.EndedBatch,
                                FacultyId = termFee.FacultyId,
                                DepartmentId = termFee.DepartmentId,
                                CurriculumId = termFee.CurriculumId,
                                CurriculumVersionId = termFee.CurriculumVersionId,
                                TermTypeId = termFee.TermTypeId,
                                Term = termFee.Term,
                                FeeItemId = termFee.FeeItemId,
                                StartedTermId = termFee.StartedTermId,
                                EndedTermId = termFee.EndedTermId,
                                IsOneTime = termFee.IsOneTime,
                                Amount = termFee.Amount,
                                TermType = termType
                            })
                            .IgnoreQueryFilters()
                            .ToList();

            return termFees;
        }

        public List<TermFee> GetStudentTermFees(Guid studentId)
        {
            var student = _db.Students.Include(x => x.StudentFeeGroup)
                                        .ThenInclude(x => x.TermFees)
                                        .ThenInclude(x => x.FeeItem)
                                      .SingleOrDefault(x => x.Id == studentId);

            if (student == null)
            {
                return null;
            }

            var termFees = student.StudentFeeGroup
                                  .TermFees
                                  .Select(x =>
                                  {
                                      var allTerms = _db.Terms.IgnoreQueryFilters();
                                      x.StartedTerm = allTerms.SingleOrDefault(y => y.Id == x.StartedTermId);
                                      x.EndedTerm = allTerms.SingleOrDefault(y => y.Id == x.EndedTermId);
                                      return x;
                                  })
                                  .Where(x => x.IsActive
                                              && (x.StartedBatch == null || x.StartedBatch <= student.AcademicInformation.Batch)
                                              && (x.EndedBatch == null || x.EndedBatch >= student.AcademicInformation.Batch)
                                              && (x.FacultyId == null || x.FacultyId == student.AcademicInformation.FacultyId)
                                              && (x.DepartmentId == null || x.DepartmentId == student.AcademicInformation.DepartmentId)
                                              && (x.CurriculumId == null || x.CurriculumId == student.AcademicInformation.CurriculumVersion.CurriculumId)
                                              && (x.CurriculumVersionId == null || x.CurriculumVersionId == student.AcademicInformation.CurriculumVersionId))
                                  .GroupBy(x => x.FeeItemId)
                                  .Select(x => x.OrderByDescending(y => y.StartedBatch)
                                                .ThenByDescending(y => y.EndedBatch)
                                                .ThenByDescending(y => y.FacultyId)
                                                .ThenByDescending(y => y.DepartmentId)
                                                .ThenByDescending(y => y.CurriculumId)
                                                .ThenByDescending(y => y.CurriculumVersionId)
                                                .First())
                                  .ToList();
            return termFees;
        }

        public List<TermFee> GetStudentTermFees(Guid studentId, long termId)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            if (term == null)
            {
                return null;
            }

            var termFees = GetStudentTermFees(studentId)?.Where(x => (x.TermTypeId == null || x.TermTypeId == term.TermTypeId)
                                                                      && (x.Term == null || x.Term == term.AcademicTerm)
                                                                      && (x.StartedTermId == null
                                                                          || term.AcademicYear > x.StartedTerm.AcademicYear
                                                                          || (term.AcademicYear == x.StartedTerm.AcademicYear
                                                                              && term.AcademicTerm >= x.StartedTerm.AcademicTerm))
                                                                      && (x.EndedTermId == null
                                                                          || term.AcademicYear < x.EndedTerm.AcademicYear
                                                                          || (term.AcademicYear == x.EndedTerm.AcademicYear
                                                                              && term.AcademicTerm <= x.EndedTerm.AcademicTerm)))
                                                         .GroupBy(x => x.FeeItemId)
                                                         .Select(x => x.OrderByDescending(y => y.TermTypeId)
                                                                       .OrderByDescending(y => y.Term)
                                                                       .OrderByDescending(y => y.StartedTermId)
                                                                       .OrderByDescending(y => y.EndedTermId)
                                                                       .First())
                                                         .ToList();

            return termFees;
        }

        public List<InvoiceItem> GetTuitionFees(List<RegistrationCourse> registeringCourses,
                                                long facultyId,
                                                long? departmentId,
                                                long curriculumId,
                                                long? curriculumVersionId,
                                                int batch,
                                                long studentFeeTypeId,
                                                long termId)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            if (term == null)
            {
                return null;
            }

            var fees = (from fee in _db.TuitionFees
                        join registration in registeringCourses on fee.CourseId equals registration.CourseId
                        join section in _db.Sections on registration.SectionId equals section.Id into sections
                        from section in sections.DefaultIfEmpty()
                        join item in _db.FeeItems on fee.FeeItemId equals item.Id
                        join course in _db.Courses on fee.CourseId equals course.Id
                        join faculty in _db.Faculties on fee.FacultyId equals faculty.Id into faculties
                        from faculty in faculties.DefaultIfEmpty()
                        join department in _db.Departments on fee.DepartmentId equals department.Id into departments
                        from department in departments.DefaultIfEmpty()
                        join curriculum in _db.Curriculums on fee.CurriculumId equals curriculum.Id into curriculums
                        from curriculum in curriculums.DefaultIfEmpty()
                        join curriculumVersion in _db.CurriculumVersions on fee.StartedTermId equals curriculumVersion.Id into curriculumVersions
                        from curriculumVersion in curriculumVersions.DefaultIfEmpty()
                        join startedTerm in _db.Terms on fee.StartedTermId equals startedTerm.Id into startedTerms
                        from startedTerm in startedTerms.DefaultIfEmpty()
                        join endedTerm in _db.Terms on fee.EndedTermId equals endedTerm.Id into endedTerms
                        from endedTerm in endedTerms.DefaultIfEmpty()
                        where (fee.FacultyId == null || fee.FacultyId == facultyId)
                              && (fee.DepartmentId == null || fee.DepartmentId == departmentId)
                              && (fee.CurriculumId == null || fee.CurriculumId == curriculumId)
                              && (fee.CurriculumVersionId == null || fee.CurriculumVersionId == curriculumVersionId)
                              && (fee.StartedBatch == null || batch >= fee.StartedBatch)
                              && (fee.EndedBatch == null || batch <= fee.EndedBatch)
                              && (fee.StartedTermId == null
                                  || term.AcademicYear > startedTerm.AcademicYear
                                  || (term.AcademicYear == startedTerm.AcademicYear
                                      && term.AcademicTerm >= startedTerm.AcademicTerm))
                              && (fee.EndedTermId == null
                                  || term.AcademicYear < endedTerm.AcademicYear
                                  || (term.AcademicYear == endedTerm.AcademicYear
                                      && term.AcademicTerm <= endedTerm.AcademicTerm))
                              && (string.IsNullOrEmpty(fee.SectionNumber) || fee.SectionNumber == section.Number)
                        select new
                        {
                            fee,
                            course,
                            FeeItemName = item.NameEn,
                            Amount = fee.Amount,
                            Section = section
                        }).ToList();

            // grouping fee item after calculate
            List<InvoiceItem> invoiceItems = new List<InvoiceItem>();
            if (fees != null && fees.Any())
            {
                foreach (var fee in fees.GroupBy(x => new { x.fee, x.course }))
                {
                    invoiceItems.Add(new InvoiceItem()
                    {
                        CourseId = fee.Key.fee.CourseId,
                        CourseCode = fee.Key.course.Code,
                        CourseName = fee.Key.course.NameEn,
                        CourseAndCredit = fee.Key.course.CourseAndCredit,
                        FeeItemId = fee.Key.fee.FeeItemId,
                        FeeItemName = fee.FirstOrDefault().FeeItemName,
                        Amount = CalculateTuitionFeeRate(fee.Key.fee, fee.Key.course, studentFeeTypeId, batch),
                        SectionId = fee.FirstOrDefault().Section.Id
                    });
                }
            }
            return invoiceItems;
        }

        public List<ScholarshipFeeItem> GetScholarshipFeeItems(Guid studentId, long termId)
        {
            var scholarshipStudent = _scholarshipProvider.GetCurrentScholarshipByTerm(studentId, termId);
            if (scholarshipStudent == null)
            {
                return new List<ScholarshipFeeItem>();
            }

            var feeItems = _db.ScholarshipFeeItems.AsNoTracking()
                                                  .Where(x => x.ScholarshipId == scholarshipStudent.ScholarshipId)
                                                  .GroupBy(x => x.FeeItemId)
                                                  .Select(x => x.OrderByDescending(y => y.UpdatedAt)
                                                                .First())
                                                  .ToList();
            return feeItems;
        }

        public TermFeeModalViewModel GetTermFeeViewModel(long termFeeId)
        {
            var model = (from termFee in _db.TermFees
                         join feeItem in _db.FeeItems on termFee.FeeItemId equals feeItem.Id into feeItems
                         from feeItem in feeItems.DefaultIfEmpty()
                         join startedTerm in _db.Terms on termFee.StartedTermId equals startedTerm.Id into startedTerms
                         from startedTerm in startedTerms.DefaultIfEmpty()
                         join endedTerm in _db.Terms on termFee.EndedTermId equals endedTerm.Id into endedTerms
                         from endedTerm in endedTerms.DefaultIfEmpty()
                         join faculty in _db.Faculties on termFee.FacultyId equals faculty.Id into faculties
                         from faculty in faculties.DefaultIfEmpty()
                         join department in _db.Faculties on termFee.DepartmentId equals department.Id into departments
                         from department in departments.DefaultIfEmpty()
                         join curriculum in _db.Curriculums on termFee.CurriculumId equals curriculum.Id into curriculums
                         from curriculum in curriculums.DefaultIfEmpty()
                         join curriculumVersion in _db.CurriculumVersions on termFee.CurriculumVersionId equals curriculumVersion.Id into curriculumVersions
                         from curriculumVersion in curriculumVersions.DefaultIfEmpty()
                         where termFee.Id == termFeeId
                         select new TermFeeModalViewModel
                         {
                             FeeItemName = feeItem.NameEn,
                             StartedBatch = termFee.StartedBatch ?? 0,
                             EndedBatch = termFee.EndedBatch ?? 0,
                             StartedTerm = startedTerm.TermText,
                             EndedTerm = endedTerm.TermText,
                             Faculty = faculty.NameEn,
                             Department = department.NameEn,
                             Curriculum = curriculum.AbbreviationEn,
                             CurriculumVersion = curriculumVersion.NameEn,
                         })
                        .IgnoreQueryFilters()
                        .SingleOrDefault();

            return model;
        }

        public List<TermFeeSimulateItemViewModel> GetStudentTermFees(long studentFeeGroupId, int batch, long academicLevelId, long termId, int numberOfTerms, long termTypeId)
        {
            List<TermFeeSimulateItemViewModel> items = new List<TermFeeSimulateItemViewModel>();
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var terms = _db.Terms.Where(x => x.AcademicLevelId == academicLevelId
                                             && (x.AcademicYear > term.AcademicYear
                                                 || (x.AcademicYear == term.AcademicYear && x.AcademicTerm >= term.AcademicTerm))
                                             && (termTypeId == 0 || x.TermTypeId == termTypeId))
                                 .OrderBy(x => x.AcademicYear)
                                 .ThenBy(x => x.AcademicTerm)
                                 .Take(numberOfTerms)
                                 .ToList();

            foreach (var item in terms)
            {
                var termFees = from termFee in _db.TermFees
                               join feeItem in _db.FeeItems on termFee.FeeItemId equals feeItem.Id
                               join startTerm in _db.Terms on termFee.StartedTermId equals startTerm.Id into startTerms
                               from startTerm in startTerms.DefaultIfEmpty()
                               join endTerm in _db.Terms on termFee.EndedTermId equals endTerm.Id into endTerms
                               from endTerm in endTerms.DefaultIfEmpty()
                               where termFee.StudentFeeGroupId == studentFeeGroupId
                                     && (termFee.StartedBatch == null
                                         || batch >= termFee.StartedBatch)
                                     && (termFee.EndedBatch == null
                                         || batch <= termFee.EndedBatch)
                                     && (termFee.StartedTermId == null
                                         || (startTerm != null
                                             && (item.AcademicYear > startTerm.AcademicYear
                                                 || (item.AcademicYear == startTerm.AcademicYear && item.AcademicTerm >= startTerm.AcademicTerm))))
                                     && (termFee.EndedTermId == null
                                         || (endTerm != null
                                             && (item.AcademicYear < endTerm.AcademicYear
                                                 || (item.AcademicYear == endTerm.AcademicYear && item.AcademicTerm <= endTerm.AcademicTerm))))
                                     && (termFee.Term == null
                                         || termFee.Term == item.AcademicTerm)
                                     && (termFee.TermTypeId == null
                                         || termFee.TermTypeId == term.TermTypeId)
                               select new
                               {
                                   FeeItemName = feeItem.NameEn,
                                   termFee.TermTypeId,
                                   termFee.Term,
                                   termFee.StartedTermId,
                                   termFee.EndedTermId,
                                   termFee.Amount
                               };

                if (termFees.Any())
                {
                    foreach (var fee in termFees.GroupBy(x => x.FeeItemName))
                    {
                        items.Add(new TermFeeSimulateItemViewModel
                        {
                            TermText = item.TermText,
                            FeeItemName = fee.Key,
                            Amount = fee.OrderByDescending(y => y.TermTypeId)
                                                  .OrderByDescending(y => y.Term)
                                                  .OrderByDescending(y => y.StartedTermId)
                                                  .OrderByDescending(y => y.EndedTermId)
                                                  .First().Amount
                        });
                    }
                }
                else
                {
                    items.Add(new TermFeeSimulateItemViewModel
                    {
                        TermText = item.TermText,
                        FeeItemName = string.Empty,
                        Amount = 0
                    });
                }
            }

            return items;
        }

        public List<StudentFeeGroup> GetStudentFeeGroups(long academicLevelId, long facultyId, long departmentId, long curriculumId, long curriculumVersionId,
                                                         long nationalityId, int batch, long studentGroupId, long studentFeeTypeId)
        {
            var isThai = _masterProvider.GetNationality(nationalityId)?.NameEn.ToLower().Contains("thai") ?? true;
            var studentFeeGroups = _db.StudentFeeGroups.Where(x => (x.AcademicLevelId == null
                                                                    || x.AcademicLevelId == academicLevelId)
                                                                   && (x.FacultyId == null
                                                                       || x.FacultyId == facultyId)
                                                                   && (x.DepartmentId == null
                                                                       || x.DepartmentId == departmentId)
                                                                   && (x.CurriculumId == null
                                                                       || x.CurriculumId == curriculumId)
                                                                   && (x.CurriculumVersionId == null
                                                                       || x.CurriculumVersionId == curriculumVersionId)
                                                                   && (x.NationalityId == null
                                                                       || x.NationalityId == nationalityId)
                                                                   && (x.IsThai == null
                                                                       || x.IsThai == isThai)
                                                                   && (x.StartedBatch == null
                                                                       || x.StartedBatch >= batch)
                                                                   && (x.EndedBatch == null
                                                                       || x.EndedBatch <= batch)
                                                                   && (x.StudentGroupIds == null
                                                                       || !x.StudentGroupIdsLong.Any()
                                                                       || x.StudentGroupIdsLong.Contains(studentGroupId))
                                                                   && (x.StudentFeeTypeId == null
                                                                       || x.StudentFeeTypeId == studentFeeTypeId))
                                                       .ToList();

            studentFeeGroups.Select(x =>
            {
                x.StartedTerm = _academicProvider.GetTerm(x.StartedTermId ?? 0)?.TermText ?? "";
                x.EndedTerm = _academicProvider.GetTerm(x.EndedTermId ?? 0)?.TermText ?? "";
                return x;
            }).ToList();

            studentFeeGroups.Select(x =>
            {
                x.StartedTermAcademicYear = x.StartedTermId != null ? int.Parse(x.StartedTerm.Split('/')[0]) : 0;
                x.StartedTermAcademicTerm = x.StartedTermId != null ? int.Parse(x.StartedTerm.Split('/')[1]) : 0;
                x.EndedTermAcademicYear = x.EndedTermId != null ? int.Parse(x.EndedTerm.Split('/')[0]) : 0;
                x.EndedTermAcademicTerm = x.EndedTermId != null ? int.Parse(x.EndedTerm.Split('/')[1]) : 0;
                return x;
            }).ToList();

            var currentTerm = _cacheProvider.GetCurrentTerm(academicLevelId);
            studentFeeGroups.Where(x => (x.StartedTermAcademicYear < currentTerm.AcademicYear
                                         || (x.StartedTermAcademicYear == currentTerm.AcademicYear
                                             && x.StartedTermAcademicTerm <= currentTerm.AcademicTerm))
                                        && (x.EndedTermAcademicYear > currentTerm.AcademicYear
                                            || (x.EndedTermAcademicYear == currentTerm.AcademicYear
                                                && x.EndedTermAcademicTerm >= currentTerm.AcademicTerm)))
                            .ToList();

            return studentFeeGroups;
        }

        public FeeItem GetFeeItem(long feeItemId)
        {
            var fee = _db.FeeItems.IgnoreQueryFilters()
                                  .SingleOrDefault(x => x.Id == feeItemId);
            return fee;
        }

        public bool SaveFinanceOtherFeeInvoiceAndReceipt(FinanceOtherFeeFormModel model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
                    var address = student.StudentAddresses.Where(x => x.Type == "Current")
                                                          .FirstOrDefault();
                    var invoice = new Invoice
                    {
                        RunningNumber = GetNextInvoiceRunningNumber(),
                        TermId = model.TermId,
                        StudentId = student.Id,
                        Name = student.FullNameEn,
                        Address = address?.AddressEn1 ?? "",
                        Address2 = address?.AddressEn2 ?? "",
                        //IsPaid = true,
                        Amount = 0,
                        TotalVATAmount = 0,
                        TotalAmount = 0,
                        //Type = "o",
                        Year = DateTime.Now.Year
                    };

                    invoice.Number = GetFeeInvoiceNumber(invoice.RunningNumber);

                    _db.Invoices.Add(invoice);
                    _db.SaveChanges();
                    List<InvoiceItem> items = new List<InvoiceItem>();
                    foreach (var item in model.FeeItems.GroupBy(x => x.FeeItemId))
                    {
                        var feeItem = _masterProvider.GetFeeItem(item.Key);
                        if (feeItem.DefaultPrice > 0)
                        {
                            var amount = feeItem.DefaultPrice * item.Sum(x => x.Quantity);
                            var invoiceItem = new InvoiceItem
                            {
                                Type = amount >= 0 ? "o" : "cr",
                                InvoiceId = invoice.Id,
                                FeeItemId = item.Key,
                                FeeItemName = feeItem?.NameEn ?? "",
                                Amount = amount,
                                ScholarshipAmount = 0,
                                // PersonalPayAmount = amount,
                                TotalVATAmount = 0,
                                TotalAmount = amount,
                                IsPaid = amount >= 0
                            };

                            items.Add(invoiceItem);
                        }
                    }

                    _db.InvoiceItems.AddRange(items);
                    invoice.Amount = invoice.TotalAmount = items.Sum(x => x.Amount);
                    invoice.Reference2 = GenerateInvoiceReference2(invoice.CreatedAt, invoice.CreatedAt.AddDays(15));
                    string studentCode = student.Code.Substring(0, 2) + student.Code.Substring(3, 4); // Ignore third digit
                    invoice.Reference1 = GenerateInvoiceReference1(studentCode + invoice.Number, invoice.Reference2, Math.Abs(invoice.TotalAmount));

                    invoice.Type = invoice.TotalAmount >= 0 ? invoice.Type : "cr";
                    invoice.IsPaid = invoice.TotalAmount >= 0;
                    _db.SaveChanges();

                    if (invoice.TotalAmount > 0)
                    {
                        if (model.PaymentMethods == null || invoice.TotalAmount != model.PaymentMethods.Sum(x => x.Amount))
                        {
                            throw new Exception("Paid Amount is not equal");
                        }
                        var paymentTransaction = new BankPaymentResponse
                        {
                            InvoiceId = invoice?.Id,
                            TotalAmount = invoice?.TotalAmount,
                            IsPaymentSuccess = true,
                            PaidAmount = invoice.TotalAmount,
                            Number = invoice.Number,
                            Reference1 = invoice.Reference1,
                            Reference2 = invoice.Reference2,
                            Reference3 = invoice.Reference3,
                            TransactionId = invoice.Id + "",
                            TransactionDateTime = DateTime.UtcNow,
                            RawResponse = $"Other Fee Update By {model.UpdatedBy}",
                            CreatedAt = DateTime.UtcNow.ToString()
                        };
                        _db.BankPaymentResponses.Add(paymentTransaction);
                    }
                    else
                    {
                        var paymentTransaction = new BankPaymentResponse
                        {
                            InvoiceId = invoice?.Id,
                            TotalAmount = invoice?.TotalAmount,
                            IsPaymentSuccess = true,
                            PaidAmount = invoice.TotalAmount,
                            Number = invoice.Number,
                            Reference1 = invoice.Reference1,
                            Reference2 = invoice.Reference2,
                            Reference3 = invoice.Reference3,
                            TransactionId = invoice.Id + "",
                            TransactionDateTime = DateTime.UtcNow,
                            RawResponse = $"MANUAL TOTAL AMOUNT LESS THAN ZERO, Update By {model.UpdatedBy}",
                            CreatedAt = DateTime.UtcNow.ToString()
                        };
                        _db.BankPaymentResponses.Add(paymentTransaction);
                    }

                    var receipt = _mapper.Map<Invoice, Receipt>(invoice);
                    receipt.RunningNumber = GetNextReceiptRunningNumber();
                    receipt.Number = GetFeeReceiptNumber(receipt.RunningNumber, ReceiptNumberType.OTHER);

                    _db.Receipts.Add(receipt);
                    _db.SaveChanges();

                    if (invoice.TotalAmount > 0)
                    {
                        model.PaymentMethods.Select(x =>
                        {
                            x.ReceiptId = receipt.Id;
                            return x;
                        })
                                        .ToList();
                        _db.ReceiptPaymentMethods.AddRange(model.PaymentMethods);
                        _db.SaveChanges();
                    }

                    foreach (var item in items)
                    {
                        var receiptItem = _mapper.Map<InvoiceItem, ReceiptItem>(item);
                        receiptItem.ReceiptId = receipt.Id;
                        receiptItem.RemainingAmount = receiptItem.TotalAmount;

                        _db.ReceiptItems.Add(receiptItem);
                    }
                    _db.SaveChanges();

                    transaction.Commit();

                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public string GenerateReceiptAfterPayingInUSpark(long invoiceId)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var creditNoteInvoice = _db.Invoices.SingleOrDefault(x => x.Id == invoiceId);
                    if (creditNoteInvoice.Type == "cr")
                    {
                        creditNoteInvoice.IsConfirm = true;
                        _db.SaveChanges();
                        transaction.Commit();
                        return "";
                    }
                    else
                    {
                        var invoice = _masterProvider.GetInvoice(invoiceId);
                        if (invoice.IsPaid)
                        {
                            return "Invoice is already paid.";
                        }

                        invoice.InvoiceItems.Select(x =>
                        {
                            x.IsPaid = true;
                            return x;
                        })
                                            .ToList();
                        var receipt = _mapper.Map<Invoice, Receipt>(invoice);
                        receipt.Year = DateTime.UtcNow.Year;
                        receipt.Month = DateTime.UtcNow.Month;
                        //receipt.RunningNumber = GetNextReceiptRunningNumber(invoice.TermId.Value);
                        receipt.RunningNumber = GetNextReceiptRunningNumber();
                        receipt.Number = GetFeeReceiptNumber(receipt.RunningNumber, ReceiptNumberType.REGISTRATION);

                        _db.Receipts.Add(receipt);
                        _db.SaveChanges();

                        // Create Receipt Item
                        receipt.ReceiptItems = invoice.InvoiceItems.Select(x => _mapper.Map<InvoiceItem, ReceiptItem>(x))
                                                            .ToList();

                        receipt.ReceiptItems.Select(x =>
                        {
                            x.ReceiptId = receipt.Id;
                            x.RemainingAmount = x.TotalAmount;
                            return x;
                        })
                                            .ToList();

                        _db.ReceiptItems.AddRange(receipt.ReceiptItems);
                        _db.SaveChanges();

                        // Add Payment Method
                        var ePaymentMethod = _db.PaymentMethods.SingleOrDefault(x => x.NameEn == "E-Payment");
                        var receiptPaymentMethod = new ReceiptPaymentMethod();
                        receiptPaymentMethod.ReceiptId = receipt.Id;
                        receiptPaymentMethod.PaymentMethodId = ePaymentMethod.Id;
                        receiptPaymentMethod.Amount = invoice.TotalAmount;
                        _db.ReceiptPaymentMethods.Add(receiptPaymentMethod);

                        // mark paid flag in invoice when all items are paid
                        var isAllInvoiceItemPaid = invoice.InvoiceItems.All(x => x.IsPaid);
                        invoice.IsPaid = isAllInvoiceItemPaid;

                        _db.SaveChanges();

                        _scholarshipProvider.CreateTransactionFromReceipt(receipt.Id, DateTime.UtcNow, "a");

                        // Registration Course
                        var student = _db.Students.Include(x => x.StudentFeeGroup)
                                                  .SingleOrDefault(x => x.Id == invoice.StudentId);

                        if (student.StudentFeeGroup != null && student.StudentFeeGroup.IsLumpsumPayment)
                        {
                            var registrationCourses = _db.RegistrationCourses.Where(x => x.TermId == invoice.TermId
                                                                                         && x.StudentId == student.Id
                                                                                         && x.Status != "d");
                            if (registrationCourses != null)
                            {
                                foreach (var registrationCourse in registrationCourses)
                                {
                                    registrationCourse.IsPaid = true;
                                }
                                _db.SaveChanges();
                            }
                        }
                        else
                        {
                            var registrationCourseIds = invoice.InvoiceItems.Select(x => x.RegistrationCourseId).Distinct();
                            var registrationCourses = _db.RegistrationCourses.Where(x => registrationCourseIds.Contains(x.Id));
                            if (registrationCourses != null)
                            {
                                foreach (var registrationCourse in registrationCourses)
                                {
                                    registrationCourse.IsPaid = true;
                                }
                                _db.SaveChanges();
                            }
                        }

                        UpdateStudentStateByInvoice(invoice);
                        transaction.Commit();
                        return "";
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateStudentStateByInvoice(Invoice invoice)
        {
            // Student State
            if (invoice.Type == "r" || invoice.Type == "a")
            {
                var studentState = _db.StudentStates.FirstOrDefault(x => x.StudentId == invoice.StudentId
                                                                        && x.TermId == invoice.TermId);

                if (studentState != null)
                {
                    if (studentState.State == "REG" || studentState.State == "PAY_REG")
                    {
                        studentState.State = "ADD";
                        _db.SaveChanges();
                    }
                    else if (studentState.State == "ADD")
                    {
                        studentState.State = "PAY_ADD";
                        _db.SaveChanges();
                    }
                }
            }
        }

        public Invoice GenerateInvoiceNumber(Invoice invoice, string prefix)
        {
            var today = DateTime.UtcNow;
            var latestInvoiceRunningNumber = _db.Receipts.Where(x => x.Month == today.Month
                                                                     && x.Year == today.Year)
                                                         .OrderByDescending(x => x.RunningNumber)
                                                         .FirstOrDefault()?.RunningNumber;
            var runningNumber = (latestInvoiceRunningNumber ?? 0) + 1;

            invoice.Year = today.Year;
            invoice.Month = today.Month;
            invoice.RunningNumber = runningNumber;

            var runningYearString = (invoice.Year % 100).ToString();
            var runningMonthString = invoice.Month.ToString("00");
            var RunningNumberString = invoice.RunningNumber.ToString("00000");

            invoice.Number = $"{ prefix }{ runningYearString }{ runningMonthString }{ RunningNumberString }";

            return invoice;
        }

        // Tuition Fee
        public TuitionFee GetTuitionFee(long academicLevelId, long courseId, long tuitionFeeId)
        {
            var fee = (from tuitionFee in _db.TuitionFees
                       join course in _db.Courses on tuitionFee.CourseId equals course.Id into courses
                       from course in courses.DefaultIfEmpty()
                       where tuitionFee.Id == tuitionFeeId
                       select new
                       {
                           tuitionFee,
                           course.CodeAndName,
                       })
                       .IgnoreQueryFilters()
                       .SingleOrDefault();

            if (fee != null)
            {
                var model = new TuitionFee();
                model = fee.tuitionFee;
                model.AcademicLevelId = academicLevelId;
                model.CourseId = courseId;
                model.Active = fee.tuitionFee.IsActive;
                return model;
            }

            return null;
        }

        public CourseTuitionFeeViewModel GetCourseTuitionFee(long academicLevelId, long courseId)
        {
            var course = _db.Courses.Include(x => x.AcademicLevel)
                                    .Include(x => x.Faculty)
                                    .Include(x => x.Department)
                                    .SingleOrDefault(x => x.Id == courseId);

            var tuitionFees = (from tuitionFee in _db.TuitionFees
                               join faculty in _db.Faculties on tuitionFee.FacultyId equals faculty.Id into facultys
                               from faculty in facultys.DefaultIfEmpty()
                               join department in _db.Departments on tuitionFee.DepartmentId equals department.Id into departments
                               from department in departments.DefaultIfEmpty()
                               join curriculum in _db.Curriculums on tuitionFee.CurriculumId equals curriculum.Id into curriculums
                               from curriculum in curriculums.DefaultIfEmpty()
                               join curriculumVersion in _db.CurriculumVersions on tuitionFee.CurriculumVersionId equals curriculumVersion.Id into curriculumVersions
                               from curriculumVersion in curriculumVersions.DefaultIfEmpty()
                               join studentFeeType in _db.StudentFeeTypes on tuitionFee.StudentFeeTypeId equals studentFeeType.Id into studentFeeTypes
                               from studentFeeType in studentFeeTypes.DefaultIfEmpty()
                               join startedTerm in _db.Terms on tuitionFee.StartedTermId equals startedTerm.Id into startedTerms
                               from startedTerm in startedTerms.DefaultIfEmpty()
                               join endedTerm in _db.Terms on tuitionFee.EndedTermId equals endedTerm.Id into endedTerms
                               from endedTerm in endedTerms.DefaultIfEmpty()
                               join tuitionFeeFormula in _db.TuitionFeeFormulas on tuitionFee.TuitionFeeFormulaId equals tuitionFeeFormula.Id into tuitionFeeFormulas
                               from tuitionFeeFormula in tuitionFeeFormulas.DefaultIfEmpty()
                               join feeItem in _db.FeeItems on tuitionFee.FeeItemId equals feeItem.Id
                               where tuitionFee.CourseId == courseId
                               select new
                               {
                                   TuitionFee = tuitionFee,
                                   Faculty = faculty.CodeAndName,
                                   Department = department.CodeAndName,
                                   Curriculum = curriculum.CodeAndName,
                                   CurriculumVersion = curriculumVersion.CodeAndName,
                                   StudentFeeType = studentFeeType.NameEn,
                                   StartedTerm = startedTerm,
                                   EndedTerm = endedTerm,
                                   TuitionFeeFormula = tuitionFeeFormula.Name,
                                   FeeItem = feeItem
                               })
                              .IgnoreQueryFilters()
                              .ToList()
                              .Select(x =>
                              {
                                  x.TuitionFee.AcademicLevelId = academicLevelId;
                                  x.TuitionFee.CourseId = courseId;
                                  x.TuitionFee.Faculty = x.Faculty == null ? "All" : x.Faculty;
                                  x.TuitionFee.Department = x.Department == null ? "All" : x.Department;
                                  x.TuitionFee.Curriculum = x.Curriculum == null ? "All" : x.Curriculum;
                                  x.TuitionFee.CurriculumVersion = x.CurriculumVersion == null ? "All" : x.CurriculumVersion;
                                  x.TuitionFee.StudentFeeType = x.StudentFeeType == null ? "All" : x.StudentFeeType;
                                  x.TuitionFee.StartedTerm = x.StartedTerm;
                                  x.TuitionFee.EndedTerm = x.EndedTerm;
                                  x.TuitionFee.FeeItem = x.FeeItem;
                                  x.TuitionFee.TuitionFeeFormula = x.TuitionFeeFormula == null ? "-" : x.TuitionFeeFormula;
                                  return x.TuitionFee;
                              })
                              .ToList();

            var courseTuitionFee = new CourseTuitionFeeViewModel
            {
                AcademicLevelId = academicLevelId,
                AcademicLevel = course?.AcademicLevel?.NameEn ?? "",
                Division = course?.Faculty?.CodeAndName ?? "",
                Major = course?.Department?.CodeAndName ?? "",
                CourseId = courseId,
                Course = course?.CodeAndName ?? "",
                TuitionFees = tuitionFees
            };

            return courseTuitionFee;
        }

        public TuitionFeeType GetTuitionFeeType(long? id)
        {
            var model = _db.TuitionFeeTypes.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public TuitionFeeRate GetTuitionFeeRate(long? id)
        {
            var model = _db.TuitionFeeRates.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public TuitionFeeFormula GetTuitionFeeFormula(long? id)
        {
            var model = _db.TuitionFeeFormulas.IgnoreQueryFilters()
                                              .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public bool IsTuitionFeeRateOverlapBatch(TuitionFeeRate model)
        {
            return _db.TuitionFeeRates.Any(x => x.Id != model.Id
                                                && x.TuitionFeeTypeId == model.TuitionFeeTypeId
                                                && x.StartedBatch <= model.EndedBatch
                                                && model.StartedBatch <= x.EndedBatch);
        }

        public string GenerateInvoiceReference1(string invoiceNumber, string reference2, decimal totalAmount)
        {
            var ref1 = "063" + invoiceNumber.PadLeft(14, '0') + "1";
            var checkSum = GenerateInvoiceReference1CheckSum(ref1, reference2, Convert.ToInt32(totalAmount * 100).ToString());
            var reference1 = "063" + invoiceNumber.PadLeft(14, '0') + "1" + checkSum;
            return reference1;
        }

        public string GenerateInvoiceReference2(DateTime createAt, DateTime expiredAt)
        {
            var reference2 = string.Format("{0:yyyyMMddHHmmss}", createAt) + string.Format("{0:ddMMyy}", expiredAt);
            return reference2;
        }

        private string GenerateInvoiceReference1CheckSum(string reference1, string reference2, string reference3)
        {
            reference1 = reference1.PadLeft(18, '0');
            reference2 = reference2.PadLeft(20, '0');
            reference3 = reference3.PadLeft(20, '0');

            int checkSum = 0;
            int checkSum2 = 0;
            int checkSum3 = 0;
            for (int i = 0; i < 20; i++)
            {
                if (i % 2 == 0)
                {
                    if (i < 18)
                    {
                        checkSum += (Convert.ToInt32(reference1[i].ToString()) * 3);
                    }
                    checkSum2 += (Convert.ToInt32(reference2[i].ToString()) * 5);
                    checkSum3 += (Convert.ToInt32(reference3[i].ToString()) * 3);
                }
                else
                {
                    if (i < 18)
                    {
                        checkSum += (Convert.ToInt32(reference1[i].ToString()) * 9);
                    }
                    checkSum2 += (Convert.ToInt32(reference2[i].ToString()) * 7);
                    checkSum3 += (Convert.ToInt32(reference3[i].ToString()) * 5);
                }
            }
            var total = checkSum + checkSum2 + checkSum3;

            string result = total.ToString().PadLeft(3, '0');
            return result.Substring(1, 2);
        }

        public decimal CalculateTuitionFeeRate(TuitionFee tuitionFee, Course course, long studentFeeTypeId, int batch)
        {
            return CalculateTuitionFeeRate(tuitionFee, course, studentFeeTypeId, batch, _db);
        }

        public decimal CalculateTuitionFeeRate(TuitionFee tuitionFee, Course course, long studentFeeTypeId, int batch, ApplicationDbContext db)
        {
            if (tuitionFee == null)
            {
                return 0;
            }
            // Fix
            if (tuitionFee.TuitionFeeFormulaId == null)
            {
                return tuitionFee.Amount;
            }
            else // Formula
            {
                var tuitionFeFormula = GetTuitionFeeFormula(tuitionFee.TuitionFeeFormulaId);
                var customCourseGroupIds = db.CustomCourseGroups.AsNoTracking()
                                                                 .ToList()
                                                                 .Where(x => string.IsNullOrEmpty(x.CourseIds)
                                                                             || JsonConvert.DeserializeObject<List<long>>(x.CourseIds).Contains(course.Id))
                                                                 .Select(x => x.Id).ToList();

                var first = db.TuitionFeeRates.AsNoTracking()
                                               .Where(x => x.TuitionFeeTypeId == tuitionFeFormula.FirstTuitionFeeTypeId
                                                           && (x.StartedBatch == null || x.StartedBatch <= batch)
                                                           && (x.EndedBatch == null || x.EndedBatch >= batch)
                                                           && (x.StudentFeeTypeId == null || x.StudentFeeTypeId == studentFeeTypeId)
                                                           && (x.CustomCourseGroupId == null || customCourseGroupIds.Contains(x.CustomCourseGroupId ?? 0)))
                                               .ToList()
                                               .Where(x => string.IsNullOrEmpty(x.WhitelistMajorIds) || JsonConvert.DeserializeObject<List<long>>(x.WhitelistMajorIds).Contains(course.DepartmentId ?? 0))
                                               .OrderByDescending(x => x.StartedBatch)
                                               .ThenByDescending(x => x.EndedBatch)
                                               .ThenByDescending(x => x.StudentFeeTypeId)
                                               .ThenByDescending(x => x.CustomCourseGroupId)
                                               .ThenByDescending(x => x.WhitelistMajorIds)
                                               .FirstOrDefault();

                var second = db.TuitionFeeRates.AsNoTracking()
                                                .Where(x => x.TuitionFeeTypeId == tuitionFeFormula.SecondTuitionFeeTypeId
                                                            && (x.StartedBatch == null || x.StartedBatch <= batch)
                                                            && (x.EndedBatch == null || x.EndedBatch >= batch)
                                                            && (x.StudentFeeTypeId == null || x.StudentFeeTypeId == studentFeeTypeId)
                                                            && (x.CustomCourseGroupId == null || customCourseGroupIds.Contains(x.CustomCourseGroupId ?? 0)))
                                                .ToList()
                                                .Where(x => string.IsNullOrEmpty(x.WhitelistMajorIds) || JsonConvert.DeserializeObject<List<long>>(x.WhitelistMajorIds).Contains(course.DepartmentId ?? 0))
                                                .OrderByDescending(x => x.StartedBatch)
                                                .ThenByDescending(x => x.EndedBatch)
                                                .ThenByDescending(x => x.StudentFeeTypeId)
                                                .ThenByDescending(x => x.CustomCourseGroupId)
                                                .ThenByDescending(x => x.WhitelistMajorIds)
                                                .FirstOrDefault();

                // A = Credit
                // B = Lecture
                // C = Lab
                return (course.Lecture * (first != null ? first.Amount : 0))
                        + ((course.RegistrationCredit - course.Lecture) * (second != null ? second.Amount : 0));
            }
        }

        public RegistrationFeeInvoice GetRegistrationFeeInvoice(long invoiceId)
        {
            var model = _db.Invoices.Include(x => x.InvoiceItems)
                                        .ThenInclude(x => x.FeeItem)
                                    .Include(x => x.Student)
                                    .Where(x => x.Id == invoiceId
                                        && !x.IsCancel)
                                    .Select(x => new RegistrationFeeInvoice
                                    {
                                        Number = x.Number,
                                        TermId = x.TermId ?? 0,
                                        InvoiceDateText = x.CreatedAtText,
                                        InvoiceType = x.TypeText,
                                        Reference1 = x.Reference1,
                                        Reference2 = x.Reference2,
                                        Reference3 = x.Reference3,
                                        StudentFullName = x.Student.FullNameEn,
                                        InvoiceItems = x.InvoiceItems.Select(y => new RegistrationFeeInvoiceItem
                                        {
                                            Id = y.Id,
                                            Fee = y.FeeItem.CodeAndName,
                                            FeeCode = y.FeeItem.Code,
                                            FeeItem = y.FeeItem.NameEn,
                                            Course = y.Course.CourseAndCredit,
                                            CourseCode = y.Course.Code,
                                            CourseName = y.Course.NameEn,
                                            Section = y.Section.Number,
                                            Type = y.TypeText,
                                            IsPaid = y.IsPaid,
                                            Amount = y.Amount,
                                            ScholarshipAmount = y.ScholarshipAmount,
                                            DiscountAmount = y.DiscountAmount,
                                            DiscountRemark = y.DiscountRemark,
                                            TotalAmount = y.TotalAmount,
                                        })
                                                                                  .ToList(),
                                        TotalDiscount = x.TotalDiscount,
                                        DiscountRemark = x.DiscountRemark,
                                        TotalAmount = x.TotalAmount,
                                        TotalDeductAmount = x.TotalDeductAmount,
                                    })
                                    .SingleOrDefault();
            return model;
        }

        [Obsolete] //try to go use ReceiptProvider.GetRegistrationResultWithAmountAndCreditReport
        public FeeReportViewModel GetFeeReportPivotByStudent(Criteria criteria)
        {
            var feeReportViewModel = new FeeReportViewModel();

            // Fee Group
            feeReportViewModel.FeeGroups = new List<Tuple<long, string>>();
            var feeGroups = _db.FeeGroups.Where(x => criteria.FeeGroupIds == null
                                                     || criteria.FeeGroupIds.Count == 0
                                                     || criteria.FeeGroupIds.Contains(x.Id)).ToList();
            feeReportViewModel.FeeGroups.AddRange(feeGroups.Select(x => new Tuple<long, string>(x.Id, x.NameEn))
                                                           .ToList());

            var feeReportItemViewModels = new List<FeeReportItemViewModel>();
            // Invoice
            var invoiceItems = _db.InvoiceItems.Include(x => x.FeeItem)
                                                    .ThenInclude(x => x.FeeGroup)
                                               .Include(x => x.Invoice)
                                                    .ThenInclude(x => x.Student)
                                                    .ThenInclude(x => x.AcademicInformation)
                                                    .ThenInclude(x => x.Department)
                                                    .ThenInclude(x => x.Faculty)
                                               .Where(x => !x.Invoice.IsCancel
                                                           && x.Invoice.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                           && (criteria.FacultyId == 0
                                                               || x.Invoice.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                           && (criteria.DepartmentId == 0
                                                               || x.Invoice.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                           && (x.Invoice.TermId == criteria.TermId)
                                                           && (string.IsNullOrEmpty(criteria.SlotType)
                                                               || x.Invoice.Type == criteria.SlotType)
                                                           && (string.IsNullOrEmpty(criteria.PaymentStatus)
                                                               || x.IsPaid == (criteria.PaymentStatus == "p")))
                                               .Select(x => new
                                               {
                                                   x.InvoiceId,
                                                   x.Invoice.Number,
                                                   x.Invoice.TypeText,
                                                   x.Invoice.StudentId,
                                                   StudentCode = x.Invoice.Student.Code,
                                                   x.Invoice.Student.FullNameEn,
                                                   Department = x.Invoice.Student.AcademicInformation.Department.Abbreviation,
                                                   Faculty = x.Invoice.Student.AcademicInformation.Department.Faculty.Abbreviation,
                                                   //x.Amount,
                                                   x.TotalAmount,
                                                   FeeGroupId = x.FeeItem.FeeGroup.Id,
                                                   x.Invoice.IsPaid

                                               })
                                               .ToList();
            if (invoiceItems != null && invoiceItems.Any())
            {
                var studentIds = invoiceItems.Select(x => x.StudentId).Distinct().ToList();
                foreach (var studentId in studentIds)
                {
                    var invoiceItem = invoiceItems.FirstOrDefault(x => x.StudentId == studentId);
                    var feeReportItemViewModel = new FeeReportItemViewModel();
                    feeReportItemViewModel.InvoiceNumber = invoiceItem.Number;
                    feeReportItemViewModel.Type = invoiceItem.TypeText;
                    feeReportItemViewModel.StudentCode = invoiceItem.StudentCode;
                    feeReportItemViewModel.StudentName = invoiceItem.FullNameEn;
                    feeReportItemViewModel.Faculty = invoiceItem.Faculty;
                    feeReportItemViewModel.Department = invoiceItem.Department;

                    feeReportItemViewModel.FeeItemAmounts = new List<string>();
                    foreach (var feeGroup in feeReportViewModel.FeeGroups)
                    {
                        if (invoiceItems.Any(x => x.StudentId == studentId
                                                  && x.FeeGroupId == feeGroup.Item1))
                        {
                            var feeGroupAmount = invoiceItems.Where(x => x.StudentId == studentId
                                                                         && x.FeeGroupId == feeGroup.Item1)
                                                             .Sum(x => x.TotalAmount);
                            feeReportItemViewModel.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                        }
                        else
                        {
                            feeReportItemViewModel.FeeItemAmounts.Add("-");
                        }
                    }
                    feeReportItemViewModel.TotalAmount = invoiceItems.Where(x => x.StudentId == studentId)
                                                                     .Sum(x => x.TotalAmount)
                                                                     .ToString(StringFormat.Money);
                    feeReportItemViewModel.Method = GetReceiptPaymentMethodTextByInvoiceId(invoiceItem.InvoiceId);
                    feeReportItemViewModel.Status = invoiceItems.Any(x => x.StudentId == studentId && x.IsPaid) ? "Paid" : "UnPaid";
                    feeReportItemViewModels.Add(feeReportItemViewModel);
                }

                // Summary Row by Department
                var summaryRow = new FeeReportItemViewModel();
                summaryRow.StudentCode = "";
                summaryRow.FeeItemAmounts = new List<string>();
                foreach (var feeGroup in feeReportViewModel.FeeGroups)
                {
                    if (invoiceItems.Any(x => x.FeeGroupId == feeGroup.Item1))
                    {
                        var feeGroupAmount = invoiceItems.Where(x => x.FeeGroupId == feeGroup.Item1)
                                                            .Sum(x => x.TotalAmount);
                        summaryRow.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                    }
                    else
                    {
                        summaryRow.FeeItemAmounts.Add("-");
                    }
                }
                summaryRow.TotalAmount = invoiceItems.Sum(x => x.TotalAmount)
                                                               .ToString(StringFormat.Money);
                feeReportItemViewModels.Add(summaryRow);

            }
            feeReportViewModel.FeeReportItemViewModels = feeReportItemViewModels;
            return feeReportViewModel;
        }

        public FeeReportViewModel GetFeeReportPivotByStudentFacultyDepartment(Criteria criteria)
        {
            var feeReportViewModel = new FeeReportViewModel();

            // Fee Group
            feeReportViewModel.FeeGroups = new List<Tuple<long, string>>();
            var feeGroups = _db.FeeGroups.Where(x => criteria.FeeGroupIds == null
                                                     || criteria.FeeGroupIds.Count == 0
                                                     || criteria.FeeGroupIds.Contains(x.Id))
                                         .ToList();

            feeReportViewModel.FeeGroups.AddRange(feeGroups.Select(x => new Tuple<long, string>(x.Id, x.NameEn))
                                                           .ToList());

            var feeReportItemViewModels = new List<FeeReportItemViewModel>();
            // Invoice
            var invoiceItems = _db.InvoiceItems.Include(x => x.FeeItem)
                                                    .ThenInclude(x => x.FeeGroup)
                                               .Include(x => x.Invoice)
                                                    .ThenInclude(x => x.Student)
                                                    .ThenInclude(x => x.AcademicInformation)
                                                    .ThenInclude(x => x.Department)
                                                    .ThenInclude(x => x.Faculty)
                                               .Where(x => !x.Invoice.IsCancel
                                                           && x.Invoice.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                           && (criteria.FacultyId == 0
                                                               || x.Invoice.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                           && (criteria.DepartmentId == 0
                                                               || x.Invoice.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                           && (x.Invoice.TermId == criteria.TermId)
                                                           && (string.IsNullOrEmpty(criteria.SlotType)
                                                               || x.Invoice.Type == criteria.SlotType)
                                                           && (string.IsNullOrEmpty(criteria.PaymentStatus)
                                                               || x.IsPaid == (criteria.PaymentStatus == "p")))
                                               .Select(x => new
                                               {
                                                   x.InvoiceId,
                                                   x.Invoice.Number,
                                                   x.Invoice.TypeText,
                                                   x.Invoice.StudentId,
                                                   StudentCode = x.Invoice.Student.Code,
                                                   x.Invoice.Student.FullNameEn,
                                                   Department = x.Invoice.Student.AcademicInformation.Department.Abbreviation,
                                                   Faculty = x.Invoice.Student.AcademicInformation.Department.Faculty.Abbreviation,
                                                   //x.Amount,
                                                   x.TotalAmount,
                                                   FeeGroupId = x.FeeItem.FeeGroup.Id,
                                                   x.Invoice.IsPaid
                                               })
                                               .OrderBy(x => x.Faculty)
                                                    .ThenBy(x => x.Department)
                                                    .ThenBy(x => x.StudentCode)
                                               .ToList();
            if (invoiceItems != null && invoiceItems.Any())
            {
                var departments = invoiceItems.Select(x => x.Department).Distinct().ToList();
                foreach (var department in departments)
                {
                    var studentIds = invoiceItems.Where(x => x.Department == department).Select(x => x.StudentId).Distinct().ToList();
                    foreach (var studentId in studentIds)
                    {
                        var invoiceItem = invoiceItems.FirstOrDefault(x => x.StudentId == studentId);
                        var feeReportItemViewModel = new FeeReportItemViewModel();
                        feeReportItemViewModel.InvoiceNumber = invoiceItem.Number;
                        var receipt = GetReceipt(invoiceItem.InvoiceId);
                        if (receipt == null)
                        {
                            feeReportItemViewModel.ReceiptNumber = "-";
                            feeReportItemViewModel.TransactionDateText = "-";
                        }
                        else
                        {
                            feeReportItemViewModel.ReceiptNumber = receipt.Number;
                            feeReportItemViewModel.TransactionDateText = receipt.CreatedAt.ToString(StringFormat.ShortDate);
                        }
                        feeReportItemViewModel.Type = invoiceItem.TypeText;
                        feeReportItemViewModel.StudentCode = invoiceItem.StudentCode;
                        feeReportItemViewModel.StudentName = invoiceItem.FullNameEn;
                        feeReportItemViewModel.Faculty = invoiceItem.Faculty;
                        feeReportItemViewModel.Department = invoiceItem.Department;

                        feeReportItemViewModel.FeeItemAmounts = new List<string>();
                        foreach (var feeGroup in feeReportViewModel.FeeGroups)
                        {
                            if (invoiceItems.Any(x => x.StudentId == studentId
                                                      && x.FeeGroupId == feeGroup.Item1))
                            {
                                var feeGroupAmount = invoiceItems.Where(x => x.StudentId == studentId
                                                                             && x.FeeGroupId == feeGroup.Item1)
                                                                 .Sum(x => x.TotalAmount);
                                feeReportItemViewModel.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                            }
                            else
                            {
                                feeReportItemViewModel.FeeItemAmounts.Add("-");
                            }
                        }
                        feeReportItemViewModel.TotalAmount = invoiceItems.Where(x => x.StudentId == studentId)
                                                                         .Sum(x => x.TotalAmount)
                                                                         .ToString(StringFormat.Money);
                        feeReportItemViewModel.Method = GetReceiptPaymentMethodTextByInvoiceId(invoiceItem.InvoiceId);
                        feeReportItemViewModel.Status = invoiceItems.Any(x => x.StudentId == studentId && x.IsPaid) ? "Paid" : "UnPaid";
                        feeReportItemViewModels.Add(feeReportItemViewModel);
                    }

                    // Summary Row by Department
                    var summaryDepartmentRow = new FeeReportItemViewModel();
                    summaryDepartmentRow.StudentCode = "";
                    summaryDepartmentRow.FeeItemAmounts = new List<string>();
                    foreach (var feeGroup in feeReportViewModel.FeeGroups)
                    {
                        if (invoiceItems.Any(x => x.Department == department
                                                  && x.FeeGroupId == feeGroup.Item1))
                        {
                            var feeGroupAmount = invoiceItems.Where(x => x.Department == department
                                                                         && x.FeeGroupId == feeGroup.Item1)
                                                             .Sum(x => x.TotalAmount);
                            summaryDepartmentRow.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                        }
                        else
                        {
                            summaryDepartmentRow.FeeItemAmounts.Add("-");
                        }
                    }
                    summaryDepartmentRow.TotalAmount = invoiceItems.Where(x => x.Department == department)
                                                                     .Sum(x => x.TotalAmount)
                                                                     .ToString(StringFormat.Money);
                    feeReportItemViewModels.Add(summaryDepartmentRow);
                }

                // Summary Row
                var summaryRow = new FeeReportItemViewModel();
                summaryRow.StudentCode = "";
                summaryRow.FeeItemAmounts = new List<string>();
                foreach (var feeGroup in feeReportViewModel.FeeGroups)
                {
                    if (invoiceItems.Any(x => x.FeeGroupId == feeGroup.Item1))
                    {
                        var feeGroupAmount = invoiceItems.Where(x => x.FeeGroupId == feeGroup.Item1)
                                                         .Sum(x => x.TotalAmount);
                        summaryRow.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                    }
                    else
                    {
                        summaryRow.FeeItemAmounts.Add("-");
                    }
                }
                summaryRow.TotalAmount = invoiceItems.Sum(x => x.TotalAmount)
                                                     .ToString(StringFormat.Money);
                feeReportItemViewModels.Add(summaryRow);
            }
            feeReportViewModel.FeeReportItemViewModels = feeReportItemViewModels;
            return feeReportViewModel;
        }

        public FeeReportViewModel GetFeeReportPivotReceiptOnlyByStudentFacultyDepartment(Criteria criteria)
        {
            var feeReportViewModel = new FeeReportViewModel();

            // Fee Group
            feeReportViewModel.FeeGroups = new List<Tuple<long, string>>();
            var feeGroups = _db.FeeGroups.Where(x => criteria.FeeGroupIds == null
                                                     || criteria.FeeGroupIds.Count == 0
                                                     || criteria.FeeGroupIds.Contains(x.Id))
                                         .ToList();

            feeReportViewModel.FeeGroups.AddRange(feeGroups.Select(x => new Tuple<long, string>(x.Id, x.NameEn))
                                                           .ToList());

            var feeReportItemViewModels = new List<FeeReportItemViewModel>();
            // Receipt
            var receiptItems = _db.ReceiptItems.Include(x => x.FeeItem)
                                                    .ThenInclude(x => x.FeeGroup)
                                               .Include(x => x.Receipt)
                                                    .ThenInclude(x => x.Student)
                                                    .ThenInclude(x => x.AcademicInformation)
                                                    .ThenInclude(x => x.Department)
                                                    .ThenInclude(x => x.Faculty)
                                               .Include(x => x.Invoice)
                                               .Where(x => !x.Invoice.IsCancel
                                                           && x.Invoice.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                           && (criteria.FacultyId == 0
                                                               || x.Invoice.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                           && (criteria.DepartmentId == 0
                                                               || x.Invoice.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                           && (x.Invoice.TermId == criteria.TermId)
                                                           && (string.IsNullOrEmpty(criteria.SlotType)
                                                               || x.Invoice.Type == criteria.SlotType)
                                                           && (!criteria.ReceiptDateFrom.HasValue
                                                               || x.Receipt.CreatedAt.Date >= criteria.ReceiptDateFrom.Value)
                                                           && (!criteria.ReceiptDateTo.HasValue
                                                               || x.Receipt.CreatedAt.Date <= criteria.ReceiptDateTo.Value))
                                               .Select(x => new
                                               {
                                                   InvoiceId = x.Invoice.Id,
                                                   InvoiceNumber = x.Invoice.Number,
                                                   ReceiptNumber = x.Receipt.Number,
                                                   x.Invoice.TypeText,
                                                   x.Invoice.StudentId,
                                                   StudentCode = x.Invoice.Student.Code,
                                                   x.Invoice.Student.FullNameEn,
                                                   Department = x.Invoice.Student.AcademicInformation.Department.Abbreviation,
                                                   Faculty = x.Invoice.Student.AcademicInformation.Department.Faculty.Abbreviation,
                                                   //x.Amount,
                                                   x.TotalAmount,
                                                   FeeGroupId = x.FeeItem.FeeGroup.Id,
                                                   x.Receipt.CreatedAt
                                               })
                                               .OrderBy(x => x.Faculty)
                                                    .ThenBy(x => x.Department)
                                                    .ThenBy(x => x.StudentCode)
                                               .ToList();
            if (receiptItems != null && receiptItems.Any())
            {
                var departments = receiptItems.Select(x => x.Department).Distinct().ToList();
                foreach (var department in departments)
                {
                    var studentIds = receiptItems.Where(x => x.Department == department).Select(x => x.StudentId).Distinct().ToList();
                    foreach (var studentId in studentIds)
                    {
                        var filterReceiptItems = receiptItems.Where(x => x.StudentId == studentId);
                        var receiptItem = filterReceiptItems.FirstOrDefault(x => x.StudentId == studentId);
                        var feeReportItemViewModel = new FeeReportItemViewModel();
                        feeReportItemViewModel.InvoiceNumber = String.Join(",", filterReceiptItems.Select(x => x.InvoiceNumber).Distinct());
                        feeReportItemViewModel.ReceiptNumber = String.Join(",", filterReceiptItems.Select(x => x.ReceiptNumber).Distinct());
                        feeReportItemViewModel.TransactionDateText = receiptItem.CreatedAt.ToString(StringFormat.ShortDate);
                        feeReportItemViewModel.Type = receiptItem.TypeText;
                        feeReportItemViewModel.StudentCode = receiptItem.StudentCode;
                        feeReportItemViewModel.StudentName = receiptItem.FullNameEn;
                        feeReportItemViewModel.Faculty = receiptItem.Faculty;
                        feeReportItemViewModel.Department = receiptItem.Department;

                        feeReportItemViewModel.FeeItemAmounts = new List<string>();
                        foreach (var feeGroup in feeReportViewModel.FeeGroups)
                        {
                            if (filterReceiptItems.Any(x => x.StudentId == studentId
                                                      && x.FeeGroupId == feeGroup.Item1))
                            {
                                var feeGroupAmount = filterReceiptItems.Where(x => x.StudentId == studentId
                                                                             && x.FeeGroupId == feeGroup.Item1)
                                                                 .Sum(x => x.TotalAmount);
                                feeReportItemViewModel.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                            }
                            else
                            {
                                feeReportItemViewModel.FeeItemAmounts.Add("-");
                            }
                        }
                        feeReportItemViewModel.TotalAmount = filterReceiptItems.Sum(x => x.TotalAmount)
                                                                         .ToString(StringFormat.Money);
                        feeReportItemViewModel.Method = GetReceiptPaymentMethodTextByInvoiceId(receiptItem.InvoiceId);
                        feeReportItemViewModel.Status = "Paid";
                        feeReportItemViewModels.Add(feeReportItemViewModel);
                    }

                    // Summary Row by Department
                    var summaryDepartmentRow = new FeeReportItemViewModel();
                    summaryDepartmentRow.StudentCode = "";
                    summaryDepartmentRow.FeeItemAmounts = new List<string>();
                    foreach (var feeGroup in feeReportViewModel.FeeGroups)
                    {
                        if (receiptItems.Any(x => x.Department == department
                                                  && x.FeeGroupId == feeGroup.Item1))
                        {
                            var feeGroupAmount = receiptItems.Where(x => x.Department == department
                                                                         && x.FeeGroupId == feeGroup.Item1)
                                                             .Sum(x => x.TotalAmount);
                            summaryDepartmentRow.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                        }
                        else
                        {
                            summaryDepartmentRow.FeeItemAmounts.Add("-");
                        }
                    }
                    summaryDepartmentRow.TotalAmount = receiptItems.Where(x => x.Department == department)
                                                                     .Sum(x => x.TotalAmount)
                                                                     .ToString(StringFormat.Money);
                    feeReportItemViewModels.Add(summaryDepartmentRow);
                }

                // Summary Row
                var summaryRow = new FeeReportItemViewModel();
                summaryRow.StudentCode = "";
                summaryRow.FeeItemAmounts = new List<string>();
                foreach (var feeGroup in feeReportViewModel.FeeGroups)
                {
                    if (receiptItems.Any(x => x.FeeGroupId == feeGroup.Item1))
                    {
                        var feeGroupAmount = receiptItems.Where(x => x.FeeGroupId == feeGroup.Item1)
                                                         .Sum(x => x.TotalAmount);
                        summaryRow.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                    }
                    else
                    {
                        summaryRow.FeeItemAmounts.Add("-");
                    }
                }
                summaryRow.TotalAmount = receiptItems.Sum(x => x.TotalAmount)
                                                     .ToString(StringFormat.Money);
                feeReportItemViewModels.Add(summaryRow);
            }
            feeReportViewModel.FeeReportItemViewModels = feeReportItemViewModels;
            return feeReportViewModel;
        }

        [Obsolete]
        public FeeReportViewModel GetFeeReportPivotByDepartment(Criteria criteria)
        {
            var feeReportViewModel = new FeeReportViewModel();
            // Department
            var departments = _db.Departments.Include(x => x.Faculty)
                                             .Where(x => (criteria.FacultyId == 0 || x.FacultyId == criteria.FacultyId)
                                                         && (criteria.DepartmentId == 0 || x.FacultyId == criteria.DepartmentId))
                                             .OrderBy(x => x.Abbreviation)
                                             .ToList();

            // Fee Group
            feeReportViewModel.FeeGroups = new List<Tuple<long, string>>();
            var feeGroups = _db.FeeGroups.Where(x => criteria.FeeGroupIds == null
                                                     || criteria.FeeGroupIds.Count == 0
                                                     || criteria.FeeGroupIds.Contains(x.Id)).ToList();
            feeReportViewModel.FeeGroups.AddRange(feeGroups.Select(x => new Tuple<long, string>(x.Id, x.NameEn))
                                                           .ToList());

            var feeReportItemViewModels = new List<FeeReportItemViewModel>();
            // Invoice
            var invoiceItems = _db.InvoiceItems.Include(x => x.FeeItem)
                                                    .ThenInclude(x => x.FeeGroup)
                                               .Include(x => x.Invoice)
                                                    .ThenInclude(x => x.Student)
                                                    .ThenInclude(x => x.AcademicInformation)
                                                    .ThenInclude(x => x.Department)
                                               .Where(x => !x.Invoice.IsCancel
                                                           && x.Invoice.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                           && (criteria.FacultyId == 0
                                                               || x.Invoice.Student.AcademicInformation.FacultyId == criteria.FacultyId)
                                                           && (criteria.DepartmentId == 0
                                                               || x.Invoice.Student.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                           && (x.Invoice.TermId == criteria.TermId)
                                                           && (string.IsNullOrEmpty(criteria.SlotType)
                                                               || x.Invoice.Type == criteria.SlotType)
                                                           && (string.IsNullOrEmpty(criteria.PaymentStatus)
                                                               || x.IsPaid == (criteria.PaymentStatus == "p")))
                                               .Select(x => new
                                               {
                                                   InvoiceId = x.Invoice.Id,
                                                   x.Invoice.Number,
                                                   x.Invoice.TypeText,
                                                   DepartmentId = x.Invoice.Student.AcademicInformation.Department.Id,
                                                   //x.Amount,
                                                   x.TotalAmount,
                                                   FeeGroupId = x.FeeItem.FeeGroup.Id,
                                                   x.Invoice.IsPaid
                                               })
                                               .ToList();

            if (invoiceItems != null && invoiceItems.Any())
            {
                foreach (var department in departments)
                {
                    var feeReportItemViewModel = new FeeReportItemViewModel();
                    feeReportItemViewModel.Faculty = department.Faculty.Abbreviation;
                    feeReportItemViewModel.Department = department.Abbreviation;
                    feeReportItemViewModel.FeeItemAmounts = new List<string>();

                    var invoiceItem = invoiceItems.FirstOrDefault(x => x.DepartmentId == department.Id);
                    if (invoiceItem != null)
                    {
                        foreach (var feeGroup in feeReportViewModel.FeeGroups)
                        {
                            if (invoiceItems.Any(x => x.FeeGroupId == feeGroup.Item1))
                            {
                                var feeGroupAmount = invoiceItems.Where(x => x.DepartmentId == department.Id
                                                                             && x.FeeGroupId == feeGroup.Item1)
                                                                 .Sum(x => x.TotalAmount);
                                feeReportItemViewModel.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                            }
                            else
                            {
                                feeReportItemViewModel.FeeItemAmounts.Add("-");
                            }
                        }
                        feeReportItemViewModel.TotalAmount = invoiceItems.Where(x => x.DepartmentId == department.Id)
                                                                         .Sum(x => x.TotalAmount)
                                                                         .ToString(StringFormat.Money);
                    }
                    else
                    {
                        foreach (var feeGroup in feeReportViewModel.FeeGroups)
                        {
                            feeReportItemViewModel.FeeItemAmounts.Add("-");
                        }
                        feeReportItemViewModel.TotalAmount = "-";
                    }
                    feeReportItemViewModels.Add(feeReportItemViewModel);
                }

                // Summary Row
                var summaryRow = new FeeReportItemViewModel();
                summaryRow.Department = "";
                summaryRow.FeeItemAmounts = new List<string>();
                foreach (var feeGroup in feeReportViewModel.FeeGroups)
                {
                    if (invoiceItems.Any(x => x.FeeGroupId == feeGroup.Item1))
                    {
                        var feeGroupAmount = invoiceItems.Where(x => x.FeeGroupId == feeGroup.Item1)
                                                         .Sum(x => x.TotalAmount);
                        summaryRow.FeeItemAmounts.Add(feeGroupAmount.ToString(StringFormat.Money));
                    }
                    else
                    {
                        summaryRow.FeeItemAmounts.Add("-");
                    }
                }
                summaryRow.TotalAmount = invoiceItems.Sum(x => x.TotalAmount)
                                                     .ToString(StringFormat.Money);
                feeReportItemViewModels.Add(summaryRow);
            }
            feeReportViewModel.FeeReportItemViewModels = feeReportItemViewModels;
            return feeReportViewModel;
        }

        private string GetReceiptPaymentMethodTextByInvoiceId(long invoiceId)
        {
            if (_db.ReceiptPaymentMethods.Include(x => x.Receipt)
                                         .Any(x => x.Receipt.InvoiceId == invoiceId))
            {
                var methods = _db.ReceiptPaymentMethods.Include(x => x.Receipt)
                                                       .Include(x => x.PaymentMethod)
                                                       .Where(x => x.Receipt.InvoiceId == invoiceId)
                                                       .Select(x => x.PaymentMethod.NameEn);
                return string.Join(",", methods);
            }
            else
            {
                return "-";
            }
        }

        private Receipt GetReceipt(long invoiceId)
        {
            return _db.Receipts.FirstOrDefault(x => x.InvoiceId == invoiceId);
        }

        public string GetInvoiceReceiptNumber(long termId, int runningNumber)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            var today = DateTime.UtcNow;

            return $"{today:yy}{term.AcademicTerm}{runningNumber:00000}000";
            //return $"{prefix}{term.AcademicYear}{term.AcademicTerm}{runningNumber:00000}000";
        }

        public string GetFeeReceiptNumber(int runningNumber, ReceiptNumberType type)
        {
            var today = DateTime.UtcNow;
            var runningNumberString = runningNumber.ToString("00000");
            var runningYearString = (today.Year % 100).ToString();
            var runningMonthString = today.Month.ToString("00");

            return $"{ ReceiptNumberTypeExtension.ToStringValue(type) }{ runningMonthString }{ runningYearString }{ runningNumberString }";
        }

        public string GetFeeInvoiceNumber(int runningNumber)
        {
            var today = DateTime.UtcNow;
            var RunningNumberString = runningNumber.ToString("000000");
            var runningYearString = (today.Year % 100).ToString();
            return $"{ runningYearString }{ RunningNumberString }";
        }

        public int GetNextInvoiceRunningNumber()
        {
            var latestInvoiceRunningNumber = _db.Invoices.AsNoTracking()
                                                         .Where(x => x.Year == DateTime.UtcNow.Year)
                                                         .IgnoreQueryFilters()
                                                         .OrderByDescending(x => x.RunningNumber)
                                                         .FirstOrDefault()?.RunningNumber;

            return (latestInvoiceRunningNumber ?? 0) + 1;
        }

        public int GetNextReceiptRunningNumber()
        {
            var latestReceiptRunningNumber = _db.Receipts.Where(x => x.Year == DateTime.UtcNow.Year)
                                                         .IgnoreQueryFilters()
                                                         .OrderByDescending(x => x.RunningNumber)
                                                         .FirstOrDefault()?.RunningNumber;
            return (latestReceiptRunningNumber ?? 0) + 1;
        }

        public void UpdateUSparkPayment(long USOrderId, string receiptNumber, string receiptUrl)
        {
            // var invoice = _db.Invoices.SingleOrDefault(x => x.Id == invoiceId);
            // if (invoice != null && invoice.USOrderId.HasValue)
            // {
            try
            {
                if (String.IsNullOrEmpty(receiptUrl))
                {
                    _db.Database.ExecuteSql($"spUpdateUsparkOrder {USOrderId}, {receiptNumber}");
                }
                else
                {
                    _db.Database.ExecuteSql($"spUpdateUsparkOrder {USOrderId}, {receiptNumber}, {receiptUrl}");
                }
            }
            catch (Exception) { }
            // }
        }

        public USparkCalculateFeeResponse GetUsparkTuitionFeeResponse(string studentCode, long ksTermId)
        {
            try
            {
                var tuitionItems = CalculateTuitionFeeV3(studentCode, ksTermId).ToList();

                if (tuitionItems.Any(x => x.Type == "fee"))
                {
                    if(!tuitionItems.Any(x => x.Type == "one-time-fee")
                        && !tuitionItems.Any(x => x.RegistrationCourseId.HasValue))
                    {
                        return new USparkCalculateFeeResponse
                        {
                            Message = "no registration data"
                        };
                    }
                }

                var penaltyItems = CalculatePenaltyFee(studentCode, ksTermId, tuitionItems).ToList();

                var allItem = tuitionItems.Concat(penaltyItems).ToList();

                if (!allItem.Any())
                {
                    return new USparkCalculateFeeResponse
                    {
                        Message = "all tuition fee is up to date with invoice"
                    };
                }

                var feeItemIds = allItem.Select(x => x.FeeItemId)
                                       .Distinct()
                                       .ToList();

                var feeItems = _db.FeeItems.AsNoTracking()
                                           .Where(x => x.IsActive && feeItemIds.Contains(x.Id))
                                           .ToList();

                var sectionIds = allItem.Where(x => x.SectionId.HasValue)
                                        .Select(x => x.SectionId.Value)
                                        .Distinct()
                                        .ToList();

                var sections = _db.Sections.AsNoTracking()
                                           .Where(x => sectionIds.Contains(x.Id))
                                           .ToList();

                var student = _db.Students.AsNoTracking()
                                          .SingleOrDefault(x => x.Code == studentCode);

                var term = _db.Terms.AsNoTracking()
                                    .SingleOrDefault(x => x.Id == ksTermId);

                var isAddDropTuition = _db.Invoices.AsNoTracking()
                                                   .Any(x => x.StudentId == student.Id
                                                             && x.TermId == ksTermId
                                                             && x.Type == "r");

                var isLateRegisApply = false;

                if (!isAddDropTuition)
                {
                    var registrationLogs = _db.RegistrationLogs.AsNoTracking()
                                                               .Where(x => x.StudentId == student.Id
                                                                           && x.TermId == ksTermId)
                                                               .ToList();

                    var lastRegistrationDate = term.FirstRegistrationEndedAt ?? DateTime.UtcNow.EndOfDay(-1);

                    isLateRegisApply = DateTime.UtcNow > lastRegistrationDate &&
                                           (!registrationLogs.Any()
                                           || !registrationLogs.Where(x => x.CreatedAt < lastRegistrationDate).Any());

                }

                //TODO: this thing
                if (term != null && term.TermTypeId == 2)
                {
                    isLateRegisApply = false; //TODO: Summer no late regis
                }

                var response = new USparkCalculateFeeResponse
                {
                    Message = "Success",
                    Result = new USparkOrder
                    {
                        StudentCode = studentCode,
                        KSTermID = ksTermId,
                        TotalAmount = allItem.Sum(x => x.TotalAmount),
                        OrderDetails = (from item in allItem
                                        let feeItem = feeItems.SingleOrDefault(x => x.Id == item.FeeItemId)
                                        let sectionNumber = !item.SectionId.HasValue ? string.Empty : $"[{sections.SingleOrDefault(x => x.Id == item.SectionId.Value)?.Number ?? string.Empty}]"
                                        select new USparkOrderDetail
                                        {
                                            ItemCode = feeItem?.Code,
                                            ItemNameEn = (item.ScholarshipStudentId.HasValue ? $"Scholarship for " : string.Empty)
                                                                                             + $"{feeItem?.NameEn} {item.CourseAndCredit} {sectionNumber}",
                                            ItemNameTh = (item.ScholarshipStudentId.HasValue ? $"ทุนสำหรับ" : string.Empty)
                                                                                             + $"{feeItem?.NameTh} {item.CourseAndCredit} {sectionNumber}",
                                            KSRegistrationCourseId = item.RegistrationCourseId,
                                            KSCourseId = item.CourseId,
                                            KSSectionId = item.SectionId,
                                            Amount = item.TotalAmount,
                                            LumpSumAddDrop = item.LumpSumAddDrop
                                        })
                                        .ToList(),
                        LatePaymentDate = isAddDropTuition || isLateRegisApply ? term.AddDropPaymentEndedAt
                                                                               : term.FirstRegistrationPaymentEndedAt
                    }
                };

                return response;
            }
            catch (InvalidOperationException ex)
            {
                return new USparkCalculateFeeResponse
                {
                    Message = ex.Message,
                    Result = null
                };
            }
        }

        public UsparkInvoiceAddDropCourses GetUsparkCourseChanges(string studentCode, long ksTermId)
        {
            try
            {
                var tuitionItems = CalculateTuitionFeeV3(studentCode, ksTermId).ToList();

                if (!tuitionItems.Any() || tuitionItems.All(x => !x.RegistrationCourseId.HasValue))
                {
                    return new UsparkInvoiceAddDropCourses
                    {
                        AddCourses = Enumerable.Empty<Guid>(),
                        DropCourses = Enumerable.Empty<Guid>()
                    };
                }

                var registrationIds = (from item in tuitionItems
                                       where item.RegistrationCourseId.HasValue
                                       select item.RegistrationCourseId.Value)
                                      .ToList();

                var registrationCourses = _db.RegistrationCourses.AsNoTracking()
                                                                 .Where(x => registrationIds.Contains(x.Id))
                                                                 .ToList();

                var response = new UsparkInvoiceAddDropCourses
                {
                    AddCourses = (from item in tuitionItems
                                  where item.RegistrationCourseId.HasValue && item.TotalAmount > decimal.Zero
                                  join regis in registrationCourses on item.RegistrationCourseId.Value equals regis.Id
                                  where regis.USPreregistrationId.HasValue
                                  select regis.USPreregistrationId.Value)
                                  .ToList(),
                    DropCourses = (from item in tuitionItems
                                   where item.RegistrationCourseId.HasValue && item.TotalAmount <= decimal.Zero
                                   join regis in registrationCourses on item.RegistrationCourseId.Value equals regis.Id
                                   where regis.USPreregistrationId.HasValue
                                   select regis.USPreregistrationId.Value)
                                  .ToList(),
                };

                return response;
            }
            catch (InvalidOperationException)
            {
                return new UsparkInvoiceAddDropCourses
                {
                    AddCourses = Enumerable.Empty<Guid>(),
                    DropCourses = Enumerable.Empty<Guid>()
                };
            }
        }

        public IEnumerable<InvoiceItem> CalculateTuitionFeeV3(string studentCode, long ksTermId)
        {
            var student = _db.Students.AsNoTracking()
                                      .IgnoreQueryFilters()
                                      .Include(x => x.AcademicInformation)
                                      .ThenInclude(x => x.CurriculumVersion)
                                      .Include(x => x.StudentFeeGroup)
                                      .SingleOrDefault(x => x.Code == studentCode);

            var terms = _db.Terms.AsNoTracking()
                                 .IgnoreQueryFilters()
                                 .ToList();

            var term = terms.SingleOrDefault(x => x.Id == ksTermId);

            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            if (term == null)
            {
                throw new ArgumentNullException(nameof(term));
            }

            ////As of 9 May 2022. Ms.Anothai say registra office must be able to do this
            //var notAllowPaymentForStudentStatusConfig = _db.Configurations.AsNoTracking()
            //                                                        .SingleOrDefault(x => x.Key == "ExcludedRegistrationStatus");

            //var blockStudentStatus = notAllowPaymentForStudentStatusConfig == null
            //                         || string.IsNullOrEmpty(notAllowPaymentForStudentStatusConfig.Value)
            //                                ? Enumerable.Empty<string>()
            //                                : notAllowPaymentForStudentStatusConfig.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
            //                                                                             .Select(x => x.Trim())
            //                                                                             .ToList();

            //if(blockStudentStatus.Any() && blockStudentStatus.Contains(student.StudentStatus, StringComparer.InvariantCultureIgnoreCase))
            //{
            //    throw new InvalidOperationException($"Current student status ({student.StudentStatus}) is not allow to perform fee calculation.");
            //}

            var existingRegistrationCourses = _db.RegistrationCourses.AsNoTracking()
                                                                     .Where(x => x.StudentId == student.Id
                                                                                 && x.TermId == term.Id)
                                                                     .ToList();

            var registerRegistrationCourses = existingRegistrationCourses.Where(x => !x.IsTransferCourse
                                                                                     && !string.Equals(x.Status, "d", StringComparison.InvariantCultureIgnoreCase))
                                                                         .ToList();

            var registerSectionIds = registerRegistrationCourses.Select(x => x.SectionId)
                                                                .Distinct()
                                                                .ToList();

            var registerSections = _db.Sections.AsNoTracking()
                                               .Include(x => x.Course)
                                               .Where(x => registerSectionIds.Contains(x.Id))
                                               .ToList();

            // GET Non-cancel invoice
            var allInvoices = _db.Invoices.AsNoTracking()
                                          .IgnoreQueryFilters()
                                          .Include(x => x.InvoiceItems)
                                          .ThenInclude(x => x.Course)
                                          .Include(x => x.InvoiceItems)
                                          .ThenInclude(x => x.Section)
                                          .Include(x => x.InvoiceItems)
                                          .ThenInclude(x => x.RegistrationCourse)
                                          .Where(x => x.StudentId == student.Id
                                                      && x.TermId == term.Id
                                                      && !x.IsCancel
                                                      && x.Type != "o") // OTHER INVOICE NOT RELATE TO TUITION
                                          .ToList();

            var nonCancelInvoices = allInvoices.Where(x => x.TermId == term.Id)
                                               .ToList();

            // EMPTY LIST SINCE HAVE NON-PAID INVOICE
            if (nonCancelInvoices.Any(x => x.IsActive && !x.IsPaid && x.Amount > decimal.Zero))
            {
                throw new InvalidOperationException("Only one pending payment invoice is allow.");
            }

            var paidInvoiceItems = nonCancelInvoices.Where(x => x.IsPaid || x.Amount <= decimal.Zero)
                                                    .SelectMany(x => x.InvoiceItems)
                                                    .ToList();

            var invoiceSectionSummary = (from item in paidInvoiceItems
                                         where !item.ScholarshipStudentId.HasValue
                                               && item.RegistrationCourseId.HasValue
                                               && !item.RegistrationCourse.IsTransferCourse
                                         group item by item.SectionId into groupBySection
                                         select new
                                         {
                                             SectionId = groupBySection.Key,
                                             Sum = groupBySection.Sum(x => x.Amount)
                                         })
                                         .ToList();

            // NEW COURSES (ADD)
            var newRegistrationSections = (from data in registerSections
                                           join registrationCourse in registerRegistrationCourses on data.Id equals registrationCourse.SectionId
                                           let matchingInvoiceItem = invoiceSectionSummary.SingleOrDefault(x => x.SectionId == data.Id)
                                           where matchingInvoiceItem == null
                                                 || matchingInvoiceItem.Sum <= 0
                                                 || (!registrationCourse.IsPaid && matchingInvoiceItem.Sum > 0) // RE-ADD DROP SECTIONS
                                           select data)
                                          .ToList();

            // REMOVE COURSES (DROP)
            var dropSections = (from data in invoiceSectionSummary
                                let registrationCourse = registerRegistrationCourses.SingleOrDefault(x => x.SectionId == data.SectionId)
                                where data.SectionId != null
                                      && data.Sum > 0
                                      && (registrationCourse == null || !registrationCourse.IsPaid)
                                select data)
                                  .ToList();

            // GET TERM FEE (1st tuition payment)
            var isAlreadyHavePaidInvoice = nonCancelInvoices.Any(x => x.IsPaid);

            var termFees = isAlreadyHavePaidInvoice ? new List<TermFee>()
                                                    : _db.TermFees.AsNoTracking()
                                                                  .Include(x => x.FeeItem)
                                                                  .Where(x => x.IsActive
                                                                              && x.StudentFeeGroupId == student.StudentFeeGroupId)
                                                                  .ToList();

            var activeTermFees = (from data in termFees
                                  let score = CalculateTermFeeScore(terms, term, data, student.AcademicInformation)
                                  where score >= 0
                                  orderby score descending
                                  select data)
                                  .ToList();

            // REGISTRATION COURSE NOT CHANGE AND HAS NO TERM FEE
            if (!newRegistrationSections.Any() && !dropSections.Any() && !activeTermFees.Any())
            {
                return Enumerable.Empty<InvoiceItem>();
            }

            var feeItems = new List<InvoiceItem>();

            var isLumpSumPayment = student.StudentFeeGroupId.HasValue
                                   && student.StudentFeeGroup.IsLumpsumPayment;


            // ADD REFUND / DROP COURSE ITEM
            if (dropSections.Any())
            {
                var refundInvoiceItems = new List<InvoiceItem>();

                foreach (var section in dropSections)
                {
                    var lastPaidItem = paidInvoiceItems.Where(x => x.SectionId == section.SectionId
                                                                    && x.Amount > decimal.Zero)
                                                        .OrderByDescending(x => x.InvoiceId)
                                                        .ThenByDescending(x => x.RegistrationCourseId.Value)
                                                        .First();



                    var refundItem = (from item in paidInvoiceItems
                                      where item.RegistrationCourseId.HasValue
                                            && item.RegistrationCourseId.Value == lastPaidItem.RegistrationCourseId
                                            && item.InvoiceId == lastPaidItem.InvoiceId
                                      select item)
                                     .ToList();

                    refundInvoiceItems.AddRange(refundItem);
                }

                var refundInvoiceItem = (from invoiceItem in refundInvoiceItems
                                         select new InvoiceItem
                                         {
                                             Type = "cr",
                                             SectionId = invoiceItem.SectionId,
                                             CourseId = invoiceItem.CourseId,
                                             RegistrationCourseId = invoiceItem.RegistrationCourseId,
                                             CourseCode = invoiceItem.Course.Code,
                                             CourseName = invoiceItem.Course.NameEn,
                                             CourseAndCredit = invoiceItem.Course.CodeAndCredit,
                                             FeeItemId = invoiceItem.FeeItemId,
                                             FeeItemName = invoiceItem.FeeItemName,
                                             Amount = -1 * invoiceItem.Amount,
                                             TotalAmount = -1 * invoiceItem.TotalAmount,
                                             ScholarshipAmount = -1 * invoiceItem.ScholarshipAmount,
                                             ScholarshipStudentId = invoiceItem.ScholarshipStudentId
                                         })
                                         .ToList();

                feeItems.AddRange(refundInvoiceItem);
            }

            // ADD TERM FEE
            if (activeTermFees != null && activeTermFees.Any())
            {
                var allPaidInvoiceItems = allInvoices.Where(x => x.IsPaid || x.Amount <= decimal.Zero)
                                                     .SelectMany(x => x.InvoiceItems)
                                                     .ToList();

                var matchingYearTermIds = terms.Where(x => x.AcademicYear == term.AcademicYear)
                                               .Select(x => x.Id)
                                               .ToList();

                var sameYearPaidInvoiceItems = allInvoices.Where(x => (x.IsPaid || x.Amount <= decimal.Zero)
                                                                      && x.TermId.HasValue
                                                                      && matchingYearTermIds.Contains(x.TermId.Value))
                                                          .SelectMany(x => x.InvoiceItems)
                                                          .ToList();

                foreach (var fee in activeTermFees)
                {
                    // IS ONE TIME PAYMENT FEE
                    if (fee.IsOneTime)
                    {
                        if (allPaidInvoiceItems.Any(x => x.FeeItemId == fee.FeeItemId))
                        {
                            continue;
                        }
                    }

                    // IS ONE TIME PER ACADEMIC YEAR FEE
                    if (fee.IsPerYear)
                    {
                        if(sameYearPaidInvoiceItems.Any(x => x.FeeItemId == fee.FeeItemId))
                        {
                            continue;
                        }
                    }

                    var termFeeInvoiceItem = new InvoiceItem
                    {
                        // TODO : GET CORRECT TYPE
                        Type = fee.IsOneTime ? "one-time-fee" :
                               fee.IsPerYear ? "yearly-fee" : "fee",
                        FeeItemId = fee.FeeItemId,
                        FeeItemName = fee.FeeItem.NameEn,
                        Amount = fee.Amount,
                        TotalAmount = fee.Amount
                    };

                    feeItems.Add(termFeeInvoiceItem);
                }
            }

            // GENERATE NEW COURSES
            if (newRegistrationSections.Any())
            {
                var courseIds = newRegistrationSections.Select(x => x.CourseId)
                                                      .Distinct()
                                                      .ToList();


                var tuitionFees = _db.TuitionFees.AsNoTracking()
                                                 .Where(x => x.CourseId.HasValue
                                                             && courseIds.Contains(x.CourseId.Value))
                                                 .ToList();

                var applyTuitionFees = new List<TuitionFee>();

                var groupFeeItem = tuitionFees.GroupBy(x => new { x.CourseId, x.FeeItemId })
                                              .ToList();

                foreach (var groupFee in groupFeeItem)
                {
                    var applyFee = (from data in groupFee
                                    let score = CalculateTuitionFeeScore(terms, term, newRegistrationSections, data, student.AcademicInformation)
                                    where score >= 0
                                    orderby score descending
                                    select data)
                                    .FirstOrDefault();

                    if (applyFee == null)
                    {
                        continue;
                    }

                    applyTuitionFees.Add(applyFee);
                }


                var courses = _db.Courses.AsNoTracking()
                                         .Where(x => courseIds.Contains(x.Id))
                                         .ToList();

                var feeItemIds = applyTuitionFees.Select(x => x.FeeItemId)
                                                 .Distinct()
                                                 .ToList();

                var tuitionFeeItems = _db.FeeItems.AsNoTracking()
                                                  .Where(x => feeItemIds.Contains(x.Id))
                                                  .ToList();

                var customCourseGroup = _db.CustomCourseGroups.AsNoTracking()
                                                              .ToList();

                var tuitionFeeFormulas = _db.TuitionFeeFormulas.AsNoTracking()
                                                              .Where(x => x.IsActive)
                                                              .ToList();

                var tuitionFeeRates = _db.TuitionFeeRates.AsNoTracking()
                                                         .Where(x => x.IsActive
                                                                     && x.StudentFeeTypeId == student.StudentFeeTypeId)
                                                         .ToList();

                var matchingTuitionFeeRate = new List<TuitionFeeRate>();

                foreach (var rate in tuitionFeeRates)
                {
                    if (rate.StartedBatch.HasValue
                        && rate.StartedBatch.Value > student.AcademicInformation.Batch)
                    {
                        continue;
                    }

                    if (rate.EndedBatch.HasValue
                       && rate.EndedBatch.Value < student.AcademicInformation.Batch)
                    {
                        continue;
                    }

                    matchingTuitionFeeRate.Add(rate);
                }

                var newInvoiceItems = (from fee in applyTuitionFees
                                       join feeItem in tuitionFeeItems on fee.FeeItemId equals feeItem.Id
                                       join course in courses on fee.CourseId equals course.Id
                                       let registerSection = newRegistrationSections.SingleOrDefault(x => x.CourseId == fee.CourseId)
                                       let amount = CalculateCourseTuitionFee(
                                                       applyTuitionFees, courses,
                                                       tuitionFeeFormulas, customCourseGroup,
                                                       matchingTuitionFeeRate,
                                                       fee.Id, course.Id, student)
                                       where registerSection != null
                                       select new InvoiceItem
                                       {
                                           // TODO : GET CORRECT TYPE
                                           Type = "a",
                                           SectionId = registerSection.Id,
                                           CourseId = course.Id,
                                           RegistrationCourseId = registerRegistrationCourses.SingleOrDefault(x => x.CourseId == course.Id && x.SectionId == registerSection.Id)?.Id,
                                           CourseCode = course.Code,
                                           CourseName = course.NameEn,
                                           CourseAndCredit = course.CodeAndCredit,
                                           FeeItemId = feeItem.Id,
                                           FeeItemName = feeItem.NameEn,
                                           Amount = amount,
                                           TotalAmount = amount
                                       })
                                      .ToList();

                feeItems.AddRange(newInvoiceItems);

            }


            // APPLY SCHOLARSHIP LOGIC IF NOT LUMP SUM PAYMENT
            if (!isLumpSumPayment)
            {
                // APPLY SCHOLARSHIP
                var studentScholarships = _db.ScholarshipStudents.AsNoTracking()
                                                                 .Include(x => x.FinancialTransactions)
                                                                 .Include(x => x.Scholarship)
                                                                 .ThenInclude(x => x.ScholarshipFeeItems)
                                                                 .Where(x => x.StudentId == student.Id
                                                                             && x.IsActive
                                                                             && x.IsApproved)
                                                                 .ToList();

                var effectiveScholarships = (from data in studentScholarships
                                             where IsActiveScholarShip(terms, term, data)
                                             select data)
                                            .ToList();

                if (effectiveScholarships.Any())
                {
                    var fullAmountScholarShip = effectiveScholarships.Where(x => x.Scholarship.IsFullAmount)
                                                                     .FirstOrDefault();

                    var isFullAmountScholarship = fullAmountScholarShip != null;

                    if (isFullAmountScholarship)
                    {
                        var scholarshipInvoiceItems = (from item in feeItems
                                                       where item.Amount > decimal.Zero
                                                       select new InvoiceItem
                                                       {
                                                           // TODO : GET CORRECT TYPE
                                                           Type = "sc",
                                                           SectionId = item.SectionId,
                                                           CourseId = item.CourseId,
                                                           RegistrationCourseId = item.RegistrationCourseId,
                                                           CourseCode = item.CourseCode,
                                                           CourseName = item.CourseName,
                                                           CourseAndCredit = item.CourseAndCredit,
                                                           FeeItemId = item.FeeItemId,
                                                           FeeItemName = $"Scholarship {item.FeeItemName}",
                                                           Amount = 0,
                                                           ScholarshipStudentId = fullAmountScholarShip.Id,
                                                           ScholarshipAmount = Math.Abs(item.TotalAmount),
                                                           TotalAmount = item.TotalAmount * -1
                                                       })
                                                       .ToList();

                        feeItems.AddRange(scholarshipInvoiceItems);
                    }
                    else
                    {
                        var refundScholarships = (from item in feeItems
                                                  where item.Type == "cr" && item.ScholarshipStudentId.HasValue
                                                  group item by item.ScholarshipStudentId.Value into groupByScholarship
                                                  select new
                                                  {
                                                      ScholarshipStudentId = groupByScholarship.Key,
                                                      Amount = Math.Abs(groupByScholarship.Sum(x => x.ScholarshipAmount))
                                                  })
                                                 .ToList();

                        foreach (var refundItem in refundScholarships)
                        {
                            var scholarShip = effectiveScholarships.SingleOrDefault(x => x.Id == refundItem.ScholarshipStudentId);

                            if (scholarShip is null)
                            {
                                continue;
                            }

                            var refundRecord = new FinancialTransaction { UsedScholarship = refundItem.Amount * -1 };

                            scholarShip.FinancialTransactions.Add(refundRecord);
                        }

                        var scholarshipFeeItems = effectiveScholarships.SelectMany(x => x.Scholarship.ScholarshipFeeItems)
                                                                       .ToList();

                        var applyScholarshipFeeItems = (from data in scholarshipFeeItems
                                                        where data.IsActive
                                                        group data by data.FeeItemId into groupByFeeItemId
                                                        let discountData = groupByFeeItemId.OrderByDescending(x => x.Percentage).First()
                                                        select discountData)
                                                        .ToList();

                        foreach (var item in applyScholarshipFeeItems)
                        {
                            if (item.Percentage == decimal.Zero)
                            {
                                continue;
                            }

                            var scholarShipInfo = effectiveScholarships.Single(x => x.ScholarshipId == item.ScholarshipId);

                            var isUnlimitedScholarShip = scholarShipInfo.LimitedAmount == decimal.Zero;

                            var remainingAmount = isUnlimitedScholarShip ? decimal.MaxValue
                                                                         : scholarShipInfo.LimitedAmount - scholarShipInfo.FinancialTransactions.Sum(x => x.UsedScholarship);

                            var addCourseInvoiceItems = feeItems.Where(x => x.FeeItemId == item.FeeItemId
                                                                            && x.Amount > decimal.Zero)
                                                                .ToList();

                            foreach (var invoiceItem in addCourseInvoiceItems)
                            {
                                if (!isUnlimitedScholarShip && remainingAmount == decimal.Zero)
                                {
                                    break;
                                }

                                var discountAmount = new[] { invoiceItem.Amount * (item.Percentage / 100), remainingAmount }.Min();

                                remainingAmount -= discountAmount;

                                scholarShipInfo.FinancialTransactions.Add(new FinancialTransaction { UsedScholarship = discountAmount });

                                var discountInvoiceItems = new InvoiceItem
                                {
                                    // TODO : GET CORRECT TYPE
                                    Type = "sc",
                                    SectionId = invoiceItem.SectionId,
                                    CourseId = invoiceItem.CourseId,
                                    RegistrationCourseId = invoiceItem.RegistrationCourseId,
                                    CourseCode = invoiceItem.CourseCode,
                                    CourseName = invoiceItem.CourseName,
                                    CourseAndCredit = invoiceItem.CourseAndCredit,
                                    FeeItemId = invoiceItem.FeeItemId,
                                    FeeItemName = $"Scholarship {invoiceItem.FeeItemName}",
                                    Amount = 0,
                                    ScholarshipStudentId = scholarShipInfo.Id,
                                    ScholarshipAmount = discountAmount,
                                    TotalAmount = discountAmount * -1
                                };

                                feeItems.Add(discountInvoiceItems);
                            }
                        }

                    }
                }
            }

            var totalCourseInvoiceAmount = feeItems.Sum(x => x.TotalAmount);

            // APPLY LUMPSUM DISCOUNT
            if (isLumpSumPayment)
            {
                var feeItemIds = feeItems.Select(x => x.FeeItemId).Distinct()
                                                                  .ToList();

                var feeItemData = _db.FeeItems.AsNoTracking()
                                              .Where(x => feeItemIds.Contains(x.Id))
                                              .ToList();

                var lumpsumDiscount = (from invoiceItem in feeItems
                                       join data in feeItemData on invoiceItem.FeeItemId equals data.Id
                                       where !data.IsLumpsum
                                       select invoiceItem.TotalAmount)
                                       .Sum();

                var config = _db.Configurations.AsNoTracking()
                                               .SingleOrDefault(x => x.Key == "LumpsumDiscount");

                var discountId = config is null ? null
                                                : long.TryParse(config.Value, out long feeItemId) ? feeItemId
                                                                                                  : (long?)null;

                var feeItem = !discountId.HasValue ? null
                                                   : _db.FeeItems.AsNoTracking()
                                                                 .SingleOrDefault(x => x.Id == discountId);

                var lumpsumDiscountItem = new InvoiceItem
                {
                    Type = "lumpsum",
                    FeeItemId = feeItem?.Id ?? 0,
                    FeeItemName = "Lumpsum discount",
                    Amount = lumpsumDiscount * -1,
                    TotalAmount = lumpsumDiscount * -1
                };

                feeItems.Add(lumpsumDiscountItem);

                totalCourseInvoiceAmount += (lumpsumDiscount * -1);
            }

            // APPLY REFUND PENALTY IF COURSE TUITION IS LESS THAN ZERO
            if (!isLumpSumPayment && totalCourseInvoiceAmount < decimal.Zero)
            {
                var feeItemIdConfiguration = _db.Configurations.AsNoTracking()
                                                               .FirstOrDefault(x => x.Key == "TuitionFeeRefundDeduction");

                if (feeItemIdConfiguration != null
                   && long.TryParse(feeItemIdConfiguration.Value, out long feeItemId))
                {
                    var feeItem = _db.FeeItems.AsNoTracking()
                                              .SingleOrDefault(x => x.Id == feeItemId);

                    var refundPenaltyItem = new InvoiceItem
                    {
                        Type = "fee_deduct",
                        FeeItemId = feeItem?.Id ?? 0,
                        FeeItemName = feeItem?.NameEn,
                        Amount = Math.Abs(totalCourseInvoiceAmount * (feeItem.RefundPercentage / 100)),
                        TotalAmount = Math.Abs(totalCourseInvoiceAmount * (feeItem.RefundPercentage / 100))
                    };

                    feeItems.Add(refundPenaltyItem);
                }
            }

            // APPLY SCHOLARSHIP REFUND PENALTY
            var totalScholarshipFee = feeItems.Where(x => x.ScholarshipStudentId.HasValue)
                                              .GroupBy(x => x.ScholarshipStudentId)
                                              .ToList();

            foreach (var scholarship in totalScholarshipFee)
            {
                var totalAmount = scholarship.Sum(x => x.ScholarshipAmount);

                if (totalAmount >= decimal.Zero)
                {
                    continue;
                }

                var config = _db.Configurations.AsNoTracking()
                                               .SingleOrDefault(x => x.Key == "ScholarShipRefundDeduction");

                var discountId = config is null ? null
                                                : long.TryParse(config.Value, out long feeItemId) ? feeItemId
                                                                                                  : (long?)null;
                var feeItem = !discountId.HasValue ? null
                                                   : _db.FeeItems.AsNoTracking()
                                                                 .SingleOrDefault(x => x.Id == discountId);

                var deductAmount = feeItem != null ? Math.Abs(totalAmount * (feeItem.RefundPercentage / 100))
                                                   : Math.Abs(totalAmount * 0.1m);

                var scholarShipDeductFee = new InvoiceItem
                {
                    Type = "sc_deduct",
                    FeeItemId = feeItem?.Id ?? 0,
                    FeeItemName = feeItem.NameEn,
                    Amount = 0,
                    ScholarshipStudentId = scholarship.Key,
                    ScholarshipAmount = deductAmount,
                    TotalAmount = -1 * deductAmount
                };

                var balanceSchoplarShipDeductFee = new InvoiceItem
                {
                    Type = "sc_deduct",
                    FeeItemId = feeItem?.Id ?? 0,
                    FeeItemName = feeItem.NameEn,
                    Amount = 0,
                    ScholarshipStudentId = null,
                    ScholarshipAmount = -1 * deductAmount,
                    TotalAmount = deductAmount
                };

                feeItems.Add(scholarShipDeductFee);
                feeItems.Add(balanceSchoplarShipDeductFee);
            }

            return feeItems;
        }

        public IEnumerable<InvoiceItem> CalculatePenaltyFee(string studentCode, long ksTermId, IEnumerable<InvoiceItem> courseInvoiceItems)
        {
            var student = _db.Students.AsNoTracking()
                                      .Include(x => x.StudentFeeGroup)
                                      .SingleOrDefault(x => x.Code == studentCode);

            var isLumpSumStudent = student.StudentFeeGroupId.HasValue && student.StudentFeeGroup.IsLumpsumPayment;

            var term = _db.Terms.AsNoTracking()
                                .SingleOrDefault(x => x.Id == ksTermId);

            var registrationLogs = _db.RegistrationLogs.AsNoTracking()
                                                       .Where(x => x.StudentId == student.Id
                                                                   && x.TermId == term.Id)
                                                       .ToList();

            var now = DateTime.UtcNow;
            var lastRegistrationDate = term.FirstRegistrationEndedAt ?? DateTime.UtcNow.EndOfDay(-1);
            var registrationLatePaymentDate = term.FirstRegistrationPaymentEndedAt ?? DateTime.UtcNow.EndOfDay(-1);
            var addLatePaymentDate = term.AddDropPaymentEndedAt ?? DateTime.UtcNow.EndOfDay(-1);

            var isLateRegisApply = now > lastRegistrationDate &&
                                   (!registrationLogs.Any()
                                    || !registrationLogs.Where(x => x.CreatedAt < lastRegistrationDate).Any());

            //TODO: this thing
            if (term != null && term.TermTypeId == 2)
            {
                isLateRegisApply = false; //TODO: Summer no late regis
            }

            var invoices = _db.Invoices.AsNoTracking()
                                       .Include(x => x.InvoiceItems)
                                       .Where(x => x.StudentId == student.Id
                                                   && x.TermId == term.Id
                                                   && !x.IsCancel
                                                   && x.Type != "o")
                                       .ToList();

            // ONLY CONSIDIER INVOICE WITH REGISTRATION COURSE
            var countAddDropInvoiceIds = invoices.SelectMany(x => x.InvoiceItems)
                                                 .Where(x => x.RegistrationCourseId.HasValue)
                                                 .Select(x => x.InvoiceId)
                                                 .Distinct()
                                                 .ToList();

            var addDropCount = isLumpSumStudent ? 0 : countAddDropInvoiceIds.Count() - 1;

            var paidInvoices = invoices.Where(x => x.IsPaid || x.TotalAmount <= decimal.Zero)
                                       .ToList();

            var latePaymentDate = isLateRegisApply || paidInvoices.Any() ? addLatePaymentDate
                                                                         : registrationLatePaymentDate;

            var latePaymentDayCount = DateTime.UtcNow < latePaymentDate ? 0
                                                                        : new[] { 0, Convert.ToInt32(Math.Ceiling((DateTime.UtcNow - latePaymentDate).TotalDays)) }.Max();

            var pendingPaymentAmount = courseInvoiceItems.Sum(x => x.TotalAmount);

            var response = new List<InvoiceItem>();

            if (isLateRegisApply)
            {
                var lateRegistrationConfigurations = _db.LateRegistrationConfigurations.AsNoTracking()
                                                                                      .Include(x => x.FromTerm)
                                                                                      .Include(x => x.ToTerm)
                                                                                      .ToList();

                var applyConfiguration = (from config in lateRegistrationConfigurations
                                          where IsMatchingTermPeriod(term, config.FromTerm, config.ToTerm)
                                          select config)
                                          .FirstOrDefault();

                if (applyConfiguration != null)
                {
                    var feeItemIdConfiguration = _db.Configurations.AsNoTracking()
                                                                   .FirstOrDefault(x => x.Key == "LateRegistrationFeeItem");

                    if (feeItemIdConfiguration != null
                       && long.TryParse(feeItemIdConfiguration.Value, out long feeItemId))
                    {
                        var isAlreadyPaidLateRegis = paidInvoices.SelectMany(x => x.InvoiceItems)
                                                                 .Any(x => x.FeeItemId == feeItemId);

                        if (!isAlreadyPaidLateRegis)
                        {
                            var feeItem = _db.FeeItems.AsNoTracking()
                                                      .SingleOrDefault(x => x.Id == feeItemId);

                            var lateRegisItem = new InvoiceItem
                            {
                                Type = "fee",
                                FeeItemId = feeItem.Id,
                                FeeItemName = feeItem.NameEn,
                                Amount = applyConfiguration.Amount,
                                ScholarshipAmount = 0,
                                TotalAmount = applyConfiguration.Amount,
                            };

                            response.Add(lateRegisItem);

                            pendingPaymentAmount += lateRegisItem.TotalAmount;
                        }
                    }
                }
            }

            if (addDropCount > 0 && courseInvoiceItems.Any(x => x.Type == "a" || x.Type == "cr"))
            {
                var addDropConfigurations = _db.AddDropFeeConfigurations.AsNoTracking()
                                                                       .Include(x => x.FromTerm)
                                                                       .Include(x => x.ToTerm)
                                                                       .ToList();

                var applyAddDropConfiguration = (from config in addDropConfigurations
                                                 where IsMatchingTermPeriod(term, config.FromTerm, config.ToTerm)
                                                 select config)
                                                 .FirstOrDefault();

                if (applyAddDropConfiguration != null && addDropCount >= applyAddDropConfiguration.FreeAddDropCount)
                {
                    var feeItemIdConfiguration = _db.Configurations.AsNoTracking()
                                                                   .FirstOrDefault(x => x.Key == "AddDropFeeItem");

                    if (feeItemIdConfiguration != null
                       && long.TryParse(feeItemIdConfiguration.Value, out long feeItemId))
                    {
                        var feeItem = _db.FeeItems.AsNoTracking()
                                                  .SingleOrDefault(x => x.Id == feeItemId);

                        var addDropFeeItem = new InvoiceItem
                        {
                            Type = "fee",
                            FeeItemId = feeItem.Id,
                            FeeItemName = feeItem.NameEn,
                            Amount = applyAddDropConfiguration.Amount,
                            ScholarshipAmount = 0,
                            TotalAmount = applyAddDropConfiguration.Amount,
                        };

                        response.Add(addDropFeeItem);

                        pendingPaymentAmount += addDropFeeItem.TotalAmount;
                    }
                }
            }

            // TODO : CONFIRM FLOW ON CALCULATE LATE PAYMENT FOR REFUND INVOICE
            if (latePaymentDayCount > 0
                && pendingPaymentAmount > decimal.Zero
                && (courseInvoiceItems != null && courseInvoiceItems.Any()))
            {
                var latePaymentConfigurations = _db.LatePaymentConfigurations.AsNoTracking()
                                                                             .Include(x => x.FromTerm)
                                                                             .Include(x => x.ToTerm)
                                                                             .ToList();

                var applyLatePaymentConfiguration = (from config in latePaymentConfigurations
                                                     where IsMatchingTermPeriod(term, config.FromTerm, config.ToTerm)
                                                     select config)
                                                     .FirstOrDefault();

                if (applyLatePaymentConfiguration != null)
                {
                    var feeItemIdConfiguration = _db.Configurations.AsNoTracking()
                                                                   .FirstOrDefault(x => x.Key == "LatePaymentFeeItem");

                    if (feeItemIdConfiguration != null
                       && long.TryParse(feeItemIdConfiguration.Value, out long feeItemId))
                    {
                        var feeItem = _db.FeeItems.AsNoTracking()
                                                  .SingleOrDefault(x => x.Id == feeItemId);

                        if (feeItem != null)
                        {
                            var applyDays = new[] { latePaymentDayCount, applyLatePaymentConfiguration.MaximumDays }.Min();

                            var isMaxLatePaymentPenaltyApply = applyDays == applyLatePaymentConfiguration.MaximumDays;

                            var totalAmount = applyLatePaymentConfiguration.AmountPerDay * applyDays;

                            var latePaymentItem = new InvoiceItem
                            {
                                Type = "latepayment_fee",
                                FeeItemId = feeItem.Id,
                                FeeItemName = feeItem.NameEn,
                                Amount = totalAmount,
                                ScholarshipAmount = 0,
                                TotalAmount = totalAmount,
                                LatePaymentExpiryDate = isMaxLatePaymentPenaltyApply ? term.LastPaymentEndedAt ?? DateTime.UtcNow.EndOfDay(-1)
                                                                                      : now.EndOfDay().AddHours(-7)
                            };

                            response.Add(latePaymentItem);
                        }
                    }
                }
            }

            return response;
        }

        private static double CalculateTermFeeScore(IEnumerable<Term> terms, Term compareTerm,
                                                 TermFee termFee, AcademicInformation academicInformation)
        {
            if (terms == null
                || !terms.Any()
                || compareTerm == null
                || termFee == null
                || academicInformation == null)
            {
                return -1;
            }

            var score = 0d;

            if (termFee.StartedTermId.HasValue)
            {
                var matchingTerm = terms.SingleOrDefault(x => x.Id == termFee.StartedTermId.Value);

                if (matchingTerm == null
                    || matchingTerm.AcademicYear > compareTerm.AcademicYear
                    || (
                          matchingTerm.AcademicYear == compareTerm.AcademicYear
                          && matchingTerm.AcademicTerm > compareTerm.AcademicTerm
                       ))
                {
                    return -1;
                }
            }

            if (termFee.EndedTermId.HasValue)
            {
                var matchingTerm = terms.SingleOrDefault(x => x.Id == termFee.EndedTermId.Value);

                if (matchingTerm == null
                    || matchingTerm.AcademicYear < compareTerm.AcademicYear
                    || (
                          matchingTerm.AcademicYear == compareTerm.AcademicYear
                          && matchingTerm.AcademicTerm < compareTerm.AcademicTerm
                       ))
                {
                    return -1;
                }
            }

            if (termFee.TermTypeId.HasValue)
            {
                if (compareTerm.TermTypeId != termFee.TermTypeId.Value)
                {
                    return -1;
                }
            }

            if (termFee.StartedBatch.HasValue)
            {
                if (termFee.StartedBatch.Value > academicInformation.Batch)
                {
                    return -1;
                }

                score += Math.Pow(2, 5);
            }

            if (termFee.EndedBatch.HasValue)
            {
                if (termFee.EndedBatch.Value < academicInformation.Batch)
                {
                    return -1;
                }

                score += Math.Pow(2, 4);
            }

            if (termFee.CurriculumVersionId.HasValue)
            {
                if (!academicInformation.CurriculumVersionId.HasValue
                   || termFee.CurriculumVersionId.Value != academicInformation.CurriculumVersionId.Value)
                {
                    return -1;
                }

                score += Math.Pow(2, 3);
            }

            if (termFee.CurriculumId.HasValue)
            {
                if (!academicInformation.CurriculumVersionId.HasValue
                    || termFee.CurriculumId != academicInformation.CurriculumVersion.CurriculumId)
                {
                    return -1;
                }

                score += Math.Pow(2, 2);
            }

            if (termFee.FacultyId.HasValue)
            {
                if (termFee.FacultyId.Value != academicInformation.FacultyId)
                {
                    return -1;
                }

                score += Math.Pow(2, 1);
            }

            if (termFee.DepartmentId.HasValue)
            {
                if (!academicInformation.DepartmentId.HasValue
                   || termFee.DepartmentId.Value != academicInformation.DepartmentId.Value)
                {
                    return -1;
                }

                score += Math.Pow(2, 0);
            }

            return score;
        }

        private static double CalculateTuitionFeeScore(IEnumerable<Term> terms, Term compareTerm,
                                                       IEnumerable<Section> registerSections,
                                                       TuitionFee fee, AcademicInformation academicInformation)
        {
            if (terms == null
                || !terms.Any()
                || compareTerm == null
                || registerSections == null
                || !registerSections.Any()
                || fee == null
                || academicInformation == null)
            {
                return -1;
            }

            var score = 0d;

            if (fee.StartedTermId.HasValue)
            {
                var matchingTerm = terms.SingleOrDefault(x => x.Id == fee.StartedTermId.Value);

                if (matchingTerm == null
                    || matchingTerm.AcademicYear > compareTerm.AcademicYear
                    || (
                          matchingTerm.AcademicYear == compareTerm.AcademicYear
                          && matchingTerm.AcademicTerm > compareTerm.AcademicTerm
                       ))
                {
                    return -1;
                }
            }

            if (fee.EndedTermId.HasValue)
            {
                var matchingTerm = terms.SingleOrDefault(x => x.Id == fee.EndedTermId.Value);

                if (matchingTerm == null
                    || matchingTerm.AcademicYear < compareTerm.AcademicYear
                    || (
                          matchingTerm.AcademicYear == compareTerm.AcademicYear
                          && matchingTerm.AcademicTerm < compareTerm.AcademicTerm
                       ))
                {
                    return -1;
                }
            }


            if (!string.IsNullOrEmpty(fee.SectionNumber))
            {
                var matchingRegis = registerSections.SingleOrDefault(x => x.CourseId == fee.CourseId.Value
                                                                      && x.Number == fee.SectionNumber);

                if (matchingRegis == null)
                {
                    return -1;
                }

                score += Math.Pow(2, 6);
            }


            if (fee.StartedBatch.HasValue)
            {
                if (fee.StartedBatch.Value > academicInformation.Batch)
                {
                    return -1;
                }

                score += Math.Pow(2, 5);
            }

            if (fee.EndedBatch.HasValue)
            {
                if (fee.EndedBatch.Value < academicInformation.Batch)
                {
                    return -1;
                }

                score += Math.Pow(2, 4);
            }

            if (fee.CurriculumVersionId.HasValue)
            {
                if (!academicInformation.CurriculumVersionId.HasValue
                   || fee.CurriculumVersionId.Value != academicInformation.CurriculumVersionId.Value)
                {
                    return -1;
                }

                score += Math.Pow(2, 3);
            }

            if (fee.CurriculumId.HasValue)
            {
                if (!academicInformation.CurriculumVersionId.HasValue
                    || fee.CurriculumId != academicInformation.CurriculumVersion.CurriculumId)
                {
                    return -1;
                }

                score += Math.Pow(2, 2);
            }

            if (fee.DepartmentId.HasValue)
            {
                if (!academicInformation.DepartmentId.HasValue
                   || fee.DepartmentId.Value != academicInformation.DepartmentId.Value)
                {
                    return -1;
                }

                score += Math.Pow(2, 1);
            }

            if (fee.FacultyId.HasValue)
            {
                if (fee.FacultyId.Value != academicInformation.FacultyId)
                {
                    return -1;
                }

                score += Math.Pow(2, 0);
            }

            return score;
        }

        private static bool IsActiveScholarShip(IEnumerable<Term> terms, Term compareTerm,
                                         ScholarshipStudent scholarShip)
        {
            if (terms == null || !terms.Any()
               || compareTerm == null
               || scholarShip == null)
            {
                return false;
            }

            var effectiveTerm = terms.SingleOrDefault(x => x.Id == scholarShip.EffectivedTermId);

            if (effectiveTerm == null
                || effectiveTerm.AcademicYear > compareTerm.AcademicYear
                || (effectiveTerm.AcademicYear == compareTerm.AcademicYear && effectiveTerm.AcademicTerm > compareTerm.AcademicTerm))
            {
                return false;
            }

            if (scholarShip.ExpiredTermId.HasValue)
            {
                var expireTerm = terms.SingleOrDefault(x => x.Id == scholarShip.ExpiredTermId.Value);

                if (expireTerm == null
                || expireTerm.AcademicYear < compareTerm.AcademicYear
                || (expireTerm.AcademicYear == expireTerm.AcademicYear && expireTerm.AcademicTerm < compareTerm.AcademicTerm))
                {
                    return false;
                }
            }

            return true;
        }

        private static decimal CalculateCourseTuitionFee(IEnumerable<TuitionFee> fees, IEnumerable<Course> courses,
                                                         IEnumerable<TuitionFeeFormula> formulas,
                                                         IEnumerable<CustomCourseGroup> courseGroups,
                                                         IEnumerable<TuitionFeeRate> tuitionFeeRates,
                                                         long tuitionFeeId, long courseId, Student student)
        {
            var matchingFee = fees.SingleOrDefault(x => x.Id == tuitionFeeId);

            var matchingCourse = courses.SingleOrDefault(x => x.Id == courseId);

            if (matchingFee == null || matchingCourse == null)
            {
                throw new ArgumentException($"fee id ({tuitionFeeId}) AND course id ({courseId}) not found");
            }

            if (!matchingFee.TuitionFeeFormulaId.HasValue)
            {
                return matchingFee.Amount;
            }

            var matchingFormula = formulas.SingleOrDefault(x => x.Id == matchingFee.TuitionFeeFormulaId.Value);

            var matchingCourseGroups = courseGroups.Where(x => x.CourseIdsLong.Contains(courseId))
                                                   .Select(x => x.Id)
                                                   .ToList();

            var matchingRates = (from rate in tuitionFeeRates
                                 let score = CalcaulteTuitionFeeRateCourseScore(rate, matchingCourseGroups, matchingCourse)
                                 where score >= 0
                                 orderby score descending
                                 select rate)
                                 .ToList();

            var first = matchingRates.FirstOrDefault(x => x.TuitionFeeTypeId == matchingFormula.FirstTuitionFeeTypeId);
            var second = matchingRates.FirstOrDefault(x => x.TuitionFeeTypeId == matchingFormula.SecondTuitionFeeTypeId);


            var response = (matchingCourse.Lecture * (first != null ? first.Amount : 0))
                            + ((matchingCourse.RegistrationCredit - matchingCourse.Lecture) * (second != null ? second.Amount : 0));

            return response;
        }

        private static double CalcaulteTuitionFeeRateCourseScore(
            TuitionFeeRate rate, IEnumerable<long> customCourseGroupIds, Course course)
        {
            var score = 0d;

            if (rate.CustomCourseGroupId.HasValue)
            {
                if (!customCourseGroupIds.Contains(rate.CustomCourseGroupId.Value))
                {
                    return -1;
                }

                score += Math.Pow(2, 1);
            }

            if (!string.IsNullOrEmpty(rate.WhitelistMajorIds))
            {
                if (!course.DepartmentId.HasValue
                    || !rate.WhitelistMajorIds.Contains(course.DepartmentId.Value.ToString()))
                {
                    return -1;
                }

                score += Math.Pow(2, 0);
            }

            return score;
        }

        private static bool IsMatchingTermPeriod(Term term, Term fromTerm, Term toTerm)
        {
            if (term is null)
            {
                return false;
            }

            if (fromTerm != null)
            {
                if (fromTerm.AcademicYear > term.AcademicYear ||
                   (fromTerm.AcademicYear == term.AcademicYear
                    && fromTerm.AcademicTerm > term.AcademicTerm))
                {
                    return false;
                }
            }

            if (toTerm != null)
            {
                if (toTerm.AcademicYear < term.AcademicYear ||
                   (fromTerm.AcademicYear == term.AcademicYear
                    && fromTerm.AcademicTerm < term.AcademicTerm))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
