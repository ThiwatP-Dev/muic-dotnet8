$(document).ready( function() {
    CheckInput();
});
$(document).on('change', '#NameEn', function() {
    CheckInput();
});

$(document).on('change', '#NameTh', function() {
    CheckInput();
});

function CheckInput()
{
    var nameTh = $('#NameTh').val();
    var nameEn = $('#NameEn').val();
    console.log()
    if(nameTh == null || nameTh == "" || nameEn == null || nameEn == "")
    {
        $('#js-save-event-catagory').prop("disabled", true);
    }
    else
    {
        $('#js-save-event-catagory').prop("disabled", false);
    }
}
