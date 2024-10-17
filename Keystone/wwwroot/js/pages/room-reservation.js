var reservationSelect = "#room-reservation-select";

$(document).ready(function() {

    $('#js-room-reservation-modal').on('shown.bs.modal', function(e) {
        let id = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: RoomReservationDetails,
                data: {
                    id: id,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#js-room-reservation-content').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    });
});
function cascadeRoom(){
    let startDate, endDate, startTime, endTime, startTimeMatch, endTimeMatch;
    let isSun, isMon, isTue, isWed, isThu, isFri, isSat;

    $('.js-single-date').each(function() {
        if($(this).data("type") == "dateFrom")
        {
            startDate = $(this).val();
        } else if($(this).data("type") == "dateTo") {
            endDate = $(this).val();
        }
    })

    $('.form-check-input').each(function() {
        if($(this).data("type") == "sun")
        {
            isSun = $(this).prop("checked");
        } else if($(this).data("type") == "mon") {
            isMon = $(this).prop("checked");
        } else if($(this).data("type") == "tue") {
            isTue = $(this).prop("checked");
        } else if($(this).data("type") == "wed") {
            isWed = $(this).prop("checked");
        } else if($(this).data("type") == "thu") {
            isThu = $(this).prop("checked");
        } else if($(this).data("type") == "fri") {
            isFri = $(this).prop("checked");
        } else if($(this).data("type") == "sat") {
            isSat = $(this).prop("checked")
        };
    })

    $('.js-time-mask').each(function() {
        if($(this).data("type") == "timeStart")
        {
            startTime = $(this).val();
            startTimeMatch =$(this).val().toString().match(/^([01]\d|2[0-3]):?([0-5]\d)$/);
        } else if($(this).data("type") == "timeEnd") {
            endTime = $(this).val();
            endTimeMatch = $(this).val().toString().match(/^([01]\d|2[0-3]):?([0-5]\d)$/);
        }
    })
    $(reservationSelect).append(getDefaultSelectOption($(reservationSelect)));
    $(reservationSelect).trigger("chosen:updated");

    if((isSun == true || isMon == true || isTue == true || isWed == true 
        || isThu == true || isFri == true || isSat == true)
        && startTime 
        && startTimeMatch
        && endTimeMatch
        && endDate)
    {
        $(".js-room-fade").addClass("d-none");
        $(".js-spinner-fade").removeClass("d-none");
        var ajax = new AJAX_Helper(
            {
                url: SelectListURLDict.GetAvailableRoomByDates,
                data: {
                    dateFrom : startDate,
                    dateTo : endDate,
                    startTimeText : startTime,
                    endTimeText : endTime,
                    IsSunday :isSun,
                    IsMonday :isMon,
                    IsTuesday :isTue,
                    IsWednesday :isWed,
                    IsThursday :isThu,
                    IsFriday :isFri,
                    IsSaturday :isSat,

                },
                dataType: 'json'
            }
        );
        ajax.POST().done(function (response) { 
            $(reservationSelect).empty();
            $(reservationSelect).append(getDefaultSelectOption($(reservationSelect)));
            response.forEach((item) => {
                $(reservationSelect).append(getSelectOptions(item.value, item.text));
            });
            $(reservationSelect).trigger("chosen:updated");
            setTimeout(function() {           
                $(".js-spinner-fade").addClass("d-none");
                $(".js-room-fade").removeClass("d-none");
            }, 1000);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
            setTimeout(function() {           
                $(".js-spinner-fade").addClass("d-none");
                $(".js-room-fade").removeClass("d-none");
            }, 1000);
        });
    }
}
$(document).on("focusout", ".js-time-mask, .js-single-date", function() {
    cascadeRoom();
});

$(document).on("click", ".js-suggestion-item, .ui-state-default", function() {
    cascadeRoom();
});

$(document).on("change", ".form-check-input, .js-single-date", function() {
    cascadeRoom();
});