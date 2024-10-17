

$(function () {
    $('.all-slot-dt-export').DataTable({
        "bInfo": false,
        ordering: false,
        scrollX: true,
        paging: false,
        searching: false,
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
            }, {
                extend: 'csv',
                text: 'Csv',
                title: $('h1').text(),
                exportOptions: {
                    columns: ':not(.no-print)'
                },
                footer: true
            }, {
                extend: 'pdf',
                text: 'Pdf',
                title: $('h1').text(),
                exportOptions: {
                    columns: ':not(.no-print)',
                },
                orientation: 'landscape',
                footer: true,
                customize: function (doc) {
                    doc.defaultStyle.fontSize = 8; //<-- set fontsize to 16 instead of 10 
                }
            }, {
                extend: 'print',
                text: 'Print',
                title: $('h1').text(),
                exportOptions: {
                    columns: ':not(.no-print)'
                },
                footer: true,
                autoPrint: true
            }],
            dom: {
                container: {
                    className: 'dt-buttons'
                },
                button: {
                    className: 'btn btn--primary'
                }
            },
        },
        language: {
            emptyTable: '<i class="text-danger">No Data</i>'
        }
    });
})