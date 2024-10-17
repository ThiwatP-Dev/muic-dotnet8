// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/*------------------------------------
  set active at menu sidebar
------------------------------------*/

// $(function () {
//   $(document).scroll(function () {
//       var $nav = $(".ks-heading");
//       $nav.toggleClass('ks-heading--scrolled', $(this).scrollTop() > $nav.height());      
//   });
// });

$(window).on("load", function () {
    $('.loader').fadeOut();
    $("#preloader").delay(100).fadeOut();
});


$('form').on('submit', function () {
    var isValid = true;

    $(this).find('input, textarea, select').each(function(){
        if($(this).attr('data-val-required')){
            if(!$(this).val())
            {
                isValid = false;
            }
        }
    });

    if(isValid)
    {
        $('#preloader').fadeIn();

        if (this.submitted && this.submitted == "Export") {
            $("#preloader").delay(2000).fadeOut();
        }
    }
})

$('.page-link').on('click', function () {
    $('#preloader').fadeIn();
})

/*------------------------------------
    Back To Top
------------------------------------*/

$(function () {
    // Show or hide the sticky footer button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 350) {
            $('.go-top').fadeIn(100);
        } else {
            $('.go-top').fadeOut(200);
        }
    });

    // Animate the scroll to top
    $('.go-top').click(function (event) {
        event.preventDefault();

        $('html, body').animate({
            scrollTop: 0
        }, 800);
    })
});

$(function () {
    $('.js-data-table').DataTable();

    $('.js-reset-btn').click(function () {
        $('form')
            .find("input,textarea,select")
            .not('input[name="__RequestVerificationToken"]')
            .val('')
            .end()
            .find("input[type=checkbox], input[type=radio]")
            .prop("checked", "")
            .end();
        $('.chosen-select').val('').trigger('chosen:updated');

        var uri = window.location.toString();
        if (uri.indexOf("?") != -1) {
            var clean_uri = uri.substring(0, uri.indexOf("?"));
            window.history.replaceState({}, document.title, clean_uri);
            window.location.reload();
        }
    });

    $('.form-group table td .chosen-container-single').parent('td').css('max-width', '200px')
});

var Report = (function (config) {

    var doc = new jsPDF(config.orientation, config.unit, config.paperSize);
    var type = config.type;

    doc.setFontSize(12);
    doc.setFontStyle('arial');
    doc.setProperties(config.props);

    var source = $('#js-report-content')[0];

    defaults = {
        fontSize: {
            title: 18,
            normal: 12,
        },
        position: {
            center: (35, 25)
        }
    };

    margins = {
        top: 80,
        bottom: 60,
        left: 40,
        width: 522
    };

    specialElementHandlers = {
        // element with id of "bypass" - jQuery style selector
        '#bypassme': function (element, renderer) {
            // true = "handled elsewhere, bypass text extraction"
            return true
        }
    };

    async function PreviewReport() {

        var newDoc = await GenerateReport();

        newDoc.setFontSize(defaults.fontSize.normal);
        newDoc.text("Office of the university registra", 20, 30);

        var string = newDoc.output('datauristring');
        var iframe = "<iframe width='100%' height='100%' src='" + string + "'></iframe>";
        var x = window.open();
        x.document.open();
        x.document.write(iframe);
        x.document.close();

        $('#js-preview').attr('src', newDoc.output('datauristring'));
    }

    async function GenerateReport() {
        console.log("Genreating...");

        var template = doc;

        console.log(source);

        template.fromHTML(
            source, // HTML string or DOM elem ref.
            margins.left, // x coord
            margins.top, { // y coord
            'width': margins.width, // max width of content on PDF
            'elementHandlers': specialElementHandlers
        },

            function (dispose) {
                // dispose: object with X, Y of the last line add to the PDF 
                //          this allow the insertion of new lines after html  
                // template.save('Test.pdf');
            }, margins
        );

        return template;
    }

    var HeaderTemplate = function (text) {
        var template = doc;
        template.setFontSize(defaults.fontSize.title);
        template.text(text, 20, 20);

        return template;
    };

    var StudentCardTemplate = function (serailizedContent) {
        console.log(content);

        var template = doc;

        HeaderTemplate("Id Card Substitute".toUpperCase());

        template.setFontSize(defaults.fontSize.normal);
        template.text("Office of the university registra", 20, 30);

        return template;
    };

    return {
        previewReport: PreviewReport,
    };
});

$('.js-print-btn').on('click', function () {
    var css = '@page { size: portrait; }',
    head = document.head || document.getElementsByTagName('head')[0],
    style = document.createElement('style');

    style.type = 'text/css';
    style.media = 'print';

    if (style.styleSheet){
        style.styleSheet.cssText = css;
    } else {
        style.appendChild(document.createTextNode(css));
    }

    head.appendChild(style);

    print();
});

$('.js-print-btn-landscape').on('click', function () {
    var css = '@page { size: landscape; }',
    head = document.head || document.getElementsByTagName('head')[0],
    style = document.createElement('style');

    style.type = 'text/css';
    style.media = 'print';

    if (style.styleSheet){
        style.styleSheet.cssText = css;
    } else {
        style.appendChild(document.createTextNode(css));
    }

    head.appendChild(style);

    print();
});

$(document).on('click',"#button-logout", function() {
    localStorage.removeItem('ExpiredToken');
    localStorage.removeItem('TokenMenu');
});