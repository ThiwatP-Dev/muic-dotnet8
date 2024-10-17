function saveEditGrade(studentScoreId) {
    var ajax = new AJAX_Helper(
        {
            url: GradeScoreSummaryUpdateGrade,
            data: {
                studentRawScoreId: studentScoreId,
                gradeId: $('#select-grade').val(),
                remark: $('#remark').val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $('#result-edit-grade-modal').modal('hide');
        if(response.IsInvalid)
        {
            Alert.renderAlert("Warning", "Unable to save ,Data invalid!", "warning");
        } else {
            $("#student-grade-" + studentScoreId).html(response.grade);
            // $("#student-grade-" + studentScoreId).addClass("text-danger");
            $("#student-grade-" + studentScoreId).parents("tr").addClass("text-danger bg-light");
            SuccessCallback("Save Success");
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}


function getGradeInfo(studentScoreId, courseId, termId) {
    var ajax = new AJAX_Helper(
        {
            url: GradeScoreSummaryGetGradeInfo,
            data: {
                studentRawScoreId: studentScoreId,
                courseId: courseId,
                termId: termId
            },
            dataType: 'html'
        }
    );

    ajax.GET().done(function (response) {
        $('#select-grade').empty()
        response.grades.forEach(element => {
            $('#select-grade').append(new Option(element.name, element.id));
        });

        $('#current-grade').val(response.currentGrade);
        $('#grade-score-student').val(response.student);
        $('#grade-score-course').val(response.course);
        $('#select-grade option[value="'+ response.currentGradeId +'"]').attr("selected", true);
        if(!response.isGradeMember)
        {
            $('#select-grade').prop("disabled", true);
            $('#check-grade-member').addClass('d-none');
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getGradelog(studentScoreId) {
    var ajax = new AJAX_Helper(
        {
            url: GradeScoreSummaryGetGradelogs,
            data: {
                studentRawScoreId: studentScoreId
            },
            dataType: 'html'
        }
    );

    ajax.GET().done(function (response) {
        $('#grade-log-content').empty().append(response);
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).ready(function() {
    var table = $('#grade-summary-table').DataTable({
        paging: false,
        // fixedHeader : {
        //     header : true,
        //     footer : false,
        //     headerOffset: 0
        // },
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": [0, 1, 6, -1]
        } ],
        "order": [[ 2, 'asc' ]]
    });
    table.on( 'order.dt search.dt', function () {
        table.column(0, {search:'applied', order:'applied'}).nodes().each( function (cell, i) {
            cell.innerHTML = i+1;
        } );
    } ).draw();
    $('#result-edit-grade-modal').on('shown.bs.modal', function(event) {
        let button = event.relatedTarget;
        let studentScoreId = $(button).data('studentscoreid');
        let courseId = $("#grade-score-course-id").val();
        let termId = $("#grade-score-term-id").val();
        let grade = $(button).data('grade');
        $('#current-grade').val(grade);
        $('#student-score-id').val(studentScoreId);
        getGradeInfo(studentScoreId, courseId, termId);
    });
    
    $('#grade-log-modal').on('shown.bs.modal', function(event) {
        let button = event.relatedTarget;
        let studentScoreId = $(button).data('studentscoreid');
        getGradelog(studentScoreId);
    });

    $('#js-save-edit-grade').click(function() {
        saveEditGrade($('#student-score-id').val());
    });
})
