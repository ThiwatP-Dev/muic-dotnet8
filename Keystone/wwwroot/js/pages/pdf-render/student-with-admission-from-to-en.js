var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "STUDENT STATUS CERTIFICATION",
    p3 : "This is to certify that",
    p4 : ["Adm. no.", "  was a bona fide", "student at Mahidol University."],
    p5 : "had been enrolled in",
    p6 : ["the", "-year"],
    p7 : ["during", "to"],
    p8 : "at Mahidol University of Thailand.",
    p9 : "Date of Issue :",
    p10 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
           "www.registrar.au.edu/certificationcheck.html"]
}

function renderCertificate() {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYN = defaultSetting.NormalLine,
        lineYS = defaultSetting.SmallLine,
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);

    var dataBody = dataModel.Body;
    var issuedDate = new Date(dataBody.CreatedAt);
    var language = dataBody.Language.toLowerCase();

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

    let p4StudentCode = report.getTextWidth(`${ page.p4[0] } ${ dataBody.StudentCode }, ${ page.p4[1] } ${ dataBody.StudentYear } ${ page.p4[2] }`);
    let p4StudentCodeCenter = p4StudentCode / 2;
    let p4AdmNo = report.getTextWidth(page.p4[0]) + 2;
    let p4StudentCodeText = report.getTextWidth(dataBody.StudentCode) + 1;

    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p4[0], centerX - p4StudentCodeCenter + 1, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(`${ GetStudentCodeFormat(dataBody.StudentCode) }, `,  centerX - p4StudentCodeCenter + p4AdmNo, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p4[1] } ${ dataBody.StudentYear } ${ page.p4[2] }`, centerX - p4StudentCodeCenter + p4AdmNo + p4StudentCodeText, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ dataBody.Pronoun } ${ page.p5 }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6[0] } ${ dataBody.StudyYear }${ page.p6[1] } ${ dataBody.DegreeName }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p7[0] } ${ getDateText(language, new Date(dataBody.AdmissionDate)) } ${ page.p7[1] } ${ getDateText(language, new Date(dataBody.GraduatedAt)) }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p8, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p9 } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifXS)
    let verifyCode = (dataBody.VerifyCode).toString(),
        p10FullText = report.getTextWidth(`${ page.p10[0] } ${ verifyCode } ${ page.p10[1] }`),
        p10FullTextCenter = p10FullText / 2,
        p10_0Width = report.getTextWidth(page.p10[0]),
        verifyCodeWidth = report.getTextWidth(verifyCode);

    currentX = centerX - p10FullTextCenter;
    report.text(page.p10[0], currentX, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(verifyCode, currentX += p10_0Width + 1, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS)
    report.text(page.p10[1], currentX += verifyCodeWidth + 1, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(page.p10[2], centerX, fromBottom, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})