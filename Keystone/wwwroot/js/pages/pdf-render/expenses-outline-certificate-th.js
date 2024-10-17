var dataModel = $('#certificate-preview').data('model');

var page = {
    p1 : "Reg. C. ",
    p2 : "หนังสือรับรอง",
    p3 : ["              ขอรับรองว่า ", "รหัส|ประ|จำ|ตัว|นัก|ศึก|ษา", "เป็น|นัก|ศึก|ษา|ชั้น|ปี|ที่", "ปี|การ|ศึก|ษา", "ของ|มหา|วิทยา|ลัย|อัส|สัม|ชัญ"],
    p4 : ["              ใน|ภาค|การ|ศึก|ษา|ที่", "ปี|การ|ศึก|ษา", "นัก|ศึก|ษา|ดัง|กล่าว|ได้|ลง|ทะ|เบียน โดย|ชำระ|ค่า|ใช้|จ่าย|ตาม|ใบ|เสร็จ|เลข|ที่", "ลง|วัน|ที่", "ดัง|ต่อ|ไป|นี้"],
    p5 : ["รวมทั้งสิ้น", "บาท"],
    p6 : "ให้ไว้ ณ วันที่"
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
    
    var dataBody = dataModel.Body;
    var language = dataBody.Language;
    var issuedDate = new Date(dataModel.Body.CreatedAt);

    var studentFullName = `${ dataBody.Title } ${ dataBody.StudentFirstName } ${ dataBody.StudentLastName }`;

    /* -------------------------------------------------- page 1 --------------------------------------------------*/
    PdfSettings.format(report, textFormats.sansN)
    let p1Width = report.getTextWidth(page.p1);
    report.text(page.p1, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataBody.ReferenceNumber, paddingX + p1Width, currentY, PdfSettings.style(textStyles.CertificateLeft))

    PdfSettings.format(report, textFormats.sansBoldHeader)
    report.text(page.p2, centerX, currentY += lineYL, PdfSettings.style(textStyles.CertificateCenter));

    currentY += lineYL;
    PdfSettings.format(report, textFormats.sansN)
    let p3 = `${ page.p3[0] } ${ studentFullName } ${ page.p3[1]} ${ GetStudentCodeFormat(dataBody.StudentCode) } ${ page.p3[2] } ${ dataBody.StudyYear } ${ page.p3[3]} ${ dataBody.Year } ${ dataBody.DegreeName } ${ page.p3[4] }`;
    let splittedP3 = PdfSettings.justifyTh(report, p3, maxWidth);
    splittedP3.forEach( function(item, index, array) {
        report.text(item, paddingX, currentY, PdfSettings.style(textStyles.ReportLeft));
        if (index !== array.length - 1) { 
            currentY += lineYS;
        }
    });

    currentY += lineYN;
    PdfSettings.format(report, textFormats.sansN)
    let termSplitted = dataBody.TermText.split('/');
    let p4 = `${ page.p4[0] } ${ termSplitted[0] } ${ page.p4[1] } ${ (parseInt(termSplitted[1]) + 543) } ${ page.p4[2] } ${ dataBody.ReceiptNumber } ${ page.p4[3]} ${ getDateText(language, new Date(dataBody.PaidAt)) } ${ page.p4[4] }`;
    let splittedP4 = PdfSettings.justifyTh(report, p4, maxWidth);
    splittedP4.forEach( function(item, index, array) {
        report.text(item, paddingX, currentY, PdfSettings.style(textStyles.ReportLeft));
        if (index !== array.length - 1) { 
            currentY += lineYS;
        }
    });

    if (dataBody.Receipt.ReceiptItems != undefined && dataBody.Receipt.ReceiptItems.length > 0) {
        PdfSettings.format(report, textFormats.sansN)
        var index = 1;
        var subTotalReceiptAmountWidth = 0;
        $(dataBody.Receipt.ReceiptItems).each( function() {
            currentY += lineYN;
            report.text(`              ${ index }. ${ this.FeeItemName }`, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
                  .text(`${ this.TotalAmountText } ${ page.p5[1]}`, (207 - paddingX), currentY, PdfSettings.style(textStyles.CertificateRight));
            index++;
            subTotalReceiptAmountWidth = report.getTextWidth(this.TotalAmountText);
        })
        
        report.line(((207 - paddingX) - 7.5) - subTotalReceiptAmountWidth, currentY + 1, (207 - paddingX) - 7.5, currentY + 1);
        
        PdfSettings.format(report, textFormats.sansBoldN)
        currentY += lineYN;
        var totalReceiptAmountWidth = report.getTextWidth(dataBody.Receipt.TotalAmountText);
        report.text(`                  ${ page.p5[0] } (${ ArabicNumberToText(dataBody.Receipt.TotalAmount) })`, paddingX, currentY, PdfSettings.style(textStyles.CertificateLeft))
              .text(`${ dataBody.Receipt.TotalAmountText } ${ page.p5[1]}`, (207 - paddingX), currentY, PdfSettings.style(textStyles.CertificateRight));

        report.line(((207 - paddingX) - 7.5) - totalReceiptAmountWidth, currentY + 1, (207 - paddingX) - 7.5, currentY + 1);
    }

    PdfSettings.format(report, textFormats.sansN)
    report.text(`${ page.p6 } ${ getDateText(language, issuedDate) }`, paddingX + p1Width, currentY += lineYL, PdfSettings.style(textStyles.CertificateLeft));

    PdfSettings.format(report, textFormats.sansN)
    report.text(`( ${ dataBody.ApprovedByName } )`, centerX, currentY += 40, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataBody.Position, centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter))
          .text(getDateText(language, issuedDate), centerX, currentY += lineYS, PdfSettings.style(textStyles.CertificateCenter));

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    renderCertificate();
})