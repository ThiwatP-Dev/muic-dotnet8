function setButtonAvailability() {
    var saveButton = $('#save-button');
    var atLeastOneIsChecked = $('input[name="studentListId"]:checked').length > 0;
    if (atLeastOneIsChecked && $('select[name="instructorId"]')[0].selectedIndex > 0) {
        saveButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
    }
}

$('select[name="instructorId"]').on('change', function () {
    setButtonAvailability();
})

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