var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "ที่ ทบ. ร. ",
    p2 : "ใบรับรองสถานภาพการศึกษา",
    p3 : "หนังสือรับรองฉบับนี้ออกให้เพื่อแสดงว่า",
    p4 : "รหัสประจำตัว",
    p5 : "เป็นนักศึกษาระดับ",
    p6 : "คณะ",
    p7 : "สาขาวิชา",
    p8 : "ของมหาวิทยาลัยอัสสัมชัญ",
    p9 : ["จำนวนหน่วยกิจที่สอบไล่ได้", "หน่วยกิจ"],
    p10 : ["จากจำนวนหน่วยกิจทั้งหมดของหลักสูตร", "หน่วยกิจ"],
    p11 : "โดยได้คะแนนเฉลี่ยสะสม",
    p12 : "และคาดว่าจะศึกษาครบตามหลักสูตร ในปี พ.ศ. ",
    p13 : "ขณะนี้นักศึกษาดังกล่าว กำลังอยู่ในระหว่างการรอผลวิชา",
    p14 : "เพื่อให้ครบตามหลักสูตร",
    p15 : "ให้ไว้ ณ วันที่ ",
    p16 : ["สามารถตรวจสอบได้ว่าข้อความในเอกสารฉบับนี้ได้ออกจากมหาวิทยาลัยจริงโดยใช้รหัส", "ตรวจสอบได้จาก",
           "www.registrar.au.edu/certificationcheck.html"]
}

function renderCertificate(dataModel) {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let paddingX = defaultSetting.paddingC,
        currentY = defaultSetting.paddingC,
        centerX = defaultSetting.centerX,
        lineYL = defaultSetting.LargeLine,
        lineYN = defaultSetting.NormalLine,
        lineYS = defaultSetting.SmallLine,
        a4Width = defaultSetting.A4Width,
        fromBottom = defaultSetting.A4Height - (defaultSetting.paddingC / 2);

    var informationConfig = {
        align: 'center',
        lineHeightFactor: defaultSetting.lineHeightLarge,
        maxWidth: a4Width - (paddingX * 2)
    }

    var dataBody = dataModel.Body;
    var fullName = `${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`;
    var gpaText = dataModel.Body.GPA == -1 ? 'N/A' : NumberFormat.renderDecimalTwoDigits(dataModel.Body.GPA);

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.sansN);
    report.text(`${ page.p1 }${ dataBody.ReferenceNumber }`, paddingX, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansBoldHeader);
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN);
    report.text(page.p3, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN);
    report.text(fullName, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));

    let formatStudentCode = GetStudentCodeFormat(dataBody.StudentCode);
    let createdAt = getDateText(dataBody.Language, new Date(dataModel.Body.CreatedAt));

    PdfSettings.format(report, textFormats.sansBoldN);
    report.text(`${ page.p4 } ${ formatStudentCode }`, centerX, currentY += lineYN, PdfSettings.style(textStyles.CertificateCenter));
    
    PdfSettings.format(report, textFormats.sansN);
    report.text(`${ page.p5 }${ dataBody.AcademicLevelName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansBoldN);
    report.text(`${ page.p6 }${ dataBody.FacultyName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p7 }${ dataBody.DepartmentName }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN);
    report.text(page.p8, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p9[0] } ${ dataBody.CreditEarned } ${ page.p9[1] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p10[0] } ${ dataBody.TotalCredit } ${ page.p10[1] }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p11 } ${ gpaText }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p12 } ${ dataBody.Year }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN);
    let page13 = report.getTextWidth(`${ page.p13 } ${ dataBody.CourseCodeAndNames }`)
    let availableSpace = a4Width - (paddingX + page13 + paddingX);
    let page13Ending = `${ page.p13 } ${ dataBody.CourseCodeAndNames.join(", ") } `
    let p13EndingLines = report.splitTextToSize(page13Ending, availableSpace);
    let newP13Ending = p13EndingLines.slice(1).join(',');
    let newP13EndingLength = report.splitTextToSize(newP13Ending, a4Width).length;
    report.text(p13EndingLines[0], paddingX += page13, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(page13Ending, centerX, currentY, informationConfig);

    report.text(page.p14, centerX, currentY += lineYS * newP13EndingLength, PdfSettings.style(textStyles.CertificateCenter))
          .text(`${ page.p15 } ${ createdAt }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 30, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    currentY = fromBottom;
    PdfSettings.format(report, textFormats.sansXS);
    let verifyCode = (dataBody.VerifyCode).toString();
    let p16FullText = report.getTextWidth(`${ page.p16[0] } ${ verifyCode } ${ page.p16[1] }`);
    let p16FullTextCenter = p16FullText / 2;
    let p160Width = report.getTextWidth(page.p16[0]);
    let verifyCodeWidth = report.getTextWidth(verifyCode);
    let currentX = centerX - p16FullTextCenter;
    
    report.text(page.p16[0], currentX, currentY -= lineYS, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansBoldXS);
    report.text(verifyCode, currentX += p160Width + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    PdfSettings.format(report, textFormats.sansXS);
    report.text(page.p16[1], currentX += verifyCodeWidth + 1, currentY, PdfSettings.style(textStyles.CertificateLeft));
    
    PdfSettings.format(report, textFormats.sansBoldXS);
    report.text(page.p16[2], centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate(dataModel);
})