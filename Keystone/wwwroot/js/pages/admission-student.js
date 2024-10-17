var activeModelForm = "";
var generalSection = '.ks-general-info';
var admissionSection = '.ks-admission-info';
var academicLevel = '.js-cascade-academic-level';
var admissionRound = '.js-cascade-admission-round';
var newStudentCode = '#js-student-code';
var errorMessage = '#js-error-message';
var faculty = '.js-cascade-faculty';
var department = '.js-cascade-department';
var curriculum = '.js-curriculum-id';
var curriculumVersion = '.js-cascade-curriculum-version';
var nationality = '.js-get-nationality';
var batch = '.js-batch';
var studentGroup = '.js-student-group';
var studentStatus = '.js-student-status';
var studentFeeGroup = '.js-student-fee-group';
var studentFeeType = '.js-student-fee-type';

function isCodeAvailable (studentCode) {
    var ajax = new AJAX_Helper(
        {
            url: AdmissionStudentCheckExistStudentCodeUrl,
            data: {
               code: studentCode
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        console.log(response)
        if (response) {
            $(errorMessage).removeClass('d-none');
        } else {
            $(errorMessage).addClass('d-none');
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(generalSection).on('change', admissionRound, function () {
    let currentAcademicLevel = $(generalSection).find(academicLevel).val();
    let currentAdmissionRound = $(this).val();

    var ajax = new AJAX_Helper(
        {
            url: AdmissionStudentGenerateStudentCodeUrl,
            data: {
                academicLevelId: currentAcademicLevel,
                admissionRoundId: currentAdmissionRound
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(newStudentCode).val(response);

        if (!$(errorMessage).hasClass('d-none')) {
            $(errorMessage).addClass('d-none');
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

$(studentGroup).on('change', function () {
    let currentAcademicLevel = $(admissionSection).find(academicLevel).val();

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetStudentFeeGroups,
            data: {
                academicLevelId: currentAcademicLevel,
                facultyId: $(admissionSection).find(faculty).val(),
                departmentId: $(admissionSection).find(department).val(),
                curriculumId: $(admissionSection).find(curriculum).val(),
                curriculumVersionId: $(admissionSection).find(curriculumVersion).val(),
                nationalityId: $(generalSection).find(nationality).val(),
                batch: $(batch).val(),
                studentGroupId: $(studentGroup).val(),
                studentFeeTypeId: $(generalSection).find(studentFeeType).val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(studentFeeGroup).append(getDefaultSelectOption($(studentFeeGroup)));

        response.forEach((item) => {
            $(studentFeeGroup).append(getSelectOptions(item.value, item.text));
        });

        $(studentFeeGroup).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

$(newStudentCode).on('keyup', function() {
    let currentCodeInput = $(this).val();

    if (currentCodeInput.length >= 7) {
        isCodeAvailable(currentCodeInput);
    }
});

$(function() {
    var tab = '#nav-link-' + getUrlParameter('tabIndex');
    $(tab).tab('show');

    var inputTable = new RowAddAble({
        TargetTable: '#js-exam-table',
        TableTitle: 'Examination',
        ButtonTitle: 'Examination'
    })
    inputTable.RenderButton();

    $('#confirm-status-modal').on('shown.bs.modal', function(event) {
        let button = $(event.relatedTarget);
        let controller = button.data('controller');
        let action = button.data('action');

        let studentCode = $('#Code').val();
        $('.js-current-code').html(studentCode);

        let studentId = $('#Id').val();
        let fullRoute = `/${ controller }/${ action }?studentId=${ studentId }`;
    
        let confirmButton = $('.js-confirm-changed');
        confirmButton.attr("href", `${ fullRoute }`)
    })

    $('.js-modal-form').on('show.bs.modal', function (event) {
        activeModelForm = `#${$(this)[0].id}`
    });

});