var formId = '.questionnaire-by-instructor-form';

$(formId).on('submit', function() {
    $('.loader').fadeOut();
    $("#preloader").delay(200).fadeOut("slow");
})