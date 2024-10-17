$(document).ready( function() {
    $('#update-verification-modal').on('shown.bs.modal', function(e) {
        let verificationId = $(e.relatedTarget).data('value');
        let receivedNumber = $(e.relatedTarget).data('received-number');
        let receivedAt = $(e.relatedTarget).data('received-at');
        let rerferenceNumber = $(e.relatedTarget).data('reference-number');
        $('.js-letter-id').val(verificationId);
        $('.js-get-reference-number').html(rerferenceNumber);
        $('input[name="receivedNumber"]').val(receivedNumber);
        $('input[name="receivedAt"]').val(receivedAt);
    })
})