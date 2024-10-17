let studentId = '#StudentId';
let type = '#Type';
let approvedAt = '#ApprovedAt';
let approvedBy = '#ApprovedBy';
let remark = '#Remark';
let termId = '#TermId';
let academicLevelId = '#AcademicLevelId';
let latePaymentId = '#Id';
let createdAt = '#CreatedAt';
let createdBy = '#CreatedBy';

var statusCode = 'add';

function CheckLatePaymentInput() {
    var ajax = new AJAX_Helper(
        {
            url: LatePaymentValidateUrl,
            data: {
                StudentId : $(studentId).val(),
                Type : $(type).val(),
                ApprovedAt : ConvertDateFormat($(approvedAt).val()),
                ApprovedBy : $(approvedBy).val(),
                Remark : $(remark).val(),
                TermId : $(termId).val(),
                AcademicLevelId : $(academicLevelId).val(),
                Id : $(latePaymentId).val(),
                CreatedAt : ConvertDateFormat($(createdAt).val()),
                CreatedBy : $(createdBy).val(),
                Status : statusCode
            },
            dataType: 'json'
        }
    );

    ajax.GET().done(function (response) { 
        if (response == '0') {
            FlashMessage.header("Danger", ErrorMessage.InvalidInput);
        } else if (response == '1') {
            FlashMessage.header("Danger", `Student ${ ErrorMessage.DataDuplicate }`);
        } else if (response == '2') {
            $('#flash-message').empty();
            AddLatePayment('add');
        } else {
            $('#flash-message').empty();
            AddLatePayment('edit');
        }
        
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });

    $('#preloader').fadeOut();
}

function AddLatePayment(type) {
    if (type == 'add') {
        urlController = LatePaymentCreateUrl
    } else {
        urlController = LatePaymentEditUrl
    }

    var ajax = new AJAX_Helper(
        {
            url: urlController,
            data: {
                StudentId : $(studentId).val(),
                Type : $(type).val(),
                ApprovedAt : ConvertDateFormat($(approvedAt).val()),
                ApprovedBy : $(approvedBy).val(),
                Remark : $(remark).val(),
                TermId : $(termId).val(),
                AcademicLevelId : $(academicLevelId).val(),
                Id : $(latePaymentId).val(),
                CreatedAt : ConvertDateFormat($(createdAt).val()),
                CreatedBy : $(createdBy).val()
            },
            dataType: 'html'
        }
    );

    ajax.GET().done(function (response) { 
        $('#LatePaymentDetails').empty().append(response);
        RenderTableStyle.columnAlign();
        FlashMessage.header("Confirmation", ErrorMessage.SaveSuccess);
        clearInput();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });

    $('#preloader').fadeOut();
}

function FindLatePayment(studentCode) {

    var ajax = new AJAX_Helper(
        {
            url: LatePaymentFindUrl,
            data: {
                StudentCode : studentCode,
                TermId : $(termId).val()
            },
            dataType: 'html'
        }
    );

    ajax.GET().done(function (response) {
        $(studentId).val(response.studentId)
                       .trigger("chosen:updated");
        $(type).val(response.type)
                  .trigger("chosen:updated");
        $(approvedAt).val(moment(response.approvedAt, "YYYY-MM-DDThh:mm:ss").format("DD/MM/YYYY"));
        $(approvedBy).val(response.approvedBy)
                        .trigger("chosen:updated");
        $(remark).val(response.remark);
        $(latePaymentId).val(response.id);
        $(createdBy).val(response.createdBy);
        $(createdAt).val(moment(response.createdAt, "YYYY-MM-DDThh:mm:ss").format("DD/MM/YYYY"));
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });

    $('#preloader').fadeOut();
}

function clearInput() {
    $(studentId).val(0)
                   .attr('student-id', 0)
                   .prop('disabled', false)
                   .trigger("chosen:updated");
    $(type).val(0)
              .trigger("chosen:updated");;
    $(approvedBy).val(0)
                    .trigger("chosen:updated");
    $(approvedAt).val('');
    $(remark).val('');
    $(latePaymentId).val(0);

    $('#js-toggle-header').html('Add Late Payment Student');
}

function ConvertDateFormat(dateStr) {
    var parts = dateStr.split('/');
    if (parts.length == 3) {
        var date = parts[1] + '/' + parts[0] + '/' + parts[2];
        return date;
    }
    return dateStr;
}

$(document).on('click', '.js-suggestion-item', function() {
    let relateInput = $($(this).parents()[1]).find('input');
    let studentId = $(this).data('student-id');

    relateInput.attr('student-id', studentId);    
});

$(document).on('keyup','.js-student-suggest', function(e) {
    if (e.keyCode === 13) {
        //enter
        let bestMatchItem = $(this).parent().find('.js-suggestion-item.active');
        let studentId = $(bestMatchItem).data('student-id');

        $(this).attr('student-id', studentId);
    }
    
});

$(document).on('click', '#js-create-late-payment', function() {
    $('#preloader').fadeIn();
    CheckLatePaymentInput();
});

$(document).on('click', '.js-edit-late-payment', function() {
    let parentCell = $(this).closest('tr').find('td');
    let studentCode = $(parentCell[1]).html();
    statusCode = 'edit';

    $(studentId).prop('disabled', true)
    $('#js-toggle-header').html('Edit Late Payment Student');

    $('#preloader').fadeIn();
    FindLatePayment(studentCode);
});