let reportType = '.js-report-type';
let checkStudentCourse = '.js-check-student-course';
let checkStudentBatch = '.js-check-student-batch';
let checkFacultyDepartment = '.js-check-faculty-department';

$(function() {
    checkReportType($(reportType));
})

$(reportType).on('change', function() {
    checkReportType($(this));
})

function checkReportType(reportType) {
    switch (reportType.val()) {
        case "b":
            $(checkStudentBatch).prop('disabled', false).parent().removeClass('disable-item')
            $(checkStudentCourse).addClass('disable-item').removeClass('enable-item')
            $(checkStudentCourse).find('select').prop('disabled', true).trigger('chosen:updated');
            $(checkFacultyDepartment).addClass('disable-item').removeClass('enable-item')
            $(checkFacultyDepartment).find('select').prop('disabled', true).trigger('chosen:updated');
            break;
        case "t":
            $(checkStudentCourse).addClass('enable-item').removeClass('disable-item')
            $(checkStudentCourse).find('select').prop('disabled', false).trigger('chosen:updated');
            $(checkStudentBatch).prop('disabled', true).parent().addClass('disable-item')
            $(checkFacultyDepartment).addClass('disable-item').removeClass('enable-item')
            $(checkFacultyDepartment).find('select').prop('disabled', true).trigger('chosen:updated');
            break;
        case "f":
            $(checkFacultyDepartment).addClass('enable-item').removeClass('disable-item')
            $(checkFacultyDepartment).find('select').prop('disabled', false).trigger('chosen:updated');
            $(checkStudentBatch).prop('disabled', true).parent().addClass('disable-item')
            $(checkStudentCourse).addClass('disable-item').removeClass('enable-item')
            $(checkStudentCourse).find('select').prop('disabled', true).trigger('chosen:updated');
            break;
        default:
            $(checkStudentCourse).addClass('disable-item')
            $(checkStudentCourse).removeClass('enable-item')
            $(checkStudentCourse).find('select').prop('disabled', true).trigger('chosen:updated');
            $(checkStudentBatch).prop('disabled', true).parent().addClass('disable-item')
            $(checkFacultyDepartment).addClass('disable-item')
            $(checkFacultyDepartment).removeClass('enable-item')
            $(checkFacultyDepartment).find('select').prop('disabled', true).trigger('chosen:updated');
            break;
    }
}