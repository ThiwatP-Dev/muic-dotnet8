$(document).ready( function() {

    var table = $('.js-datatable-export-excel').DataTable({
        paging: false,
        "bInfo": false,
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
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [0, -1]
        }],
        "order": [[ 1, 'asc' ]]
    });

    table.on( 'order.dt search.dt', function () {
        table.column(0, {search:'applied', order:'applied'}).nodes().each( function (cell, i) {
            cell.innerHTML = i+1;
            table.cell(cell).invalidate('dom');
        } );
    } ).draw();

    var tableSortOnly = $('.js-datatable-sort').DataTable({
        paging: false,
        "bInfo": false,
        dom: 'Bfrtip',
        buttons: [],
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [0, -1]
        }],
        "order": [[ 1, 'asc' ]]
    });

    tableSortOnly.on( 'order.dt search.dt', function () {
        tableSortOnly.column(0, {search:'applied', order:'applied'}).nodes().each( function (cell, i) {
            cell.innerHTML = i+1;
            tableSortOnly.cell(cell).invalidate('dom');
        } );
    } ).draw();
})
