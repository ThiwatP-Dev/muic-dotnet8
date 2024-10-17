var standardGradingGroup = '#js-standard-grading-score';
var gradeScores = '.js-grade-score';
var gradeMaxScores = 'input[data-score="max"]';
var gradeMinScores = 'input[data-score="min"]';
var minimumScore = '.js-minimum-score';
var maximumScore = '.js-maximum-score';
var gradeTemplate = '.js-select-grade-template';
var standardGradingGroupBody = '.js-standard-grading-group-body'

$(document).ready( function() {
    $(minimumScore).on('keyup', function() {
        var score = convertStringToInt(this);
        var firstRow = $(standardGradingGroup).children('tbody').children('tr:first');
        var minScore= firstRow.find(gradeMinScores);
        minScore.val(score);
    });
    
    $(maximumScore).on('keyup', function() {
        var score = convertStringToInt(this);
        var lastRow = $(standardGradingGroup).children('tbody').children('tr:last');
        var maxScore= lastRow.find(gradeMaxScores);
        maxScore .val(score);
    });
});

function convertStringToInt(number) {
    var convertedNumber = parseInt($(number).val());
    if (isNaN(convertedNumber)) {
        $(number).val(0);
        return 0;
    } else {
        return convertedNumber;
    }
}

$(standardGradingGroup).on('keyup', gradeScores, function() {
    gradeScoresKeyUp($(this));
    
    var isFirstRow = $(this).closest("tr").is(":first-child");
    if (isFirstRow) {
        var firstRow = $(standardGradingGroup).children('tbody').children('tr:first');
        var minScore= firstRow.find(gradeMinScores);
        minScore.val($(minimumScore).val());
    }

    var isLastRow = $(this).closest("tr").is(":last-child");
    if (isLastRow) {
        var lastRow = $(standardGradingGroup).children('tbody').children('tr:last');
        var maxScore= lastRow.find(gradeMaxScores);
        maxScore.val($(maximumScore).val());
    }
})

function gradeScoresKeyUp(obj) {
    let score = obj.val();
    let scoreType = obj.data('score');
    let currentRow = obj.parents('tr');

    gradePointSuggestion(score, scoreType, currentRow);
}

function gradePointSuggestion(score, scoreType, currentRow) {
    let nextRow = currentRow.next('tr');
    let prevRow = currentRow.prev('tr');

    if (score.match(RegularExpressions.IsIntOrFloat) != null) {
        if (nextRow.length != 0 && scoreType === 'max' && score != 0) {
            nextRow.find(gradeMinScores).val((parseInt(score) + 1).toFixed(2))
        } else {
            nextRow.find(gradeMinScores).val((parseInt(score)).toFixed(2))
        }
        
        if (prevRow.length != 0 && scoreType === 'min') {
            prevRow.find(gradeMaxScores).val((parseInt(score) + 1).toFixed(2))
        }
    } else if (score === '') {
        if (nextRow.length != 0 && scoreType === 'max') {
            nextRow.find(gradeMinScores).val('')
        }
        
        if (prevRow.length != 0 && scoreType === 'min') {
            prevRow.find(gradeMaxScores).val('')
        }
    }
}

$(gradeTemplate).on('change', function() {
    var ajax = new AJAX_Helper(
        {
            url: StandardGradingGroupGradeTemplate,
            data: {
                id: $(this).val(),
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(standardGradingGroupBody).empty().append(response);

        var convertedMinimumScore = convertStringToInt(minimumScore);
        var firstRow = $(standardGradingGroup).children('tbody').children('tr:first');
        var minScore= firstRow.find(gradeMinScores);
        minScore.val(convertedMinimumScore);

        var convertedMaxmimumScore = convertStringToInt(maximumScore);
        var lastRow = $(standardGradingGroup).children('tbody').children('tr:last');
        var maxScore= lastRow.find(gradeMaxScores);
        maxScore.val(convertedMaxmimumScore).prop('disabled');
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})