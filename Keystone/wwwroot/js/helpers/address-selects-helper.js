function getProvinceByCountry(value, selectGroup) {
    var cascadeSelect = $(`select[data-group="${ selectGroup }"][data-type="province"]`);

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetProvinceByCountryId,
            data: {
                id: value,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        response.forEach((item, index) => {
            cascadeSelect.append(getSelectOptions(item.id, item.nameEn));
        });

        cascadeSelect.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getDistrictByProvince(value, selectGroup) {
    var cascadeSelect = $(`select[data-group="${ selectGroup }"][data-type="district"]`);

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetDistrictByProvinceId,
            data: {
                id: value
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        response.forEach((item, index) => {
            cascadeSelect.append(getSelectOptions(item.id, item.nameEn));
        });

        cascadeSelect.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getSubdistrictByDistrict(value, selectGroup) {
    var cascadeSelect = $(`select[data-group="${ selectGroup }"][data-type="subdistrict"]`);

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSubdistrictByDistrictId,
            data: {
                id: value
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        response.forEach((item, index) => {
            cascadeSelect.append(getSelectOptions(item.id, item.nameEn));
        });

        cascadeSelect.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getStateByCountry(value, selectGroup) {
    var cascadeSelect = $(`select[data-group="${ selectGroup }"][data-type="state"]`);

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetStateByCountryId,
            data: {
                id: value
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {

        response.forEach((item, index) => {
            cascadeSelect.append(getSelectOptions(item.id, item.nameEn));
        });

        cascadeSelect.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getCityByState(value, selectGroup) {
    var cascadeSelect = $(`select[data-group="${ selectGroup }"][data-type="city"]`);

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCityByStateIdSelectList,
            data: {
                id: value
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        response.forEach((item, index) => {
            cascadeSelect.append(getSelectOptions(item.id, item.nameEn));
        });

        cascadeSelect.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function resetSelectsData(selectGroup, resetTarget) {
    var selects = $(`select[data-group="${ selectGroup }"]`);

    selects.each( function() {
        var currentType = $(this).data('type');

        resetTarget.forEach( function(targetType) {
            if (currentType === targetType) {
                let currentSelect = $(`select[data-group="${ selectGroup }"][data-type="${ currentType }"]`)

                currentSelect.append(getDefaultSelectOption(currentSelect));
                currentSelect.trigger("chosen:updated");
            }
        })
    });
}

function disableSelects(selectGroup, currentCountry) {
    var selects = $(`select[data-group="${ selectGroup }"]`);

    selects.each( function() {
        let currentType = $(this).data('type');

        if (currentCountry.includes("THAILAND")) {

            if (currentType === "state" || currentType === "city") {
                $(this).prop('disabled', true)
                $(this).parent().addClass('disable-item')
            } else {
                $(this).prop('disabled', false)
                $(this).parent().removeClass('disable-item')
            }
        } else {
            if (currentType === "province" || currentType === "district" || currentType === "subdistrict") {
                $(this).prop('disabled', true)
                $(this).parent().addClass('disable-item')
            } else {
                $(this).prop('disabled', false)
                $(this).parent().removeClass('disable-item')
            }
        }
        $(this).trigger("chosen:updated");
    });
}

$(document).on('change', '.js-address-cascade', function() {
    var value = $(this).val();
    var selectGroup = $(this).data('group');
    var selectType = $(this).data('type');

    switch(selectType) {
        case "country":
            // reset children selects
            var resetTarget = [AddressType.State, AddressType.Province, AddressType.City, AddressType.District, AddressType.Subdistrict];
            resetSelectsData(selectGroup, resetTarget);

            // ajax get child's select data
            getProvinceByCountry(value, selectGroup);
            getStateByCountry(value, selectGroup);

            // disable select
            let currentCountry = $(this).next().find('.chosen-single span').html().toUpperCase();
            disableSelects(selectGroup, currentCountry);
            break;
        case "province":
            var resetTarget = [AddressType.District, AddressType.Subdistrict];
            resetSelectsData(selectGroup, resetTarget)

            getDistrictByProvince(value, selectGroup)
            break;
        case "district":
            var resetTarget = [AddressType.Subdistrict];
            resetSelectsData(selectGroup, resetTarget)

            getSubdistrictByDistrict(value, selectGroup)
            break;
        case "state":
            var resetTarget = [AddressType.City];
            resetSelectsData(selectGroup, resetTarget)

            getCityByState(value, selectGroup)
            break;
        default:
            break;
    }
})

$( function() {
    let birthGroup = 'birth';
    let birthSelect = $(`select[data-group="${ birthGroup }"][data-type="country"]`);
    let birthCountry = $(birthSelect).next().find('.chosen-single span').html();
    disableSelects(birthGroup, birthCountry)
})