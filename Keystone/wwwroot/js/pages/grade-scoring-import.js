var allocationCount = parseInt($('#js-allocation-count').val());
// var listColumnInput = range(9, 9 + allocationCount); dinamic [10, 11, 12, 13, 14, 15, 16, 17, 18]

$(document).ready(function() {
    var importFailTable = $('#js-import-fail-scoring-table').DataTable({
        paging: false,
        "columnDefs": [ {
            "searchable": false,
            "orderable": false,
            "targets": 0
        },
        {
            "searchable": false,
            "orderable": false,
            "targets": [1]
        },
        {
            "searchable": false,
            "orderable": false,
            "targets": [2]
        },
        {
            "searchable": false,
            "orderable": false,
            "targets": -1
        } ],
        "order": [[ 3, 'asc' ]]
    });

    importFailTable.on( 'order.dt search.dt', function () {
        importFailTable.column(0, {search:'applied', order:'applied'}).nodes().each( function (cell, i) {
            cell.innerHTML = i+1;
        } );
    } ).draw();

    var importTable = $('#js-import-scoring-table').DataTable({
        paging: false,
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": [0, 1, 2]
        },
        {
            "targets": [9],
            "orderDataType": "dom-text"
        }],
        "order": [[ 3, 'asc' ]]
    });

    importTable.on( 'order.dt search.dt', function () {
        importTable.column(0, {search:'applied', order:'applied'}).nodes().each( function (cell, i) {
            cell.innerHTML = i+1;
        } );
    } ).draw();
});

function range(start, end) {
    return Array(end - start + 1).fill().map((_, idx) => start + idx)
  }