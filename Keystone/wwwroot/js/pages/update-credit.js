var summitSave = '#js-update-credit-submit';
var summitSave2 = '#js-update-credit-submit-2';

$(document).ready( function() {
    CheckList.renderCheckbox('#js-update-credit');
    $(".js-render-nicescroll").niceScroll();
    checkSaveButton();
});


$(document).on('change', '.js-update-select', function() {

    $(summitSave).prop('disabled', true);
    $(summitSave2).prop('disabled', true);
     if($(this).is(':checked'))
     {
        $(summitSave).prop('disabled', false);
        $(summitSave2).prop('disabled', false);
     } else {
        checkSaveButton();
     }
});

$('#checkAll').change(function() {

    $(summitSave).prop('disabled', true);
    $(summitSave2).prop('disabled', true);
     if($(this).is(':checked'))
     {
        $(summitSave).prop('disabled', false);
        $(summitSave2).prop('disabled', false);
     } else {
        checkSaveButton();
     }
});

$('#minimum-credit').focus(function() {
    $(this).keyup(function() {
        checkSaveButton()
    });

    $(this).change(function() {
        checkSaveButton()
    });
});


$('#maximum-credit').focus(function() {
    $(this).keyup(function() {
        checkSaveButton()
    });

    $(this).change(function() {
        checkSaveButton()
    });
});

function checkCreditInput()
{
    var min = parseInt($('#minimum-credit').val());
    var max = parseInt($('#maximum-credit').val());
    if(min > max)
    {
        $("#message-warning").show(200);
        $(summitSave).prop('disabled', true);
        $(summitSave2).prop('disabled', true);
        return false;
    } else {
        $("#message-warning").hide(200);
        return true;
    }
}

function checkSaveButton()
{
    if(checkCreditInput())
    {
        var studentSelects = $('#js-items .js-update-select');
        $(summitSave).prop('disabled', true);
        $(summitSave2).prop('disabled', true);
        studentSelects.each((_, value) => {
            if($(value).is(':checked'))
            {
                $(summitSave).prop('disabled', false);
                $(summitSave2).prop('disabled', false);
                return;
            }       
        });
    }
}