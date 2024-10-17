var markAllocationForm = '#js-mark-allocation-form';
var scoreLimit = '.js-display-limit';
var markScore = '.js-mask-score';
var maskTotalScore = '.js-total-score';
var maskTotalScoreValue = '.js-total-score-val';
var curveScoreTable = '.js-curve-table';
var curveTotalScore = '.js-curve-total-score';
var curveMinScore = '.curve-min-score';
var curveMaxScore = '.curve-max-score';
var curveMaxScoreValue = '.curve-max-score-val';
var isScoreChanged = '.js-score-changed';

$(document).ready(function() {
    calculateTotalScore(markScore, maskTotalScore);
    checkTotalScore();
    checkDisableCurveTab();
    checkMarkAllocationSaveButton();

    $(markAllocationForm).on('keyup', markScore, function() {
        calculateTotalScore(markScore, maskTotalScore);
        checkTotalScore();
        checkDisableCurveTab();
        checkMarkAllocationSaveButton();
    })
    
    $(markAllocationForm).on('click', '.js-move-up, .js-move-down', function () {
        RowSwitcher.switchRow(this);
        RowSwitcher.orderRow($(this).parents('tbody'));
    })

    var rowCount = $('#js-mark-allocation-table').find('.js-mark-allocation-details').length;
    if (rowCount > 0) {
        $('#nav-link-1').removeClass('disabled');
    } else {
        $('#nav-link-1').addClass('disabled');
    }
});

function checkTotalScore() {
    let totalScore = parseFloat($(maskTotalScore).html());
    let limit = 100;

    if (totalScore > limit) {
        $(scoreLimit).html('Score exceed limit');
        $(maskTotalScore).removeClass('color-success').addClass('color-warning')
    } else if (totalScore <= limit) {
        let leftScore = parseFloat(limit - totalScore).toFixed(2)
        $(scoreLimit).html(`${ leftScore } left`);
        $(maskTotalScore).removeClass('color-warning').addClass('color-success')
    } else {
        $(scoreLimit).html(`No limit score`);
    }
}

function calculateTotalScore(scoreList, resultArea) {
    let currentScore = 0;

    $(scoreList).each( function() {
        let currentFullScore = $(this).val();

        if (currentFullScore === "") {
            currentFullScore = 0;
        }

        currentScore += parseFloat(currentFullScore);
    })

    $(maskTotalScoreValue).val(currentScore.toFixed(2))
    $(resultArea).html(currentScore.toFixed(2));
}

$(curveScoreTable).on('keyup', curveMinScore, function () {
    var nextMaxScoreValue = $(this).val() - 1;
    var currentRow = $(this).closest('tr');
    var nextRow = currentRow.next('tr');
    var nextRowAll = currentRow.nextAll('tr');
    var nextTwoRowAll = $(this).closest('tr').next('tr').nextAll('tr');

    if (!$(this).val().toString().match(/^[-]?\d*\.?\d*$/)) {
        Alert.renderAlert("Warning", "score invalid!", "warning");
        nextRow.find(curveMinScore).prop('disabled', true);
        nextRow.find(curveMaxScore).html('');
        $(this).val("0");
    } else {
        if ($(this).val()) {
            var currentRowMax = parseInt(currentRow.find(curveMaxScoreValue).val());
            var currentRowMin = parseInt(currentRow.find(curveMinScore).val());
            nextRow.find(curveMaxScore).html(nextMaxScoreValue);
            nextRow.find(curveMaxScoreValue).val(nextMaxScoreValue);
            nextRow.find(curveMinScore).prop('disabled', false);
            if (currentRowMin > currentRowMax) {
                Alert.renderAlert("Warning", "min score must less than max score", "warning");
                nextRow.find(curveMinScore).prop('disabled', true);
                nextRow.find(curveMaxScore).html('');
                $(this).val("0");
            }
        } else {
            nextRow.find(curveMinScore).prop('disabled', true);
            nextRow.find(curveMaxScore).html('');
        }
    }

    nextTwoRowAll.find(curveMinScore).prop('disabled', true);
    nextTwoRowAll.find(curveMaxScore).html('');
    nextRowAll.find(curveMinScore).val('');
});

$(document).on('click', '.js-del-score-row', function() {
    var table = $(this).parents('table');
    var tableId = table.attr('id');
    var eventRow = $(this).closest('.js-mark-allocation-details');
    var rowCount = table.find('.js-mark-allocation-details').length;
    var rowIndex = eventRow[0].rowIndex;
    if (rowCount > 1) {
        RenderTable.deleteRow('#' + tableId, rowIndex);
    } else {
        eventRow.find('input').val('');
    }

    calculateTotalScore(markScore, maskTotalScore);
    checkTotalScore();
    checkDisableCurveTab();
    checkMarkAllocationSaveButton();
});

function checkDisableCurveTab() {
    if ($(maskTotalScoreValue).val() !== $(curveTotalScore).val()) {
        $('#nav-link-1').addClass('disabled');
        $(isScoreChanged).val(true);
    } else {
        $('#nav-link-1').removeClass('disabled');
        $(isScoreChanged).val(false);
    }
}

function checkMarkAllocationSaveButton() {
    var button = '.js-save-mark-allocation';
    if ($(maskTotalScoreValue).val() < 100) {
        $(button).prop('disabled', true);
    } else {
        $(button).prop('disabled', false);
    }
}