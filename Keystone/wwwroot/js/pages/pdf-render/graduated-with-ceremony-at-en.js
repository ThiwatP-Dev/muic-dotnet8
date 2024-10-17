var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "CERTIFICATION",
    p3 : ["          This is to certify that", "Adm. no.", "has completed", "credits with a grade point average of", 
          "and thus has fulfilled all the requirements towards a", "majoring in ", "from Mahidol University of Thailand on"],
    p4 : ["          Degree certificate will be conferred on", "and", 
          "must attend the formal graduation ceremonies on that day to fulfill the last part of the requirement as an Mahidol University student."],
    p5 : "          I also certify that English is the medium of instruction at Mahidol University.",
    p6 : "Date of issue :",
    p7 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
          "www.registrar.au.edu/certificationcheck.html"]
}

function renderCertificate(dataModel) {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYS = defaultSetting.SmallLine,
        lineYN = defaultSetting.NormalLine,
        maxWidth = defaultSetting.A4Width - (paddingX * 2);
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);

    var dataBody = dataModel.Body;
    let language = dataBody.Language;
    var graduatedDate = new Date(dataBody.GraduatedAt);
    var ceremonyDate = new Date(dataBody.CeremonyAt);
    var gpaText = dataBody.GPA == -1 ? 'N/A' : NumberFormat.renderDecimalTwoDigits(dataBody.GPA);
    var fullName = `\\${ dataBody.Title } \\${ dataBody.StudentFirstName } \\${ dataBody.StudentLastName }`;
    var issuedDate = new Date(dataBody.CreatedAt);

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN);
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader)
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    let page3 = `${ page.p3[0] } ${ fullName } ${ page.p3[1] } \\${ GetStudentCodeFormat(dataBody.StudentCode) }, ${ page.p3[2] } ${ dataBody.CreditComp } ${ page.p3[3] } ${ gpaText } ${ page.p3[4] } ${ dataBody.DegreeName }, ${ page.p3[5] } ${ dataBody.DepartmentName } ${ page.p3[6] } ${ getDateText(language, graduatedDate) }.`
    let extractedPage3 = report.splitTextToSize(page3, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedPage3, currentY += lineYL);

    let page4 = `${ page.p4[0] } ${ getDateText(language, ceremonyDate) } ${ page.p4[1] } ${ (dataBody.Pronoun).toLowerCase() } ${ page.p4[2] }`;
    let extractedPage4 = report.splitTextToSize(page4, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedPage4, currentY += lineYN);

    let page5 = page.p5;
    let extractedPage5 = report.splitTextToSize(page5, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedPage5, currentY += lineYN);

    PdfSettings.format(report, textFormats.serifN)
    report.text(`${ page.p6 } ${ getDateText('en', issuedDate)}`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.serifXS)
    let verifyCode = (dataBody.VerifyCode).toString();
    let p7FullText = report.getTextWidth(`${ page.p7[0] } ${ verifyCode } ${ page.p7[1] }`);
    let p7FullTextCenter = p7FullText / 2;
    let p70Width = report.getTextWidth(page.p7[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p7FullTextCenter;
    report.text(page.p7[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(verifyCode, currentX += p70Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS)
    report.text(page.p7[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(page.p7[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate(dataModel);
})