using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.Report;

namespace KeystoneLibrary.Helpers
{
    public static class UIHelper
    {
        public static string GetChild(CourseGroup courseGroup, string htmlString, string returnUrl, bool isSpecialGroup = false)
        {
            htmlString = "";
            if (!string.IsNullOrEmpty(courseGroup.DescriptionEn))
            {
                htmlString += $"<div class='font-size-12 mt-1'> <span style='white-space: pre-wrap;'>{courseGroup.DescriptionEn} </span></div>";
            }
            var index = 1;
            if (courseGroup.CurriculumCourses.Any())
            {
                htmlString += "<table class='table table-bordered'>";
                htmlString += "<thead>";
                htmlString += "<tr>";
                htmlString += "<th class='text-center'>#</th>";
                htmlString += "<th>Code</th>";
                htmlString += "<th>Name</th>";
                htmlString += "<th class='text-center'>Required</th>";
                htmlString += "<th class='text-center'>Grade Required</th>";
                htmlString += "<th class='text-center'>Credit</th>";
                htmlString += "</tr>";
                htmlString += "</thead>";
                htmlString += "<tbody>";
                foreach (var course in courseGroup.CurriculumCourses)
                {
                    htmlString += "<tr>";
                    htmlString += $"<td>{ index++ }</td>";
                    htmlString += $"<td>{ course.Course?.Code }</td>";
                    htmlString += $"<td>{ course.Course?.NameEn }</td>";
                    htmlString += "<td>";
                    htmlString += course.IsRequired ? "<span class='text-success'>Yes</span>" : "<span class='text-danger'>No</span>";
                    htmlString += "</td>";
                    htmlString += $"<td>{ course.RequiredGradeText }</td>";
                    htmlString += $"<td>{ course.Course?.CreditText }</td>";
                    htmlString += "</tr>";
                }
                htmlString += "</tbody>";
                htmlString += "</table>";
            }
                
            if (courseGroup.ChildCourseGroups.Any())
            {
                foreach (var group in courseGroup.ChildCourseGroups)
                {
                    htmlString += $"<header><h4 class='mr-auto my-1'>{ group.NameEn }";
                    htmlString += group.Credit == 0 ? "" : $" ({ group.Credit } Credits)";
                    htmlString += "</h4>";
                    htmlString += "<div class='d-flex align-self-end actions-group'>";
                    htmlString += $"<a class='popover-link js-curriculum-option' data-group-id='{ group.Id }' data-return-url='{ returnUrl }' data-special-group='{ isSpecialGroup }'>";
                    htmlString += "<i class='la la-plus check'></i>";
                    htmlString += "</a>";
                    htmlString += $"<a class='popover-link js-curriculum-edit-option' data-group-id='{ group.Id }' data-return-url='{ returnUrl }' data-special-group='{ isSpecialGroup }'>";
                    htmlString += "<i class='la la-edit edit'></i>";
                    htmlString += "</a>";
                    htmlString += $"<a class='popover-link' data-toggle='modal' data-target='#delete-confirm-modal' data-controller='CourseGroup' data-action='DeleteCourseGroup' data-value='{ group.Id }' data-return-url='{ returnUrl }'>";
                    htmlString += "<i class='la la-trash delete'></i>";
                    htmlString += "</a>";
                    htmlString += "</div>";
                    htmlString += "</header>";
                    htmlString += GetChild(group, htmlString, returnUrl);
                }
            }

            return htmlString;
        }

