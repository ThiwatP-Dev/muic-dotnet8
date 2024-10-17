form = '.js-pivot-form';

$(document).ready(function() {
    $('#js-pivot-by-student').DataTable({
        "columnDefs": [{
            "targets": "no-sort",
            "orderable": false
        }]
    });
});

$(form).on('submit', function() {
    $('.loader').fadeOut();
    $("#preloader").delay(1000).fadeOut("slow");
})