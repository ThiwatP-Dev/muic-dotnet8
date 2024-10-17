$( function() {

    $('#edit-address-modal').on('shown.bs.modal', function(e) {
        let buttonId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: StudentDetailsSubURL.AddressEditUrl,
                data: {
                    id: buttonId,
                },
                dataType: 'html'
            }
        );
    
        ajax.GET().done(function (response) { 
            $('#modalWrapper-address-edit').empty().append(response);
            $('.chosen-select').chosen();
            $('.chosen-select-no-search').chosen();

            let addressGroup = 'address';
            let addressSelect = $(`select[data-group="${ addressGroup }"][data-type="country"]`);
            let addressCountry = $(addressSelect).find(":selected").text();
            disableSelects(addressGroup, addressCountry);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    });
})