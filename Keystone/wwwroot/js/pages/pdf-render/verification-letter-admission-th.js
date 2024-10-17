var dataModel = $('#verification-letter-preview').data('model');
var universityNameTh = $('body').data('uname-th');
var page1 = {
    p1 : "ที่ ทบ. (ตว.) ",
    p2 : ["88 หมู่ 8 ถนนบางนา-ตราด","ตำบลบางเสาธง อำเภอบางเสาธง", "สมุทรปราการ 10570", "วันที่"],
    p3 : "เรื่อง ขอความอนุเคราะห์ตรวจสอบหลักฐานระเบียนแสดงผลการศึกษา",
    p4 : "เรียน",
    p5 : ["สิ่งที่ส่งมาด้วย", "1.รายชื่อนักศึกษาที่ขอตรวจสอบ", "2.สำเนาระเบียนแสดงผลการศึกษา", "จำนวน", "ฉบับ", "1", "9"],
    p6 : ["                    ด้วยมหาวิทยาลัยอัสสัมชัญรับสมัครนักศึกษาเข้าศึกษาต่อในระดับปริญญาตรี ซึ่ง|มี|นัก|ศึก|ษา|ที่|ได้|ยื่น|วุฒิ|การ|ศึก|ษา|ระบุ|ว่า|จบ|จาก|",
          "จึง|ใคร่|ขอ|ความ|อนุ|เคราะห์|จาก|ท่าน|โปรด|ตรวจ|สอบ|หลัก|ฐาน|ระ|เบียน|แสดง|ผล|การ|ศึก|ษา|ดัง|กล่าว |ว่า|เป็น|เอก|สาร|ที่|ออก|จาก|",
          "ของ|ท่าน|จริง|หรือ|ไม่ |ดัง|เอก|สาร|แนบ |ทั้ง|นี้ |เพื่อ|ประ|กอบ|การ|พิจาร|ณา|ให้|มี|สถาน|ภาพ|เป็น|นัก|ศึก|ษา|ของ|มหา|วิทยา|ลัย|อัส|สัม|ชัญ|ต่อ|ไป"],
    p7 : ["                    จึงเรียนมาเพื่อโปรดพิจารณาและแจ้งผลการตรวจสอบให้ทราบด้วยที่ฝ่ายรับเข้า|ศึก|ษา|และ|ระ|เบียน|ประ|วัติ|สำ|นัก|ทะ|เบียน|และ|ประ|มวล|ผล ",
          "มหา|วิทยา|ลัย|อัส|สัม|ชัญ |จัก|เป็น|พระ|คุณ|ยิ่ง"],
    p8 :  "                    จึงเรียนมาเพื่อโปรดทราบ",
    p9 : ["ขอแสดงความนับถือ", "นายทะเบียนมหาวิทยาลัยอัสสัมชัญ"],
    p10 : ["โทรศัพท์:", "อีเมล์:"]
};

var page2 = {
    p1 : "รายชื่อนักเรียนที่ระบุว่าจบการศึกษาจาก",
    p2 : ["(โปรดทำเครื่องหมาย", "ลงใน O ช่องสถานะจบการศึกษา)"],
    p3 : ["รวมทั้งหมด", "คน"],
    p4 : ["สรุปจำนวนนักเรียนที่สำเร็จจาก", "สรุปจำนวนนักเรียนที่ไม่สำเร็จจาก", "............... คน"],
    p5 : "......................................................................"
};

function drawStudentsTable(letter, currentY, studentData) {
    let studentsTable = []
        currentX = 10,
        indentX = 2.5;

    $(studentData).each( function(index) {
        studentsTable.push([index + 1, this.Code, this.FullName, this.PreviousSchool, 'O จบ    O ไม่จบ']);
    })

    letter.autoTable({
        head: [['ลำดับ', 'รหัสนักศึกษา', 'ชื่อ-สกุล', 'โรงเรียน', 'สถานะจบการศึกษา']],
        body: studentsTable,
        theme: 'grid',
        styles: {
            halign: 'center',
            font: 'sarabun',
            fontSize: 16,
            fillColor: defaultColor.white,
            lineColor: '#000000',
            lineWidth: 0.1,
            cellPadding: {top: 1.25, bottom: 1.25},
        },
        headStyles: {
            fillColor: defaultColor.white,
            textColor: defaultColor.black,
        },
        columnStyles : { 
            0: { cellWidth: 15 },
            1: { cellWidth: 30 },
            2: { halign: 'left' },
            3: { halign: 'left' },
            4: { cellWidth: 45 } 
        },
        margin: {left: currentX},
        startY: currentY + 5,
        tableWidth: defaultSetting.A4MaxWidth,
    });

    return letter.previousAutoTable.finalY;
}

