
$(document).ready(function() {
    var table = $('#ja-grade-score-main-table').DataTable({
        paging: false,
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": [0, 8]
        }],
        "order": [[ 1, 'asc' ]]
    });

    table.on( 'order.dt search.dt', function () {
        table.column(0, {search:'applied', order:'applied'}).nodes().each( function (cell, i) {
            cell.innerHTML = i+1;
        } );
    } ).draw();
})
