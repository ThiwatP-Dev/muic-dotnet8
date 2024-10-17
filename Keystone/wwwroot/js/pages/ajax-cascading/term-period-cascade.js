let startTerm = '.js-cascade-start-term';
let endTerm = '.js-cascade-end-term';

$(startTerm).on('change', function() {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetEndedTermByStartedTerm,
            data: {
                id: $(this).val(),
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(endTerm).empty().append(getDefaultSelectOption($(endTerm)));

        response.forEach((item, index) => {
            $(endTerm).append(getSelectOptions(item.value, item.text));
        });

        $(endTerm).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})