var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "STUDENT STATUS CERTIFICATION",
    p3 : "This is to certify that",
    p4 : "Adm. No. ",
    p5 : ["is enrolled in the"],
    p6 : "majoring in",
    p7 : ["and has completed", "credits out of the required", "credits"],
    p8 : "with a cumulative grade point average of",
    p9 : ["During this semester, the said student has registered for", "credits"],
    p10 : "and is expected to complete all the requirements for graduation by the year",
    p11 : "The final examination result are still pending.",
    p12 : "I also certify that English is the medium of instruction at Mahidol University.",
    p13 : "Date of issue :",
    p14 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
    "www.registrar.au.edu/certificationcheck.html"]
}

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
    var issuedDate = new Date(dataBody.CreatedAt);

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

    let p4DataWidth = report.getTextWidth(`${ page.p4 } ${ GetStudentCodeFormat(dataBody.StudentCode) }`);
    let p4DataWidthCenter = p4DataWidth / 2;
    let p4Width  = report.getTextWidth(page.p4);

    PdfSettings.format(report, textFormats.serifN)
    report.text(page.p4, centerX - p4DataWidthCenter, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldN)
    report.text(GetStudentCodeFormat(dataBody.StudentCode), centerX - p4DataWidthCenter + p4Width , currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p5[0]} ${ dataBody.DegreeName },`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6} ${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p7[0]} ${ dataBody.CreditComp } ${ page.p7[1] } ${ dataBody.TotalCredit } ${ page.p7[2] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p8 } ${ dataBody.GPA }.`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p9[0]} ${ dataBody.RegistringCredit } ${ page.p9[1] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p10 } ${ dataBody.GraduatedYear }.`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p11, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p12, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p13 } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.serifXS)
    let verifyCode = (dataBody.VerifyCode).toString();
    let p14FullText = report.getTextWidth(`${ page.p14[0] } ${ verifyCode } ${ page.p14[1] }`);
    let p14FullTextCenter = p14FullText / 2;
    let p140Width = report.getTextWidth(page.p14[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p14FullTextCenter;
    report.text(page.p14[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(verifyCode, currentX += p140Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS)
    report.text(page.p14[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(page.p14[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})