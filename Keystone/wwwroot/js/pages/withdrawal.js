// by Course
var courseSelect = '.js-cascade-course';
var sectionSelect = '.js-cascade-section';
var byCourseContent = '#js-withdrawal-course-content';

let witdrawalStudentTable = '#js-withdrawal-students';
let addWithdrawalTable = '#js-selected-students';
let checkStudent = '.js-add-student';

var temporaryDefaultRow;
var studentIdInputs = '.js-student-id';

// by Student
var studentInput = '#js-search-student-id'; // currently not in use
var byStudentContent = '#js-withdrawal-student-content';

// Withdrawal content filter
var typeFilter = '.js-render-type';
var approvedFilter = '.js-render-approved-by';

var returnUrl = '#return-url';
var registrationTerm = '.js-cascade-term';

let currentTab = getUrlParameter('tabIndex');

function addWithdrawalStudent(studentRow) {
    $(addWithdrawalTable).find('#js-default-row').remove();
    $(addWithdrawalTable).find('.js-empty-row').remove();
    let rowCount = $(addWithdrawalTable).find('tr').length;

    let studentId = $(studentRow).find(checkStudent).data('student-id');
    let paidStatus = $(studentRow).find(checkStudent).data('paid-status');
    let registrationCourseId = $(studentRow).find(checkStudent).data('registrationcourse-id');

    $(studentRow).find('td:first').empty().html(rowCount);
    $(studentRow).find('td:last').remove();
    $(studentRow).find('td:nth-child(2)').append(
        `<input class="js-student-id" name="StudentWithdrawals[${ rowCount - 1 }].StudentId" value="${ studentId }" hidden>
         <input name="StudentWithdrawals[${ rowCount - 1 }].RegistrationCourseId" value="${ registrationCourseId }" hidden>
         <input name="StudentWithdrawals[${ rowCount - 1 }].IsPaid" value="${ paidStatus }" hidden>
         <input name="StudentWithdrawals[${ rowCount - 1 }].Status" value="d" hidden>`
    );

    $(addWithdrawalTable).children('tbody').append(studentRow);
    RenderTableStyle.columnAlign();
}

function removeWithdrawalStudent(studentId) {
    let addWithdrawalRow = $(addWithdrawalTable).find('tbody tr');
    var currentIndex = 1;

    addWithdrawalRow.each( function() {
        let rowStudentId = $(this).find(studentIdInputs).val();

        if (rowStudentId === studentId) {
            currentIndex = $(this)[0].rowIndex;
            $(this).remove();
        }
    })

    let rowCount = $(addWithdrawalTable).find('tr').length;
    if (rowCount <= 1) {
        $(addWithdrawalTable).append(temporaryDefaultRow);
    } else {
        RenderTable.orderRow(addWithdrawalTable, currentIndex);
    }
}

// by Course Event
$(byCourseContent).on('change', '.js-check-all', function() {
    let allStudent = $(witdrawalStudentTable).find('tbody tr');
    $(addWithdrawalTable).find('tbody').empty();
    
    if ($(this).prop('checked')) {
        allStudent.each( function() {
            let currentStudentRow = $(this).clone(false);
            addWithdrawalStudent(currentStudentRow);
        })
    } else {
        $(addWithdrawalTable).append(temporaryDefaultRow);
    }
})

$(byCourseContent).on('change', checkStudent, function() {
    let currentStudent = $(this).parents('tr').clone(false);
    
    if ($(this).prop('checked')) {
        addWithdrawalStudent(currentStudent);
    } else {
        let studentId = $(currentStudent).find(checkStudent).data('student-id');
        removeWithdrawalStudent(studentId);
    }
})

$(byCourseContent).on('keyup', '#js-search', function() {
    let keywords = $(this).val();
    InputSuggestion.elementSuggest(keywords, '.js-suggestion-parent');
})

$(document).on('change', typeFilter, function() {   
    let chosenApprovedBy = $(this).closest('div').next().find(approvedFilter);
    var ajaxData = {
        type: $(this).val(),
        sectionId:  $(sectionSelect).val(),
        studentCode: "",
        termId: $(registrationTerm).val(),
        page: currentTab
    };

    if (currentTab === "1")
    {
        ajaxData.studentCode = $('#student-code').val();
    }

    var ajax = new AJAX_Helper(
        {
            url: WithdrawalGetApprovedbyUrl,
            data: ajaxData,
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        $(chosenApprovedBy).append(getDefaultSelectOption(chosenApprovedBy));

        response.forEach((item, index) => {
            $(chosenApprovedBy).append(getSelectOptions(item.id, item.codeAndName));
        });

        $(chosenApprovedBy).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

$('.block__action-row').on('click', '.js-cancel-confirm-btn', function() {
    $('#cancel-confirm-form').submit();
});

$(document).ready(function() {

    let maxTableHeight = $(witdrawalStudentTable).find('tr').length;
    for (let c = 0; c < maxTableHeight; c++) {
        $(addWithdrawalTable).find('tbody').append(`<tr class="h-50 js-empty-row"></tr>`)
    }
    $(".js-render-nicescroll").niceScroll();

    if (currentTab === "0") {
        CheckList.renderCheckbox(witdrawalStudentTable);
        // for using in empty add withdrawal row
        temporaryDefaultRow = $(addWithdrawalTable).find('#js-default-row').clone(false);
    } else if (currentTab === "1") {
        CheckList.renderCheckAllBtn('.regis-course','.js-check-all');
    }

    var tab = '#nav-link-' + currentTab;
    $(tab).tab('show');

    $('#cancel-confirm-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget); // Button that triggered the modal
        
        let controller = button.data('controller');
        let action = button.data('action');
        let id = button.data('id');
        let returnUrl = button.data('return-url');
        let encodedUrl = encodeURIComponent(returnUrl);
    
        let fullRoute = `/${ controller }/${ action }?id=${ id }&returnUrl=${ encodedUrl }`;
    
        let cancelConfirmBtn = $('.js-cancel-confirm-btn');
        cancelConfirmBtn.attr("href", `${ fullRoute }`)
    });

    $('#approve-confirm-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget); // Button that triggered the modal

        let controller = button.data('controller');
        let action = button.data('action');
        let id = button.data('id');
        let returnUrl = button.data('return-url');
        let encodedUrl = encodeURIComponent(returnUrl);

        let fullRoute = `/${controller}/${action}?id=${id}&returnUrl=${encodedUrl}`;

        let approveConfirmBtn = $('.js-approve-confirm-btn');
        approveConfirmBtn.attr("href", `${fullRoute}`)
    });

    $('#reject-confirm-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget); // Button that triggered the modal

        let controller = button.data('controller');
        let action = button.data('action');
        let id = button.data('id');
        let returnUrl = button.data('return-url');
        let encodedUrl = encodeURIComponent(returnUrl);

        let fullRoute = `/${controller}/${action}?id=${id}&returnUrl=${encodedUrl}`;

        let rejectConfirmBtn = $('.js-reject-confirm-btn');
        rejectConfirmBtn.attr("href", `${fullRoute}`)
    });

    $('#js-withdrawal-logs').on('shown.bs.modal', function(e) {
        let withdrawalId = $(e.relatedTarget).data('value');

        var ajax = new AJAX_Helper(
            {
                url: WithdrawalLogsUrl,
                data: {
                    id: withdrawalId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#js-withdrawal-log-details').empty().append(response);
            RenderTableStyle.columnAlign();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
});