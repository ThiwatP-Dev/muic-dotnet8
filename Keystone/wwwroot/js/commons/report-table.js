var ReportTable = (function() {

    var GeneratePrintableTable = function(printTable) {
        $(printTable).DataTable({
            "bInfo": false,
            paging: true,
            pageLength: 50,
            ordering: false,
            scrollX: true,
            scrollY: 500,
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
                },{
                    extend: 'csv',
                    text: 'Csv',
                    title: $('h1').text(),
                    exportOptions: {
                        columns: ':not(.no-print)'
                    },
                    footer: true
                },{
                    extend: 'pdf',
                    text: 'Pdf',
                    title: $('h1').text(),
                    exportOptions: {
                        columns: ':not(.no-print)',
                    },
                    footer: true
                },{
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
    }

    return {
        print : GeneratePrintableTable,
    };

})();

var ReportTableSortAndSearch = (function() {

    var GeneratePrintableTable = function(printTable) {
        var table = $(printTable).DataTable({
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
                },{
                    extend: 'csv',
                    text: 'Csv',
                    title: $('h1').text(),
                    exportOptions: {
                        columns: ':not(.no-print)'
                    },
                    footer: true
                },{
                    extend: 'pdf',
                    text: 'Pdf',
                    title: $('h1').text(),
                    exportOptions: {
                        columns: ':not(.no-print, .no-pdf)',
                    },
                    customize: function(doc) {
                        doc.defaultStyle.fontSize = 8;
                    },
                    footer: true
                },{
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
    }
    return {
        print : GeneratePrintableTable,
    };

})();

var ExcelTable = (function() {

    var GeneratePrintableTable = function(printTable) {
        $(printTable).DataTable({
            "bInfo": false,
            paging: false,
            ordering: false,
            scrollX: true,
            scrollY: 500,
            dom: 'Bfrtip',
            search: false,
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
            }
        });
    }

    return {
        print : GeneratePrintableTable,
    };

})();

var ReportInfoTable = (function() {

    var GeneratePrintableTable = function(printTable) {
        $(printTable).DataTable({
            paging: true,
            pageLength: 50,
            ordering: false,
            scrollX: true,
            scrollY: 500,
            dom: '<"block block--underline"<"block__title"<"row"<"col-6 mod-header"i>>><"block__body"<"table-responsive"Bftp>>>',
            // dom: 'Bftip',
            buttons: {
                buttons: [{
                    extend: 'excel',
                    text: 'Excel',
                    title: $('h1').text(),
                    exportOptions: {
                        columns: ':not(.no-print)'
                    },
                    footer: true
                },{
                    extend: 'csv',
                    text: 'Csv',
                    title: $('h1').text(),
                    exportOptions: {
                        columns: ':not(.no-print)'
                    },
                    footer: true
                },{
                    extend: 'pdf',
                    text: 'Pdf',
                    title: $('h1').text(),
                    exportOptions: {
                        columns: ':not(.no-print)',
                    },
                    footer: true
                },{
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
    }

    return {
        print : GeneratePrintableTable,
    };

})();

$(function() {
    ReportTable.print('.js-report-table');
    ReportTableSortAndSearch.print('.js-report-table-sort');
})