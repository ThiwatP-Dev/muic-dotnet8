$(document).ready( function() {
    $('.js-baht-format').each( function() {
        let currentValue = $(this).html();
        $(this).html(NumberFormat.renderBahtCurrency(currentValue));
    })

    $('#active-modal').on('shown.bs.modal', function(event) {
        let button = $(event.relatedTarget);
        let scholarshipId = button.data('scholarship');
        let studentId = button.data('student');

        $('input[name="scholarshipId"]').val(scholarshipId)
        $('input[name="studentId"]').val(studentId)
    })
})