let paymentSection = '.js-payment-section';
let requiredPayment = '.js-required-payment';
let documentAmount = '#js-document-amount';
let documentFee = '#js-document-fee';
let issuedDate = '#js-limit-date';

$(requiredPayment).change( function () {
    if ($(requiredPayment).is(":checked")) {
        $(paymentSection).removeClass('d-none');
    } else {
        $(documentAmount).val(0);
        $(documentFee).val(0);
        $(paymentSection).addClass('d-none');
    }
})

$(documentAmount).change( function () {
    let price = $(documentAmount).val() * 50;
    $(documentFee).val(price);
})

$(document).ready( function () {
    
    let currentDate = $(issuedDate).val();
    singleDateFormat.minDate = currentDate;

    $(issuedDate).daterangepicker(singleDateFormat);
    $(issuedDate).on('apply.daterangepicker', function(ev, picker) {
        $(this).val(picker.startDate.format('DD/MM/YYYY'));
    });
})