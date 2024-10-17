let paymentStatus = '.search-payment-type';
let registrationType = '.js-search-registration-type'

$(function() {
    checkPaymentStatus($(registrationType));
})

$(registrationType).on('change', function() {
    checkPaymentStatus($(this));
})

function checkPaymentStatus(registrationType) {
    if (registrationType.val() === 'true') {
        $(paymentStatus).addClass('enable-item')
        $(paymentStatus).removeClass('disable-item')
        $(paymentStatus).find('select').prop('disabled', false).trigger('chosen:updated');
    } else {
        $(paymentStatus).addClass('disable-item')
        $(paymentStatus).removeClass('enable-item')
        $(paymentStatus).find('select').prop('disabled', true).trigger('chosen:updated');
    }
}