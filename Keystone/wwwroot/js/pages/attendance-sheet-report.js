var currentValue = "";
var currentType = "";
var attendanceInstructor = '.js-cascade-attendance-instructor';
var term = '.js-cascade-term';
var course = '.js-cascade-course';

function cascadeInstructors(termId, courseId) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetInstructorByCourseId,
            data: {
                termId: termId,
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(attendanceInstructor).append(getDefaultSelectOption($(attendanceInstructor)));

        response.forEach((item) => {
            $(attendanceInstructor).append(getSelectOptions(item.value, item.text));
        });

        $(attendanceInstructor).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    })
}

$(document).on('change', course, function() {
    var courseId = $(this).val();
    var termId = $(term).val();

    if (termId === null) {
        Alert.renderAlert("Warning", "Section and Instructor won't available to choose until select Academic Level and Term first.", "warning")
    } else {
        if ($(attendanceInstructor).length > 0) {
            cascadeInstructors(termId, courseId);
        }
    }
});