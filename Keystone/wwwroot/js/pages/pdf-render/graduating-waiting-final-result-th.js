var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "ที่ ทบ. ร. ",
    p2 : "ใบรับรองสถานภาพการศึกษา",
    p3 : "หนังสือรับรองฉบับนี้ออกให้เพื่อแสดงว่า",
    p4 : "รหัสประจำตัว",
    p5 : "เป็นนักศึกษาระดับ",
    p6 : "คณะ",
    p7 : "สาขา",
    p8 : "ของมหาวิทยาลัยอัสสัมชัญ",
    p9 : ["จำนวนหน่วยกิตที่สอบไล่ได้", "หน่วยกิต"],
    p10 : ["จากจำนวนหน่วยกิตทั้งหมดของหลักสูตร", "หน่วยกิต"],
    p11 : "โดยได้คะแนนเฉลี่ยสะสม",
    p12 : "และคาดว่าจะจบตามหลักสูตร ในปี พ.ศ.",
    p13 : "ขณะนี้กำลังรอผลภาคเรียนสุดท้าย",
    p14 : "ให้ไว้ ณ​ วันที่",
    p15 : ["สามารถตรวจสอบว่าข้อความในเอกสารฉบับนี้ได้ออกจากมหาวิทยาลัยจริงโดยใช้รหัส", "ตรวจสอบได้จาก",
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

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ page.p4 } ${GetStudentCodeFormat(dataBody.StudentCode) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p5 }${ dataBody.AcademicLevelName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN)
    report.text(`${ page.p6 }${ dataBody.FacultyName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
    report.text(`${ page.p7 }${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(page.p8, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p9[0] } ${ dataBody.CreditComp } ${ page.p9[1] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p10[0] } ${ dataBody.TotalCredit } ${ page.p10[1] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p11 } ${ dataBody.GPA }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p12 } ${ dataBody.GraduatedYear }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.p13, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p14} ${ getDateText(language, issuedDate) }`, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.sansXS)
    let verifyCode = (dataBody.VerifyCode).toString();
    let p15FullText = report.getTextWidth(`${ page.p15[0] } ${ verifyCode } ${ page.p15[1] }`);
    let p15FullTextCenter = p15FullText / 2;
    let p150Width = report.getTextWidth(page.p15[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p15FullTextCenter;
    report.text(page.p15[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(verifyCode, currentX += p150Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansXS)
    report.text(page.p15[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansBoldXS)
    report.text(page.p15[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})