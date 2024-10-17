$(document).on('click', '#cancel-barcode', function() {
    var barcodeId = $('#js-barcode-id').val();
    var returnUrl = $('#return-url').val();
    window.location=`${ GradeBarcodeCancelUrl }?barcodeId=${ barcodeId }&returnUrl=${ returnUrl }`;
})

$('#js-barcode-input').click( function() {
    $(this).select();
})

$(document).ready( function() {
    $('#js-barcode-input').select();
})