var reenterCourseCheck = '.js-reenter-course-check';
var reenterCredit = '.js-reenter-credit';
var reenterSumCredit = '.js-reenter-sum-credit';
var reenterTransferCredit = '.js-reenter-transfer-credit';
var reenterCourseSave = '.js-reenter-course-save';
var reenterCheckAll = '.js-check-all';
var sumCredit = 0;

$(document).ready( function() {
    CheckList.renderCheckbox('#js-reenter-students');
    $(".js-render-nicescroll").niceScroll();
});

$(document).on('change', reenterCourseCheck, function() {
    let currentCourseCheck = $(this).parents('tr').find(reenterCredit);
    if ($(this).prop('checked')) {
        sumCredit += Number($(currentCourseCheck).val());
    } else {
        sumCredit -= Number($(currentCourseCheck).val());
    }
    
    if (sumCredit > $(reenterTransferCredit).val()) {
        $(reenterSumCredit).text(sumCredit).addClass('text-danger');
        $(reenterCourseSave).prop('disabled', true);
    } else {
        $(reenterSumCredit).text(sumCredit).removeClass('text-danger');
        $(reenterCourseSave).prop('disabled', false);
    }
});

$(document).on('change', reenterCheckAll, function() {
    let currentCourseCheck = $(reenterCourseCheck).parents('tr').find(reenterCredit);
    sumCredit = 0;
    $(currentCourseCheck).each( function() {
        if ($(reenterCheckAll).prop('checked')) {
            sumCredit += Number($(this).val());
        } else {
            sumCredit = 0;
        }

        if (sumCredit > $(reenterTransferCredit).val()) {
            $(reenterSumCredit).text(sumCredit).addClass('text-danger');
            $(reenterCourseSave).prop('disabled', true);
        } else {
            $(reenterSumCredit).text(sumCredit).removeClass('text-danger');
            $(reenterCourseSave).prop('disabled', false);
        }
    })
});