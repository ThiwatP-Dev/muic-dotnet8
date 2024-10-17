$(document).ready( function() {
    CheckList.renderCheckbox('#js-grade-member-user');
    $(".js-render-nicescroll").niceScroll();
    // $('.form-check-input, .js-check-all').on('change', function() {
        //     setSaveButton();
        // })
})

$(document).on('click', '.js-delete', function () {
    var username =  $(this).closest('tr').find('.js-username').text();
    $("#delete-confirm-modal-content").empty().html("Are you sure to delete this <br> UserName : " + username + " ?");
});

function setSaveButton() {
    var saveButton = $('.js-save-button');
    var atLeastOneIsChecked = $('#js-grade-member-user').find('.form-check-input:checked').length > 0;
    if (atLeastOneIsChecked) {
        saveButton.prop('disabled', false);
    } else {
        saveButton.prop('disabled', true);
    }
}