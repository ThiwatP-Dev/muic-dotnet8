$(document).ready( function () {
    $("#registration-condition-modal").on('shown.bs.modal', function (event) {
        let id = $(event.relatedTarget).data('value');
        console.log(id);
        var ajax = new AJAX_Helper(
            {
                url: `${ RegistrationConditionDetail }?id=${ id }`,
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
            }
        );

        ajax.GET().done(function (response) {
            $('#modalWrapper-registration-condition').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    });
});