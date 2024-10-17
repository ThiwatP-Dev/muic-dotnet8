var dataModel = $('#js-pdf-preview').data('model');

var page = {
    header : ["วิทยาลัยนานาชาติ มหาวิทยาลัยมหิดล", "MAHIDOL UNIVERSITY INTERNATIONAL COLLEGE",
              "999 ถ.พุทธมณฑล 4 ต.ศาลายา อ.พุทธมณฑล จ.นครปฐม 73170 โทรศัพท์ 0-2700-5000",
              "999 Phutthamonthon 4 Rd., Salaya, Phutthamonthon, Nakhon Pathom 73170, THAILAND Tel.0-2700-5000",
              "ใบเสร็จรับเงิน ต้นฉบับ (Original Receipt)", "ใบเสร็จรับเงิน สำเนา (Copy Receipt)", "เลขที่", "Number", "วันที่", "Date"],
    tableHeader : ["ได้รับเงินจาก", "สถานที่ Issued at :", "Received From :", "ออกโดย By", "เลขที่อ้างอิง", "Reference No. :", "ปีการศึกษา", "Academic Year :", "รหัสนักศึกษา","Student Code :", "วันที่ชำระ", "Paid Date :"],
    tableHeader2 : ["รายการ (Description)", "จำนวน (Quantity)", "จำนวนเงิน (บาท) Amount (THB)", "รวม TOTAL"],
    payment : ["เงินสด", "เช็ค ธนาคาร", "สาขา", "เลขที่เช็ค"],
    footer : ["สำหรับลูกค้า", "ผู้รับเงิน Receiver", "ใบเสร็จรับเงินนี้ จะสมบูรณ์เมื่อมีลายเซ็นผู้รับเงินของวิทยาลัย และเรียกเก็บเงินตามเช็คฉบับนี้ได้เรียบร้อยแล้ว"]
};

function switchFormat(pdf, format) {
    switch (format) {
        case "bold":
            PdfSettings.format(pdf, textFormats.sansBoldXS)
            break;

        case "normal":
            PdfSettings.format(pdf, textFormats.sansXS)
            break;
    }
}