function admissionVerificationLetter(dataModel) {
    var letter = new jsPDF('p', 'mm', 'a4');

    let lineYN = defaultSetting.NormalLine,
        lineYS = defaultSetting.SmallLine,
        padding = defaultSetting.inchMm,
        center = defaultSetting.centerX;

    let letterConfig = {
        align: 'left',
        lineHeightFactor: defaultSetting.lineHeight,
        maxWidth: defaultSetting.A4Width - (padding * 2)
    }

    /* ------------------------------ Page 1 ------------------------------ */
    let currentY = padding / 2;
    let logoSize = 35;
    letter.addImage(PdfSettings.img($('body').data('logo-url')), 'png', center - (logoSize / 2), currentY, logoSize, logoSize);

    let sentAt = getDateText('th', new Date(dataModel.SentAt));
    
    PdfSettings.format(letter, textFormats.sansN);
    currentY += logoSize + lineYN;
    let p1Width = letter.getTextWidth(page1.p1);
    letter.setFontSize(16)
          .text(page1.p1, padding, currentY, letterConfig)
          .text(dataModel.RunningNumber, padding + p1Width, currentY, letterConfig);

    letterConfig.align = 'right';
    let rightest = defaultSetting.A4Width - padding;
    letter.text(universityNameTh, rightest, currentY, letterConfig)
          .text(page1.p2[0], rightest, currentY += lineYS, letterConfig)
          .text(page1.p2[1], rightest, currentY += lineYS, letterConfig)
          .text(page1.p2[2], rightest, currentY += lineYS, letterConfig);

    letterConfig.align = 'left';
    letter.text(`${ page1.p2[3] } ${ sentAt }`, center, currentY += lineYN, letterConfig)
          .text(page1.p3, padding, currentY += lineYN, letterConfig)
          .text(`${ page1.p4 } ${ dataModel.Recipient }`, padding, currentY += lineYN, letterConfig);

    let p5_0Width = letter.getTextWidth(page1.p5[0]) + 2
        p5_2Width = letter.getTextWidth(page1.p5[2]) + 2;
    letter.text(page1.p5[0], padding, currentY += lineYS, letterConfig)
          .text(page1.p5[1], padding + p5_0Width, currentY, letterConfig)
          .text(`${ page1.p5[3] } ${ page1.p5[5] } ${ page1.p5[4] }`, padding + p5_0Width + p5_2Width, currentY, letterConfig)
          .text(page1.p5[2], padding + p5_0Width, currentY += lineYS, letterConfig)
          .text(`${ page1.p5[3] } ${ page1.p5[6] } ${ page1.p5[4] }`, padding + p5_0Width + p5_2Width, currentY, letterConfig);

    currentY += lineYN;
    let fullP6 = `${ page1.p6[0] }|${ dataModel.SchoolName } |${ page1.p6[1] }|${ dataModel.SchoolType }|${ page1.p6[2] }`;
    let splittedP6 = PdfSettings.justifyTh(letter, fullP6, letterConfig.maxWidth);
    splittedP6.forEach( function(item, index, array) {
        letter.text(item, padding, currentY, PdfSettings.style(textStyles.ReportLeft));
        if (index !== array.length - 1) { 
            currentY += lineYS;
        }
    });

    currentY += lineYS;
    let fullP7 = `${ page1.p7[0] } |${ page1.p7[1] }`;
    let splittedP7 = PdfSettings.justifyTh(letter, fullP7, letterConfig.maxWidth);
    splittedP7.forEach( function(item, index, array) {
        letter.text(item, padding, currentY, PdfSettings.style(textStyles.ReportLeft));
        if (index !== array.length - 1){ 
            currentY += lineYS;
        }
    });

    letter.text(page1.p8, padding, currentY += lineYN, letterConfig);

    letterConfig.align = 'center';
    let p9_0width = letter.getTextWidth(page1.p9[0]);
    let signatoryCenter = center + (p9_0width / 2);
    letter.text(page1.p9[0], signatoryCenter, currentY += lineYN, letterConfig)
          .text(`(${ dataModel.Signatory })`, signatoryCenter, currentY += (lineYN * 2), letterConfig)
          .text(page1.p9[1], signatoryCenter, currentY += lineYS, letterConfig);

    letterConfig.align = 'left';
    letter.text(dataModel.OfficerName, padding, currentY += lineYN, letterConfig)
          .text(dataModel.OfficerPosition, padding, currentY += lineYS, letterConfig)
          .text(dataModel.OfficerDivision, padding, currentY += lineYS, letterConfig)
          .text(`${ page1.p10[0] } ${ dataModel.OfficerPhone }`, padding, currentY += lineYS, letterConfig)
          .text(`${ page1.p10[1] } ${ dataModel.OfficerEmail }`, padding, currentY += lineYS, letterConfig)
    
    /* ------------------------------ Page 2 ------------------------------ */
    letter.addPage('a4', 'p');
    currentY = padding;

    let pp1Width = letter.getTextWidth(page2.p1);
    letter.text(page2.p1, padding, currentY, letterConfig)
          .text(dataModel.SchoolName, padding + pp1Width, currentY, letterConfig);

    
    let check = String.fromCharCode(10003)
        pp2_0Width = letter.getTextWidth(page2.p2[0]) + 2
        currentX = padding;
    letter.setFontSize(16)
          .text(page2.p2[0], currentX, currentY += lineYS, letterConfig);

    PdfSettings.format(letter, "unicode");
    letter.text(check, currentX += pp2_0Width, currentY, letterConfig);

    PdfSettings.format(letter, textFormats.sansN);
    letter.setFontSize(16)
          .text(page2.p2[1], currentX += 5, currentY, letterConfig);
    
    // draw student table
    //let circle = String.fromCharCode()
    let endTable = drawStudentsTable(letter, currentY, dataModel.Students);

    currentY = endTable + lineYN;
    letter.text(`${ page2.p3[0] } ${ dataModel.Students.length.toString() } ${ page2.p3[1] }`,
                 padding, currentY, letterConfig);

    letter.text(`${ page2.p4[0] }${ dataModel.SchoolName }     ${ page2.p4[2] }`,
                 padding, currentY += lineYS, letterConfig);

    letter.text(`${ page2.p4[1] }${ dataModel.SchoolName }  ${ page2.p4[2] }`,
                 padding, currentY += lineYS, letterConfig);

    letterConfig.align = 'center';
    letter.text(page2.p5, center, currentY += 25, letterConfig)
          .text(dataModel.Recipient, center, currentY += lineYS, letterConfig)

    $('#js-print-preview').attr('src', letter.output('datauristring'));
}

$(function() {
    admissionVerificationLetter(dataModel);
})