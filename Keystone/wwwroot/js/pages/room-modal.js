$(document).ready(function() {
    $('#details-room-modal').on('shown.bs.modal', function(e) {
        let roomId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: RoomDetailsUrl,
                data: {
                    id: roomId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-room-details').empty().append(response);
            RenderTableStyle.columnAlign();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})