var statusCode = 'add';
var courses = $('#CourseId');

$('#js-add-exam-period').on('click', function() {
    $('#preloader').fadeIn();
    
    var examinationData = {
        CourseId : courses.val(),
        MidtermDate : $('#MidtermDate').val(),
        MidtermStart : $('#MidtermStart').val(),
        MidtermEnd : $('#MidtermEnd').val(),
        FinalDate : $('#FinalDate').val(),
        FinalStart : $('#FinalStart').val(),
        FinalEnd : $('#FinalEnd').val(),
        TermId : $('#TermId').val(),
        IsEvening : $('#IsEvening').prop('checked'),
        Id : $('#ExaminationPeriodId').val()
    }

    var ajax = new AJAX_Helper({
        url: ExaminationValidateUrl,
        data: {
            model : examinationData,
            status : statusCode 
        },
        dataType: 'json'
    });

    ajax.POST().done(function (response) { 
        if (response == '0') {
            $("#ErrorMessage").html('<h3 class="text-danger">Unable to create, invalid input in some fields.</h3>');
        } else if (response == '1') {
            $("#ErrorMessage").html('<h3 class="text-danger">Course already exists in database.</h3>');
        } else if (response == '2') {
            addExaminationPeriod(examinationData);
        } else {
            editExaminationPeriod(examinationData);
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });

    $('#preloader').fadeOut();
})

function addExaminationPeriod(examinationData) {
    
    var ajax = new AJAX_Helper({
        url: ExaminationCreateUrl,
        data: examinationData,
        dataType: 'html'
    });

    ajax.POST().done(function (response) { 
        $('#ExamPeriods').empty().append(response);

        $('.js-single-date').daterangepicker({
            timePicker: false,
            singleDatePicker: true,
            locale: {
                format: 'DD/MM/YYYY'
            }
        });
        InputMask.renderTimeMask();
        RenderTableStyle.columnAlign();
        clearInput();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
    
    $('#preloader').fadeOut();
}

function editExaminationPeriod(examinationData) {

    var ajax = new AJAX_Helper({
        url: ExaminationEditUrl,
        data: examinationData,
        dataType: 'html'
    });

    ajax.POST().done(function (response) {
        $('#ExamPeriods').empty().append(response);

        $('.js-single-date').daterangepicker({
            timePicker: false,
            singleDatePicker: true,
            locale: {
                format: 'DD/MM/YYYY'
            }
        });
        InputMask.renderTimeMask();
        RenderTableStyle.columnAlign();
        clearInput();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
    
    $('#preloader').fadeOut();
}

function getExaminationPeriod(courseCode, isEvening) {

    var ajax = new AJAX_Helper({
        url: ExaminationGetUrl,
        data: {
            CourseCode : courseCode,
            TermId : $('#TermId').val(),
            IsEvening : isEvening
        },
        dataType: 'html'
    });

    ajax.GET().done(function (response) {
        courses.val(response.courseId);
        courses.attr('disabled', true).trigger('chosen:updated');
        $('#IsEvening').prop('checked', response.isEvening);
        $('#MidtermDate').val(moment(response.midtermDate).format('DD/MM/YYYY'));
        $('#MidtermStart').val(response.midtermStart);
        $('#MidtermEnd').val(response.midtermEnd);
        $('#FinalDate').val(moment(response.finalDate).format('DD/MM/YYYY'));
        $('#FinalStart').val(response.finalStart);
        $('#FinalEnd').val(response.finalEnd);
        $('#ExaminationPeriodId').val(response.id);
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function clearInput() {
    $('#js-toggle-header').html('Add Examination Period');
    courses.append(`<option selected disabled value="">Select</option>`)
    courses.val('');
    courses.attr("disabled", false).trigger("chosen:updated");
    $('#IsEvening').prop("checked", false);
    $('#MidtermDate').val(moment(new Date()).format("DD/MM/YYYY"));
    $('#MidtermStart').val("00:00");
    $('#MidtermEnd').val("00:00");
    $('#FinalDate').val(moment(new Date()).format("DD/MM/YYYY"));
    $('#FinalStart').val("00:00");
    $('#FinalEnd').val("00:00");
    $('#ExaminationPeriodId').val('');
    $("#ErrorMessage").empty();

    $('#js-clear-form').addClass('d-none');
    statusCode = 'add';
}

$(document).on('click', '.js-edit-examination', function() {
    $('#preloader').fadeIn();
    let parentCell = $(this).closest('tr').find('td');
    let courseCode = $(parentCell[1]).html();
    let isEvening = $(parentCell[5]).html().trim() != "";

    $('#js-toggle-header').html('Edit Examination Period');
    $('#js-clear-form').removeClass('d-none');
    statusCode = 'edit';
    getExaminationPeriod(courseCode, isEvening);
    $('#preloader').fadeOut();
});

$(document).on('click', '#js-clear-form', function() {
    clearInput();
});

$(document).ready( function() {
    InputMask.renderTimeMask();
})