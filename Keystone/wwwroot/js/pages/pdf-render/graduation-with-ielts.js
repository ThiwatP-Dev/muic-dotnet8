var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C.",
    p2 : "CERTIFICATION",
    p3 : ["              This is to certify that", "Adm. no.", "has completed", "credits with a grade point average of", 
          "and fulfilled all the requirements for a", ", majoring in ", "from Mahidol University of Thailand on", 
          "I also certify that English is the medium of instruction at Mahidol University."],
    p4 : ["              The said student has successfully completed the core required English courses ", 
          "(Levels 1-4) at this university, and is therefore deemed to possess an English aptitude equivalency of IELTS score"],
    p5 : ["This is a certified copy. For verification purposes, use this verified code No.", "to check it out at",
          "www.registrar.au.edu/certificationcheck.html"]
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
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);
    
    let dataBody = dataModel.Body;
    let language = dataBody.Language;
    let fullName = `\\${ dataBody.Title } \\${ dataBody.StudentFirstName } \\${ dataBody.StudentLastName }`;
    var gpaText = dataBody.GPA === -1 ? 'N/A' : NumberFormat.renderDecimalTwoDigits(dataBody.GPA);
    var graduatedDate = new Date(dataBody.GraduatedAt);
    var scoreText = dataBody.IELTSScore < 0 ? 0 : NumberFormat.renderDecimalOneDigits(dataBody.IELTSScore);
    let issuedDate = new Date(dataBody.CreatedAt);

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.serifN);
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader);
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN);
    let page3 = `${ page.p3[0] } ${ fullName } ${ page.p3[1] } \\${ GetStudentCodeFormat(dataBody.StudentCode) }, ${ page.p3[2] } ${ dataBody.CreditComp } ${ page.p3[3] } ${ gpaText } ${ page.p3[4] } ${ dataBody.DegreeName }${ page.p3[5] } ${ dataBody.DepartmentName } ${ page.p3[6] } ${ getDateText('en', graduatedDate) }. ${ page.p3[7] }`;
    let extractedPage3 = report.splitTextToSize(page3, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedPage3, currentY += lineYL);

    let page4 = `${ page.p4[0] } ${ page.p4[1] } ${ scoreText }.`;
    let extractedPage4 = report.splitTextToSize(page4, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedPage4, currentY += lineYN);

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 40, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(getDateText(language, issuedDate), centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));
    
    currentY = fromBottom;
    PdfSettings.format(report, textFormats.serifXS)
    let verifyCode = (dataBody.VerifyCode).toString();
    let p5FullText = report.getTextWidth(`${ page.p5[0] } ${ verifyCode } ${ page.p5[1] }`);
    let p5FullTextCenter = p5FullText / 2;
    let p50Width = report.getTextWidth(page.p5[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p5FullTextCenter;
    report.text(page.p5[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(verifyCode, currentX += p50Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.serifXS)
    report.text(page.p5[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.serifBoldXS)
    report.text(page.p5[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})