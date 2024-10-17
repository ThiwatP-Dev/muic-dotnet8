$(document).ready(function() {
    $('#curriculum-petition-modal').on('shown.bs.modal', function(e) {
        let petitionId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: CurriculumPetitionUrl,
                data: {
                    id: petitionId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-curriculum-petition').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})