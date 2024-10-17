let campus = '.js-cascade-campus';
let building ='.js-cascade-building';
let room ='.js-cascade-room';
let rooms ='.js-cascade-rooms';

function cascadeBuildings(campusId) {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetBuildingByCampusId,
            data: {
                id: campusId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(building).append(getDefaultSelectOption($(building)));
        $(room).empty().trigger("chosen:updated");
        $(rooms).empty().trigger("chosen:updated");

        response.forEach((item) => {
            $(building).append(getSelectOptions(item.id, item.nameEn));
        });

        $(building).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeRooms(buildingId) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetRoomByBuildingId,
            data: {
                id: buildingId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(room).append(getDefaultSelectOption($(room)));
        $(rooms).empty();

        response.forEach((item) => {
            $(room).append(getSelectOptions(item.id, item.nameEn));
            $(rooms).append(getSelectOptions(item.id, item.nameEn));
        });

        $(room).trigger("chosen:updated");
        $(rooms).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(campus).on('change', function() {
    let campusId = $(this).val();

    if ($(building).length > 0) {
        cascadeBuildings(campusId);
    }
})

$(building).on('change', function() {
    let buildingId = $(this).val();

    if ($(room).length > 0 || $(rooms).length > 0) {
        cascadeRooms(buildingId);
    }
})