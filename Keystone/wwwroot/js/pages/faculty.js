$(document).ready(function() {
    $('#add-faculty-member').on('shown.bs.modal', function(e) {
        let facultyId = $(e.relatedTarget).data('value');
        let type = $(e.relatedTarget).data('type');
        let returnUrl = $(e.relatedTarget).data('return-url');

        var ajax = new AJAX_Helper(
            {
                url: AddFacultyProgramDirector,
                data: {
                    id: facultyId,
                    type: type,
                    returnUrl: returnUrl
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('.modalWrapper-faculty-member').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#edit-faculty-member').on('shown.bs.modal', function(e) {
        let facultyMemberId = $(e.relatedTarget).data('value')
        let type = $(e.relatedTarget).data('type')
        let returnUrl = $(e.relatedTarget).data('return-url');

        var ajax = new AJAX_Helper(
            {
                url: EditFacultyProgramDirector,
                data: {
                    id: facultyMemberId,
                    type: type,
                    returnUrl: returnUrl
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('.modalWrapper-faculty-member').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})