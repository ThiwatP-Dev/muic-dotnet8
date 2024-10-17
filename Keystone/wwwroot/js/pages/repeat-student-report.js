var formId = '#export-excel-form';

$(document).ready( function() {

    var table = $('#js-repeat-student').DataTable({
        paging: false,
        dom: 'Bfrtip',
        buttons: {
            buttons: [{
                extend: 'excel',
                text: 'Excel',
                title: $('h1').text(),
                exportOptions: {
                    columns: ':not(.no-print)'
                },
                footer: true
            }],
            dom: {
                container: {
                    className: 'dt-buttons'
                },
                button: {
                    className: 'btn btn--primary'
                }
            }
        },
        language: {
            emptyTable: '<i class="text-danger">No Data</i>'
        },
        "order": [[ 0, 'asc' ]]
    });
})
