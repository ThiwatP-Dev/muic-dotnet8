var firstConditionType = '.js-first-condition-type';
var secondConditionType = '.js-second-condition-type';
var firstCondition = '.js-first-condition';
var secondCondition = '.js-second-condition';

function cascadeConditionsByType(conditionType, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetConditionsByType,
            data: {
                conditionType: conditionType
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        target.append(getDefaultSelectOption(target));

        response.forEach((item) => {
            target.append(getSelectOptions(item.value, item.text));
        });

        target.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', firstConditionType, function() {
    var firstType = $(firstConditionType).val();
    let currentSection = $(this).closest('section');

    let sectionFirstCondition = currentSection.find(firstCondition);

    if ($(firstCondition).length > 0) {
        cascadeConditionsByType(firstType, sectionFirstCondition);
    }
});

$(document).on('change', secondConditionType, function() {
    var secondType = $(secondConditionType).val();
    let currentSection = $(this).closest('section');

    let sectionSecondCondition = currentSection.find(secondCondition);

    if ($(secondCondition).length > 0) {
        cascadeConditionsByType(secondType, sectionSecondCondition);
    }
});