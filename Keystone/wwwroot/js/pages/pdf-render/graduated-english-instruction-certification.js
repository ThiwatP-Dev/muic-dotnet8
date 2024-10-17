var dataModel = $('#certificate-preview').data('model');

var page = {
    p1: "Reg. C. ",
    p2: "CERTIFICATION",
    p3: ["          This is to certify that as of", "Adm. no.", "has completed", "credits with a grade point average of", "and thus has fulfilled all the", 
         "requirements towards a", ", majoring in", "from", "Mahidol University of Thailand"],
    p4: ["          Accrediatation and Recognition : Graduates enjoy the privileges accorded to State University graduates.", 
         "Its academic standards are accepted by the Civil Service Commission of Thailand.",
         "Mahidol University is further recognized in the U.S.A and other countries enabling the admission of transfer students and Graduates from Mahidol University by foreign universities.",
         "The University is listed in the Handbook of Universities and other Instituitions of the INTERNATIONAL ASSOCIATION OF UNIVERSITIES in Paris,",
         "France as well as the Association of Christain Univerisities and Colleges in Asia (ACUCA),",
         "the Association of Southeast Asian Instituition of Higher Learning (ASAIHL), and the International Federation of Catholic Universities. (IFCU)"],
    p5: "           I also certify that English is the medium of instruction and assessment at Mahidol University."
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
    
    let dataBody = dataModel.Body;
    let language = dataBody.Language;
    let fullName = `\\${ dataBody.Title } \\${ dataBody.StudentFirstName } \\${ dataBody.StudentLastName }`; // `\\` is used to bold the text in drawEqualParagraph function
    let issuedDate = new Date(dataBody.CreatedAt)

    /* -------------------------------------------------- page 1 -------------------------------------------------- */
    PdfSettings.format(report, textFormats.serifN);
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.serifBoldHeader);
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.serifN);

    let page3 = `${ page.p3[0] } ${ getDateText(language, issuedDate) }, ${ fullName }, ${ page.p3[1] } \\${ GetStudentCodeFormat(dataBody.StudentCode) }, ${ page.p3[2] } ${ dataBody.CreditEarned } ${ page.p3[3] } ${ dataBody.GPA } ${ page.p3[4] } ${ page.p3[5] } ${ dataBody.DegreeName }${ page.p3[6] } ${ 'Major' } ${ page.p3[7] } ${ page.p3[8] }.`;
    let extractedPage3 = report.splitTextToSize(page3, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedPage3, currentY += lineYL);

    let connectedP4 = "";
    for (word in page.p4) {
        connectedP4 += `${page.p4[word]} `;
    }

    let extractedP4 = report.splitTextToSize(connectedP4, maxWidth);
    currentY = PdfSettings.justifyEn(report, extractedP4, currentY += lineYS);

    let extractedP5 = report.splitTextToSize(page.p5, maxWidth);
    PdfSettings.justifyEn(report, extractedP5, currentY += lineYN);

    PdfSettings.format(report, textFormats.serifN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 40, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(getDateText(language, issuedDate), centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate(dataModel)
})