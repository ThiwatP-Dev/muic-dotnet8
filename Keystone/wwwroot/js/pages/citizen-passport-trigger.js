var nationality = '.js-cascade-identity';
var identityField = '.js-identity-type';

function checkType(currentNationality) {

    if (currentNationality.toUpperCase().includes("THAI")) {
        $(identityField).each(function() {
            let identityType = $(this).data('identity')
            
            if (identityType === "thai") {
                $(this).prop('disabled', false)
                $(this).parent().removeClass('disable-item')
            } else {
                $(this).prop('disabled', true)
                $(this).parent().addClass('disable-item')
            }
        });
    } else {

        $(identityField).each(function() {
            let identityType = $(this).data('identity')
            
            if (identityType === "foreign") {
                $(this).prop('disabled', false)
                $(this).parent().removeClass('disable-item')
            } else {
                $(this).prop('disabled', true)
                $(this).parent().addClass('disable-item')
            }
        });
    }
}

$(nationality).on('change', function() {
    let currentNationality = $(this).next().find('.chosen-single span').html();
    checkType(currentNationality);
})

$(document).ready(function() {
    let currentNationality = $(nationality).next().find('.chosen-single span').html();
    checkType(currentNationality);
})