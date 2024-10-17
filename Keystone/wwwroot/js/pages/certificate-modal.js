$(document).ready(function() {
    $('#details-certificate-modal').on('shown.bs.modal', function(e) {
        let certificateId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: CertificateDetailUrl,
                data: {
                    id: certificateId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-certificate-details').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})

$(document).ready(function() {
    $('#edit-certificate').on('shown.bs.modal', function(e) {
        let certificateId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: EditCertificateUrl,
                data: {
                    id: certificateId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-certificate-edit').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})