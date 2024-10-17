var listSortable = '.list-sortable';

function reorderIndex(list) {
    if (list !== null) {
        let listId = $(list).attr('id').split('-');
        let listIndex = `[${ listId[3] }]`;
        var listItemsId = "#" + $(list).attr('id') + " li";
        var groupId = $(list).data('id');

        $(listItemsId).each( function(index) {
            let inputName = $(this).find('input').attr('name');
            let nameParts = inputName.split('.')
            let newName = '';
            let itemIndex = `[${ index }]`;

            $(nameParts).each( function(index) {
                if (index === 0) {
                    newName += this.replace(RegularExpressions.SingleDigitInBracket, listIndex);
                } else if (index === 1) {
                    newName += `.${ this.replace(RegularExpressions.SingleDigitInBracket, itemIndex) }`;
                } else {
                    newName += `.${ this }`;
                }
            })
            
            $(this).find('input').each(function (i) {
                var itemInputName = $(this).attr('name');
                let itemNameParts = itemInputName.split('.');
                let newNameParts = newName.split('.');
                $(this).attr('name', newNameParts[0] + "." + newNameParts[1] + "." + itemNameParts[2]);
                if (itemNameParts[2].startsWith("CourseGroupId")) {
                    $(this).val(groupId);
                }

                var itemInputId = $(this).attr('id');
                let itemIdParts = itemInputId.split('__');
                let newId = '';

                $(this).attr('id', itemIdParts[0].replace(/\d+/, listId[3]) + "__" + itemIdParts[1].replace(/\d+/, index) + "__" + itemIdParts[2]);
            });
            //$(this).find('input').attr('name', newName);
        })
    }
}

$(listSortable).on('sortupdate', function(event, uiObj) {
    reorderIndex(uiObj.sender);
    reorderIndex(event.currentTarget);
})

$(function() {
    $(listSortable).sortable({
        connectWith: listSortable
    }).disableSelection();
});