var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "ที่ ทบ. ร. ",
    p2 : "หนังสือรับรอง",
    p3 : "หนังสือรับรองฉบับนี้ออกให้เพื่อแสดงว่า",
    p4 : "รหัสประจำตัวนักศึกษา  ",
    p5 : "เคยเป็นนักศึกษา",
    p6 : ["ระดับ", " คณะ"],
    p7 : ["เริ่มเข้าศึกษาตั้งแต่วันที่", "- วันที่"],
    p8 : "ของมหาวิทยาลัยอัสสัมชัญ",
    p9 : "ให้ไว้ ณ วันที่",
    p10 : ["สามารถตรวจสอบว่าข้อความในเอกสารฉบับนี้ได้ออกจากมหาวิทยาลัยจริงโดยใช้รหัส", "ตรวจสอบได้จาก",
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
    PdfSettings.format(report, textFormats.sansN)
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansBoldHeader)
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(page.p3, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    let p4StudentCode = report.getTextWidth(`${ page.p4 } ${ dataBody.StudentCode }`);
    let p4StudentCodeCenter = p4StudentCode / 2;
    let p4AdmNo = report.getTextWidth(page.p4);

    PdfSettings.format(report, textFormats.sansN)
    report.text(page.p4, centerX - p4StudentCodeCenter + 1, currentY += lineYN, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(GetStudentCodeFormat(dataBody.StudentCode),  centerX - p4StudentCodeCenter + p4AdmNo, currentY, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p5 }${ page.p6[0] }${ dataBody.AcademicLevelName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ page.p6[1]}${ dataBody.FacultyName }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p7[0] } ${ getDateText(language, new Date(dataBody.AdmissionDate)) } ${ page.p7[1] } ${ getDateText(language, new Date(dataBody.GraduatedAt)) }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p8, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p9 } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansXS)
    let verifyCode = (dataBody.VerifyCode).toString(),
        p10FullText = report.getTextWidth(`${ page.p10[0] } ${ verifyCode } ${ page.p10[1] }`),
        p10FullTextCenter = p10FullText / 2,
        p10_0Width = report.getTextWidth(page.p10[0]),
        verifyCodeWidth = report.getTextWidth(verifyCode);

    currentX = centerX - p10FullTextCenter;
    report.text(page.p10[0], currentX, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(verifyCode, currentX += p10_0Width + 1, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansXS)
    report.text(page.p10[1], currentX += verifyCodeWidth + 1, fromBottom - 8, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(page.p10[2], centerX, fromBottom, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})