        public static Tuple<string, int> GetChildGraduation(CourseGroup courseGroup, string htmlString, int totalCredit)
        {
            htmlString = "";
            totalCredit = 0;
            var index = 1;
            if (courseGroup.ChildCourseGroups.Any())
            {
                foreach (var group in courseGroup.ChildCourseGroups)
                {
                    htmlString += "<tr>";
                    htmlString += "<td></td>";
                    htmlString += $"<td><b>{ group.NameEn }</b></td>";
                    htmlString += "<td></td>";
                    htmlString += "<td></td>";
                    htmlString += "<td></td>";
                    htmlString += $"<td><b>{ group.Credit }</b></td>";
                    htmlString += "</tr>";

                    (string html, int credit) = GetChildGraduation(group, htmlString, totalCredit);
                    htmlString += html;
                    totalCredit += credit;

                    htmlString += "<tr class=\"table-warning\">";
                    htmlString += "<td></td>";
                    htmlString += $"<td><b>Total</b></td>";
                    htmlString += "<td></td>";
                    htmlString += "<td></td>";
                    htmlString += "<td></td>";
                    htmlString += $"<td><b>{ totalCredit.ToString(StringFormat.NumberString) }</b></td>";
                    htmlString += "</tr>";
                }
            }
            else
            {
                var groupCredit = 0;
                foreach (var course in courseGroup.CurriculumCourses)
                {
                    int credit = course.Course?.Credit ?? 0;
                    groupCredit += credit;
                    totalCredit += credit;
                    htmlString += "<tr>";
                    htmlString += $"<td>{ index++ }</td>";
                    htmlString += $"<td>{ course.Course?.Code }</td>";
                    htmlString += $"<td>{ course.Course?.NameEnAndCredit }</td>";
                    htmlString += $"<td>{ course.RegistrationGradeText }</td>";
                    htmlString += $"<td>{ course.RequiredGradeText }</td>";
                    htmlString += $"<td>{ credit.ToString(StringFormat.NumberString) }</td>";
                    htmlString += "</tr>";
                }

                htmlString += "<tr class=\"bg-secondary-lighter\">";
                htmlString += "<td></td>";
                htmlString += $"<td><b>Group Total</b></td>";
                htmlString += "<td></td>";
                htmlString += "<td></td>";
                htmlString += "<td></td>";
                htmlString += $"<td><b>{ groupCredit.ToString(StringFormat.NumberString) }</b></td>";
                htmlString += "</tr>";
            }

            return new Tuple<string, int>(htmlString, totalCredit);
        }

        public static string GetChildCourseGroup(CourseGroupStructureViewModel courseGroup, string htmlString, string indent = "")
        {
            htmlString = "";
            var index = 1;
            if (courseGroup.CourseGroups.Any())
            {
                foreach (var group in courseGroup.CourseGroups)
                {
                    htmlString += "<div class='block block--underline' id='parents-" + group.CourseGroupId + "'>" + Environment.NewLine;

                    htmlString += "<div class='block__title'>" + Environment.NewLine;

                    htmlString += $"<header>{ group.Name }";
                    htmlString += group.TotalCredit == 0 ? "" : $" ({ group.TotalCredit } Credits)";
                    htmlString += "</header>" + Environment.NewLine;

                    // htmlString += "<div class='tools pr-4'>" + Environment.NewLine;
                    // htmlString += "<a class='btn btn--white btn--circle' data-toggle='collapse' data-parent='#parents-" + group.CourseGroupId + "' ";
                    // htmlString += "data-target='#accordion-" + group.CourseGroupId + "-1'>" + Environment.NewLine;
                    // htmlString += "<i class='la la-angle-down'></i>" + Environment.NewLine;
                    // htmlString += "</a>" + Environment.NewLine;
                    // htmlString += "</div>" + Environment.NewLine;

                    htmlString += "</div>" + Environment.NewLine;

                    htmlString += "<div class='block__body'>" + Environment.NewLine;
                    if(!string.IsNullOrEmpty(group.Description))
                    {
                        htmlString += "<span class='block__body btn--primary pt-0 text-pre-warp'>" + Environment.NewLine;
                        htmlString += group.Description + Environment.NewLine;
                        htmlString += "</span>" + Environment.NewLine;
                    }
                    // htmlString += "<div id='accordion-" + group.CourseGroupId + "-1'>" + Environment.NewLine;
                    // htmlString += "<hr class='p-y-0'>" + Environment.NewLine;
                    htmlString += "<div>" + Environment.NewLine;
                    htmlString += GetChildCourseGroup(group, htmlString);
                    // htmlString += "</div>" + Environment.NewLine;
                    htmlString += "</div>" + Environment.NewLine;

                    htmlString += "</div>" + Environment.NewLine;

                    htmlString += "</div>" + Environment.NewLine;
                }
            }
            else
            {
                if (courseGroup.Courses.Any())
                {
                    htmlString += "<table class='table table-bordered'>";
                    htmlString += "<thead>";
                    htmlString += "<tr>";
                    htmlString += "<th>#</th>";
                    htmlString += "<th>Code</th>";
                    htmlString += "<th>Name</th>";
                    htmlString += "<th class='text-center'>Credit</th>";
                    htmlString += "<th class='text-center'>Passing Grade</th>";
                    htmlString += "</tr>";
                    htmlString += "</thead>";
                    htmlString += "<tbody>";
                    foreach (var course in courseGroup.Courses)
                    {
                        htmlString += "<tr>";
                        htmlString += $"<td>{ index++ }</td>";
                        htmlString += $"<td>{ course.Code }</td>";
                        htmlString += $"<td>{ course.Name }</td>";
                        htmlString += $"<td>{ course.Credit }</td>";
                        htmlString += $"<td>{ course.Grade }</td>";
                        htmlString += "</tr>";
                        if (course.Prerequisites != null && course.Prerequisites.Any())
                        {
                            htmlString += "<tr>";
                            htmlString += $"<td></td>";
                            htmlString += "<td colspan='4' class='color-danger'>";
                            htmlString += "Prerequisites:<br>";
                            foreach (var prerequisite in course.Prerequisites)
                            {
                                htmlString += "- " + prerequisite + "<br>";
                            }
                            htmlString += "</td>";
                            htmlString += "</tr>";
                        }
                    }
                    htmlString += "</tbody>";
                    htmlString += "</table>";
                }
            }

            return htmlString;
        }

