const defaultTextSize = {
    headerLarge : 35, // should use with bold font
    title : 28,
    header : 25, // should use with bold font
    bodyLarge : 20,
    body : 16,
    bodySmall : 14,
    bodyExSmall : 11,
    bodyInfo : 6.5
}

const defaultColor = {
    black : '#000000',
    gray : '#999999',
    silver : '#C0C0C0',
    lightGray : '#D3D3D3',
    white : '#FFFFFF'
}

const defaultSetting = {
    paddingX : 10,
    paddingY : 10,
    paddingC : 38,
    centerX : 105,
    lineHeightSmallTh : 0.9,
    lineHeightSmall : 1,
    lineHeight : 1.25,
    lineHeightLarge : 1.5,
    A4Width : 210,
    A4Height : 297,
    A4MaxWidth : 190,
    inchMm : 25.4,
    CertificateMaxWidth : 134,
    NormalLine : 11,
    LargeLine : 18,
    SmallLine : 8
}

const textFormats = {
    serifTitleHeader : "serifTitleHeader",
    serifTitle : "serifTitle",
    serifHeader : "serifHeader",
    serifBoldHeader : "serifHeaderBold",
    serifN : "serifContent",
    serifBoldN : "serifContentBold",
    serifL : "serifContentLarge",
    serifBoldL : "serifContentLargeBold",
    serifS : "serifContentSmall",
    serifBoldS : "serifContentSmallBold",
    serifXS : "serifContentExtraSmall",
    serifBoldXS : "serifContentExtraSmallBold",
    sansHeader : "sansHeader",
    sansBoldHeader : "sansHeaderBold",
    sansBoldHeaderEn : "sansHeaderEnBold",
    sansN : "sansContent",
    sansBoldN : "sansContentBold",
    sansS : "sansContentSmall",
    sansBoldS : "sansContentSmallBold",
    sansXS : "sansContentExtraSmall",
    sansBoldXS : "sansContentExtraSmallBold",
    sansXSEn : "sansContentExtraSmallEn",
    sansBoldXSEn : "sansContentExtraSmallEnBold",
}

const textStyles = {
    CertificateLeft : "CertificateLeft",
    CertificateRight : "CertificateRight",
    CertificateCenter : "CertificateCenter",
    ReportLeft : "ReportLeft",
    ReportCenter : "ReportCenter",
    ReportRight : "ReportRight",
    SignatureLeft : "SignatureLeft",
    SignatureCenter : "SignatureCenter",
}

