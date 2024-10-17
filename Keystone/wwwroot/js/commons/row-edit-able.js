var startEditButton = '.js-start-edit';
var cancelEditButton = '.js-cancel-edit';
var editedInput = '.js-edit-input';
var editedValue = '.js-edit-value';
var hideClass = 'd-none';
var parentRow;

var EditTable = (function() {
    
    var RenderEditTable = function(editTableClass) {
        let editTable = $(editTableClass);
        var saveButton = $(editTable).find('.js-save-edit');
        let cancelButton = $(editTable).find(cancelEditButton);
        let editButton = $(editTable).find(startEditButton);
        
        $('.editable-chosen-select').chosen();

        var nextElementOfSelect = $(editTableClass).find('select').next();
        nextElementOfSelect.toggleClass(hideClass);

        $(cancelButton).click(function() {
            parentRow = $(this).parents('tr');
            let restoreInputs = parentRow.find(editedInput);
            
            restoreInputs.each(function() {
                let defaultValue = $(this).data('default-value');
                $(this).val(defaultValue);
            })
            toggleHide(parentRow);
            toggleChosenHide(parentRow);
        })

        $(editButton).click(function() {
            parentRow = $(this).parents('tr');
            toggleHide(parentRow);
            parentRow.find('.chosen-container').removeClass(hideClass);
        })

        $(saveButton).click(function() {
            parentRow = $(this).parents('tr');
            saveEdit(parentRow);
            toggleHide(parentRow);
            toggleChosenHide(parentRow);
        })
    }

    var ToggleEdit = function(parentRow)  {
        toggleHide(parentRow);
    }

    var toggleHide = function(parentRow) {
        let inputElement = parentRow.find(editedInput);
        let valueElement = parentRow.find(editedValue);
        let selectElement = parentRow.find('select');
        let nextElementOfSelect = selectElement.next();

        inputElement.toggleClass(hideClass);
        valueElement.toggleClass(hideClass);
        nextElementOfSelect.toggleClass(hideClass);
        
        selectElement.chosen({inherit_select_classes: true});
        selectElement.trigger('chosen:updated');
                
        parentRow.find(startEditButton).toggleClass(hideClass);
        parentRow.find('.js-quit-edit').toggleClass(hideClass);
    }

    var toggleChosenHide = function(parentRow) {
        parentRow.find('.chosen-container').toggleClass(hideClass);
    }

    var saveEdit = function(parentRow) {
        let valueElements = parentRow.find(editedValue);
        let inputElement = parentRow.find(`input${ editedInput }, select${ editedInput }`);

        inputElement.each( function(index, item) {
            if ($(this).is('input')) {
                $(this).data('default-value', item.value);
                $(valueElements[index]).html(item.value);
            } else if ($(this).is('select')) {
                let selectedText = $(item).next().find('a > span').html();
                $(this).data('default-value', item.value);
                $(valueElements[index]).html(selectedText);
            }
            
        })
    }

    return {
        toggleHideEdit : ToggleEdit,
        renderEditTable : RenderEditTable
    };
})();