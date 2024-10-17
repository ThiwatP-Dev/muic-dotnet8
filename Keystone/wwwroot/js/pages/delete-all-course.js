function setButtonAvailability() {
    var deleteButton = $('#delete-button');
    var atLeastOneIsChecked = $('input[name="deleteItem"]:checked').length > 0;
    if (atLeastOneIsChecked) {
        deleteButton.prop('disabled', false);
    } else {
        deleteButton.prop('disabled', true);
    }
}

$('.student-checkbox').on('change', function() {
    setButtonAvailability();
})

$(document).ready(function() {
    CheckList.renderCheckbox('.js-checklist-student');
    $('#student-check-all').on('change', function() {
        CheckList.renderCheckbox('.js-checklist-student');
        setButtonAvailability();
    })
})