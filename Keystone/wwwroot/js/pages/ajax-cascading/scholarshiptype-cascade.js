var scholarshipType ='.js-cascade-scholarship-type';
var scholarship ='.js-cascade-scholarship';

function cascadeScholarship(scholarshipTypeId) {
    var ajax = new AJAX_Helper({
            url: SelectListURLDict.GetScholarshipsByScholarshipTypeId,
            data: {
                scholarshipTypeId: scholarshipTypeId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(scholarship).append(getDefaultSelectOption($(scholarship)));

        response.forEach((item) => {
            $(scholarship).append(getSelectOptions(item.value, item.text));
        });

        $(scholarship).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(scholarshipType).on('change', function() {
    let scholarshipTypeId = $(this).val();
    
    if ($(scholarship).length > 0) {
        cascadeScholarship(scholarshipTypeId);
    }
})