var dataModel = $('#id-card-preview').data('model');
var text = {
    header : ["Temporary Examination Card", "Request Form",],
    title : ["Name : ", "Student ID : ", "Subject : ", `${dataModel.Body.ExaminationTypeName} Date : `, "Student's Mobile Phone : "],
    condition : [
                    "Procedures : ",
                    "[1] Complete this form along with ",
                    "500 baht fee ",
                    "and submit to The Registrar Unit.",
                    "[2] A temporary examination card will be issued at The Registrar Unit.",
                    "[3] Students can pick up an official receipt 2 days after a temporary examination ",
                    "card is issued."
                ],
    footer : ["Student's Signature ", "Date "],
    officer : ["Official Receipt No. ", "Signature ", "Financial Officer", "Date ", "Staff's Signature"],
};

function drawDotLine(size) {
    let dot = '.';
    for (let count = 1; count < size; count++) { dot += '.' }
    return dot;
}

function substituteIdCard(dataModel) {
    let data = dataModel.Body;

    var card = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(card, dataModel);

    let lineYN = defaultSetting.NormalLine,
        lineYS = defaultSetting.SmallLine,
        margin = 20,
        padding = 10
        centerX = defaultSetting.centerX;

    //draw logo
    let logoSize = [82, 28];
    card.addImage($('#js-default-logo')[0], 'png', margin, margin, logoSize[0], logoSize[1]);
    
    //write main information
    let currentY = margin + logoSize[1] + padding;
    
    let pIndent1 = margin + padding;
    PdfSettings.format(card, textFormats.sansBoldN);
    card.text(text.title[0], pIndent1, currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(text.title[1], centerX + pIndent1, currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(text.title[2], pIndent1, currentY += lineYS, PdfSettings.style(textStyles.ReportLeft))
        .text(text.title[3], pIndent1, currentY += lineYS, PdfSettings.style(textStyles.ReportLeft))
        .text(text.title[4], pIndent1, currentY += lineYS, PdfSettings.style(textStyles.ReportLeft))
        .text(text.condition[0], pIndent1, currentY += lineYN, PdfSettings.style(textStyles.ReportLeft));

    PdfSettings.format(card, textFormats.sansN);
    currentY = margin + logoSize[1] + padding;
    pIndent1 += 2;
    card.text(`${ data.Title } ${ data.FirstName } ${ data.LastName }`,
              pIndent1 + card.getTextWidth(text.title[0]), currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(data.Code, centerX + pIndent1 + card.getTextWidth(text.title[1]), currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(`${ data.CourseCode }: ${ data.CourseName }: Section ${ data.SectionNumber }`,
              pIndent1 + card.getTextWidth(text.title[2]), currentY += lineYS, PdfSettings.style(textStyles.ReportLeft))
        .text(data.ExamDateAndTime, pIndent1 + card.getTextWidth(text.title[3]), currentY += lineYS, PdfSettings.style(textStyles.ReportLeft))
        .text(drawDotLine(50), pIndent1 + card.getTextWidth(text.title[4]), currentY += lineYS, PdfSettings.style(textStyles.ReportLeft));

    let pIndent2 = pIndent1 + card.getTextWidth(text.condition[0]);
    card.text(text.condition[1], pIndent2, currentY += lineYN, PdfSettings.style(textStyles.ReportLeft))
        .text(text.condition[3], pIndent2 + card.getTextWidth(text.condition[1]) + card.getTextWidth(text.condition[2]) + 2,
              currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(text.condition[4], pIndent2, currentY + lineYS, PdfSettings.style(textStyles.ReportLeft))
        .text(text.condition[5], pIndent2, currentY + (lineYS * 2), PdfSettings.style(textStyles.ReportLeft))
        .text(text.condition[6], pIndent2, currentY + (lineYS * 3), PdfSettings.style(textStyles.ReportLeft));

    PdfSettings.format(card, textFormats.sansBoldN);
    card.text(text.condition[2], pIndent2 + card.getTextWidth(text.condition[1]) - 4, currentY, PdfSettings.style(textStyles.ReportLeft));

    //draw officer box
    card.rect(margin, currentY += (lineYS * 3) + lineYN, 90, 30);

    //write footer
    card.text(`${ text.footer[0] } ${ drawDotLine(35) }`, centerX + padding, currentY += lineYS, PdfSettings.style(textStyles.ReportLeft))
        .text(`${ text.footer[1] } ${ drawDotLine(62) }`, centerX + padding, currentY + lineYN, PdfSettings.style(textStyles.ReportLeft))

    //write officer footer
    PdfSettings.format(card, textFormats.sansN);
    card.text(`${ text.officer[0] } ${ drawDotLine(20) }`, margin + 5, currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(`${ text.officer[1] } ${ drawDotLine(36) }  ${ text.officer[2] }`, margin + 5, currentY += lineYS, PdfSettings.style(textStyles.ReportLeft))
        .text(`${ text.officer[3] } ${ drawDotLine(47) }`, margin + 5, currentY += lineYS, PdfSettings.style(textStyles.ReportLeft))

    //draw separate line
    currentY = 170;
    card.text(drawDotLine(190), centerX, currentY, PdfSettings.style(textStyles.ReportCenter));

    //write annotation header
    PdfSettings.format(card, textFormats.sansS);
    let currentDateTime = data.DateIssued.split('T');
    card.text(data.DateIssuedString, pIndent1, currentY += lineYN, PdfSettings.style(textStyles.ReportCenter))
        .text(text.header[0], centerX, currentY, PdfSettings.style(textStyles.ReportCenter))
        .text(`Printed Time: ${ currentDateTime[0] } ${ currentDateTime[1].substring(0,8) }`, centerX, currentY += lineYS, PdfSettings.style(textStyles.ReportCenter));

    //write substitue id card
    PdfSettings.format(card, textFormats.sansBoldHeader);
    card.text("MAHIDOL UNIVERSITY", centerX, currentY += lineYN, PdfSettings.style(textStyles.ReportCenter))
    
    PdfSettings.format(card, textFormats.sansBoldN);
    card.text(text.header[0], centerX, currentY += lineYS, PdfSettings.style(textStyles.ReportCenter))

    //draw student's picture
    var imgHeight = $('#js-default-student')[0].height;
    var imageWidth = $('#js-default-student')[0].width;
    var imgRatio = imgHeight / imageWidth
    var imgRatioWidth = 25 * (1 / imgRatio)
    card.addImage($('#js-default-student')[0], 'png', margin * 2, currentY + padding + 3, imgRatioWidth, 25); // will use picture from model later

    currentY = currentY + padding;
    //write student's info
    let pIndent3 = (margin * 2) + imgRatioWidth + padding;
    let pIndent4 = pIndent3 + card.getTextWidth(text.title[3]);
    PdfSettings.format(card, textFormats.sansBoldN);
    var subject = `${ data.CourseCode }: ${ data.CourseName }: Section ${ data.SectionNumber }`;
    var splitSubject = card.splitTextToSize(subject, 90);
    card.text(text.title[0], pIndent3, currentY += (lineYN / 2), PdfSettings.style(textStyles.ReportLeft))
        .text(`${ data.Title } ${ data.FirstName } ${ data.LastName }`, pIndent4, currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(text.title[1], pIndent3, currentY += (lineYN / 2), PdfSettings.style(textStyles.ReportLeft))
        .text(data.Code, pIndent4, currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(text.title[2], pIndent3, currentY += (lineYN / 2), PdfSettings.style(textStyles.ReportLeft))
    splitSubject.forEach((item) => {
        card.text(item, pIndent4, currentY, PdfSettings.style(textStyles.ReportLeft))
        currentY += (lineYN / 2)
    });
    card.text(text.title[3], pIndent3, currentY, PdfSettings.style(textStyles.ReportLeft))
        .text(data.ExamDateAndTime, pIndent4, currentY, PdfSettings.style(textStyles.ReportLeft));

    //draw signage area
    currentY += 30;
    card.setDrawColor(defaultColor.black)
        .setLineWidth(0.25)
        .line(margin * 2, currentY, (margin * 2) + 50, currentY)
        .line(centerX + margin, currentY, centerX + margin + 50, currentY)
        .text(text.footer[0], (margin * 2) + 25, currentY += 5, PdfSettings.style(textStyles.ReportCenter))
        .text(text.officer[4], centerX + margin + 25, currentY, PdfSettings.style(textStyles.ReportCenter));
   
    $('#js-print-preview').attr('src', card.output('datauristring'));
}

$(function() {
    substituteIdCard(dataModel);
})