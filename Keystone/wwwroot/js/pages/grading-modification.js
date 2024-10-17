let buttonSave = '#edit-grade';
let updatedGradeId = '.js-grade-id';
let remarkInput = '#Remark';
let tableId = '.js-grade-editor-table';

$(document).on('change keydown paste input', '#edit-gradinglog', function(event){
    var grade = $(updatedGradeId).find('option:selected').val();
    var remark = $(remarkInput).val().trim();
    var isSaveDisabled = remark.length == 0;
    $(buttonSave).prop('disabled', isSaveDisabled);
});

$(document).on('click', '.js-confirm-btn', function() {
    $('#change-publication-confirm-form').submit();
});

$(document).on('click', '.js-start-edit', function() {
    var parentRow = $(this).parents('tr');
    parentRow.addClass('bg-secondary-lighter');
});

$(document).on('click', '.js-cancel-edit', function() {
    var parentRow = $(this).parents('tr');
    parentRow.removeClass('bg-secondary-lighter');
});

$(document).on('change keydown paste input', '.js-edit-input', function(event){
    var remark = $(this).val().trim();
    var isSaveDisabled = (remark.length == 0);
    var saveLogButton = $(this).parents('tr').find('.js-save-edit');

    if (isSaveDisabled) {
        saveLogButton.addClass('js-alert-invalid');
        saveLogButton.css('cursor', 'not-allowed');
    } else {
        saveLogButton.removeClass('js-alert-invalid');
        saveLogButton.css('cursor', 'pointer');
    }
});

$(document).on('click', '.js-save-edit', function() {
    var parentRow = $(this).parents('tr');
    var inputElement = parentRow.find('.js-edit-input');
    var displayElement = parentRow.find('.js-edit-value');

    var originalData = inputElement.data('original');
    var newData = inputElement.val().trim();
    var gradingLogId = inputElement.data('id');
    
    if ($(this).hasClass('js-alert-invalid')) {
        Alert.renderAlert('Error', ErrorMessage.EmptyOrSpaceData, 'error');
        displayElement.html(originalData);
        parentRow.removeClass('bg-secondary-lighter');
    } else {
        $('#preloader').fadeIn();

        var ajax = new AJAX_Helper(
            {
                url: GradeMaintenanceEditGradingLogRemarkUrl,
                data: {
                    id: gradingLogId,
                    remark: newData
                },
                dataType: 'json'
            }
        );

        ajax.POST().done(function (response) {
            parentRow.find('.js-quit-edit').addClass('d-none')
            parentRow.find('.js-start-edit').removeClass('d-none')
            displayElement.text(response.remark);
            parentRow.find('#EditedAt').html(response.updatedAtText);
            parentRow.find('#EditedBy').html(response.updatedBy);
            parentRow.find('#Type').html(response.typeText);
            
            SuccessCallback();
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });

        parentRow.removeClass('bg-secondary-lighter');
        $('#preloader').fadeOut();
    }
})

$(document).ready(function() {
    $(".js-render-nicescroll").niceScroll();

    $("#js-grade-editor-modal").on('show.bs.modal', function(event) {
        let button = event.relatedTarget;
        let registrationCourseId = $(button).data('registrationcourseid');

        var ajax = new AJAX_Helper (
            {
                url: GetGradeLogs,
                data: {
                    registrationCourseId : registrationCourseId
                },
                dataType: 'html',
            }
        );

        ajax.POST().done(function (response) {
            $('#js-grade-editor-modal .modal-content').html(response);
            EditTable.renderEditTable(tableId)
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#js-change-publication-confirm-modal').on('show.bs.modal', function (event) {
        let button = event.relatedTarget; 
        let registrationCourseId = $(button).data('registrationcourseid');
        let isGradePublished = $(button).data('gradepublishedstatus');
        let termId = $('#Criteria_TermId').val();
        let studentCode = $('#Criteria_Code').val();
        let modalHeader;

        if (isGradePublished == "True") {
            modalHeader = "Unpublish Grade";
        } else {
            modalHeader = "Publish Grade";
        }

        $('#RegistrationCourseId').val(registrationCourseId);
        $('#IsGradePublished').val(isGradePublished);
        $('#TermId').val(termId);
        $('#StudentCode').val(studentCode);
        $('.js-modal-header').html(modalHeader);
    });
});