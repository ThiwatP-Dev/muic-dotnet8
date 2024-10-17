var formId = '#export-excel-form';
$(document).ready(function() {
    $('#edit-examination-reservation').on('shown.bs.modal', function(e) {
        let examinationReservationId = $(e.relatedTarget).data('value');
        let returnUrl = $(e.relatedTarget).data('return-url');
        let roomId = $(e.relatedTarget).data('room-id');

        var ajax = new AJAX_Helper(
            {
                url: ExaminationReservationUrl,
                data: {
                    id: examinationReservationId,
                    returnUrl: returnUrl
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-examination-reservation').empty().append(response);
            $('.chosen-select').chosen();
            CheckButtonSave();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
        if(roomId != 0)
        {
            CheckRoom(examinationReservationId, roomId);
        }
        $(document).on('change', '#RoomId', function(){
            roomId = $(this).val();
            CheckRoom(examinationReservationId, roomId);
            CheckButtonSave();
        });
        $(document).on('change', '.js-status', function(){
            CheckButtonSave();
        });
    })
})

$(formId).on('submit', function() {
    $('.loader').fadeOut();
    $("#preloader").delay(1000).fadeOut("slow");
})
function CheckButtonSave()
{
    let status = $('.js-status').val();
    let roomId = $('#RoomId').val();
    if(status == "a" && roomId == null){
        $(".js-button-save").prop("disabled", true);
    } else {
        $(".js-button-save").prop("disabled", false);
    }
}
function CheckRoom(examId, roomId)
{
    var ajax = new AJAX_Helper(
        {
            url: CheckExaminationReservationUrl,
            data: {
                id: examId,
                roomId: roomId
            },
            dataType: 'json'
        }
    );

    ajax.GET().done(function (response) { 
        if(response) {
            Alert.renderAlert("Warning", response, "warning");
        } 
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}