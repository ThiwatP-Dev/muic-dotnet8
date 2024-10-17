using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Keystone.Permission;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("StudentImportImage", PolicyGenerator.Write)]
    public class StudentImportImageController : BaseController
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStudentProvider _studentProvider;
        protected readonly IStudentPhotoProvider _studentPhotoProvider;

        public StudentImportImageController(ApplicationDbContext db,
                                            IFlashMessage flashMessage,
                                            ISelectListProvider selectListProvider,
                                            IStudentProvider studentProvider,
                                            IHostEnvironment hostingEnvironment,
                                            IStudentPhotoProvider studentPhotoProvider,
                                            IHttpContextAccessor httpContextAccessor) : base(db, flashMessage, selectListProvider) 
        {
            _studentProvider = studentProvider;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _studentPhotoProvider = studentPhotoProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(List<IFormFile> files)
        {
            List<string> successList = new List<string>();
            List<string> failList = new List<string>();
            foreach (var formFile in files)
            {
                try 
                {
                    var result = false;
                    if (formFile.Length > 0)
                    {
                        var studentCode = Path.GetFileNameWithoutExtension(formFile.FileName);
                        if (studentCode.Contains("u")) 
                        {
                            studentCode = studentCode.Replace("u", "");
                        }

                        var student = _studentProvider.GetStudentByCode(studentCode);
                        if (student != null) 
                        {
                            //var finalFileName = studentCode + Path.GetExtension(formFile.FileName).ToLower();
                            //var urlPath = "https" + "://" + _httpContextAccessor.HttpContext.Request.Host.ToString() + "/uploaded/" + finalFileName;                     
                            //string targetPath = Path.Combine(_hostingEnvironment.WebRootPath + "/uploaded/", finalFileName);
                            //using (var stream = System.IO.File.Create(targetPath))
                            //{
                            //    formFile.CopyTo(stream);
                            //}

                            var imageUrl = await _studentPhotoProvider.UploadFile(formFile, student.Code);

                            result = _studentProvider.UpdateImageUrl(student.Id, imageUrl);
                        }
                    }

                    if (result) 
                    {
                        successList.Add(formFile.FileName);
                    }
                    else 
                    {
                        failList.Add(formFile.FileName);
                    }
                }
                catch 
                {
                    failList.Add(formFile.FileName);
                }
            }
            
            return RedirectToAction(nameof(Save), new { successList = successList, failList = failList });
        }

        public ActionResult Save(List<string> successList, List<string> failList)
        {
            var result = new StudentImportImageResultModel();
            result.SuccessFileNameList = successList;
            result.FailFileNameList = failList;
            return View(result);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }
    }
}