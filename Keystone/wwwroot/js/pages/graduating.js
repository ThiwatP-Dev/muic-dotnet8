$(document).ready( function() {

    var table = $('#js-export-excel').DataTable({
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
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": [0]
        } ],
        "order": [[ 10, 'desc' ]]
    });
})
