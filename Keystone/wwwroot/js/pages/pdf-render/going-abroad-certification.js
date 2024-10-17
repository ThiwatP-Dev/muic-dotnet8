var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C.",
    p2 : "CERTIFICATION",
    p3 : ["              This is to certify that", "Adm. no.", "is a bona fide", "student in the", "-year", "degree program, majoring in ", "at Mahidol University, Bangkok, Thailand."],
    p4 : ["              The said student would like to go to", "from", "and we expect this student will return in order to continue", "studies."],
    p5 : "              I certify that English is the medium of instruction at Mahidol University.",
    p6 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
          "www.registrar.au.edu/certificationcheck.html"]
};

function getDateMonthText(language, fullDate) {
    var date = fullDate.getDate();
    var monthName = getMonthNames(language)[fullDate.getMonth()];
    return `${ monthName } ${ date }`;
}

function renderCertificate() {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYS = defaultSetting.SmallLine,
        maxWidth = defaultSetting.A4Width - (paddingX * 2);
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);
    
    var dataBody = dataModel.Body;
    var abroadFromDate = new Date(dataBody.AbroadFromDate);
    var abroadToDate = new Date(dataBody.AbroadToDate);
    let fullName = `\\${ dataBody.Title } \\${ dataBody.StudentFirstName } \\${ dataBody.StudentLastName }`;
    let country = toCapitalizationFormat(dataBody.AbroadCountry);
    let language = dataBody.Language;
    var issuedDate = new Date(dataBody.CreatedAt);

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN);
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader);
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN);
    let page3 = `${ page.p3[0] } ${ fullName } ${ page.p3[1] } \\${ GetStudentCodeFormat(dataBody.StudentCode) }, ${ page.p3[2] } ${ dataBody.StudentYear } ${ page.p3[3] } ${ dataBody.StudyYear }${ page.p3[4] } ${ dataBody.DegreeName } ${ page.p3[5] } ${ dataBody.DepartmentName } ${ page.p3[6] }`;
    let extractedPage3 = report.splitTextToSize(page3, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedPage3, currentY += lineYL);
    
    let page4 = `${ page.p4[0] } ${ country } ${ page.p4[1] } ${ getDateMonthText('en', abroadFromDate) } - ${ getDateText('en', abroadToDate) }. ${ page.p4[2] } ${ dataBody.Possessive } ${ page.p4[3] }`;
    let extractedP4 = report.splitTextToSize(page4, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedP4, currentY += lineYS);

    let page5 = page.p5
    let extractedp5 = report.splitTextToSize(page5, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedp5, currentY += lineYS);

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 40, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(getDateText(language, issuedDate), centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

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