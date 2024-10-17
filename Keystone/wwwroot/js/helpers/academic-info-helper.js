/* --- main --- */
var academicLevel = '.js-cascade-academic-level';

/* --- present related --- */
var term = '.js-cascade-term';
var graduatedYear = '.js-cascade-grad-year';
var graduatedTerm = '.js-cascade-grad-term';
var multiTerm = '.js-cascade-multiple-term';
var faculty = '.js-cascade-faculty';
var facultyAll = '.js-cascade-faculty-all';
var fixFaculty = '.js-cascade-fix-faculty';
var multiFaculty = '.js-cascade-multiple-faculty';
var courseFaculty = '.js-cascade-course-faculty';
var department = '.js-cascade-department';
var multiDepartment = '.js-cascade-multiple-department';
var minor = '.js-cascade-minor';
var concentration = '.js-cascade-concentration';
var course = '.js-cascade-course';
var courseGroup = '.js-cascade-course-group';
var section = '.js-cascade-section';
var sectionStatus = '.js-cascade-section-status';
var academicProgram = '.js-cascade-academic-program';

var curriculum = '.js-cascade-curriculum';
var multiCurriculum = '.js-cascade-multiple-curriculum';
var curriculumVersion = '.js-cascade-curriculum-version';
var multiCurriculumVersion = '.js-cascade-multiple-curriculum-version';
var curriculumCourse = '.js-cascade-curriculum-course';
var courseGroup = '.js-cascade-course-group';
var selectableCurriculumVersion = '.js-cascade-selectable-curriculum-version';
var multiCourse = '.js-cascade-multiple-course';
var multiSection = '.js-cascade-multiple-section';
var degreenName = '.js-casacde-degree-name';

var studentCode = '.js-cascade-student-code';
var student = '.js-cascade-student';
var instructor = '.js-cascade-instructor';
var instructorSection = '.js-cascade-instructor-section';
var multiInstructor = '.js-cascade-multiple-instructor';

/* --- admission related --- */
var admissionAcademicLevel = '.js-cascade-admission-academic-level';
var admissionTerm = '.js-cascade-admission-term';
var admissionRound = '.js-cascade-admission-round';

var startedCode = '.js-cascade-started-code';
var endedCode = '.js-cascade-ended-code';

var documentGroup = '.js-cascade-document-group';
var previousSchoolCountry = '.js-cascade-preschool-country';
var previousSchool = '.js-cascade-previous-school';

/* --- other --- */
var firstClassDate = '.js-cascade-first-class-date';
var nationality = '.js-get-nationality'; // get nationality id from view
var multiFeeGroup = '.js-cascade-multiple-fee-group';