function registrationReceipt(dataModel, report, copyPage = 0) {
    PdfSettings.property(report, dataModel);
    let centerX = defaultSetting.centerX;
        paddingX = defaultSetting.paddingX
        currentY = 10;
        headerLine = 7;
        contentLine = 5;
        maxWidth = defaultSetting.A4MaxWidth - paddingX;
        boldText = "bold";
        simpleText = "normal";
        paragraphCenter = {
            align: 'center',
            lineHeightFactor: 1.25,
        };

    var sizes = [12, 12, 16]
    switchFormat(report, boldText);
    report.addImage($('#js-muic-logo')[0], 'png', 10, 10, 25, 25)
          .setFontSize(sizes[2])
          .text(page.header[0], paddingX + 30, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.header[1], paddingX + 30, currentY += 5, PdfSettings.style(textStyles.CertificateLeft));

    switchFormat(report, simpleText);
    report.setFontSize(sizes[0])
          .text(page.header[2], paddingX + 30, currentY += 5)
          .text(page.header[3], paddingX + 30, currentY += 5);

    switchFormat(report, boldText);
    report.setFontSize(sizes[2])
          .text(page.header[4], centerX, currentY += 8, PdfSettings.style(textStyles.CertificateCenter))
    
    currentY = 10;
    switchFormat(report, simpleText);
    report.setFontSize(sizes[0])
          .text(page.header[6], centerX + 60, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(dataModel.ReceiptNumber, centerX + 95, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[7], centerX + 60, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[8], centerX + 75, currentY += 5, PdfSettings.style(textStyles.CertificateRight))
          .text(dataModel.PrintedAt, centerX + 95, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[9], centerX + 75, currentY += 2, PdfSettings.style(textStyles.CertificateRight));

    report.setFillColor(255, 255, 255)
          .rect(8, 40, 194, 32, 'DF')
          .rect(8, 70, 194, 5, 'DF')
          .rect(8, 75, 194, 32, 'DF')
          .rect(125, 107, 30, 8, 'DF')
          .rect(155, 107, 47, 8, 'DF')
          .line(125, 70, 125, 107)
          .line(155, 40, 155, 107)
          .line(0, 148.5, 210, 148.5);

    currentY = 45;
    report.setFontSize(sizes[1])
          .text(page.tableHeader[0], paddingX + 5, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[8], paddingX + 100, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[9], paddingX + 100, currentY + 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[2], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.FullName, paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.StudentCode, paddingX + 125, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[3], centerX + 55, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.PrintedBy ? dataModel.PrintedBy : "N/A", centerX + 73, currentY + 5, PdfSettings.style(textStyles.CertificateCenter));

    report.text(page.tableHeader[4], paddingX + 5, currentY += 7, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[5], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[6], paddingX + 55, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[7], paddingX + 55, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[10], paddingX + 100, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[11], paddingX + 100, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.InvoiceNumber ? dataModel.InvoiceNumber : "-", paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.Term, paddingX + 80, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.PaidAt, paddingX + 117, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`หลักสูตร (Program) : ${ dataModel.Program }`, paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader2[0], paddingX + 55, currentY += 7, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[1], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[2], centerX + 73, currentY, PdfSettings.style(textStyles.CertificateCenter));

    var currentReceiptLine = 80;
    if (dataModel.ReceiptItemDetails != null) {
        dataModel.ReceiptItemDetails.forEach((item) => {
            report.text(`- ${ item.NameTh } (${ item.NameEn })`, paddingX + 5, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft));
            report.text(item.Amount, centerX + 90, currentReceiptLine, PdfSettings.style(textStyles.CertificateRight));
            currentReceiptLine = currentReceiptLine + 5;
        })
    };

    currentY += 38;
    report.text(`( ${ dataModel.TotalAmountTextTh } )`, paddingX + 55, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[3], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataModel.TotalAmount, centerX + 90, currentY, PdfSettings.style(textStyles.CertificateRight));

    report.setFillColor(255, 255, 255)
          .rect(8, currentY += 7, 5, 5, 'DF')
          .rect(33, currentY, 5, 5, 'DF');

    report.text(page.payment[0], paddingX + 5, currentY += 4, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[1] }......................................................`,
                 paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[2] }.................................................................`,
                 centerX - 5, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[3] }......................................`,
                 centerX + 55, currentY, PdfSettings.style(textStyles.CertificateLeft));

    report.text(page.footer[0], paddingX + 5, currentY += 10, PdfSettings.style(textStyles.CertificateLeft))
          .text(`.................................................`, centerX + 70, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.footer[1], centerX + 70, currentY += 5, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.footer[2], centerX, currentY += 7, paragraphCenter)
    
    /* -------------------------------------------------- half page -------------------------------------------------- */
    switchFormat(report, boldText);
    report.addImage($('#js-muic-logo')[0], 'png', 10, currentY += 10, 25, 25)
          .setFontSize(sizes[2])
          .text(page.header[0], paddingX + 30, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.header[1], paddingX + 30, currentY += 5, PdfSettings.style(textStyles.CertificateLeft));

    switchFormat(report, simpleText);
    report.setFontSize(sizes[0])
          .text(page.header[2], paddingX + 30, currentY += 5)
          .text(page.header[3], paddingX + 30, currentY += 5);

    switchFormat(report, boldText);
    report.setFontSize(sizes[2])
          .text(page.header[5], centerX, currentY += 8, PdfSettings.style(textStyles.CertificateCenter))
    
    currentY -= 28;
    switchFormat(report, simpleText);
    report.setFontSize(sizes[0])
          .text(page.header[6], centerX + 60, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(dataModel.ReceiptNumber, centerX + 95, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[7], centerX + 60, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[8], centerX + 75, currentY += 5, PdfSettings.style(textStyles.CertificateRight))
          .text(dataModel.PrintedAt, centerX + 95, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[9], centerX + 75, currentY += 2, PdfSettings.style(textStyles.CertificateRight));
    
    report.setFillColor(255, 255, 255)
          .rect(8, 185, 194, 32, 'DF')
          .rect(8, 215, 194, 10, 'DF')
          .rect(8, 220, 194, 32, 'DF')
          .rect(125, 252, 30, 8, 'DF')
          .rect(155, 252, 47, 8, 'DF')
          .line(125, 215, 125, 252)
          .line(155, 185, 155, 252);

    currentY += 22;
    report.setFontSize(sizes[1])
          .text(page.tableHeader[0], paddingX + 5, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[8], paddingX + 100, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[9], paddingX + 100, currentY + 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[2], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.FullName, paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.StudentCode, paddingX + 125, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[3], centerX + 55, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.PrintedBy ? dataModel.PrintedBy : "N/A", centerX + 73, currentY + 5, PdfSettings.style(textStyles.CertificateCenter));

    report.text(page.tableHeader[4], paddingX + 5, currentY += 7, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[5], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[6], paddingX + 55, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[7], paddingX + 55, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[10], paddingX + 100, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[11], paddingX + 100, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.InvoiceNumber ? dataModel.InvoiceNumber : "-", paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.Term, paddingX + 80, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.PaidAt, paddingX + 117, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`หลักสูตร (Program) : ${ dataModel.Program }`, paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader2[0], paddingX + 55, currentY += 7, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[1], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[2], centerX + 73, currentY, PdfSettings.style(textStyles.CertificateCenter));

    var currentReceiptLine = currentY + 6;
    if (dataModel.ReceiptItemDetails != null) {
        dataModel.ReceiptItemDetails.forEach((item) => {
            report.text(`- ${ item.NameTh } (${ item.NameEn })`, paddingX + 5, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft));
            report.text(item.Amount, centerX + 90, currentReceiptLine, PdfSettings.style(textStyles.CertificateRight));
            currentReceiptLine = currentReceiptLine + 5;
        })
    };

    currentY += 38;
    report.text(`( ${ dataModel.TotalAmountTextTh } )`, paddingX + 55, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[3], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataModel.TotalAmount, centerX + 90, currentY, PdfSettings.style(textStyles.CertificateRight));

    report.setFillColor(255, 255, 255)
          .rect(8, currentY += 7, 5, 5, 'DF')
          .rect(33, currentY, 5, 5, 'DF');

    report.text(page.payment[0], paddingX + 5, currentY += 4, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[1] }......................................................`,
                 paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[2] }.................................................................`,
                 centerX - 5, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[3] }......................................`,
                 centerX + 55, currentY, PdfSettings.style(textStyles.CertificateLeft));

    report.text(page.footer[0], paddingX + 5, currentY += 10, PdfSettings.style(textStyles.CertificateLeft))
          .text(`.................................................`, centerX + 70, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.footer[1], centerX + 70, currentY += 5, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.footer[2], centerX, currentY += 7, paragraphCenter)

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

