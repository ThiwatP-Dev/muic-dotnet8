var dataModel = $('#id-card-preview').data('model');

function temporaryIdCardWithInvoice(dataModel) {
    var card = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(card, dataModel);
    var sizes = [25,18,14,12,9]

    // section 1-2
    var currentDocumentsLine = 20;
    PdfSettings.format(card, textFormats.sansBoldN)
    card.setFontSize(sizes[3])
        .text(['Additional required carduments'], 90, 15, 'center')

    if (dataModel.TemporaryCardDocumentDetails != null) {
        PdfSettings.format(card, textFormats.sansN) // font sarabun
        dataModel.TemporaryCardDocumentDetails.forEach((document) => {
            card.setFontSize(sizes[3])
                .text(`- ${ document.DocumentName }`, 57.5, currentDocumentsLine, 'left');
            currentDocumentsLine = currentDocumentsLine + 5;
        })
    };

    PdfSettings.format(card, textFormats.sansBoldN)
    card.setFontSize(sizes[3])
        .text(['Entrance Examination'], 90, currentDocumentsLine, 'center')
        
    if (dataModel.TemporarynExaminationDetails != null) {
        currentDocumentsLine = currentDocumentsLine += 5;
        PdfSettings.format(card, textFormats.sansN)
        dataModel.TemporarynExaminationDetails.forEach((examination) => {
            card.setFontSize(sizes[3])
                .text(`- ${ examination.Date } ${ examination.Time }\n  ${ examination.AdmissionExaminationType } ${ examination.Room }`, 57.5, currentDocumentsLine, 'left')
            currentDocumentsLine = currentDocumentsLine += 10;
        })
    };

    //section 1.3
    let currentStudentInfoLine = 55;
    card.addImage($('#js-default-student')[0], 'png', 155, 10, 35, 35)
        .setFontSize(sizes[4])
        .setTextColor(0,0,0)
        .setFont('helvetica', 'normal')
        .text(['Adm. No.','Firstname','Lastname','Faculty','Department','Date issued','Expires on'], 135, 55, 'left')
        .text([`${ dataModel.Code }`, dataModel.FirstName, dataModel.LastName, dataModel.FacultyName, dataModel.DepartmentName,`${ dataModel.DateIssuedString }`, `${ dataModel.ValidUntilText }`], 155, currentStudentInfoLine, 'left');

    //section 2
    card.setDrawColor(0,162,232)
        .setDrawColor(0,0,0)
        .setLineWidth(0.1)
        .setLineDash([1,1])
        .line(13, 133, 120, 133)
        .line(13, 138, 120, 138)
        .line(13, 143, 120, 143)
        .line(13, 148, 120, 148)
        .line(13, 153, 120, 153)
        .line(13, 158, 120, 158)
        .line(13, 163, 120, 163)
        .line(13, 168, 120, 168)
        .line(13, 173, 120, 173)
        .line(13, 178, 120, 178)
        .line(13, 183, 120, 183);
    
    // section 2 input
    var currentFeeItemLine = 132;
    if (dataModel.TemporaryCardFeeDetails.length > 0) {
        PdfSettings.format(card, textFormats.sansN)
        dataModel.TemporaryCardFeeDetails.forEach((feeItem) => {
            card.text(`${ feeItem.FeeItem }`, 14, currentFeeItemLine, 'left');
            currentFeeItemLine = currentFeeItemLine += 5;
        });
    
        var currentFeeItemAmountLine = 132;
        dataModel.TemporaryCardFeeDetails.forEach((feeItem) => {
            card.text(`${ feeItem.AmountText }`, 119, currentFeeItemAmountLine , 'right');
            currentFeeItemAmountLine = currentFeeItemAmountLine += 5;
        });
    }

    card.setFont('helvetica', 'bold')
        .setFontSize(sizes[3])
        .text(dataModel.Code, 180, 115, 'left')
        .text(`${ dataModel.FirstName } ${ dataModel.LastName }`, 144, 122, 'left')
        .text(dataModel.TotalFeeText, 66, 199, 'center')
        .text(dataModel.PaymentDueDateText, 163.5, 199, 'center');

    PdfSettings.format(card, textFormats.sansBoldN)
    card.setFontSize(sizes[2])
        .setLineDash([0])

    // section 3 input
    card.setFontSize(sizes[3])
        .text(dataModel.Code, 48, 241, 'left')
        .text(`${ dataModel.FirstName } ${ dataModel.LastName },`, 48, 246, 'left')
        .text(`${ dataModel.HouseNumber } MOO.${ dataModel.Moo }, ${ dataModel.Soi }, `, 48, 251, 'left')
        .text(`${ dataModel.Road } RD., ${ dataModel.SubDistrictName },`, 48, 256, 'left')
        .text(`${ dataModel.DistrictName }, ${ dataModel.ProvinceName } ${ dataModel.ZipCode }`, 48, 261, 'left')
        .text(`${ dataModel.TelephoneNumber }`, 118, 264, 'left')
    
    card.setFontSize(sizes[2])
        .text(ArabicNumberToText(dataModel.TotalFee), 66, 279, 'center')
        .text(dataModel.TotalFeeText, 163.5, 279, 'center');
        
    $('#js-print-preview').attr('src', card.output('datauristring'));
}

$(function() {
    let data = dataModel.Body;
    temporaryIdCardWithInvoice(data);
})