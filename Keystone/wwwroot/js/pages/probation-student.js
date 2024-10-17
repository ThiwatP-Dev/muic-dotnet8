var probationTable = '#js-probation-student';
var probationForm = "#js-submit-probation-student";

var studentCheck = ".js-check-student";
var sendEmailCheck = ".js-check-send-email";
var studentGuid = ".js-get-guid";
var studentGPA = ".js-get-gpa";
var probationId = ".js-get-probation-id";

var exportExcel = '.js-export-excel';
var formExportExcel = '#form-export-excel';

function submitForm(target) {
    let selectedProbationTermId = $("#ProbationTermId").val();
    let selectedProbationId = $("#ProbationId").val();
    let selectedRetireId = $("#RetireId").val();
    if(selectedProbationTermId == null) {
        Alert.renderAlert("Error", "Please select probation term", "error");
        return;
    }
    if (selectedProbationId == '' && selectedRetireId == '') {
        Alert.renderAlert("Error", "Please select probation or retire", "error");
        return;
    } else if (selectedProbationId != '' && selectedRetireId != '') {
        Alert.renderAlert("Error", "Please select only probation or retire", "error");
        return;
    } else {
        $('#target').val(target);
        $('#js-submit-probation-student').submit();
    }
}

function createProbationStudentsData(form) {
    let students = [];
    let selectedProbationTermId = $(form).find("#ProbationTermId").val();
    let selectedProbationId = $(form).find("#ProbationId").val();
    let selectedRetireId = $(form).find("#RetireId").val();

    $(form).find(studentCheck).each( function() {
        let currentRow = $(this).parents('tr');
        
        if ($(this).prop('checked') === true) {
            students.push(
                {
                    IsCheck : true,
                    IsSendEmail : currentRow.find(sendEmailCheck).prop('checked'),
                    StudentId : currentRow.find(studentGuid).val(),
                    StudentGPA : currentRow.find(studentGPA).val(),
                    ProbationTermId : selectedProbationTermId,
                    ProbationId : selectedProbationId,
                    RetireId : selectedRetireId
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

$('#js-submit-probation-student').on('submit', function(e) {
    $('#preloader').fadeIn();
    e.preventDefault();
    
    let targetAction = $('#target').val();
    let ajaxSetting;

    if (targetAction === "SendEmail") {
        ajaxSetting = {
            url: StudentProbationSendEmailUrl,
            data: {
                studentIds : sendEmailStudentsData(this)
            },
            dataType: 'json',
        }
    } else {
        ajaxSetting = {
            url: StudentProbationCreateUrl,
            data: {
                model : createProbationStudentsData(this),
            },
            dataType: 'json',
        }
    }

    var ajax = new AJAX_Helper(
        ajaxSetting
    );

    ajax.POST().done(function (response) {
        window.location.href = response.redirectToUrl;
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });

    $('#preloader').fadeOut();
})

$(document).ready( function() {
    CheckList.renderCheckbox('#js-probation-student');
    $(".js-render-nicescroll").niceScroll();
    var table = $('#js-probation-student').DataTable({
        paging: false,
        scrollY: false,
        scrollX: true,
        // dom: 'Bfrtip',
        "searching": false,
        // buttons: {
        //     buttons: [{
        //         extend: 'excel',
        //         text: 'Excel',
        //         title: $('h1').text(),
        //         exportOptions: {
        //             columns: ':not(.no-print)'
        //         },
        //         footer: true
        //     }],
        //     dom: {
        //         container: {
        //             className: 'dt-buttons'
        //         },
        //         button: {
        //             className: 'btn btn--primary'
        //         }
        //     }
        // },
        // language: {
        //     emptyTable: '<i class="text-danger">No Data</i>'
        // },
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [0]
        }],
        "order": [[ 1, 'asc' ]]
    });

    $(formExportExcel).on('submit', function() {
        $('#preloader').fadeIn();
        $('.loader').fadeOut();
        $("#preloader").delay(1000).fadeOut("slow");
    })
});