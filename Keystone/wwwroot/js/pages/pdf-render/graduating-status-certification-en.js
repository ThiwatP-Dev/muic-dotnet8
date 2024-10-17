var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "STUDENT STATUS CERTIFICATION",
    p3 : "I hereby certify that",
    p4 : "Adm. No.",
    p5 : ["is enrolled in the", ",", "majoring in", "and has completed", 
          "credits out of required", "credits", "with a grade point average of", ".", 
          "The said student is expected to complete all the requirements for graduation by the year", ".", 
          "I also certify that English is the medium of instruction at Mahidol University.",
          "Date of issue :"],
    p6 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
          "www.registrar.au.edu/certificationcheck.html"]
};

function renderCertificate() {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYS = defaultSetting.SmallLine,
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);
    
    var dataBody = dataModel.Body;
    var language = dataBody.Language;
    var issuedDate = new Date(dataModel.Body.CreatedAt);

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN)
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader)
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p3, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(`${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    let p4StudentCode = report.getTextWidth(`${ page.p4 } ${ dataBody.StudentCode }`);
    let p4StudentCodeCenter = p4StudentCode / 2;
    let p4 = report.getTextWidth(page.p4);

    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p4, centerX - p4StudentCodeCenter - 1, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(GetStudentCodeFormat(dataBody.StudentCode),  centerX - p4StudentCodeCenter + p4, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p5[0] } ${ dataBody.DegreeName }${ page.p5[1] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
            .text(`${ page.p5[2] } ${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
            .text(`${ page.p5[3] } ${ dataBody.CreditComp } ${ page.p5[4] } ${ dataBody.TotalCredit } ${ page.p5[5] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
            .text(`${ page.p5[6] } ${ dataBody.GPA }${ page.p5[7] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
            .text(`${ page.p5[8] } ${ dataBody.Year }${ page.p5[9] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.ReportCenter))
            .text(page.p5[10], centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter))
            .text(`${ page.p5[11] } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.serifXS)
    let verifyCode = (dataBody.VerifyCode).toString();
    let p6FullText = report.getTextWidth(`${ page.p6[0] } ${ verifyCode } ${ page.p6[1] }`);
    let p6FullTextCenter = p6FullText / 2;
    let p60Width = report.getTextWidth(page.p6[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p6FullTextCenter;
    report.text(page.p6[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(verifyCode, currentX += p60Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS)
    report.text(page.p6[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(page.p6[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})