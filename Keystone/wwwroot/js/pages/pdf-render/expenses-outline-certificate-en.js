var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "Certification",
    p3 : ["              This is to certify that ", "Adm. No.", "is a bona fide", "student in the", "-year", ", majoring in", "at Mahidol University of Thailand."],
    p4 : ["              The following details outline the expenses to be in curred for semester", "."],
    p5 : ["Total", "Baht"]
};

function renderCertificate() {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYN = defaultSetting.NormalLine,
        lineYS = defaultSetting.SmallLine,
        maxWidth = defaultSetting.A4Width - (paddingX * 2);
    
    var dataBody = dataModel.Body;
    var language = dataBody.Language;
    var issuedDate = new Date(dataModel.Body.CreatedAt);

    var studentFullName = `\\${ dataBody.Title } \\${ dataBody.StudentFirstName } \\${ dataBody.StudentLastName }`;

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN)
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft))

    PdfSettings.format(report, textFormats.serifBoldHeader)
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    let p3 = `${ page.p3[0] } ${ studentFullName } ${ page.p3[1]} \\${ GetStudentCodeFormat(dataBody.StudentCode) } ${ page.p3[2] } ${ dataBody.StudentYear } ${ page.p3[3]} ${ dataBody.StudyYear }${ page.p3[4] } ${ dataBody.DegreeName }${ page.p3[5]} ${ dataBody.DepartmentName } ${ page.p3[6]}`;
    let extractedParagraph3 = report.splitTextToSize(p3, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedParagraph3, currentY += lineYL);

    PdfSettings.format(report, textFormats.serifN)
    let p4 = `${ page.p4[0] } ${ dataBody.TermText }${ page.p4[1] }`;
    let extractedParagraph4 = report.splitTextToSize(p4, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedParagraph4, currentY += lineYN);

    if (dataBody.Receipt.ReceiptItems != undefined && dataBody.Receipt.ReceiptItems.length > 0) {
        PdfSettings.format(report, textFormats.serifN)
        var index = 1;
        var subTotalReceiptAmountWidth = 0;
        $(dataBody.Receipt.ReceiptItems).each( function() {
            currentY += lineYN;
            report.text(`              ${ index }. ${ this.FeeItemName }`, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
                  .text(`${ this.TotalAmountText } ${ page.p5[1]}`, (207 - paddingX), currentY, PdfSettings.style(textStyles.CertificateRight));
            index++;
            subTotalReceiptAmountWidth = report.getTextWidth(this.TotalAmountText);
        })
        
        report.line(((207 - paddingX) - 8) - subTotalReceiptAmountWidth, currentY + 1, (207 - paddingX) - 8, currentY + 1);
        
        PdfSettings.format(report, textFormats.serifBoldN)
        currentY += lineYN;
        var totalReceiptAmountWidth = report.getTextWidth(dataBody.Receipt.TotalAmountText);
        report.text(`                                                        ${ page.p5[0] }`, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
              .text(`${ dataBody.Receipt.TotalAmountText } ${ page.p5[1]}`, (207 - paddingX), currentY, PdfSettings.style(textStyles.CertificateRight));

        report.line(((207 - paddingX) - 8) - totalReceiptAmountWidth, currentY + 1, (207 - paddingX) - 8, currentY + 1);
    }

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 40, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(getDateText(language, issuedDate), centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})