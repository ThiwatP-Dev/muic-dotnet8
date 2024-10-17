var academicLevel = '.js-cascade-academic-level-exam';
var term = '.js-cascade-term-exam';
var course = '.js-cascade-course-master-only';
var section = '.js-cascade-section';
$(document).ready(function() {
    $('#exam-reservation-details-modal').on('shown.bs.modal', function(e) {
        let id = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: ExaminationReservationDetailsUrl,
                data: {
                    id: id,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-exam-reservation-details').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
});


function cascadeCoursesFromSectionMasterByTermId(termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesBySectionMasterByTerm,
            data: {
                termId: termId,
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

function cascadeSectionByTermAndCourse(termId, courseId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionMasterByTermIdAndCourseId,
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
            target.append(getSelectOptions(item.value, item.text))
        });
        
        target.trigger('chosen:updated')
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

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
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', academicLevel, function() {
    let currentSection = $(this).closest('section');
    let academicLevelId = $(this).val();

    //require check fields & target
    let sectionTerm = currentSection.find(term);

    if (sectionTerm.length > 0) {
        cascadeTerms(academicLevelId, sectionTerm);
    }
});

$(document).on('change', term, function() {
    let currentSection = $(this).closest('section');
    let termId = $(this).val();

    //require check fields & target
    let sectionCourse = currentSection.find(course);

    if (sectionCourse.length > 0) {
        cascadeCoursesFromSectionMasterByTermId(termId, sectionCourse);
    }
});

$(document).on('change', course, function() {
    let currentSection = $(this).closest('section');
    let courseId = $(this).val();
    let termSelect = currentSection.find(term);
    let termId = termSelect.val();
    //require check fields & target
    let sectionSection = currentSection.find(section);
    $('#InstructorId').val("0").trigger('chosen:updated');
    if (sectionSection.length > 0) {
        cascadeSectionByTermAndCourse(termId, courseId, sectionSection);
    }
});

$(document).on('change', section, function() {
    let sectionId = $(this).val();
    var ajax = new AJAX_Helper(
        {
            url: GetInstructorIdBySectionId,
            data: {
                sectionId: sectionId
            },
            dataType: 'json'
        }
    );

    ajax.GET().done(function (response) { 
        $('#InstructorId').val(response).trigger('chosen:updated');
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});