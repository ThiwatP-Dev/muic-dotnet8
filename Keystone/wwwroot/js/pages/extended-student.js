var extendedTable = '#js-extended-student';
var extendedForm = "#js-submit-extended-student";
var extendReturnUrl = '#extened-return-url';
var extendAcademicLevelId = '#extened-academic-level-id';
var summitSave = '.js-save-submit';

var studentCheck = ".js-check-student";
var sendEmailCheck = ".js-check-send-email";
var studentGuid = ".js-get-guid";
var extendedYear = ".js-get-extendedyear";

function createExtendedStudentsData(form) {
    let students = [];

    $(form).find(studentCheck).each( function() {
        let currentRow = $(this).parents('tr');
        
        if ($(this).prop('checked') === true) {
            students.push(
                {
                    IsCheck : true,
                    IsSendEmail : currentRow.find(sendEmailCheck).prop('checked'),
                    StudentId : currentRow.find(studentGuid).val(),
                    ExtendedYear : currentRow.find(extendedYear).val()
                }
            )
        } 
    })

    return students;
}

function sendEmailStudentsData(form) {
    let studentIds = []

    $(form).find(studentCheck).each( function() {
        let currentRow = $(this).parents('tr');
        
        if ($(this).prop('checked') === true) {
            studentIds.push(currentRow.find(studentGuid).val());
        } 
    })

    return studentIds;
}

function checkSaveButton() {
    if ($('.js-check-student:checked').length > 0) {
        $(summitSave).prop('disabled', false);
    } else {
        $(summitSave).prop('disabled', true);
    }
}

$(document).on('change', '.js-check-student', function() {
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

$(extendedForm).on('submit', function(e) {
    $('#preloader').fadeIn();
    e.preventDefault();
    
    let targetAction = $(e.originalEvent.submitter).data('target');
    let ajaxSetting;

    if (targetAction === "SendEmail") {
        ajaxSetting = {
            url: ExtendedStudentSendEmailUrl,
            data: {
                studentIds : sendEmailStudentsData(this)
            },
            dataType: 'json',
        }
    } else {
        ajaxSetting = {
            url: ExtendedStudentCreateUrl,
            data: {
                model : createExtendedStudentsData(this),
                returnUrl : $(extendReturnUrl).val(),
                academicLevelId : $(extendAcademicLevelId).val()
            },
            dataType: 'json',
        }
    }

    var ajax = new AJAX_Helper(
        ajaxSetting
    );

    ajax.POST().done(function (response) {
        window.location.href = response.newUrl;
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });

    $('#preloader').fadeOut();
})

$(function() {
    CheckList.renderCheckbox(extendedTable);
})