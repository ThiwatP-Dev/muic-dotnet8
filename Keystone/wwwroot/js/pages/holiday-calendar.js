var academicEventJson = $('#calendar').data('events');
let eventData = JSON.parse(JSON.stringify(academicEventJson));

(function(a) {
    a(function() {
        a("#calendar").fullCalendar({
            contentHeight: 1100,
            businessHours: true,
            defaultView: "month",
            editable: false,
            displayEventTime: false,
            navLinks: true, // can click day/week names to navigate views
            eventLimit: true, // allow "more" link when too many events
            header: {
                left: "title",
                right: "prev,next"
            },
            events: eventData,
            timeFormat: 'HH:mm',
            eventRender: function(c, b) {
                if (c.icon) {
                    b.find(".fc-title").prepend(`<i class="la la-${ c.icon }"></i>`)
                }
            },
            // eventMouseover: function(event) {
            //     $('.popover').popover().remove();
            //     var detail = `<p>${ event.remark }</p>
            //                   <p>${ moment(event.start).format("DD MMM YYYY HH:mm") } - ${ moment(event.end).format("DD MMM YYYY HH:mm") }</p>`;
            //     var eventContent = `${ detail }
            //                         <div class="text-nowarp text-center td-actions">
            //                             <a href="/AcademicCalendar/Edit/${ event.id }">
            //                                 <i class="la la-edit edit"></i>
            //                             </a>
            //                             <a href="#" id="${ event.id }" class="js-delete"
            //                                data-toggle="modal" 
            //                                data-target="#delete-confirm-modal"
            //                                data-controller="AcademicCalendar"
            //                                data-action="Delete"
            //                                data-value="${ event.id }">
            //                                 <i class="la la-trash delete"></i>
            //                             </a>
            //                         </div>`;
                                    
            //     $(this).popover({
            //         title: event.Time + event.title,
            //         content: eventContent,
            //         placement: 'top',
            //         trigger: 'hover',
            //         delay: { 
            //             show: "10", 
            //             hide: "2200"
            //          },
            //         html: true
            //     });
            //     $(this).popover('show');
            //     $(this).on('shown.bs.popover', function() {
            //         let deleteBtn = $('.popover-body').find('.js-delete');
            //         $(deleteBtn).attr('data-toggle', 'modal');
            //         $(deleteBtn).attr('data-target', '#delete-confirm-modal');
            //         $(deleteBtn).attr('data-controller', 'AcademicCalendar');
            //         $(deleteBtn).attr('data-action', 'Delete');
            //         $(deleteBtn).attr('data-value', $(deleteBtn).attr('id'));
            //     })
            // }
        })
    })
})(jQuery);

$('#delete-confirm-modal').on('shown.bs.modal', function () {
    let deleteBtnConfirm = $('.js-modal-form').find('.js-delete-confirm-btn');
    let deleteBtn = $('.popover-body').find('.js-delete');
    let controller = 'AcademicCalendar';
    let action = 'Delete';
    let value =  $(deleteBtn).attr('id');
    let fullRoute = `/${ controller }/${ action }/${ value }`;

    $(deleteBtnConfirm).attr("href", `${ fullRoute }`);
});

$(document).ready(function() {
    $('.fc-prev-button').empty().append('<i class="la la-angle-left"></i>');
    $('.fc-next-button').empty().append('<i class="la la-angle-right"></i>');

    $('#holiday-classes-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let holidayId = button.data('value')

        var ajax = new AJAX_Helper(
            {
                url: HolidayDetailUrl,
                data: {
                    id: holidayId,
                },
                dataType: 'html'
            }
        );

        ajax.GET().done(function (response) {
            $('#modalWrapper-holiday-classes-details').empty().append(response);
            RenderTableStyle.columnAlign();
            ReportTable.print('.js-holiday-table');
        })
            .fail(function (jqXHR, textStatus, errorThrown) {
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
    })
})