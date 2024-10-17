var schoolCountry = '.js-cascade-preschool-country';
var previousSchools = '.js-cascade-previous-school';
var previousSchoolsClass = '.js-cascade-previous-school';
var schoolGroup = '.js-cascade-school-group';

function cascadePreviousSchoolByCountry(schoolCountryId) {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetPreviousSchoolsByCountryId,
            data: {
                id: schoolCountryId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        $(previousSchools).append(getDefaultSelectOption($(previousSchools)));

        response.forEach((item) => {
            $(previousSchools).append(getSelectOptions(item.value, item.text));
        });

        $(previousSchools).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadePreviousSchoolsBySchoolGroup(schoolGroupId) {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetPreviousSchoolsByGroupId,
            data: {
                schoolGroupId: schoolGroupId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        $(previousSchoolsClass).append(getDefaultSelectOption($(previousSchoolsClass)));

        response.forEach((item) => {
            $(previousSchoolsClass).append(getSelectOptions(item.value, item.text));
        });

        $(previousSchoolsClass).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(schoolCountry).on('change', function() {
    let schoolCountryId = $(this).val();

    if ($(previousSchools).length > 0) {
        cascadePreviousSchoolByCountry(schoolCountryId);
    }
})

$(schoolGroup).on('change', function() {
    let schoolGroupId = $(this).val();

    if ($(previousSchoolsClass).length > 0) {
        cascadePreviousSchoolsBySchoolGroup(schoolGroupId);
    }
})