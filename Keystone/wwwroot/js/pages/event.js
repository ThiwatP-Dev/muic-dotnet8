$(document).ready( function() {
    CheckInput();
});
$(document).on('change', '#NameEn', function() {
    CheckInput();
});

$(document).on('change', '#NameTh', function() {
    CheckInput();
});

$(document).on('change', '.js-event-category', function() {
    CheckInput();
});

function CheckInput()
{
    var nameTh = $('#NameTh').val();
    var nameEn = $('#NameEn').val();
    var eventCategory = $('.js-event-category').val();
    if(nameTh == null || nameTh == "" || nameEn == null || nameEn == "" || eventCategory == null)
    {
        $('#js-save-event').prop("disabled", true);
    }
    else
    {
        $('#js-save-event').prop("disabled", false);
    }
}
