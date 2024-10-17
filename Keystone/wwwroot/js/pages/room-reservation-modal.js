$(document).ready(function() {
    $('#edit-room-reservation').on('shown.bs.modal', function(e) {
        let button = $(e.relatedTarget);
        let roomReservationId = button.data('value');
        let returnUrl = button.data('return-url');

        var ajax = new AJAX_Helper(
            {
                url: RoomReservationEditStatusUrl,
                data: {
                    id: roomReservationId,
                    returnUrl: returnUrl
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-room-reservation').empty().append(response);
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#edit-room-slot-reservation').on('shown.bs.modal', function(e) {
        let button = $(e.relatedTarget);
        let roomReservationId = button.data('value');
        let returnUrl = button.data('return-url');
        var ajax = new AJAX_Helper(
            {
                url: RoomSlotReservationCancelUrl,
                data: {
                    id: roomReservationId,
                    returnUrl: returnUrl
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-room-slot-reservation').empty().append(response);
            CheckList.renderCheckbox('#js-cancel-room-slot');
            $(".js-render-nicescroll").niceScroll();
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})