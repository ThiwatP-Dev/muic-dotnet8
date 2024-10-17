
$(document).ready(function(){
    $('#delete-invoice-confirm-modal').on('show.bs.modal', function(event) {
        let button = $(event.relatedTarget);
        let inputId = $(this).find(".js-return-url");
        inputId.val("test");
        let id = button.data('value');
        let number = button.data('number');
        let returnUrl = button.data('return-url');
        $('#invoice-id-modal').val(id);
        $('#invoice-return-url').val(returnUrl); 
        $(this).find(".js-invoice-number").html("Invoice Number: " + number);
        $(".js-remark-invoice").on('change', function(){
            if($(this).val() != null && $(this).val() != undefined && $(this).val() != "")
            {
                $('#btn-cancel-invoice').prop("disabled", false);
            } else {
                $('#btn-cancel-invoice').prop("disabled", true);
            }
        });
        // let confirmButton = $(this).find('.js-delete-confirm-btn,.js-confirm-btn');
        // confirmButton.attr("href", `${ fullRoute }`);
    });
});