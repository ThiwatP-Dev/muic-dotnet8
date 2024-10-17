let dateClass = '.js-cascade-date';
let startTimeClass ='.js-cascade-start-time';
let endTimeClass ='.js-cascade-end-time';
let roomsClass ='.js-cascade-rooms';

var reserveDate = '';
var reserveStartTime = '';
var reserveEndTime = '';

function cascadeRooms() {  
    reserveDate = $(dateClass).val();
    reserveStartTime = $(startTimeClass).val();
    reserveEndTime = $(endTimeClass).val();
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
                console.log(item);
                $(roomsClass).append(getSelectOptions(item.id, item.nameEn));
            });
    
            $(roomsClass).trigger("chosen:updated");
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    }    
}

$(dateClass).on('change', function() {
    cascadeRooms();
})

$(startTimeClass).on('change', function() {
    cascadeRooms();
})

$(endTimeClass).on('change', function() {
    cascadeRooms();
})