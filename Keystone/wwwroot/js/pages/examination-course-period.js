var examinationCourse = '.js-cascade-course';
var condition = '#js-error-condition';

function renderCourseErrorCondition(courseId) {
    var ajax = new AJAX_Helper(
        {
            url: ExaminationCoursePeriodCondition,
            data: {
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        let errorMessage = "";

        errorMessage = response.split('/').join("<br>")
        
        $(condition).html(errorMessage)
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', examinationCourse, function() {
    let currentSection = $(this).closest('section');
    let courseId = $(this).val();

    let sectionExaminationCourse = currentSection.find(examinationCourse);

    if (sectionExaminationCourse.length > 0) {
        renderCourseErrorCondition(courseId);
    }
});
