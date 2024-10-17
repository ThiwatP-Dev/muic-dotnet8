let roomSlotDate = '.js-cascade-date';
let roomSlotStartTime ='.js-cascade-start-time';
let roomSlotEndTime ='.js-cascade-end-time';
let roomsClass = '.js-cascade-rooms';
var summitSave = '.js-update-submit';

var reserveDate = '';
var reserveStartTime = '';
var reserveEndTime = '';

$( function() {
    $('#add-section-slot').on('shown.bs.modal', function(e) {
        let sectioId = $(e.relatedTarget).data('section-id');
        var ajax = new AJAX_Helper(
            {
                url: SectionSlotCreateUrl,
                data: {
                    sectionId: sectioId
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) {
            $('.js-section-slot-modal-create-content').empty().append(response);
            $('.chosen-select').chosen({
                search_contains: true
            });
            $('.js-single-date').daterangepicker({
                timePicker: false,
                singleDatePicker: true,
                //minDate: new Date(),
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

    $('#edit-section-slot').on('shown.bs.modal', function(e) {
        let id = $(e.relatedTarget).data('value');
        var ajax = new AJAX_Helper(
            {
                url: SectionSlotEditUrl,
                data: {
                    id: id
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) {
            $('.js-section-slot-modal-create-content').empty().append(response);
            $('.chosen-select').chosen();
            $('.js-single-date').daterangepicker({
                timePicker: false,
                singleDatePicker: true,
                //minDate: new Date(),
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

function cascadeRooms() {  
    reserveDate = $(roomSlotDate).val();
    reserveStartTime = $(roomSlotStartTime).val();
    reserveEndTime = $(roomSlotEndTime).val();
    $(roomsClass).append(getDefaultSelectOption($(roomsClass))).trigger('chosen:updated');
    if (reserveDate !== '' && reserveStartTime !== '' && reserveEndTime !== '') {
        var ajax = new AJAX_Helper(
            {
                url: SelectListURLDict.GetAvailableRoom,
                data: {
                    date: reserveDate,
                    start: reserveStartTime,
                    end: reserveEndTime
                },
                dataType: 'json' 
            }
        );
    
        ajax.POST().done(function (response) {
    
            response.forEach((item) => {
                $(roomsClass).append(getSelectOptions(item.id, ` ${ item.nameEn } [${ item.capacity }]` ));
            });
    
            $(roomsClass).trigger("chosen:updated");
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    }
}

$(document).on('change', roomSlotDate, function() {
    cascadeRooms();
})

$(document).on('change', roomSlotStartTime, function() {
    cascadeRooms();
})

$(document).on('change', roomSlotEndTime, function() {
    cascadeRooms();
})

$(document).ready(function () {
    CheckList.renderCheckbox('#js-section-slot');
});