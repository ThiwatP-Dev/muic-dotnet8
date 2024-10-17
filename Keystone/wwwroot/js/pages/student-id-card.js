let formAreaIdcard = '#js-get-student-card-form';
let studentIdCardCode = '.js-student-code';
let studentCodeFrom = '.js-student-code-from';
let studentCodeTo = '.js-student-code-to';
let studentStatus = '.js-student-card-status';
let academicLevelId = '.js-cascade-academic-level';
let admissionRoundId = '.js-cascade-admission-round';
let previewButton = '.js-preview-button';
let admissionPrint = '.js-admission-print';
let cardType = '#js-id-card-type';
let checkAcademicLevel = '.js-check-academic-level';
let checkAdmissionRund = '.js-check-admission-round';
var examinationType = '.js-cascade-examination-type';
var examinationDate = '.js-cascade-examination-date';
var examinationCourse = '.js-cascade-examination-course';

function generateCertificateForm(formType, formAreaIdcard) {
    let ajaxUrl = "";

    switch (formType) {
        case "idcard":
            ajaxUrl = "/StudentIdCard/IdCard";
            break;

        case "substitudecard":
            ajaxUrl = "/StudentIdCard/SubstituteIdCard";
            break;

        case "masterapplication":
            ajaxUrl = "/StudentIdCard/MasterApplication";
            break;

        default:
            break;
    }

    var studentCodeData = {
        Code: $(studentIdCardCode).val(),
        FromCode: $(studentCodeFrom).val(),
        ToCode: $(studentCodeTo).val(),
        AcademicLevelId: $(academicLevelId).val(),
        AdmissionRoundId: $(admissionRoundId).val(),
        Status: $(studentStatus).val()
    }

    var ajax = new AJAX_Helper(
        {
            url: ajaxUrl,
            data: JSON.stringify(studentCodeData),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8'
        }
    );

    ajax.POST().done(function (response) {
        formAreaIdcard.empty().append(response);
        
        var message = $('#ErrorMessage').val();
        if (message == undefined || message == "") {
            $('#flash-message').empty();
            $(previewButton).prop('disabled', false);
        } else {
            FlashMessage.header("Danger", message)
            $(previewButton).prop('disabled', true);
        }

        $('.chosen-select').chosen();
        DateTimeInput.renderSingleDate($('.js-single-date'));
        DateTimeInput.renderYear($('.js-single-year'));

        ReportTable.print('.js-report-table');
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeExaminationDate(studentCode, examinationType) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetExaminationDateByExaminationType,
            data: {
                studentCode: studentCode,
                examinationType: examinationType
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(examinationDate).append(getDefaultSelectOption($(examinationDate)));

        response.forEach((item) => {
            $(examinationDate).append(getSelectOptions(item.value, item.text));
        });

        $(examinationDate).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeExaminationCourse(examinationDate, studentCode, examinationType) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetExaminationCourseByExaminationDate,
            data: {
                examinationDate: examinationDate,
                studentCode: studentCode,
                examinationType: examinationType
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(examinationCourse).append(getDefaultSelectOption($(examinationCourse)));

        response.forEach((item) => {
            $(examinationCourse).append(getSelectOptions(item.value, item.text));
        });

        $(examinationCourse).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$('#js-get-details').on('click', function() {
    let typeId = $(cardType).val();
    if (typeId === 'idcard') {
        $(previewButton).removeClass('d-none')
    }

    if (typeId != null) {
        $('#preloader').fadeIn();
        generateCertificateForm(typeId, $(formAreaIdcard))
        $('#preloader').fadeOut();
    } else {
        Alert.renderAlert("Error", "Please Select Card Type.", "error")
    }
})

$(admissionPrint).on('click', function() {
    let codeValue = $(this).data('value');

    if (codeValue != null) {
        generateTemporaryFormByAdmissionStudent(codeValue, $(formArea))
    }
})

$(cardType).on('change', function() {
    let type = $(cardType).val();
    if (type === 'idcard') {
        $(studentIdCardCode).prop('disabled', true).parent().addClass('disable-item')
        $(studentIdCardCode).val('')
        $(studentCodeFrom).prop('disabled', false).parent().removeClass('disable-item')
        $(studentCodeTo).prop('disabled', false).parent().removeClass('disable-item')
        $(checkAcademicLevel).addClass('enable-item').removeClass('disable-item')
        $(checkAcademicLevel).find('select').prop('disabled', false).trigger('chosen:updated')
        $(checkAdmissionRund).addClass('enable-item').removeClass('disable-item')
        $(checkAdmissionRund).find('select').prop('disabled', false).trigger('chosen:updated')
        $(previewButton).addClass('d-none')
    } else if (type === 'substitudecard') {
        $(studentIdCardCode).prop('disabled', false).parent().removeClass('disable-item')
        $(studentCodeFrom).prop('disabled', true).parent().addClass('disable-item')
        $(studentCodeTo).prop('disabled', true).parent().addClass('disable-item')
        $(studentCodeFrom).val('')
        $(studentCodeTo).val('')
        $(checkAcademicLevel).addClass('disable-item').removeClass('enable-item')
        $(checkAcademicLevel).find('select').prop('disabled', true)
        $(checkAcademicLevel).find('select').val('Select').trigger('chosen:updated');
        $(checkAdmissionRund).addClass('disable-item').removeClass('enable-item')
        $(checkAdmissionRund).find('select').prop('disabled', true)
        $(checkAdmissionRund).find('select').val('Select').trigger('chosen:updated')
    }
});

$(document).on('change', examinationCourse, function() {
    if (this.length > 0) {
        $(previewButton).removeClass('d-none')
    }
});

$(document).on('change', examinationType, function() {
    var examinationType = $(this).val();
    var currentStudentCode = $(studentIdCardCode).val();
    if ($(examinationDate).length > 0) {
        cascadeExaminationDate(currentStudentCode, examinationType);
    }
});

$(document).on('change', examinationDate, function() {
    var examinationDate = $(this).val();
    var currentStudentCode = $(studentIdCardCode).val();
    var currentExaminationType = $(examinationType).val();
    if ($(examinationCourse).length > 0) {
        cascadeExaminationCourse(examinationDate, currentStudentCode, currentExaminationType);
    }
});

$(document).keypress(
    function(event) {
        if (event.key == 'Enter') {
            $('#js-get-details').click();
            event.preventDefault();
        }
});