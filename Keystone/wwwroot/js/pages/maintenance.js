$(document).ready(function() {
    $('#edit-modal-maintenance').on('shown.bs.modal', function(e) {
        let maintenanceStatusId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: StudentDetailsSubURL.MaintenanceStatusEditUrl,
                data: {
                    id: maintenanceStatusId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-maintenance-edit').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})