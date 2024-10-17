$(document).ready(function() {
    $('#details-petition-modal').on('shown.bs.modal', function(e) {
        let petitionId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: PetitionDetailUrl,
                data: {
                    id: petitionId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-petition-details').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})

$(document).ready(function() {
    $('#add-petition-log').on('shown.bs.modal', function(e) {
        let petitionId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: AddPetitionUrl,
                data: {
                    id: petitionId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-petition-log').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})