var PdfSettings = (function() {

    var FileProperties = function(pdf, dataModel) {
        pdf.setProperties({
            title: dataModel.Title,
            subject: dataModel.Subject,
            author: dataModel.Author,
            creator: dataModel.Creator
        });
    }

    var LoadImage = function(imagePath) {
        var img = new Image();
        img.src = imagePath;
        return img;
    }

    var TextProperties = function(pdf, template) {
        // Font Color
        pdf.setTextColor(defaultColor.black);

        // Font Type
        if (template.includes("serif")) {
            if (template.includes("Bold")) {
                pdf.setFont('angsana', 'bold');
            } else {
                pdf.setFont('angsana', 'normal');
            }

        } else if (template.includes("sans") && !template.includes("En")) {
            if (template.includes("Bold")) {
                pdf.setFont('sarabun', 'bold');
            } else {
                pdf.setFont('sarabun', 'normal');
            }

        } else if (template.includes("sans") && template.includes("En")) {
            if (template.includes("Bold")) {
                pdf.setFont('arial', 'bold');
            } else {
                pdf.setFont('arial', 'normal');
            }
        } else {
            pdf.setFont('arial-unicode', 'normal')
        }

        // Font Size
        if (template.includes("TitleHeader")){
            pdf.setFontSize(defaultTextSize.headerLarge)

        } else if (template.includes("Header")) {
            pdf.setFontSize(defaultTextSize.header)

        } else if (template.includes("Title")){
            pdf.setFontSize(defaultTextSize.title)

        } else if (template.includes("ContentExtraSmallEn")){
            pdf.setFontSize(defaultTextSize.bodyInfo)

        } else if (template.includes("ContentExtraSmall")){
            pdf.setFontSize(defaultTextSize.bodyExSmall)
            
        } else if (template.includes("ContentSmall")){
            pdf.setFontSize(defaultTextSize.bodySmall)
            
        } else if (template.includes("ContentLarge")){
            pdf.setFontSize(defaultTextSize.bodyLarge)
            
        } else if (template.includes("Content")){
            pdf.setFontSize(defaultTextSize.body)
        }
    }

    var TextOptions = function(config) { // options for .text() function. use after define position.
        var configObj;

        switch(config) {
            case textStyles.CertificateLeft:
                configObj = {
                    align: 'left',
                    lineHeightFactor: defaultSetting.lineHeightSmall,
                    maxWidth: defaultSetting.CertificateMaxWidth
                }
                break;
            
            case textStyles.CertificateRight:
                configObj = {
                    align: 'right',
                    lineHeightFactor: defaultSetting.lineHeightSmall,
                    maxWidth: defaultSetting.CertificateMaxWidth
                }
                break;
    
            case textStyles.CertificateCenter:
                configObj = {
                    align: 'center',
                    lineHeightFactor: defaultSetting.lineHeightSmall,
                    maxWidth: defaultSetting.CertificateMaxWidth
                }
                break;

            case textStyles.ReportLeft:
                configObj = {
                    align: 'left',
                    lineHeightFactor: defaultSetting.lineHeight,
                    maxWidth: defaultSetting.A4MaxWidth
                }
                break;
    
            case textStyles.ReportCenter:
                configObj = {
                    align: 'center',
                    lineHeightFactor: defaultSetting.lineHeight,
                    maxWidth: defaultSetting.A4MaxWidth
                }
                break;
    
            case textStyles.ReportRight:
                configObj = {
                    align: 'right',
                    lineHeightFactor: defaultSetting.lineHeight,
                    maxWidth: defaultSetting.A4MaxWidth
                }
                break;

            case textStyles.SignatureLeft:
                configObj = {
                    align: 'left',
                    lineHeightFactor: defaultSetting.lineHeightLarge,
                }
                break;

            case textStyles.SignatureCenter:
                configObj = {
                    align: 'center',
                    lineHeightFactor: defaultSetting.lineHeightLarge,
                }
                break;
        }
        return configObj;
    }

    var JustifyTextTh = function(pdf, fullText, maximumWidth) {
        let splittedText = [];
        let temporaryLine = "";
        let lineCount = 0;

        fullText.split("").forEach( function(item, index, array) {
            if (item !== "|") {
                temporaryLine += item;
            }
            let currentWidth = pdf.getTextWidth(temporaryLine);

            if (currentWidth > maximumWidth - 3) {
                if (item === "|" || currentWidth > maximumWidth + 3) {
                    splittedText[lineCount] = temporaryLine;
                    temporaryLine = "";
                    lineCount += 1;
                }
            } else if (index === array.length - 1){ 
                splittedText[lineCount] = temporaryLine;
            }
        });
        
        return splittedText;
    }

    var JustifyTextEn = function (pdf, pageSizeSplit, currentY, paddingX, lineSpacing, textFormat, fontType) {
        let defaultWordSpace = 1;
        paddingX = paddingX ? paddingX : defaultSetting.paddingC;
        let maxWidth = defaultSetting.A4Width - (paddingX * 2);
        let currentTextFormat = textFormat ? textFormat : PdfSettings.style(textStyles.CertificateLeft);
    
        for (var line in pageSizeSplit) {
            let currentX = paddingX ? paddingX : defaultSetting.paddingC;

            if (lineSpacing) {
                currentY += lineSpacing;
            } else {
                currentY += defaultSetting.SmallLine;
            }
    
            let text = pdf.getTextWidth(pageSizeSplit[line]);

            if (text <= maxWidth) {
                pdf.text(pageSizeSplit[line], paddingX, currentY, currentTextFormat);
            } else {
                let pageSpaceSplit = pageSizeSplit[line].split(' ');
                let addSpace = Math.max((maxWidth - text) / (pageSpaceSplit.length), 0);
        
                if (line == pageSizeSplit.length - 1) {
                    pdf.text(pageSizeSplit[line], paddingX, currentY, currentTextFormat);
                } else {
                    for (var word in pageSpaceSplit) {
                        if (pageSpaceSplit[word] === "") {
                            currentX += 1;
                        } else {
                            if (pageSpaceSplit[word].startsWith('\\')) {
                                fontType ? pdf.setFont(fontType, 'bold') : PdfSettings.format(pdf, textFormats.serifBoldN);
                                pdf.text(pageSpaceSplit[word].substring(1), currentX, currentY, currentTextFormat);
                            } else {
                                fontType ? pdf.setFont(fontType, 'normal') : PdfSettings.format(pdf,textFormats.serifN);
                                pdf.text(pageSpaceSplit[word], currentX, currentY, currentTextFormat);
                            }
                            currentX += (pdf.getTextWidth(pageSpaceSplit[word]) + defaultWordSpace + addSpace); 
                        }
                    }
                }
            }
            
        }
    
        return currentY;
    }

    return {
        property : FileProperties,
        img : LoadImage,
        format : TextProperties,
        style : TextOptions,
        justifyTh : JustifyTextTh,
        justifyEn : JustifyTextEn
    };

})();