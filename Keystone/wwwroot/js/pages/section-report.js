var array1 = new Array();
var array2 = new Array();
var workSheetName = $('.section-worksheet-name').val();
for (var x = 1; x <= 1; x++) {
    array1[x-1] = x;
    array2[x-1] = x + 'th';
}

$('.js-export-excel').on('click', function () {
    tablesToExcel(array1, array2, workSheetName);
})

var tablesToExcel = (function () {
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
        ctx['worksheet' + 0] = workSheetName;

        for (var k = 0; k < tables.length; ++k) {
            var exceltable;
            if (!tables[k].nodeType) exceltable = document.getElementById("section-report-export");
            ctx['table' + k] = exceltable.innerHTML;

        }

        document.getElementById("section-file-name").href = uri + base64(format(allOfIt, ctx));
        document.getElementById("section-file-name").download = filename;
        document.getElementById("section-file-name").click();
    }
})();