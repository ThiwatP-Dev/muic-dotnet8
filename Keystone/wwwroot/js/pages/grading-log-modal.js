var GradingReportExport = (function() {

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
                        doc.styles.tableHeader.fontSize = 10;
                        var rowCount = doc.content[0].table.body.length; 
                        for (i = 0; i < rowCount; i++) {
                                doc.content[0].table.body[i][0].alignment = 'center';
                                doc.content[0].table.body[i][5].alignment = 'center';
                                doc.content[0].table.body[i][6].alignment = 'center';
                                doc.content[0].table.body[i][9].alignment = 'center';
                        };
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

$(document).ready(function() {
    $('#details-grading-log-modal').on('shown.bs.modal', function(e) {
        let registrationCourseId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: GradingLogDetailUrl,
                data: {
                    id: registrationCourseId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-grading-log-details').empty().append(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    GradingReportExport.print('.js-grading-report-table');
})