function otherRegistrationReceipt(dataModel, report, copyPage = 0) {
    PdfSettings.property(report, dataModel);
    let centerX = defaultSetting.centerX;
        paddingX = defaultSetting.paddingX
        currentY = 10;
        headerLine = 7;
        contentLine = 5;
        maxWidth = defaultSetting.A4MaxWidth - paddingX;
        boldText = "bold";
        simpleText = "normal";
        paragraphCenter = {
            align: 'center',
            lineHeightFactor: 1.5,
        };

    var sizes = [12, 14, 18]
    switchFormat(report, boldText);
    report.addImage($('#js-muic-logo')[0], 'png', 10, 10, 25, 25)
          .setFontSize(sizes[2])
          .text(page.header[0], paddingX + 30, currentY += 7, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.header[1], paddingX + 30, currentY += 7, PdfSettings.style(textStyles.CertificateLeft));

    switchFormat(report, simpleText);
    report.setFontSize(sizes[0])
          .text(page.header[2], paddingX + 30, currentY += 5)
          .text(page.header[3], paddingX + 30, currentY += 5);

    switchFormat(report, boldText);
	if (dataModel.IsPrinted || copyPage > 0) {
        report.setFontSize(sizes[2])
			  .text(page.header[5], centerX, currentY += 8, PdfSettings.style(textStyles.CertificateCenter))
	} else {
		report.setFontSize(sizes[2])
			  .text(page.header[4], centerX, currentY += 8, PdfSettings.style(textStyles.CertificateCenter))
	}
    
    currentY = 10;
    switchFormat(report, simpleText);
    report.setFontSize(sizes[0])
          .text(page.header[6], centerX + 60, currentY, PdfSettings.style(textStyles.CertificateRight))
          .text(dataModel.ReceiptNumber, centerX + 95, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[7], centerX + 60, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[8], centerX + 75, currentY += 5, PdfSettings.style(textStyles.CertificateRight))
          .text(dataModel.PrintedAt, centerX + 95, currentY += 2, PdfSettings.style(textStyles.CertificateRight))
          .text(page.header[9], centerX + 75, currentY += 2, PdfSettings.style(textStyles.CertificateRight));

    report.setFillColor(255, 255, 255)
          .rect(8, 45, 194, 25, 'DF')
          .rect(8, 70, 194, 15, 'DF')
          .rect(8, 85, 194, 70, 'DF')
          .rect(125, 155, 30, 10, 'DF')
          .rect(155, 155, 47, 10, 'DF')
          .line(125, 70, 125, 155)
          .line(155, 45, 155, 155);

    currentY = 50;
    report.setFontSize(sizes[1])
          .text(page.tableHeader[0], paddingX + 5, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[8], paddingX + 100, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[9], paddingX + 100, currentY + 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[2], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.FullName, paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.StudentCode, paddingX + 125, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[3], centerX + 55, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.PrintedBy ? dataModel.PrintedBy : "N/A", centerX + 73, currentY + 5, PdfSettings.style(textStyles.CertificateCenter));

    report.text(page.tableHeader[4], paddingX + 5, currentY += 7, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[5], paddingX + 5, currentY += 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[6], paddingX + 55, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[7], paddingX + 55, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[10], paddingX + 100, currentY - 5, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader[11], paddingX + 100, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.InvoiceNumber ? dataModel.InvoiceNumber : "-", paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.Term, paddingX + 80, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(dataModel.PaidAt, paddingX + 117, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(page.tableHeader2[0], paddingX + 55, currentY += 9, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[1], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[2], centerX + 73, currentY, PdfSettings.style(textStyles.CertificateCenter));

    var currentReceiptLine = 90;
    if (dataModel.ReceiptItemDetails != null) {
        dataModel.ReceiptItemDetails.forEach((item) => {
            report.text(`- ${ item.NameTh } (${ item.NameEn })`, paddingX + 5, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft));
            report.text(item.Amount, centerX + 90, currentReceiptLine, PdfSettings.style(textStyles.CertificateRight));
            currentReceiptLine = currentReceiptLine + 5;
        })
    };

    currentY = 161;
    report.text(`( ${ dataModel.TotalAmountTextTh } )`, paddingX + 55, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.tableHeader2[3], centerX + 35, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(dataModel.TotalAmount, centerX + 90, currentY, PdfSettings.style(textStyles.CertificateRight));

    report.setFillColor(255, 255, 255)
          .rect(8, 172, 5, 5, 'DF')
          .rect(33, 172, 5, 5, 'DF');

    report.text(page.payment[0], paddingX + 5, currentY += 15, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[1] }......................................................`,
                 paddingX + 30, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[2] }.................................................................`,
                 centerX - 5, currentY, PdfSettings.style(textStyles.CertificateLeft))
          .text(`${ page.payment[3] }......................................`,
                 centerX + 55, currentY, PdfSettings.style(textStyles.CertificateLeft));

    report.text(page.footer[0], paddingX + 5, currentY += 15, PdfSettings.style(textStyles.CertificateLeft))
          .text(`.................................................`, centerX + 70, currentY, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.footer[1], centerX + 70, currentY += 5, PdfSettings.style(textStyles.CertificateCenter))
          .text(page.footer[2], centerX, currentY += 10, paragraphCenter)

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(document).ready( function() {
    let data = dataModel.Body;
    var report = new jsPDF('p', 'mm', 'a4');
    if (data.InvoiceType == 'o') {
        if (data.IsPrinted) {
            otherRegistrationReceipt(data, report);
        } else {
            for (var i = 0; i < 3; i++) {
                otherRegistrationReceipt(data, report, i);
                report.addPage('a4', 'p');
            }
        }
    } else {
        registrationReceipt(data, report);
    }
});