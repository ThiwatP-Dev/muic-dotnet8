var summitSave = '.js-update-submit';
var exportExcel = '.js-export-excel';

$(document).ready( function() {
    CheckList.renderCheckbox('#js-update-fee-table');
    $(".js-render-nicescroll").niceScroll();
    checkSaveButton();
});

function checkTypeInput() {
    if ($('.js-resident-type').val() || $('.js-fee-type').val() || $('.js-fee-group').val()) {
        return true;
    } else {
        return false;
    }
}

function checkSaveButton() {
    if (checkTypeInput()) {
        if ($('.js-update-check:checked').length > 0) {
            $(summitSave).prop('disabled', false);
        } else {
            $(summitSave).prop('disabled', true);
        }
    } else {
        $(summitSave).prop('disabled', true);
    }
}

$(document).on('change', '.js-update-check', function() {
    $(summitSave).prop('disabled', true);
     if ($(this).is(':checked')) {
        checkSaveButton();
     } else {
        checkSaveButton();
     }
});

$('#checkAll').change(function() {
    $(summitSave).prop('disabled', true);
        if ($(this).is(':checked')) {
            checkSaveButton();
        } else {
           checkSaveButton();
        }
});

$('.js-resident-type').change(function() {
    checkSaveButton();
});

$('.js-fee-type').change(function() {
    checkSaveButton();
});

$('.js-fee-group').change(function() {
    checkSaveButton();
});

$(summitSave).on('click', function() {
    var form = '.update-student-fee-form';
    $(form).attr('action', '/UpdateStudentFee/Update');
});

$(exportExcel).on('click', function() {
    var form = '.update-student-fee-form';
    $(form).attr('action', '/UpdateStudentFee/ExportExcel');
});