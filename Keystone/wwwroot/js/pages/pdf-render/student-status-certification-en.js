var dataModel = $('#certificate-preview').data('model');
var page = {
    p1 : "Reg. C. ",
    p2 : "STUDENT STATUS CERTIFICATION",
    p3 : "This is to certify that",
    p4 : ["Adm. No.", "was a bona fide", "student at Mahidol University."],
    p5 : "had been enrolled in",
    p6 : ["the", "-year " ,"of "],
    p7 : "at Mahidol University of Thailand.",
    p8 : "I also certify that English is the medium of instruction at Mahidol University.",
    p9 : "Date of Issue :",
    p10 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
           "www.registrar.au.edu/certificationcheck.html"]
}

function generateCertificate(dataModel) {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYN = defaultSetting.NormalLine,
        lineYS = defaultSetting.SmallLine,
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);

        var dataBody = dataModel.Body,
        issuedDate = new Date(dataBody.CreatedAt),
        language = dataBody.Language.toLowerCase();

    /* -------------------------------------------------- page 1 -------------------------------------------------- */
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

    let studentCode = GetStudentCodeFormat(dataBody.StudentCode),
        p4Width = report.getTextWidth(`${ page.p4[0] } ${ studentCode }, ${ page.p4[1] } ${ dataBody.StudentYear } ${ page.p4[2] }`),
        p4WidthCenter = p4Width / 2,
        p4_0Width = report.getTextWidth(page.p4[0]),
        p4StudentCodeWidth = report.getTextWidth(`${ studentCode },`),
        currentX = centerX - p4WidthCenter;

    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p4[0], currentX, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(`${ studentCode }, `,  currentX += p4_0Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p4[1] } ${ dataBody.StudentYear } ${ page.p4[2] }`, currentX += p4StudentCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));

    let p6Text = `${ page.p6[0] } ${ dataBody.StudyYear }${ page.p6[1] }${ dataBody.DegreeName }`,
        pronoun = dataBody.Pronoun,
        longTextOption = PdfSettings.style(textStyles.CertificateCenter);
    longTextOption.lineHeightFactor = 1.9;
    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ pronoun } ${ page.p5 }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter))
          .text(p6Text, centerX, currentY += 12, longTextOption);

    let p6LineCount = report.splitTextToSize(p6Text, longTextOption.maxWidth).length;
    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p7, centerX, currentY += lineYN * p6LineCount, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p8, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter))
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
};

$(function() {
    generateCertificate(dataModel);
})