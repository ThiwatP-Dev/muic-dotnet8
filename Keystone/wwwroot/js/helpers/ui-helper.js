var getSaveStepsButton = function () {
    return `<li>
                <a href="#!" id="js-submit-form" data-index="0">
                    <i class="la la-save"></i> Save
                </a>
            </li>`
}

var getMessageBox = function (message) {
    return $($.parseHTML(`<div class="js-message-box message-box"><span>${ message }</span></div>`));
};

var getSelectOptions = function (value, text, prop) {
    if (typeof prop == "undefined" || prop === false) {
        return `<option value="${ value }">${ text }</option>`;
    } else {
        return `<option value="${ value }" selected>${ text }</option>`;
    }
};

var getDefaultSelectOption = function (currentSelect, value, text) {
    let defaultOption = "";

    if (value === undefined) {
        value = "";
    }

    if (text === undefined) {
        text = "Select";
    }

    if (currentSelect.find('option[disabled]').length) {
        defaultOption = `<option selected disabled value="${ value }">${ text }</option>`;
    } else {
        defaultOption = `<option selected value="${ value }">${ text }</option>`;
    }

    currentSelect.empty();
    return defaultOption;
}

var getRandomRGBColor = function (opacity, maxColorValue) {
    let red = Math.floor(Math.random() * maxColorValue)
    let green = Math.floor(Math.random() * maxColorValue)
    let blue = Math.floor(Math.random() * maxColorValue)
    
    return `rgba(${ red }, ${ green }, ${ blue }, ${ opacity })`
}

var getLevelColor = function (rangeIndex, opacity) {
    let color = []

    for (i = 0; i <= 255; i += 24) {
        if (i > 255) {
            i = 255;
        }
        color.push(`rgba(${ 50 + i }, ${ 255 - i }, ${ 0 + i }, 1)`);
    }

    return color[rangeIndex].replace('1)', `${ opacity })`)
}

var getGradeColor = function (grade, opacity) {
    let color = "";

    if (grade === "A" || grade === "S") {
        color = `rgba(69, 170, 59, ${ opacity })`;
    } else if (grade === "A-") {
        color = `rgba(95, 190, 62, ${ opacity })`;
    } else if (grade === "B+") {
        color = `rgba(124, 204, 70, ${ opacity })`;
    } else if (grade === "B") {
        color = `rgba(145, 215, 69, ${ opacity })`;
    } else if (grade === "B-") {
        color = `rgba(165, 228, 81, ${ opacity })`;
    } else if (grade === "C+") {
        color = `rgba(186, 240, 94, ${ opacity })`;
    } else if (grade === "C") {
        color = `rgba(201, 245, 112, ${ opacity })`;
    } else if (grade === "C-") {
        color = `rgba(244, 255, 95, ${ opacity })`;
    } else if (grade === "D+") {
        color = `rgba(227, 195, 82, ${ opacity })`;
    } else if (grade === "D") {
        color = `rgba(227, 147, 82, ${ opacity })`;
    } else if (grade === "F") {
        color = `rgba(227, 108, 82, ${ opacity })`;
    } else {
        color = `rgba(0, 0, 0, ${ opacity })`;
    }

    return color;
}