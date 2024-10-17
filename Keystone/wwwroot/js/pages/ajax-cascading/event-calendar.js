let categories ='#js-cascade-category';
let events ='#js-cascade-event';

$(categories).on('change', function() {

    var ajax = new AJAX_Helper(
        {
            url : SelectListURLDict.GetEvents,
            data : { 
                id: $(this).val(), 
            },
            dataType : 'json',
        }
    );

    ajax.POST().done(function (response) { 
        $(events).append(getDefaultSelectOption($(events)));

        response.forEach((item) => {
            $(events).append(getSelectOptions(item.id, item.nameEn))
        });

        $(events).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})