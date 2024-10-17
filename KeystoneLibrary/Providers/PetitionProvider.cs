using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.Logs;
using KeystoneLibrary.Models.DataModels.Petitions;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Enums;
using KeystoneLibrary.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class PetitionProvider : BaseProvider, IPetitionProvider
    {
        public PetitionProvider(ApplicationDbContext db) : base(db) { }

        public void CreateChangingCurriculumPetition(CreateUsparkChangingCurriculumPetitionViewModel request)
        {
            if(request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var student = _db.Students.AsNoTracking()
                                      .Include(x => x.AcademicInformation)
                                      .Include(x => x.AdmissionInformation)
                                      .SingleOrDefault(x => x.Code == request.StudentCode);

            var term = _db.Terms.AsNoTracking()
                                .SingleOrDefault(x => x.Id == request.KSTermId);

            var department = _db.Departments.AsNoTracking()
                                            .SingleOrDefault(x => x.Id == request.MoveToDepartmentId
                                                                  && x.IsActive == true);

            if(student == null
                || student.AcademicInformation == null
                || !student.AcademicInformation.DepartmentId.HasValue
                || !student.AcademicInformation.CurriculumVersionId.HasValue)
            {
                throw new ArgumentNullException(nameof(student));
            }

            var NotAllowForChangeMajorAddmissionTypeIdsConfig = _db.Configurations.AsNoTracking()
                                                                                  .SingleOrDefault(x => x.Key == "NotAllowForChangeMajorAddmissionTypeIds");

            var preventAdmisstionTypeIds = NotAllowForChangeMajorAddmissionTypeIdsConfig == null
                                               || string.IsNullOrEmpty(NotAllowForChangeMajorAddmissionTypeIdsConfig.Value)
                                                      ? Enumerable.Empty<long>()
                                                      : NotAllowForChangeMajorAddmissionTypeIdsConfig.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                                                                                     .Select(x => long.Parse(x.Trim()))
                                                                                           .ToList();

            if (preventAdmisstionTypeIds.Any() 
                    && (student.AdmissionInformation == null
                        || (student.AdmissionInformation.AdmissionTypeId ?? 0) == 0
                        || preventAdmisstionTypeIds.Contains(student.AdmissionInformation.AdmissionTypeId.Value)
                    )
                )
            {
                throw new InvalidOperationException("Student Admission Type are not allowed.");
            }

            if (term == null)
            {
                throw new ArgumentNullException(nameof(term));
            }

            if(department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            var today = DateTime.UtcNow.AddHours(7).Date;
            var periodConfig = _db.RegistrationTerms.AsNoTracking()
                                                    .FirstOrDefault(x => x.TermId == request.KSTermId
                                                                         && x.Type == "cc"
                                                                         && x.StartedAt <= today
                                                                         && (x.EndedAt == null || x.EndedAt >= today)
                                                                         && x.IsActive
                                                                   );
            if (periodConfig == null)
            {
                throw new InvalidOperationException("Not in change curriculum period.");
            }
 
            var petitions = _db.ChangingCurriculumPetitions.AsNoTracking()
                                                           .Where(x => x.StudentId == student.Id)
                                                           .ToList();

            if(petitions.Any(x => x.Status == PetitionStatusEnum.PENDING))
            {
                throw new InvalidOperationException("Allow only 1 on-going petition.");
            }

            

            var curriculumVersions = _db.Curriculums.AsNoTracking()
                                                    .Include(x => x.CurriculumVersions)
                                                    .Where(x => x.DepartmentId == department.Id
                                                                && x.IsActive)
                                                    .SelectMany(x => x.CurriculumVersions)
                                                    .Where(x => x.IsActive)
                                                    .ToList();

            var matchingCurriculumVersion = (from version in curriculumVersions
                                             let score = CalculateCurriculumVersionScore(version, student.AcademicInformation)
                                             where score >= 0
                                             orderby score descending
                                             select version)
                                             .FirstOrDefault();

            var petition = new ChangingCurriculumPetition
            {
                TermId = term.Id,
                StudentId = student.Id,
                StudentCode = student.Code,
                CurrentDepartmentId = student.AcademicInformation.DepartmentId.Value,
                CurrentCurriculumVersionId = student.AcademicInformation.CurriculumVersionId.Value,
                NewDepartmentId = department.Id,
                NewCurriculumVersionId = matchingCurriculumVersion?.Id,
                StudentRemark = request.StudentRemark,
                Status = PetitionStatusEnum.PENDING,
                Channel = request.Channel?.ToLower(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = student.Code,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = student.Code
            };

            using(var transaction = _db.Database.BeginTransaction())
            {
                _db.ChangingCurriculumPetitions.Add(petition);

                transaction.Commit();
            }

            _db.SaveChanges();
        }

        public PageResultViewModel<UsparkChangingCurriculumPetitionViewModel> SearchChangingCurriculumPetition(Criteria criteria, int page = 1, int pageSize = 25)
        {
            if(page < 1 || pageSize < 1)
            {
                return new PageResultViewModel<UsparkChangingCurriculumPetitionViewModel>
                {
                    TotalItem = 0,
                    TotalPage = 0,
                    Page = 1,
                    PageSize = 1
                };
            }

            var query = _db.ChangingCurriculumPetitions.AsNoTracking();

            if(criteria != null)
            {
                if (!string.IsNullOrEmpty(criteria.StudentCode))
                {
                    query = query.Where(x => x.StudentCode == criteria.StudentCode);
                }

                if(criteria.AcademicLevelId != default)
                {
                    query = query.Where(x => x.Student.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId);
                }

                if(criteria.TermId != default)
                {
                    query = query.Where(x => x.TermId == criteria.TermId);
                }

                if(criteria.FacultyId != default)
                {
                    query = query.Where(x => x.Student.AcademicInformation.FacultyId == criteria.FacultyId);
                }

                if(criteria.DepartmentId != default)
                {
                    query = query.Where(x => x.Student.AcademicInformation.DepartmentId == criteria.DepartmentId);
                }

                if(criteria.StudentStatus != default)
                {
                    query = query.Where(x => x.Student.StudentStatus == criteria.Status);
                }

                if (criteria.StartStudentBatch.HasValue)
                {
                    query = query.Where(x => x.Student.AcademicInformation.Batch >= criteria.StartStudentBatch.Value);
                }

                if (criteria.EndStudentBatch.HasValue)
                {
                    query = query.Where(x => criteria.EndStudentBatch.Value >= x.Student.AcademicInformation.Batch);
                }
            }

            var totalData = query.Count();
            var totalPage = totalData / pageSize;

            if(totalData % pageSize == 0)
            {
                totalPage += 1;
            }

            var totalSkip = (page - 1) * pageSize;

            var petitions = query.OrderByDescending(x => x.CreatedAt)
                                 .Skip(totalSkip)
                                 .Take(pageSize)
                                 .ToList();

            var items = (from petition in petitions
                        orderby petition.CreatedAt descending
                        select MapChangingCurriculumViewModel(petition))
                        .ToList();

            var response = new PageResultViewModel<UsparkChangingCurriculumPetitionViewModel>
            {
                TotalItem = totalData,
                TotalPage = totalPage,
                Page = page,
                PageSize = pageSize,
                Items = items
            };

            return response;
        }

        public UsparkChangingCurriculumPetitionViewModel GetChangingCurriculumPetitionById(long petitionId)
        {
            var petition = _db.ChangingCurriculumPetitions.AsNoTracking()
                                                          .SingleOrDefault(x => x.Id == petitionId);

            if(petition is null)
            {
                throw new ArgumentNullException("petition");
            }

            var response = MapChangingCurriculumViewModel(petition);

            return response;
        }

        public void UpdatePetitionStatus(long petitionId, PetitionStatusEnum status, string remark, string userId)
        {
            var approver = _db.Users.AsNoTracking()
                                    .SingleOrDefault(x => x.Id == userId);

            if(approver == null)
            {
                throw new ArgumentException(nameof(userId));
            }

            var petition = _db.ChangingCurriculumPetitions.SingleOrDefault(x => x.Id == petitionId);

            // TODO : CONFIRM LIMITATION
            //if(petition.Status == PetitionStatusEnum.APPROVED)
            //{
            //    throw new InvalidOperationException("NOT ALLOW UPDATED APPROVED PETITION.");
            //}

            using(var transaction = _db.Database.BeginTransaction())
            {
                petition.Status = status;
                petition.UpdatedAt = DateTime.UtcNow;
                petition.UpdatedBy = userId;

                transaction.Commit();
            }

            _db.SaveChanges();
        }

        public ChangingCurriculumPetition GetChangingCurriculumPetition(long id)
        {
            var model = _db.ChangingCurriculumPetitions.AsNoTracking()
                                                       .Include(x => x.Term)
                                                           .ThenInclude(x => x.AcademicLevel)
                                                       .Include(x => x.Student)
                                                           .ThenInclude(x => x.AcademicInformation)
                                                           .ThenInclude(x => x.Faculty)
                                                       .Include(x => x.Student)
                                                           .ThenInclude(x => x.AcademicInformation)
                                                           .ThenInclude(x => x.Department)
                                                       .Include(x => x.Student)
                                                           .ThenInclude(x => x.StudentExemptedExamScores)
                                                           .ThenInclude(x => x.ExemptedAdmissionExamination)
                                                       .Include(x => x.CurrentCurriculumVersion)
                                                       .Include(x => x.NewCurriculumVersion)
                                                       .IgnoreQueryFilters()
                                                       .SingleOrDefault(x => x.Id == id);

            return model;
        }


        private decimal CalculateCurriculumVersionScore(CurriculumVersion version, AcademicInformation academicInformation)
        {
            if(version == null || academicInformation == null)
            {
                return -1;
            }

            var score = decimal.Zero;

            if (version.StartBatch.HasValue)
            {
                if(version.StartBatch.Value > academicInformation.Batch)
                {
                    return -1;
                }

                score += (decimal) Math.Pow(2,0);
            }

            if (version.EndBatch.HasValue)
            {
                if (version.EndBatch.Value < academicInformation.Batch)
                {
                    return -1;
                }

                score += (decimal)Math.Pow(2, 1);
            }

            return score;
        }

        public ChangedNameLog GetChangedNameLog(long id)
        {
            var changedNameLog = _db.ChangedNameLogs.SingleOrDefault(x => x.Id == id);
            return changedNameLog;
        }

        private static UsparkChangingCurriculumPetitionViewModel MapChangingCurriculumViewModel(ChangingCurriculumPetition model)
        {
            return new UsparkChangingCurriculumPetitionViewModel
            {
                Id = model.Id,
                StudentCode = model.StudentCode,
                KSTermId = model.TermId,
                Channel = model.Channel,
                DepartmentIdOnRequest = model.CurrentDepartmentId,
                MoveToDepartmentId = model.NewDepartmentId,
                StudentRemark = model.StudentRemark,
                Status = model.Status,
                ApproverRemark = model.Remark,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt
            };
        }
    }
}