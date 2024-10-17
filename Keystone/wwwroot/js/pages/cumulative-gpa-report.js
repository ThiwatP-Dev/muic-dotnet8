var formId = '.export-excel-form';

$(formId).on('submit', function() {
    $('.loader').fadeOut();
    $("#preloader").delay(200).fadeOut("slow");
})