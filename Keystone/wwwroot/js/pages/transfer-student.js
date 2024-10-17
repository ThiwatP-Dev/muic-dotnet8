var checkboxCounter = 0;
var studentCheckboxes = ".js-student-check";
var studentCount = "#js-student-count";

var TransferInterface = (function() {

    var CountChecked = function(showSpan, checkInputClass) {
        checkboxCounter = 0;
        $(checkInputClass).each(function() {
            if(this.checked) {
                checkboxCounter++;
            }
        });

        if ($(checkInputClass).length == checkboxCounter) {
            $(".js-check-all")[0].checked = true;
        } else {
            $(".js-check-all")[0].checked = false;
        }

        $(showSpan).html(`(${ checkboxCounter })`);
    }

    var IsSeatEnough = function() {
        let availableSection = $('[name="SectionTransfer"]');
        if (availableSection.length) {
            availableSection.each(function() {
                let seatLeft = $(this).data('seat-left');
                if (seatLeft < checkboxCounter) {
                    $(this).prop("disabled", true);
                    $(this).prop("checked", false);
                } else {
                    $(this).prop("disabled", false);
                }
            })
        }
    }

    return {
        countChecked : CountChecked,
        isSeatEnough : IsSeatEnough
    };
})();

$(document).ready(function() {
    $(".js-render-nicescroll").niceScroll();

    $(document).on("click", studentCheckboxes, function() {
        TransferInterface.countChecked(studentCount, studentCheckboxes);
        TransferInterface.isSeatEnough();
    })

    $(".js-check-all").on('click', function() {
        if (this.checked) {
            $(studentCheckboxes).each(function() {
                this.checked = true;
            });
        } else {
            $(studentCheckboxes).each(function() {
                this.checked = false;
            });
        }

        TransferInterface.countChecked(studentCount, studentCheckboxes);
        TransferInterface.isSeatEnough();
    });
})