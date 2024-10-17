$( function() {
    $('#edit-parent-modal').on('shown.bs.modal', function(e) {
        let parentId = $(e.relatedTarget).data('value');

        var ajax = new AJAX_Helper(
            {
                url: StudentDetailsSubURL.ParentEditUrl,
                data: {
                    id: parentId,
                    page: $('.js-page').val()
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-parent-edit').empty().append(response);
            $('.chosen-select').chosen();
            $('.chosen-select-no-search').chosen();

            let parentGroup = 'parent';
            let parentSelect = $(`select[data-group="${ parentGroup }"][data-type="country"]`);
            let parentCountry = $(parentSelect).find(":selected").text();
            disableSelects(parentGroup, parentCountry);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#details-parent-modal').on('shown.bs.modal', function(e) {
        let parentId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: StudentDetailsSubURL.ParentDetailsUrl,
                data: {
                    id: parentId,
                    page: $('.js-page').val()
                },
                dataType: 'json'
            }
        );

        ajax.POST().done(function (response) { 
            $('#modalWrapper-parent-details').empty().append(response);
            $('.chosen-select').chosen();
            $('.chosen-select-no-search').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})