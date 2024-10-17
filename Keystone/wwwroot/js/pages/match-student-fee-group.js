$(document).ready(function() {
    $('#match-confirm-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        
        let form = button.data('form-id');
        let feeGroup = $('#js-get-fee-group').val();
        let confirmButton = $('.js-confirm-btn');

        $('#js-get-group').text(feeGroup);

        confirmButton.click( function() {
            $(form).submit();
        })
    });
});