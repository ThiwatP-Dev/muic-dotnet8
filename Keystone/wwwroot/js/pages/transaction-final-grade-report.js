let reportType = '.js-report-type';
let checkStudentCode = '.js-check-student-code';
let checkDate = '.js-check-date';
let checkMonth = '.js-check-month';
let checkTerm = '.js-check-term';

$(function() {
    checkReportType($(reportType));
})

$(reportType).on('change', function() {
    checkReportType($(this));
})

function checkReportType(reportType) {
    if (reportType.val() === 's') {
        $(checkStudentCode).prop('disabled', false).parent().removeClass('disable-item');
        $(checkDate).prop('disabled', true).parent().addClass('disable-item');
        $(checkMonth).addClass('disable-item').removeClass('enable-item')
        $(checkMonth).find('select').prop('disabled', true).trigger('chosen:updated');
        $(checkTerm).addClass('disable-item').removeClass('enable-item')
        $(checkTerm).find('select').prop('disabled', true).trigger('chosen:updated');
    } else if (reportType.val() === 'd') {
        $(checkDate).prop('disabled', false).parent().removeClass('disable-item');
        $(checkStudentCode).prop('disabled', true).parent().addClass('disable-item');
        $(checkMonth).addClass('disable-item').removeClass('enable-item')
        $(checkMonth).find('select').prop('disabled', true).trigger('chosen:updated');
        $(checkTerm).addClass('disable-item').removeClass('enable-item')
        $(checkTerm).find('select').prop('disabled', true).trigger('chosen:updated');
    } else if (reportType.val() === 'm') {
        $(checkMonth).addClass('enable-item').removeClass('disable-item')
        $(checkMonth).find('select').prop('disabled', false).trigger('chosen:updated');
        $(checkDate).prop('disabled', true).parent().addClass('disable-item');
        $(checkStudentCode).prop('disabled', true).parent().addClass('disable-item');
        $(checkTerm).addClass('disable-item').removeClass('enable-item')
        $(checkTerm).find('select').prop('disabled', true).trigger('chosen:updated');
    } else if (reportType.val() === 't') {
        $(checkTerm).addClass('enable-item').removeClass('disable-item')
        $(checkTerm).find('select').prop('disabled', false).trigger('chosen:updated');
        $(checkMonth).addClass('disable-item').removeClass('enable-item')
        $(checkMonth).find('select').prop('disabled', true).trigger('chosen:updated');
        $(checkDate).prop('disabled', true).parent().addClass('disable-item');
        $(checkStudentCode).prop('disabled', true).parent().addClass('disable-item');
    } else {
        $(checkStudentCode).prop('disabled', true).parent().addClass('disable-item');
        $(checkDate).prop('disabled', true).parent().addClass('disable-item');
        $(checkMonth).addClass('disable-item').removeClass('enable-item')
        $(checkMonth).find('select').prop('disabled', true).trigger('chosen:updated');
        $(checkTerm).addClass('disable-item').removeClass('enable-item')
        $(checkTerm).find('select').prop('disabled', true).trigger('chosen:updated');
    }
}