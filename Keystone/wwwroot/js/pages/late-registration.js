/* --- main --- */
var academicLevel = '.js-cascade-academic-level';

var term = '.js-cascade-term';

var termNodisable = '.js-cascade-term-not-disable';

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

function cascadeTermsNoDisable(academicLevelId, target) {
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
        target.append('<option selected>Select</option>')
        response.forEach((item) => {
            console.log(item);
            target.append(`<option value="${ item.id }">${ item.termText }</option>`);
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
    let sectionTermNoDisable = currentSection.find(termNodisable);

    //cascadeTerms

    if (sectionTerm.length > 0) {
        cascadeTerms(academicLevelId, sectionTerm);
    }

    if (sectionTermNoDisable.length > 0) {
        console.log(sectionTermNoDisable);
        cascadeTermsNoDisable(academicLevelId, sectionTermNoDisable);
    }
});