var feeTableId = '#js-term-fee-list'
var totalAmountClass = '.js-total-amount';
var amountListClass = '.js-amount-value';
var totalAmount = 0;

$(feeTableId).on('keyup', amountListClass, function() {
    totalAmount = 0;

    $(amountListClass).each( function() {
        let value = parseInt($(this).val());
        totalAmount += value;
    })
    
    $(totalAmountClass).html(NumberFormat.renderDecimalTwoDigits(totalAmount));
})

$(document).ready(function() {
    var inputTable = new RowAddAble({
        TargetTable: feeTableId,
        TableTitle: 'Term Fee List',
        ButtonTitle: 'Term Fee'
    })
    inputTable.RenderButton();
    $(totalAmountClass).html(NumberFormat.renderDecimalTwoDigits(totalAmount));
})