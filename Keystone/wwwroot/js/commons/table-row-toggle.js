var RenderToggle = (function() {

    var ToggleHiddenButton = function(hiddenRowButton) {
        $(hiddenRowButton).on('click', function() {
            var parentRow = $(this.closest('tr, .toggle-navigator'));
            var hiddenRow = parentRow.next('tr.hidden-row, div.hidden-row');
            var spanColumn = parentRow.find('.span-column');
            var toggleButton = $(this).find('i');
    
            /* toggleClass will check a element's classes.
            If it has the specify classes, remove those class.
            If it hasn't the specify class, add those class */
            toggleButton.toggleClass('fa-minus-circle');
            toggleButton.toggleClass('fa-plus-circle');
            hiddenRow.toggleClass('d-none');
    
            if (hiddenRow.hasClass('d-none')) {
                spanColumn.each(function() {
                    $(this).removeAttr('rowspan');
                })
            } else {
                spanColumn.each(function() {
                    $(this).attr('rowspan', 2);
                })
            }
        });
    }

    return {
        toggleHiddenButton : ToggleHiddenButton
    };

})();

$(document).ready(function() {
    RenderToggle.toggleHiddenButton('.hidden-row-toggle');
});