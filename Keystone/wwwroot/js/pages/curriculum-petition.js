$(document).ready(function() {
    var table = $('#js-repot-table-curriculum-petition').DataTable({
        paging: true,
        pageLength: 25,
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
})
