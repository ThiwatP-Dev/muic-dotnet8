

$( function() {
    $('#edit-section-slot-room').on('shown.bs.modal', function(e) {
        let id = $(e.relatedTarget).data('value');
        var ajax = new AJAX_Helper(
            {
                url: SectionSlotRoomEditUrl,
                data: {
                    id: id
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) {
            $('.js-section-slot-room-modal-create-content').empty().append(response);
            $('.chosen-select').chosen();
            $('.js-single-date').daterangepicker({
                timePicker: false,
                singleDatePicker: true,
                minDate: new Date(),
                locale: {
                    format: 'DD/MM/YYYY'
                }
            });
            InputMask.renderTimeMask();
            RenderTableStyle.columnAlign();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})