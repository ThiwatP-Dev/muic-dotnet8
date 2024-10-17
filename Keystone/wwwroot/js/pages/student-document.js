$(document).ready(function() {
    var inputTable = new RowAddAble({
        TargetTable: '#js-document',
        ButtonTitle: 'Document',
        TableTitle: 'Required Document'
    });
    inputTable.RenderButton();
    minMaxLimit();


    $('#view-document-modal').on('shown.bs.modal', function(event) {
        let body = $('#body');
        let button = $(event.relatedTarget); // Button that triggered the modal
        let value = button.data('value');
        let valueLower = value.toLowerCase();
        let width = 1000;
        body.empty();
        if (valueLower.endsWith('png') || valueLower.endsWith('jpg') || valueLower.endsWith('jpeg') || valueLower.endsWith('svg')) {
            body.append("<img src='" + value + "' width='" + width + "' />");
        } else if (valueLower.endsWith('doc') || valueLower.endsWith('docx') || valueLower.endsWith('xls') || valueLower.endsWith('xlsx') || valueLower.endsWith('ppt') || valueLower.endsWith('pptx')) {
            /* เอาไว้เทส "http://mvtv.co.th/ios/example/sample-docx-file-for-testing.docx"*/
            body.append("<iframe src='https://view.officeapps.live.com/op/embed.aspx?src=" + value + 
                "' width='" + width + "px' height='700px' frameborder='0'></iframe>");
        } else if (valueLower.endsWith('pdf')) {
            body.append("<embed src='" + value + "' width='" + width + "' height='700'>");
        } else {
            body.append("File type is not suppored.");
        }
    })
});

$(document).on('click', '.js-add-row', function() {
    var table = $('#js-document');
    var lastRow = table.find('tr:last')
    var count = table.find('tr').length - 2;
    lastRow.find('.js-amount').val(1);
    lastRow.find('.js-submitted-amount').val(0);
    lastRow.find('.js-document-status').removeClass('bg-warning bg-success bg-danger')
                                       .addClass('bg-danger')
                                       .html('Waiting');
    lastRow.find('.js-required-docuemnt-id').val(0);
    lastRow.find('.js-student-document-id').val(0);
    lastRow.find('.js-addmission-document-group-id').val(0);
    lastRow.find('.td-upload').html('<button class="btn btn-mw-120 btn--primary mr-2 mb-0" type="button" onclick="document.getElementById(\'StudentDocuments_' + count + '__UploadFile\').click();">Browse</button>' +
        '<span id="filename_' + count + '"></span>');
    lastRow.append('<input type="file" class="form-control-file" onchange="onSelectDocumentFile(this, ' + count + ');" id="StudentDocuments_' + count + '__UploadFile" name="StudentDocuments[' + count + '].UploadFile"></input>');
    lastRow.find('.js-current-imageurl').remove();
    lastRow.find('.la-info').remove();
    minMaxLimit();
});