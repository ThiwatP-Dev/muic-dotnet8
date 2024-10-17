var formId = '#export-excel-form';

$(formId).on('submit', function() {
    console.log("2");
    $('.loader').fadeOut();
    $("#preloader").delay(200).fadeOut("slow");
})

// $(document).ready(function(){
//     var table = $('#js-student-ability-report').DataTable({
//         paging: false,
//         dom: 'Bfrtip',
//         buttons: {

//             buttons: [{
//                 extend: 'excel',
//                 text: 'Excel',
//                 title: $('h1').text(),
//                 exportOptions: {
//                     columns: ':not(.no-print)',
//                     format: {
//                         body: function ( data, row, column, node ) {
//                             return data.replace(/<br\s*\/?>/ig, "\r");
//                         }
//                     }
//                 },
//                 customize: function( xlsx ) {
//                     var sheet = xlsx.xl.worksheets['sheet1.xml'];
//                     // set cell style: Wrapped text
//                     //ref https://datatables.net/reference/button/excelHtml5
//                     $('row c[r^="E"]', sheet).attr( 's', '55' );
//                     $('row c[r^="F"]', sheet).attr( 's', '55' );
//                     $('row c[r^="G"]', sheet).attr( 's', '55' );
//                 },
//                 footer: true
//             }],
//             dom: {
//                 container: {
//                     className: 'dt-buttons'
//                 },
//                 button: {
//                     className: 'btn btn--primary'
//                 }
//             }
//         },
//         language: {
//             emptyTable: '<i class="text-danger">No Data</i>'
//         },
//     });
// });

$(document).on('click', '#export-excel-button', function() {
    console.log("1");
    $(formId).attr('action',"/StudentAbilityReport/ExportExcel");
    $(formId).submit();
});

$(document).on('click', '#export-excel-pivot-button', function() {
    console.log("1");
    $(formId).attr('action',"/StudentAbilityReport/ExportExcelPivot");
    $(formId).submit();
});