/* --- single cascade section --- */
// academic level required
function cascadeTerms(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetTermsByAcademicLevelId,
            data: {
                id: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.termText));

            if (target.find('option[disabled]').length) {
                if (target.hasClass('js-cascade-admission-term')) {
                    if (item.isAdmission) {
                        target.find('option[selected]').prop('selected', false);
                        target.find('option:last-child').prop('selected', true);
                        let sectionAdmissionRound = target.closest('section').find(admissionRound);

                        if (sectionAdmissionRound.length > 0) {
                            cascadeAdmissionRound(item.id, sectionAdmissionRound);
                        }
                    } 
                } else {
                    if (item.isCurrent) {
                        target.find('option[selected]').prop('selected', false);
                        target.find('option:last-child').prop('selected', true);
                        var termSelectValue = target.val();
                        let sectionCourse = target.closest('section').find(course);
                        let sectionSectionStatus = target.closest('section').find(sectionStatus);
                        let sectionMultiCourse = target.closest('section').find(multiCourse);

                        if (sectionCourse.length > 0) {
                            if (sectionSectionStatus.length > 0) {
                                cascadeCoursesBySectionStatus(sectionSectionStatus.val(), termSelectValue, sectionCourse);
                            } else {
                                cascadeCourses(termSelectValue, sectionCourse);
                            }
                        }

                        if (sectionMultiCourse.length > 0) {
                            cascadeMultiCourses(termSelectValue, sectionMultiCourse);
                        }
                    }
                }
            }
            
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeExpectedGradYear(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetExpectedGraduationYearsByAcademicLevelId,
            data: {
                id: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.empty();
        target.append(getDefaultSelectOption(target));
        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
}
function cascadeExpectedGradTerm(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetExpectedGraduationTermsByAcademicLevelId,
            data: {
                id: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.empty();
        target.append(getDefaultSelectOption(target));
        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
}
function cascadeMultiTerms(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetTermsByAcademicLevelId,
            data: {
                id: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.termText));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeFaculties(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetFacultiesByAcademicLevelId,
            data: {
                id: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeMultiFaculties(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetFacultiesByAcademicLevelId,
            data: {
                id: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCurriculums(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url : SelectListURLDict.GetCurriculumByAcademicLevelId,
            data : { 
                id: academicLevelId, 
            },
            dataType : 'json',
        }
    );

    ajax.POST().done(function (response) { 
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName))
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeMultipleCurriculums(academicLevelId, facultyIds, departmentIds, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumsByDepartmentIds,
            data: {
                academicLevelId: academicLevelId,
                facultyIds: facultyIds,
                departmentIds: departmentIds,
            },
            dataType: 'json'
        }
    );
    
    ajax.POST().done(function (response) { 
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeSingleCurriculums(academicLevelId, facultyIds, departmentIds, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumsByDepartmentIds,
            data: {
                academicLevelId: academicLevelId,
                facultyIds: facultyIds,
                departmentIds: departmentIds,
            },
            dataType: 'json'
        }
    );
    
    ajax.POST().done(function (response) { 
        target.empty();
        target.append(getDefaultSelectOption(target));
        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeDegreeName(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url : SelectListURLDict.GetDegreeNamesByAcademicLevelId,
            data : { 
                academicLevelId: academicLevelId, 
            },
            dataType : 'json',
        }
    );

    ajax.POST().done(function (response) { 
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeAdmissionRounds(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url : SelectListURLDict.GetAdmissionRoundByAcademicLevelId,
            data : { 
                id: academicLevelId, 
            },
            dataType : 'json',
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text))
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeAcademicPrograms(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url : SelectListURLDict.GetAcademicProgramsByAcademicLevelId,
            data : { 
                id: academicLevelId, 
            },
            dataType : 'json',
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text))
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

// term required
function cascadeCourses(termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesByTerm,
            data: {
                termId: termId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(target).append(getDefaultSelectOption($(target)));

        response.forEach((item) => {
            $(target).append(getSelectOptions(item.value, item.text))
        });
        
        $(target).trigger('chosen:updated')
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCoursesByTermAndFaculty(termId, facultyId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesSelectList,
            data: {
                termId: termId,
                facultyId: facultyId,
                departmentId: 0
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(target).append(getDefaultSelectOption($(target)));

        response.forEach((item) => {
            $(target).append(getSelectOptions(item.value, item.text))
        });
        
        $(target).trigger('chosen:updated')
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCoursesByAcademicLevel(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesByAcademicLevelId,
            data: {
                academicLevelId: academicLevelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text))
        });
        
        target.trigger('chosen:updated')
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeMultiCourses(termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesByTerm,
            data: {
                termId: termId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(target).empty();

        response.forEach((item) => {
            $(target).append(getSelectOptions(item.value, item.text));
        });
        
        $(target).trigger('chosen:updated')
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeAdmissionRound(admissionTermId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetAdmissionRoundByTermId,
            data: {
                id: admissionTermId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        let firstDefaultOpt = target.find('option:first');
        if (firstDefaultOpt.attr('disabled')) {
            target.empty();
        } else {
            target.append(getDefaultSelectOption(target));
        }

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });
        
        target.trigger('chosen:updated');
    })
}

function cascadeAdmissionRoundByAcademicLevelIdAndTermId(academicLevelId, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetAdmissionRoundByAcademicLevelIdAndTermId,
            data: {
                academicLevelId: academicLevelId,
                termId: termId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption($(admissionRound)));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });
        
        target.trigger('chosen:updated');
    })
}

// admission round required
function cascadeFirstClassDate(admissionRoundId, target) {
    var ajax = new AJAX_Helper(
        {
            url: GetAdmissionFirstClassDate,
            data: {
                admissionRoundId: admissionRoundId
            },
            dataType: 'json'
        }
    );
    
    ajax.POST().done(function (response) {
        $(target).val(response);
        DateTimeInput.renderSingleDate($('.js-single-date'));
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

// faculty required
function cascadeInstructors(facultyId, targetInstructor, targetMultiInstructor) {
    var ajax = new AJAX_Helper(
        {
            url:  SelectListURLDict.GetInstructorsByFacultyId,
            data: {
                id: facultyId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        targetInstructor.append(getDefaultSelectOption($(instructor)));
        targetMultiInstructor.empty();

        response.forEach((item) => {
            targetInstructor.append(getSelectOptions(item.id, item.codeAndName));
            targetMultiInstructor.append(getSelectOptions(item.id, item.codeAndName));
        });

        targetInstructor.trigger("chosen:updated");
        targetMultiInstructor.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

// department required
function cascadeMinors(departmentId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetMinorsByDepartmentId,
            data: {
                id: departmentId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeConcentrations(departmentId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetConcentrationsByDepartmentId,
            data: {
                id: departmentId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCurriculumVersions(curriculumId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumVersion,
            data: {
                curriculumId: curriculumId
            }, 
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCourseGroup(curriculumVersionId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumCourseGroups,
            data: {
                versionId: curriculumVersionId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.empty().append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCurriculumCourse(versionId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumCourse,
            data: {
                versionId: versionId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCourseGroup(curriculumVersionId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumCourseGroups,
            data: {
                versionId: curriculumVersionId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCurriculumVersionsForTransfer(curriculumId, studentCode, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSelectableCurriculumVersionList,
            data: {
                curriculumId: curriculumId,
                studentCode: studentCode
            }, 
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

// admission round required
function cascadeStudentCodeRange(admissionRoundId, startedTarget, endedTarget) {
    var ajax = new AJAX_Helper(
        {
            url: StudentCodeStatusGetCodeRangeUrl,
            data: {
                admissionRoundId: admissionRoundId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        if (response != undefined) {
            startedTarget.val(response.startedCode);
            endedTarget.val(response.endedCode);
        } else {
            startedTarget.val("");
            endedTarget.val("");
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}
/* --- end section --- */

/* --- multiple cascade section --- */
// term required
function cascadeSectionsByCourseAndTerm(courseId, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionByCourseId,
            data: {
                termId: termId,
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeInstructorBySection(termId, courseId, sectionId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetInstructorBySectionId,
            data: {
                termId: termId,
                courseId: courseId,
                sectionId: sectionId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeMultiCoursesByAcademicLevel(academicLevelId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesByAcademicLevelId,
            data: {
                academicLevelId: academicLevelId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeMultipleSectionsByCourseAndTerm(courseId, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionByCourseId,
            data: {
                termId: termId,
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeSectionsByCoursesAndTerm(courseIds, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionNumbersByCourses,
            data: {
                termId: termId, 
                ids: courseIds.toString()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item, item));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeInstructorsByCourseAndTerm(courseId, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetTermInstructorsByCourseId,
            data: {
                termId: termId,
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    })
}

function cascadeCoursesByStudentAndTerm(studentId, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesByStudentAndTermForCheating,
            data: {
                studentId: studentId,
                termId: termId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text))
        });
        
        target.trigger('chosen:updated')
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCoursesBySectionStatus(sectionStatus, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesBySectionStatus,
            data: {
                termId: termId,
                sectionStatus: sectionStatus
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(target).append(getDefaultSelectOption($(target)));

        response.forEach((item) => {
            $(target).append(getSelectOptions(item.value, item.text));
        });

        $(target).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    })
}

function cascadeSectionsByInstructorAndCourseAndTerm(instructorId, termId, courseId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionsByInstructorId,
            data: {
                instructorId: instructorId,
                termId: termId,
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.number));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    })
}

function cascadeCoursesByStudentCodeAndTerm(studentCode, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesByStudentCodeAndTermForCheating,
            data: {
                studentCode: studentCode,
                termId: termId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text))
        });
        
        target.trigger('chosen:updated')
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

// academic level required
function cascadeDepartmentsByAcademicLevelAndFaculty(academicLevelId, facultyId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetDepartmentsByAcademicLevelIdAndFacultyId,
            data: {
                academicLevelId: academicLevelId,
                facultyId: facultyId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(target).append(getDefaultSelectOption($(target)));

        response.forEach((item) => {
            $(target).append(getSelectOptions(item.id, item.codeAndName));
        });

        $(target).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeMultipleDepartments(academicLevelId, facultyIds, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetDepartmentsByFacultyIds,
            data: {
                academicLevelId: academicLevelId,
                ids: facultyIds,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCurriculumsByAcademicAndFacAndDep(academicLevelId, facultyId, departmentId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumByDepartment,
            data: {
                academicLevelId: academicLevelId,
                facultyId: facultyId,
                departmentId: departmentId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(target).append(getDefaultSelectOption($(target)));
 
        response.forEach((item) => {
            $(target).append(getSelectOptions(item.value, item.text));
        });

        $(target).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeDepartmentsByFaculty(facultyId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetDepartmentsByFacultyId,
            data: {
                facultyId: facultyId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(target).append(getDefaultSelectOption($(target)));

        response.forEach((item) => {
            $(target).append(getSelectOptions(item.value, item.text));
        });

        $(target).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCourseGroupByCurriculumsVersionAndCourse(curriculumVersionId, courseId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCourseGroupByCurriculumsVersionAndCourse,
            data: {
                curriculumVersionId: curriculumVersionId,
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));
        
        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

// must refactor
function cascadeCurriculumVersionsByTermAndRoundAndFacAndDep(termId, roundId, facultyId, departmentId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumVersionForAdmissionCurriculum,
            data: {
                termId: termId,
                admissionRoundId: roundId,
                facultyId: facultyId,
                departmentId: departmentId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeMultipleCurriculumVersions(academicLevelId, curriculumIds, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCurriculumVersionsByCurriculumIds,
            data: {
                academicLevelId: academicLevelId,
                curriculumIds: curriculumIds,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.empty();

        response.forEach((item) => {
            target.append(getSelectOptions(item.id, item.codeAndName));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeAdmissionDocumentGroup(academicLevelId, facultyId, departmentId, preSchoolCountryId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetStudentDocumentGroupsByCountryId,
            data: {
                academicLevelId: academicLevelId,
                facultyId: facultyId,
                departmentId: departmentId,
                previousSchoolCountryId: preSchoolCountryId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadePreviousSchoolByCountry(schoolCountryId, target) {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetPreviousSchoolsByCountryId,
            data: {
                id: schoolCountryId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}
/* --- end section --- */

/* --- cascade events --- */
// academic level
function cascadeEvents()
{
    $(document).on('change', academicLevel, function() {
        let currentSection = $(this).closest('section');
        let academicLevelId = $(this).val();
    
        //require check fields & target
        let sectionAdmissionTerm = currentSection.find(admissionTerm);
        let sectionTerm = currentSection.find(term);
        let sectionMultiTerm = currentSection.find(multiTerm);
        let sectionFaculty = currentSection.find(faculty);
        let sectionMultiFaculty = currentSection.find(multiFaculty);
        let sectionCourseFaculty = currentSection.find(courseFaculty);
        let sectionCurriculum = currentSection.find(curriculum);
        let sectionAdmissionRound = currentSection.find(admissionRound);
        let sectionAcademicProgram = currentSection.find(academicProgram);
        let sectionDegreenName = currentSection.find(degreenName);
        let sectionDepartment = currentSection.find(department);
        let sectionCourse = currentSection.find(course);
        let sectionMultiCourse = currentSection.find(multiCourse);
    
        //cascadeTerms
        let bodyAdmissionTerm = $('body').find(admissionTerm);

        //cascadeExpectedGraduation
        let ddlExpectedGraduationYears = currentSection.find(graduatedYear);
        let ddlExpectedGraduationTerms = currentSection.find(graduatedTerm);
    
        if (sectionAdmissionTerm.length > 0 || sectionTerm.length > 0) {
            let termSelect;
            if (bodyAdmissionTerm.length > 0) {
                termSelect = $(bodyAdmissionTerm);
                cascadeTerms(academicLevelId, bodyAdmissionTerm);
            }
        
            if (sectionTerm.length > 0) {
                cascadeTerms(academicLevelId, sectionTerm);
            }
        
            if (bodyAdmissionTerm.length == 0) {
                termSelect = $(sectionTerm);
                cascadeTerms(academicLevelId, sectionTerm);
            }
        }
    
        if (sectionMultiTerm.length > 0) {
            cascadeMultiTerms(academicLevelId, sectionMultiTerm);
        }
    
        if (sectionFaculty.length > 0) {
            cascadeFaculties(academicLevelId, sectionFaculty);
        }
    
        if (sectionMultiFaculty.length > 0) {
            cascadeMultiFaculties(academicLevelId, sectionMultiFaculty);
        }
    
        if (sectionCourseFaculty.length > 0) {
            cascadeFaculties(academicLevelId, sectionCourseFaculty);
        }
        
        if (sectionCurriculum.length > 0) {
            cascadeCurriculums(academicLevelId, sectionCurriculum);
        }
        
        if (sectionAdmissionRound.length > 0) {
            cascadeAdmissionRounds(academicLevelId, sectionAdmissionRound);
        }
    
        if (sectionCourse.length > 0 && sectionTerm.length == 0) {
            cascadeCoursesByAcademicLevel(academicLevelId, sectionCourse);
        }
    
        if (sectionAcademicProgram.length > 0) {
            cascadeAcademicPrograms(academicLevelId, sectionAcademicProgram);
        }
    
        if (sectionDegreenName.length > 0) {
            cascadeDegreeName(academicLevelId, sectionDegreenName);
        }
    
        // multiple
        if (sectionFaculty.length > 0 && sectionDepartment.length > 0) {
            let secFacultyId = sectionFaculty.val();
            let sectionDocumentGroup = currentSection.find(documentGroup);
            let sectionPreviousSchool = currentSection.find(previousSchool);
        
            cascadeDepartmentsByAcademicLevelAndFaculty(academicLevelId, secFacultyId, sectionDepartment);
        
            if (sectionCurriculum.length > 0) {
                let secDepartmentId = sectionDepartment.val();
            
                cascadeCurriculumsByAcademicAndFacAndDep(academicLevelId, secFacultyId, secDepartmentId, sectionCurriculum);
            }
        
            if (sectionDocumentGroup.length > 0 && sectionPreviousSchool.length > 0) {
                let secDepartmentId = sectionDepartment.val();
                let previousSchoolCountryId = currentSection.find(previousSchoolCountry).val();
                
                cascadeAdmissionDocumentGroup(academicLevelId, secFacultyId, secDepartmentId, previousSchoolCountryId, sectionDocumentGroup);
            }
        }
    
        if (sectionMultiCourse.length > 0) {
            cascadeMultiCoursesByAcademicLevel(academicLevelId, sectionMultiCourse);
        }

        if (ddlExpectedGraduationYears.length > 0) {
            cascadeExpectedGradYear(academicLevelId, ddlExpectedGraduationYears);
        }
        if (ddlExpectedGraduationTerms.length > 0) {
            cascadeExpectedGradTerm(academicLevelId, ddlExpectedGraduationTerms);
        }
    });
    
    // term
    $(document).on('change', term, function() {
        let currentRow = $(this).closest('tr');
        let currentSection = $(this).closest('section');
        let termId = $(this).val();
    
        //require check fields & target
        let sectionCourse = currentSection.find(course);
        let sectionMultiCourse = currentSection.find(multiCourse);
        let sectionFaculty = currentSection.find(fixFaculty);
        if (currentRow.length > 0) {
            sectionCourse = currentRow.find(course);
        }
        
        if (sectionCourse.length > 0) {
            let sectionStudent = currentSection.find(student);
            let sectionSectionStatus = currentSection.find(sectionStatus);
            let sectionStudentCode = currentSection.find(studentCode);
            if (sectionStudent.length > 0) {
                cascadeCoursesByStudentAndTerm(sectionStudent.val(), termId, sectionCourse);
            } else if (sectionSectionStatus.length > 0) {
                cascadeCoursesBySectionStatus(sectionSectionStatus.val(), termId, sectionCourse);
            } else if (sectionStudentCode.length > 0) {
                cascadeCoursesByStudentCodeAndTerm(sectionStudentCode.val(), termId, sectionCourse);
            } else if (sectionFaculty.length > 0) {
                cascadeCoursesByTermAndFaculty(termId, sectionFaculty.val(), sectionCourse);
            } else {
                cascadeCourses(termId, sectionCourse);
            }
        }
    
        if (sectionMultiCourse.length > 0) {
            cascadeMultiCourses(termId, sectionMultiCourse)
        }
    })
    
    // student code
    $(document).on('change', studentCode, function() {
        let currentSection = $(this).closest('section');
        let sectionStudentCode = $(this).val();
    
        //require check fields & target
        let sectionCourse = currentSection.find(course);
        
        if (sectionCourse.length > 0) {
            let sectionTerm = currentSection.find(term);
            if (sectionTerm.length > 0) {
                cascadeCoursesByStudentCodeAndTerm(sectionStudentCode, sectionTerm.val(), sectionCourse);
            }
        }
    })
    
    // admission
    $(document).on('change', admissionTerm, function() {
        let currentSection = $(this).closest('section');
        let admissionTermId = $(this).val();
    
        //require check fields & target
        let sectionAcademicLevel = currentSection.find(academicLevel);
        let sectionAdmissionRound = currentSection.find(admissionRound);
        let sectionFirstClassDate = currentSection.find(firstClassDate);
    
        if (sectionAdmissionRound.length > 0) {
            if (sectionAcademicLevel.length > 0) {
                cascadeAdmissionRoundByAcademicLevelIdAndTermId(sectionAcademicLevel.val(), admissionTermId, sectionAdmissionRound);
            } else if (sectionFirstClassDate.length > 0) {
                cascadeFirstClassDate(sectionAdmissionRound.val(), sectionFirstClassDate);
            } else {
                cascadeAdmissionRound(admissionTermId, sectionAdmissionRound);
            }
        } 
    });
    
    $(document).on('change', admissionRound, function() {
        let currentSection = $(this).closest('section');
        let admissionRoundId = $(this).val();
    
        //require check fields & target
        let sectionFirstClassDate = currentSection.find(firstClassDate);
        let sectionStartedCode = currentSection.find(startedCode);
        let sectionEndedCode = currentSection.find(endedCode);
        let sectionTerm = currentSection.find(admissionTerm);
        let sectionFaculty = currentSection.find(faculty);
        let sectionDepartment = currentSection.find(department);
        let sectionCurriculumVersion = currentSection.find(curriculumVersion);
    
        if (sectionFirstClassDate.length > 0) {
            cascadeFirstClassDate(admissionRoundId, sectionFirstClassDate);
        }
    
        if (sectionStartedCode.length > 0 && sectionEndedCode.length > 0) {
            cascadeStudentCodeRange(admissionRoundId, sectionStartedCode, sectionEndedCode);
        }
    
        if (sectionCurriculumVersion.length > 0) {
            let termId = sectionTerm.val();
            let facultyId = sectionFaculty.val();
            let departmentId = sectionDepartment.val();
            cascadeCurriculumVersionsByTermAndRoundAndFacAndDep(termId, admissionRoundId, facultyId, departmentId, sectionCurriculumVersion);
        }
    });
    
    // faculty
    $(document).on('change', faculty, function() {
        let currentSection = $(this).closest('section');
        let facultyId = $(this).val();
        let curriculumId = $(this).val();
    
        //require check fields & target
        let sectionInstructor = currentSection.find(instructor);
        let sectionMultiInstructor = currentSection.find(multiInstructor);
        let sectionAcademicLevel = currentSection.find(academicLevel);
        let sectionDepartment = currentSection.find(department);
        let sectionCurriculum = currentSection.find(curriculum);
        let sectionCurriculumVersion = currentSection.find(curriculumVersion);
        let sectionDocumentGroup = currentSection.find(documentGroup);
        let sectionPreviousSchoolCountry = currentSection.find(previousSchoolCountry);
    
        cascadeDepartmentsByFaculty(facultyId, sectionDepartment);
    
        if (sectionInstructor.length > 0 || sectionMultiInstructor.length > 0) {
            cascadeInstructors(facultyId, sectionInstructor, sectionMultiInstructor);
        }
    
        // multiple
        if (sectionAcademicLevel.length > 0 && sectionDepartment.length > 0) {
            let secDepartmentId = sectionDepartment.val();
            let secAcademicLevelId = sectionAcademicLevel.val();
        
            if (sectionCurriculum.length > 0) {
                cascadeCurriculumsByAcademicAndFacAndDep(secAcademicLevelId, facultyId, secDepartmentId, sectionCurriculum);
            }
        
            if (sectionCurriculumVersion.length > 0) {
                cascadeCurriculumVersions(curriculumId, sectionCurriculumVersion);;
            }
        
            if (sectionDocumentGroup.length > 0 && sectionPreviousSchoolCountry.length > 0) {
                let secPreviousSchoolCountryId = sectionPreviousSchoolCountry.val();
                cascadeAdmissionDocumentGroup(secAcademicLevelId, facultyId, secDepartmentId, secPreviousSchoolCountryId, sectionDocumentGroup);
            }
        
            cascadeDepartmentsByAcademicLevelAndFaculty(secAcademicLevelId, facultyId, sectionDepartment);
        }
    });

    $(document).on('change', facultyAll, function () {
        let currentSection = $(this).closest('section');
        let facultyId = $(this).val();
        let curriculumId = $(this).val();

        //require check fields & target
        let sectionInstructor = currentSection.find(instructor);
        let sectionMultiInstructor = currentSection.find(multiInstructor);
        let sectionAcademicLevel = currentSection.find(academicLevel);
        let sectionDepartment = currentSection.find(department);
        let sectionCurriculum = currentSection.find(curriculum);
        let sectionCurriculumVersion = currentSection.find(curriculumVersion);
        let sectionDocumentGroup = currentSection.find(documentGroup);
        let sectionPreviousSchoolCountry = currentSection.find(previousSchoolCountry);

        cascadeDepartmentsByFaculty(facultyId, sectionDepartment);

        if (sectionInstructor.length > 0 || sectionMultiInstructor.length > 0) {
            cascadeInstructors(facultyId, sectionInstructor, sectionMultiInstructor);
        }

        // multiple
        if (sectionAcademicLevel.length > 0 && sectionDepartment.length > 0) {
            let secDepartmentId = sectionDepartment.val();
            let secAcademicLevelId = sectionAcademicLevel.val();

            if (sectionCurriculum.length > 0) {
                cascadeCurriculumsByAcademicAndFacAndDep(secAcademicLevelId, facultyId, secDepartmentId, sectionCurriculum);
            }

            if (sectionCurriculumVersion.length > 0) {
                cascadeCurriculumVersions(curriculumId, sectionCurriculumVersion);;
            }

            if (sectionDocumentGroup.length > 0 && sectionPreviousSchoolCountry.length > 0) {
                let secPreviousSchoolCountryId = sectionPreviousSchoolCountry.val();
                cascadeAdmissionDocumentGroup(secAcademicLevelId, facultyId, secDepartmentId, secPreviousSchoolCountryId, sectionDocumentGroup);
            }
        }
    });
    
    // department
    $(document).on('change', department, function() {
        let currentSection = $(this).closest('section');
        let departmentId = $(this).val();
        let curriculumId = $(this).val();
    
        //require check fields & target
        let sectionMinor = currentSection.find(minor);
        let sectionConcentration = currentSection.find(concentration);
        let sectionAcademicLevel = currentSection.find(academicLevel);
        let sectionFaculty = currentSection.find(faculty);
        let sectionFacultyAll = currentSection.find(facultyAll);
        let sectionCurriculumVersion = currentSection.find(curriculumVersion);
        let sectionTerm = currentSection.find(term);
        let sectionAdmissionTerm = currentSection.find(admissionTerm);
        let sectionAdmissionRound = currentSection.find(admissionRound); 
    
        if (sectionMinor.length > 0) {
            cascadeMinors(departmentId, sectionMinor);
        }
        
        if (sectionConcentration.length > 0) {
            cascadeConcentrations(departmentId, sectionConcentration);
        }
    
        if (sectionCurriculumVersion.length > 0) {
            cascadeCurriculumVersions(curriculumId, sectionCurriculumVersion);;
        }

        if (sectionAcademicLevel.length > 0 && (sectionFaculty.length > 0 || sectionFacultyAll.length > 0)) {
        
            //require check fields & target
            let sectionAcademicLevel = currentSection.find(academicLevel);
            let sectionFaculty = currentSection.find(faculty);
            let sectionFacultyAll = currentSection.find(facultyAll);
            let sectionCurriculum = currentSection.find(curriculum);
            let sectionDocumentGroup = currentSection.find(documentGroup);
            let sectionPreviousSchoolCountry = currentSection.find(previousSchoolCountry);
        
            if (sectionCurriculum.length > 0) {
                let secAcademicLevelId = sectionAcademicLevel.val();
                if (sectionFaculty.length > 0) {
                    let secFacultyId = sectionFaculty.val();
                    cascadeCurriculumsByAcademicAndFacAndDep(secAcademicLevelId, secFacultyId, departmentId, sectionCurriculum);
                } else if (sectionFacultyAll.length > 0) {
                    let secFacultyId = sectionFacultyAll.val();
                    cascadeCurriculumsByAcademicAndFacAndDep(secAcademicLevelId, secFacultyId, departmentId, sectionCurriculum);
                }
            }
        
            if (sectionDocumentGroup.length > 0 && sectionPreviousSchoolCountryCountry.length > 0) {
                let secAcademicLevelId = sectionAcademicLevel.val();
                if (sectionFaculty.length > 0) {
                    let secFacultyId = sectionFaculty.val();
                    let secPreviousSchoolCountryId = sectionPreviousSchoolCountry.val();
                    cascadeAdmissionDocumentGroup(secAcademicLevelId, secFacultyId, departmentId, secPreviousSchoolCountryId, sectionDocumentGroup);
                } else if (sectionFacultyAll.length > 0) {
                    let secFacultyId = sectionFacultyAll.val();
                    let secPreviousSchoolCountryId = sectionPreviousSchoolCountry.val();
                    cascadeAdmissionDocumentGroup(secAcademicLevelId, secFacultyId, departmentId, secPreviousSchoolCountryId, sectionDocumentGroup);
                }
            }
        
            if ($(multiCourse).length > 0) {
                cascadeSectionsByCoursesAndTerm(courseIds, termId);
            }
        
            if ($().length > 0) {
                (academicLevelId);
            }
        }
    
        if (sectionCurriculumVersion.length > 0 && sectionTerm.length > 0 && sectionAdmissionTerm.length > 0 && sectionFaculty.length > 0) {
            let secTermId = sectionTerm.val();
            let secAdmissionRoundId = sectionAdmissionRound.val();
            let secFacultyId = sectionFaculty.val();
            cascadeCurriculumVersionsByTermAndRoundAndFacAndDep(secTermId, secAdmissionRoundId, secFacultyId, departmentId, sectionCurriculumVersion);
        }
    
        // Admission Student cascade Department to Curriculum Version
        if (sectionCurriculumVersion.length > 0 && sectionAdmissionTerm.length > 0 && sectionAdmissionRound.length > 0 && sectionFaculty.length > 0) {
            let secAdmissionTermId = sectionAdmissionTerm.val();
            let secAdmissionRoundId = sectionAdmissionRound.val();
            let secFacultyId = sectionFaculty.val();
            cascadeCurriculumVersionsByTermAndRoundAndFacAndDep(secAdmissionTermId, secAdmissionRoundId, secFacultyId, departmentId, sectionCurriculumVersion);
        }
    });
    
    $(document).on('change', curriculum, function() {
        let currentSection = $(this).closest('section');
        let curriculumId = $(this).val();
    
        //require check fields & target
        let sectionCurriculumVersion = currentSection.find(curriculumVersion);
        let sectionStudentCode = currentSection.find(studentCode);
    
        if (sectionCurriculumVersion.length > 0 && sectionStudentCode.length > 0) {
            cascadeCurriculumVersionsForTransfer(curriculumId, sectionStudentCode.val(), sectionCurriculumVersion);
        } else if ($(selectableCurriculumVersion).length > 0) {
            cascadeCurriculumVersionsForTransfer($(this).val(), code);
        } else if (sectionCurriculumVersion.length > 0) {
            cascadeCurriculumVersions(curriculumId, sectionCurriculumVersion);
        }
    });
    
    $(document).on('change', curriculumVersion, function() {
        let currentSection = $(this).closest('section');
        let versionId = $(this).val();
    
        //require check fields & target
        let sectionCourseGroup = currentSection.find(courseGroup);
        let sectionCurriculumCourse = currentSection.find(curriculumCourse);
    
        if (sectionCourseGroup.length > 0) {
            cascadeCourseGroup(versionId, sectionCourseGroup);
        }
        
        if (sectionCurriculumCourse.length > 0) {
            cascadeCurriculumCourse(versionId, sectionCurriculumCourse);
        }
    });
    
    $(document).on('change', course, function() {
        let currentSection = $(this).closest('section');
        let courseId= $(this).val();
    
        //require check fields & target
        let sectionSection = currentSection.find(section);
        let sectionMultiSection = currentSection.find(multiSection);
        let sectionInstructor = currentSection.find(instructor);
        let secTermId = currentSection.find(term).val();
    
        if (secTermId === null) {
            Alert.renderAlert("Warning", "Section and Instructor won't available to choose until select Academic Level and Term first.", "warning")
        } else {
            if (sectionSection.length > 0) {
                cascadeSectionsByCourseAndTerm(courseId, secTermId, sectionSection);
            }
        
            if (sectionMultiSection.length > 0) {
                cascadeMultipleSectionsByCourseAndTerm(courseId, secTermId, sectionMultiSection);
            }
            
            if (sectionInstructor.length > 0) {
                cascadeInstructorsByCourseAndTerm(courseId, secTermId, sectionInstructor);
            }
        }
    });
    
    $(document).on('change', section, function() {
        let currentSection = $(this).closest('section');
        let sectionId = $(this).val();
    
        //require check fields & target
        let sectionInstructorSection = currentSection.find(instructorSection);
        let secCourseId = currentSection.find(course).val();
        let secTermId = currentSection.find(term).val();
    
        if (sectionInstructorSection.length > 0) {
            cascadeInstructorBySection(secTermId, secCourseId, sectionId, sectionInstructorSection);
        }
    });
    
    $(document).on('change', multiCourse, function() {
        let currentSection = $(this).closest('section');
        let courseIds = $(this).val();
    
         //require check fields & target
         let sectionMultiSection = currentSection.find(multiSection);
         let secTermId = currentSection.find(term).val();
    
        if (secTermId === null) {
            Alert.renderAlert("Warning", "Section won't available to choose until select Academic Level and Term first.", "warning")
        } else {
            if (sectionMultiSection.length > 0) {
                cascadeSectionsByCoursesAndTerm(courseIds, secTermId, sectionMultiSection);
            }
        }
    });
    
    $(document).on('change', instructor, function() {
        let currentSection = $(this).closest('section');
        let instructorId = $(this).val();
    
        //require check fields & target
        let sectionSection = currentSection.find(section);
        let secTermId = currentSection.find(term).val();
        let secCourseId = currentSection.find(course).val();
    
        if (secTermId === null) {
            Alert.renderAlert("Warning", "Section won't available to choose until select Academic Level and Term first.", "warning")
        } else {
            if (sectionSection.length > 0) {
                cascadeSectionsByInstructorAndCourseAndTerm(instructorId, secTermId, secCourseId, sectionSection);
            }
        }
    });
    
    $(document).on('change', sectionStatus, function() {
        let currentSection = $(this).closest('section');
        let sectionStatusType = $(this).val();
    
        //require check fields & target
        let sectionCourse = currentSection.find(course);
        let sectionTerm = currentSection.find(term);
    
        if (sectionCourse.length > 0 && sectionTerm.length > 0) {
            let secTermId = sectionTerm.val();
            cascadeCoursesBySectionStatus(sectionStatusType, secTermId, sectionCourse);
        }
    });
    
    $(document).on('change', multiFaculty, function() {
        let currentSection = $(this).closest('section');
        let facultyIds = $(this).val();
    
        //require check fields & target
        let sectionMultiDepartment = currentSection.find(multiDepartment);
        let secAcademicLevelId = currentSection.find(academicLevel).val();
    
        if (sectionMultiDepartment.length > 0) {
            cascadeMultipleDepartments(secAcademicLevelId, facultyIds, sectionMultiDepartment);
        }
    });
    
    $(document).on('change', multiDepartment, function() {
        let currentSection = $(this).closest('section');
        let departmentIds = $(this).val();
    
        //require check fields & target
        let sectionMultiCurriculum = currentSection.find(multiCurriculum);
        let sectionCurriculum = currentSection.find(curriculum);
        let academicLevelId = currentSection.find(academicLevel).val()
        let facultyIds = currentSection.find(multiFaculty).val()
    
        if (sectionMultiCurriculum.length > 0) {
            cascadeMultipleCurriculums(academicLevelId, facultyIds, departmentIds, sectionMultiCurriculum);
        }
    
        if (sectionCurriculum.length > 0) {
            cascadeSingleCurriculums(academicLevelId, facultyIds, departmentIds, sectionCurriculum);
        }
    });
    
    $(document).on('change', multiCurriculum, function() {
        let currentSection = $(this).closest('section');
        let curriculumIds = $(this).val();
    
        //require check fields & target
        let sectionMultiCurriculumVersion = currentSection.find(multiCurriculumVersion);
        let academicLevelId = currentSection.find(academicLevel).val()
    
        if (sectionMultiCurriculumVersion.length > 0) {
            cascadeMultipleCurriculumVersions(academicLevelId, curriculumIds, sectionMultiCurriculumVersion);
        }
    });
    
    $(document).on('change', previousSchoolCountry, function() {
        let currentSection = $(this).closest('section');
        let previousSchoolCountryId = $(this).val();
    
        //require check fields & target
        let sectionDocumentGroup = currentSection.find(documentGroup);
        let sectionPreviousSchool = currentSection.find(previousSchool);
    
        if (sectionDocumentGroup.length > 0) {
            let secFaculty = currentSection.find(faculty);
            let sectionFacultyAll = currentSection.find(facultyAll);
            let secAcademicLevelId = currentSection.find(admissionAcademicLevel).val();
            let secDepartmentId = currentSection.find(department).val();
            if (secFaculty.length > 0) {
                let secFacultyId = secFaculty.val();
                cascadeAdmissionDocumentGroup(secAcademicLevelId, secFacultyId, secDepartmentId, previousSchoolCountryId, sectionDocumentGroup);
            }
            else if (sectionFacultyAll.length > 0) {
                let secFacultyId = sectionFacultyAll.val();
                cascadeAdmissionDocumentGroup(secAcademicLevelId, secFacultyId, secDepartmentId, previousSchoolCountryId, sectionDocumentGroup);
            }
        }
    
        if (sectionPreviousSchool.length > 0) {
            cascadePreviousSchoolByCountry(previousSchoolCountryId, sectionPreviousSchool);
        } 
    });
}
$(document).ready(function(){
    cascadeEvents();

    $(document).on('shown.bs.modal', ".modal", function() {
        cascadeEvents();
    });
});
