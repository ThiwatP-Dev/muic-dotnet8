$(document).ready(function() {
    var inputTable = new RowAddAble({
        TargetTable: '#js-course-group',
        ButtonTitle: 'Course'
    })
    inputTable.RenderButton();
});

$(document).on('click', '.js-add-row', function() {
    var defaultGradeId = $('#RequiredGradeId').val();
    var defaultIsRequired = $('#IsRequired').val() == "r" ? "true" : "false";
    var lastRow = $('#js-course-group').find('tr:last');
    var requiredGrade = lastRow.find('.js-required-grade');
    var isRequired = lastRow.find('.js-is-required');
    
    requiredGrade.val(defaultGradeId).trigger("chosen:updated");
    isRequired.val(defaultIsRequired).trigger("chosen:updated");
});