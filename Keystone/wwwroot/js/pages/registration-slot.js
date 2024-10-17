var term = '.js-cascade-term';
var registrationTerm = '.js-cascade-registration-term';

function cascadeRegistrationTerm(termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetRegistrationTermsByTerm,
            data: {
                termId: termId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(target).append(getDefaultSelectOption($(target)));
        response.forEach((item) => {
            $(target).append(getSelectOptions(item.value, item.text));
        });

        $(target).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
};

$(document).on('change', term, function() {
    let currentSection = $(this).closest('section');
    let termId = $(this).val();

    let sectionRegistrationTerm = currentSection.find(registrationTerm);

    if (sectionRegistrationTerm.length > 0) {
        cascadeRegistrationTerm(termId, sectionRegistrationTerm);
    }
});