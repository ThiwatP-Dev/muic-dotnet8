$(document).ready( function () {
    $("#required-documents-modal").on('shown.bs.modal', function (event) {
        let admissionDocumentGroupId = $(event.relatedTarget).data('value');
        
        var ajax = new AJAX_Helper(
            {
                url: `${ AdmissionDocumentGroupDetailsUrl }?admissionDocumentGroupId=${ admissionDocumentGroupId }`,
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
            }
        );

        ajax.GET().done(function (response) {
            $('#modalWrapper-required-documents').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    });
});