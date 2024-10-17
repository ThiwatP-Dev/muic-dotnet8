var gradesTable =".js-student-score"
var tableScoring = "#js-scoring-table";
var allocationCount = parseInt($('#js-allocation-count').val());
//var listColumnInput = range(8, 8 + allocationCount); dinamic mark allocation ex = [10, 11, 12, 13, 14, 15, 16, 17, 18]
$(document).on('keyup focusout', gradesTable, function () {
    var row = $(this).closest('tr');
    HandleChangeScoring(row);
})

$(document).on('click', '.form-check-input', function(){
    ToggleSkipScore($(this).data("id"));
})

$(document).on('change', '.chosen-select', function(){
    let index = $(this).data("index");
    let className = '.js-student-score-' + index;
    if($(this).val() != 0)
    {

        $(className).each( function() {
            $(this).prop("disabled", true);
        })
    } else {
        $(className).each( function() {
            $(this).prop("disabled", false);
        })
    }
})

$(document).ready(function() {
    var table = $('#js-scoring-table').DataTable({
        paging: false,
        // fixedHeader : {
        //     header : true,
        //     footer : false,
        //     headerOffset: 0
        // },
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [0, 1, 2, 11]
        },
        {
            "targets": [12],
            "orderDataType": "dom-text"
        }],
        "order": [[ 3, 'asc' ]]
    });

    $('#sections-scoring-modal').on('shown.bs.modal', function() {
        setSectionSearchButton();
        $(document).on('click', '.js-check-all, .js-selected-section', function(){
            setSectionSearchButton();
        });
    });

    table.on( 'order.dt search.dt', function () {
        table.column(0, {search:'applied', order:'applied'}).nodes().each( function (cell, i) {
            cell.innerHTML = i+1;
        } );
    } ).draw();


    CheckList.renderCheckbox("#js-section-scoring-table");
    $(".js-render-nicescroll").niceScroll();
    HandleSelectGradeAll();
})

function setSectionSearchButton() {
    var saveButton = $('.js-section-search-button');
    var atLeastOneIsChecked = $('#js-section-scoring-table').find('.form-check-input:checked').length > 0;
    if (atLeastOneIsChecked) {
        saveButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
    }
}

$(document).on('click', '#js-section-select-scoring', function() {
    var sectionIds = [];
    var courseId = $('#score-term-id').val();
    var termId = $('#score-course-id').val();
    $('.js-selected-section').each(function() {

        if ($(this).prop('checked') === true) {
            let currentId = $(this).data("section-id");
            sectionIds.push(currentId);
        }
    })

    getStudentsBySections(sectionIds, courseId, termId);
    $('#sections-scoring-modal').modal('hide');
})

function getStudentsBySections(sectionIds, courseId, termId)
{
    $('#preloader').fadeIn();
    var ajax = new AJAX_Helper(
        {
            url: StudentsBySections,
            data: {
                sectionIds: sectionIds,
                courseId: courseId,
                termId: termId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $('#js-get-students-score').empty().html(response);
        HandleSelectGradeAll();

        $(tableScoring).on('keyup focusout', gradesTable, function () {
            var row = $(this).closest('tr');
            HandleChangeScoring(row);
        })

        $(tableScoring).on('click', '.form-check-input', function(){
            ToggleSkipScore($(this).data("id"));
        })
        $(tableScoring).on('change', '.chosen-select', function(){
            let index = $(this).data("index");
            let className = '.js-student-score-' + index;
            if($(this).val() != 0)
            {
        
                $(className).each( function() {
                    $(this).prop("disabled", true);
                })
            } else {
                $(className).each( function() {
                    $(this).prop("disabled", false);
                })
            }
        })
        var table = $('#js-scoring-table').DataTable({
            paging: false,
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [0, 1, 2, 11]
        },
        {
            "targets": [12],
            "orderDataType": "dom-text"
        }],
        "order": [[ 3, 'asc' ]]
        });
        $('.chosen-select').chosen();
        // $('.chosen-select').trigger("chosen:updated");
        table.on( 'order.dt search.dt', function () {
        table.column(0, {search:'applied', order:'applied'}).nodes().each( function (cell, i) {
                cell.innerHTML = i+1;
            } );
        } ).draw();


        $('#preloader').fadeOut();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
        $('#preloader').fadeOut();
    });
}

function ToggleSkipScore(index)
{
    let className = '.js-student-score-' + index;
    let id = '#checkvalue'+index;
    $(className).each( function() {
        $(this).prop("disabled", $(id).is(':checked') ? false : true);
        $('#checkvalue'+index).val($(id).is(':checked') ? "false" : "true");
    })
}

function HandleChangeScoring(row)
{
    let markAllocation = row.find(gradesTable);
    let totalScore = 0.0;
    markAllocation.each(function() {
        let rawScore = $(this).val();
        let score = parseFloat($(this).val());
        let maxScore = $(this).data("max");
        if(rawScore != "")
        {   
            if(!rawScore.toString().match(/^[-]?\d*\.?\d*$/))
            {
                Alert.renderAlert("Warning", "score invalid!", "warning");
                $(this).val("");
            } else {
                if(rawScore < 0)
                {
                    Alert.renderAlert("Warning", "score less than zero!", "warning");
                    $(this).val("0.00");
                    totalScore += 0.00;
                } else if(score > parseFloat(maxScore)) {
                    Alert.renderAlert("Warning", "score more than max score!", "warning");
                    $(this).val("0.00");
                    totalScore += 0.00;
                } else {
                    totalScore += (isNaN(score) ? 0.00 : score);
                } 
            }
        }
    });
    let totalValue = row.find(".js-total-score-value");
    if(totalScore > 0)
    {
        totalValue.val(totalScore.toFixed(2).toString());
    }
    else
    {
        totalValue.val("");
    }

}

$(document).on('click', '#js-export-score', function () {

    var oldUrl = $('#grading-score').attr('action');
    $('#grading-score').attr('action', GradeScoreExportUrl);
    $('#grading-score').submit();
    $('#grading-score').attr('action', oldUrl);
    $("#preloader").delay(100).fadeOut();
})

$(document).on('click', '#browse-btn' , function() {
    $('#upload-input').click();
});

$(document).on('change', '#upload-input', function(e) {
    $("#file-name").val(e.currentTarget.files[0].name);
    $("#grading-score").attr('action',"/ScoreByInstructor/Import");
    $("#grading-score").submit();
});

$(".btn-save").on('click', function () {
    $("#score-is-next").prop('checked', false);
    $("#grading-score").submit();
});

$(".btn-next").on('click', function () {
    $("#score-is-next").prop('checked', true);
    $("#grading-score").submit();
});
function range(start, end) {
    return Array(end - start + 1).fill().map((_, idx) => start + idx)
  }

function HandleSelectGradeAll()
{
    var listGradeSelect =  $(tableScoring).find(".form-control__table");
    if(listGradeSelect.length > 0)
    {
        listGradeSelect.each(function() {
            if($(this).val() != 0)
            {
                let index = $(this).data("index");
                let className = '.js-student-score-' + index;
                $(className).each( function() {
                    $(this).prop("disabled", true);
                })
            }
        });
    }
}