$('.js-read-items ,.js-write-items').on('change', function() {
    let groupCount = 0;
    let currentCheckGroup = $(this).data('group');
    let currentCheckItem = $(this).data('item');
    let currentCheckStatus = $(this).prop('checked');
    let allCheckboxInGroup = $(`input[data-group=${ currentCheckGroup }]`);
    let groupOrder = currentCheckGroup.split('-')

    if (currentCheckItem === 0) {
        if (currentCheckStatus) {
            if (groupOrder[0] === "write") {
                let readCheckboxOfGroup = $(`input[data-group="read-${ groupOrder[1] }"]`);
                $(readCheckboxOfGroup).each( function() {
                    $(this).prop('checked', true)
                })
            }

            $(allCheckboxInGroup).each( function() {
                $(this).prop('checked', true)
            })
        } else {
            if (groupOrder[0] === "read") {
                let writeCheckboxOfGroup = $(`input[data-group="write-${ groupOrder[1] }"]`);
                $(writeCheckboxOfGroup).each( function() {
                    $(this).prop('checked', false)
                })
            }

            $(allCheckboxInGroup).each( function() {
                $(this).prop('checked', false)
            })
        }
        
    } else {
        $(allCheckboxInGroup).each( function() {
            if ($(this).prop('checked') === true && $(this).data('item') != 0) {
                groupCount++;
            }
        })
        
        if (currentCheckStatus) {
            $(allCheckboxInGroup[0]).prop('checked', true)

            if (groupOrder[0] === "write") {
                $(`input[data-group="read-${ groupOrder[1] }"][data-item="0"]`).prop('checked', true);
                $(`input[data-group="read-${ groupOrder[1] }"][data-item="${ currentCheckItem }"]`).prop('checked', true);
            }
        } else {
            if (groupOrder[0] === "read") {
                $(`input[data-group="write-${ groupOrder[1] }"][data-item="${ currentCheckItem }"]`).prop('checked', false);
            }

            if (groupCount === 0) {
                $(allCheckboxInGroup[0]).prop('checked', false)
                $(`input[data-group="write-${ groupOrder[1] }"][data-item="0"]`).prop('checked', false);
            }
        }
    }
})
