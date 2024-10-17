var waiveAmount = '.js-waive-discount-amount';
var amount = '.js-waive-temp-total-amount';
var defaultTotalAmount = '.js-waive-ori-total-amount';
var totalWaiveAmount = '.js-waive-sum-discount-amount';
var totalInvoiceDiscountAmount = '.js-waive-invoice-discount-amount';
var totalAmount = '.js-waive-sum-total-amount';

$(document).on('keyup', waiveAmount, function() {
    let currentRow = $(this).closest('tr');
    let defaultTotalAmount = currentRow.find('.js-waive-ori-total-amount').val();
    var discountAmount = $(this).val();
    currentRow.find(amount).val(defaultTotalAmount - discountAmount);
    currentRow.find('.js-waive-total-amount').html(NumberFormat.renderDecimalTwoDigits(defaultTotalAmount - discountAmount));

    var _totalWaiveAmount = 0;
    var rows = $(waiveAmount);
    rows.each((_, value) => {
        var _amount = parseInt($(value).val());
        _totalWaiveAmount += isNaN(_amount) ? 0 : _amount;
    });
    $(totalWaiveAmount).html(NumberFormat.renderDecimalTwoDigits(_totalWaiveAmount));

    var _totalAmount = 0;
    var rows = $(amount);
    rows.each((_, value) => {
        var _amount = parseInt($(value).val());
        _totalAmount += isNaN(_amount) ? 0 : _amount;
    });

    var _invcoiceDiscountAmount = parseInt($(totalInvoiceDiscountAmount).val());
    var _invoiceDiscountAmountClean = isNaN(_invcoiceDiscountAmount) ? 0 : _invcoiceDiscountAmount;
    _totalAmount -= _invoiceDiscountAmountClean;

    $(totalAmount).html(NumberFormat.renderDecimalTwoDigits(_totalAmount));
});