        public static string GetCourseGroupRegistration(CourseGroupViewModel group, string htmlString)
        {
            htmlString = "";
            if (group.Children != null && group.Children.Any())
            {
                foreach (var child in group.Children)
                {              
                    htmlString += "<div class='block block--underline' id='parents-" + child.CourseGroupId + "'>" + Environment.NewLine;

                    htmlString += "<div class='block__title'>" + Environment.NewLine;

                    htmlString += $"<header>{ child.NameEn }";
                    htmlString += child.RequiredCreditCompleted == 0 ? "" : $" ({ child.RequiredCreditCompleted } Credits)";
                    htmlString += "</header>" + Environment.NewLine;

                    htmlString += "</div>" + Environment.NewLine;

                    htmlString += "<div class='block__body'>" + Environment.NewLine;
                    if(!string.IsNullOrEmpty(child.DescriptionEn))
                    {
                        htmlString += "<span class='block__body btn--primary pt-0 text-pre-warp'>" + Environment.NewLine;
                        htmlString += child.DescriptionEn + Environment.NewLine;
                        htmlString += "</span>" + Environment.NewLine;
                    }

                    htmlString += "<div>" + Environment.NewLine;
                    htmlString += GetCourseGroupRegistration(child, htmlString);

                    htmlString += "</div>" + Environment.NewLine;

                    htmlString += "</div>" + Environment.NewLine;

                    htmlString += "</div>" + Environment.NewLine;
                }
            }
            else
            {
                htmlString += "<div class=\"table-responsive\">";
                htmlString += "<table class=\"table table-bordered table-hover w-100x\">";
                htmlString += "<thead>";
                
                htmlString += "<tr>";
                htmlString += "<th>Course Code</th>";
                htmlString += "<th>Course Name</th>";
                htmlString += "<th class=\"text-center\">Course Credit</th>";
                htmlString += "<th class=\"text-center\">Passing Grade</th>";
                htmlString += "<th class=\"text-center\">Grade</th>";
                htmlString += "</tr>";
                htmlString += "</thead>";
                htmlString += "<tbody>";
                foreach (var course in group.Courses)
                {
                    htmlString += "<tr>";
                    htmlString += $"<td>{ course.CourseCode }</td>";
                    htmlString += $"<td>{ course.CourseNameEn }</td>";
                    htmlString += $"<td><div class=\"text-center\">{ course.CreditText }</div></td>";
                    htmlString += $"<td><div class=\"text-center\">{ course.RequiredGradeName }</div></td>";

                    string gradeText = string.Empty;
                    if(course.Grades != null)
                    {
                        foreach (var grade in course.Grades)
                        {
                            if(!string.IsNullOrEmpty(grade.RegisteredGradeName))
                            {
                                if(grade.IsTransferCourse && !grade.IsStarCourse)
                                {
                                    gradeText += $"<span class=\"ks-label bg-info mx-3 w-150 mb-1\">{ grade.RegisteredGradeName } ({ grade.TermText })</span><br/>";
                                }
                                else if (grade.IsTransferCourse && grade.IsStarCourse)
                                {
                                    gradeText += $"<span class=\"ks-label bg-info mx-3 w-150 mb-1\">{ grade.RegisteredGradeName }* ({ grade.TermText })</span><br/>";
                                }
                                else 
                                {
                                    if(grade.IsPassed)
                                    {
                                        gradeText += $"<span class=\"ks-label bg-success mx-3 w-150 mb-1\">{ grade.RegisteredGradeName } ({ grade.TermText })</span><br/>";
                                    }
                                    else 
                                    {
                                        gradeText += $"<span class=\"ks-label bg-danger mx-3 w-150 mb-1\">{ grade.RegisteredGradeName } ({ grade.TermText })</span><br/>";
                                    }
                                }
                            }
                        }
                    }

                    htmlString += $"<td>{ gradeText }</td>";
                    htmlString += "</tr>";
                }
                
                htmlString += "</tbody>";
                htmlString += "</table>";
                htmlString += "</div>";
            }

            return htmlString;
        }
    }
}