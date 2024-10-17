var refundTable = '#js-courses-list';
var refundAmount = '.js-refund-amount';
var totalAmount = '.js-total-amount';
var refundPercentage = '.js-refund-percentage';
var refundPersonalPay = '.js-refund-personal-pay';
var refundScholarship = '.js-refund-scholarship';
var refundReturnBudGet = '.js-refund-return-bugget';
var receiptRemaining = '.js-receipt-remaining';
var receiptPersonalPay = '.js-receipt-personal-pay';
var receiptScholarship = '.js-receipt-scholarship';
var checkAllClass = '.js-check-all';

$(document).ready(function() {
    CheckList.renderCheckbox(refundTable);
})

$(document).on('keyup', refundAmount, function() {
    calculateRefundTotal();
});

$(document).on('change', refundPercentage, function() {
    let currentRow = $(this).parents('tr');
    var percentage = parseFloat($(this).val());
    var _remainingAmount = parseFloat($(currentRow.find(receiptRemaining)).val());
    var _personalPay = parseFloat($(currentRow.find(receiptPersonalPay)).val());
    var _scholarship = parseFloat($(currentRow.find(receiptScholarship)).val());

    currentRow.find(refundAmount).val(_remainingAmount * percentage  / 100);
    currentRow.find(refundPersonalPay).val(_personalPay * percentage  / 100);
    currentRow.find(refundScholarship).val(_scholarship * percentage  / 100);

    calculateRefundTotal();
});

$('#js-refund-all').on('click', function(event) {
    $(checkAllClass).click();

    $("#js-courses-list tbody tr").each(function() {
        $(this).find(refundPercentage).val("100.00").trigger('chosen:updated');
        var _remainingAmount = parseFloat($($(this).find(receiptRemaining)).val());
        var _personalPay = parseFloat($($(this).find(receiptPersonalPay)).val());
        var _scholarship = parseFloat($($(this).find(receiptScholarship)).val());
        var _scholarshipStudentId = parseInt($($(this).find('.js-refund-scholarship-student-id')).val());

        $(this).find(refundAmount).val(_remainingAmount);
        $(this).find(refundPersonalPay).val(_personalPay);
        $(this).find(refundScholarship).val(_scholarship);
        if (_scholarshipStudentId > 0 && _scholarship > 0)
        {
            $(this).find(refundReturnBudGet).val("true").trigger('chosen:updated');
        }
    });

    calculateRefundTotal();
});

function calculateRefundTotal() {
    var _totalAmount = 0;
    var rows = $('#js-courses-list .js-refund-amount');
    rows.each((_, value) => {
        var refundAmount = parseFloat($(value).val());
        _totalAmount += isNaN(refundAmount) ? 0 : refundAmount;
    });

    $(totalAmount).html(NumberFormat.renderDecimalTwoDigits(_totalAmount));
}