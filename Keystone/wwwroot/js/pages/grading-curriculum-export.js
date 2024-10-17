var array1 = new Array();
var array2 = new Array();
var currentExportSection;
var currentTableCount;
var currentWorkSheet;

$('.js-export-excel').on('click', function () {
    currentExportSection = $(this).closest('section');
    currentTableCount = currentExportSection.find('.table-count').val()
    currentWorkSheet = currentExportSection.find('.worksheet-name').val()
    for (var x = 1; x <= currentTableCount; x++) {
        array1[x-1] = x;
        array2[x-1] = x + 'th';
    }
    
    tablesToExcel(array1, array2, currentWorkSheet);
})

$('.js-export-excel-grouping').on('click', function () {
    currentExportSection = $(this).closest('section');
    currentTableCount = currentExportSection.find('.table-count').val()
    currentWorkSheet = currentExportSection.find('.worksheet-name').val()
    for (var x = 1; x <= currentTableCount; x++) {
        array1[x-1] = x;
        array2[x-1] = x + 'th';
    }
    
    tablesToExcelGrouping(array1, array2, currentWorkSheet);
})

var tablesToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
    , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><xml><x:ExcelWorkbook><x:ExcelWorksheets>'
    , templateend = '</x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]-->'
    , head = '<head>'
    , headend = '</head>'
    , metachar = '<META http-equiv="Content-Type" content="text/html; charset=UTF-8">'
    , metastyle = '<META http-equiv="Content-Style-Type" content="text/css">'
    , style = '<STYLE type="text/css">.export-style { background-color: #dee2e6 }</STYLE>'
    , body = '<body>'
    , tablevar = '<table>{table'
    , tablevarend = '}</table>'
    , bodyend = '</body></html>'
    , worksheet = '<x:ExcelWorksheet><x:Name>'
    , worksheetend = '</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet>'
    , worksheetvar = '{worksheet'
    , worksheetvarend = '}'
    , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
    , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    , wstemplate = ''
    , tabletemplate = '';

    return function (table, name, filename) {
        var tables = table;

        for (var i = 0; i < tables.length; ++i) {
            tabletemplate += tablevar + i + tablevarend;
        }
        wstemplate += worksheet + worksheetvar + 0 + worksheetvarend + worksheetend;

        var allTemplate = style + template + wstemplate + templateend + head + metastyle + metachar + headend;
        var allWorksheet = body + tabletemplate + bodyend;
        var allOfIt = allTemplate + allWorksheet;

        var ctx = {};
        ctx['worksheet' + 0] = currentWorkSheet;

        for (var k = 0; k < tables.length; ++k) {
            var exceltable;
            if (!tables[k].nodeType) exceltable = document.getElementById("curriculum-export-"+tables[k]);
            ctx['table' + k] = exceltable.innerHTML;
        }

        document.getElementById("file-name").href = uri + base64(format(allOfIt, ctx));
        document.getElementById("file-name").download = filename;
        document.getElementById("file-name").click();
    }
})();

var tablesToExcelGrouping = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
    , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><xml><x:ExcelWorkbook><x:ExcelWorksheets>'
    , templateend = '</x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head>'
    , body = '<body>'
    , tablevar = '<table>{table'
    , tablevarend = '}</table>'
    , bodyend = '</body></html>'
    , worksheet = '<x:ExcelWorksheet><x:Name>'
    , worksheetend = '</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet>'
    , worksheetvar = '{worksheet'
    , worksheetvarend = '}'
    , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
    , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    , wstemplate = ''
    , tabletemplate = '';

    return function (table, name, filename) {
        var tables = table;

        for (var i = 0; i < tables.length; ++i) {
            tabletemplate += tablevar + i + tablevarend;
        }
        wstemplate += worksheet + worksheetvar + 0 + worksheetvarend + worksheetend;

        var allTemplate = template + wstemplate + templateend;
        var allWorksheet = body + tabletemplate + bodyend;
        var allOfIt = allTemplate + allWorksheet;

        var ctx = {};
        ctx['worksheet' + 0] = currentWorkSheet;

        for (var k = 0; k < tables.length; ++k) {
            var exceltable;
            if (!tables[k].nodeType) exceltable = document.getElementById("grouping-curriculum-export-"+tables[k]);
            ctx['table' + k] = exceltable.innerHTML;
        }

        document.getElementById("grouping-file-name").href = uri + base64(format(allOfIt, ctx));
        document.getElementById("grouping-file-name").download = filename;
        document.getElementById("grouping-file-name").click();
    }
})();