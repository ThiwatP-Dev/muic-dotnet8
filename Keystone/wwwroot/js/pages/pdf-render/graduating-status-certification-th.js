var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "ที่ ทบ. ร. ",
    p2 : "หนังสือรับรองสถานภาพการศึกษา",
    p3 : "หนังสือรับรองฉบับนี้ออกให้เพื่อแสดงว่า",
    p4 : "รหัสประจำตัว",
    p5 : ["เป็นนักศึกษาระดับ", "คณะ", "สาขา"],
    p6 : [ "ของมหาวิทยาลัยอัสสัมชัญ", "จำนวนหน่วยกิตที่สอบไล่ได้", 
          "หน่วยกิต", "จากจำนวนหน่วยกิตทั้งหมดของหลักสูตร", "โดยได้คะแนนเฉลี่ยสะสม", 
          "และคาดว่าจะศึกษาครบตามหลักสูตร ในปี พ.ศ.", "ให้ไว้ ณ​ วันที่"],
    p7 : ["สามารถตรวจสอบว่าข้อความในเอกสารฉบับนี้ได้ออกจากมหาวิทยาลัยจริงโดยใช้รหัส", "ตรวจสอบได้จาก",
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
    let p4 = report.getTextWidth(page.p4);

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(page.p4, centerX - p4StudentCodeCenter - 1, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(GetStudentCodeFormat(dataBody.StudentCode),  centerX - p4StudentCodeCenter + p4, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p5[0] }${ dataBody.AcademicLevelName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ page.p5[1] }${ dataBody.FacultyName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p5[2] }${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(page.p6[0], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6[1] } ${ dataBody.CreditComp } ${ page.p6[2] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6[3] } ${ dataBody.TotalCredit } ${ page.p6[2] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6[4] } ${ dataBody.GPA }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6[5] } ${ dataBody.Year }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p6[6] } ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));
          
    currentY = fromBottom;
    PdfSettings.format(report, textFormats.sansXS)
    let verifyCode = (dataBody.VerifyCode).toString();
    let p7FullText = report.getTextWidth(`${ page.p7[0] } ${ verifyCode } ${ page.p7[1] }`);
    let p7FullTextCenter = p7FullText / 2;
    let p70Width = report.getTextWidth(page.p7[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p7FullTextCenter;
    report.text(page.p7[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(verifyCode, currentX += p70Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansXS)
    report.text(page.p7[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(page.p7[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})