var checkboxCounter = 0;

var CheckList = (function() {

    var RenderCheckbox = function(checkListClass) {
        let itemCount = $(checkListClass).find(".js-item-count");
        let isCheckAll = $(checkListClass).find(".js-check-all");
        let isCheckAllColumn = $(isCheckAll).parent().index() + 1;
        let checkboxItem = $(checkListClass).find(".js-items").find(`td:nth-child(${isCheckAllColumn})`).find(":checkbox");
        
        if (isCheckAll[0].checked) {
            $(checkboxItem).each(function() {
                this.checked = true;
            });
            countChecked(isCheckAll, itemCount, checkboxItem);
        }
        
        $(isCheckAll).click(function() {
            if (isCheckAll[0].checked) {
                $(checkboxItem).each(function() {
                    if (!$(this).prop('disabled')) {
                        this.checked = true;
                    }
                });
            } else {
                $(checkboxItem).each(function() {
                    if (!$(this).prop('disabled')) {
                        this.checked = false;
                    }
                });
            }
            countChecked(isCheckAll, itemCount, checkboxItem);
        })
        
        $(checkboxItem).click(function() {
            countChecked(isCheckAll, itemCount, checkboxItem);
        })
    }

    var RenderCheckAllBtn = function(checkListClass, checkAllBtnId) {
        var checkboxItem = $(checkListClass);
        countChecked(checkAllBtnId, "null", checkboxItem);
        
        $(checkAllBtnId).click(function() {
            let isCheckAll = $(checkAllBtnId).hasClass("unchecked")
            
            if (isCheckAll) {
                $(checkboxItem).each(function() {
                    this.checked = true;
                });
                toggleCheckAllButton(true, checkAllBtnId);
            } else {
                $(checkboxItem).each(function() {
                    this.checked = false;
                });
                toggleCheckAllButton(false, checkAllBtnId);
            }
        })

        $(checkboxItem).click(function() {
            countChecked(checkAllBtnId, "null", checkboxItem);
        })
    }

    function countChecked(checkAll, itemCount, checkboxItem) {
        checkboxCounter = 0;
        $(checkboxItem).each(function() {
            if (this.checked) {
                checkboxCounter++;
            }
        });

        var checkAllType = $(checkAll)[0].nodeName;

        if (checkAllType == "INPUT") {
            if ($(checkboxItem).length == checkboxCounter) {
                $(checkAll)[0].checked = true;
            } else {
                $(checkAll)[0].checked = false;
            }
        } else if (checkAllType == "A" || checkAllType == "BUTTON") {
            if ($(checkboxItem).length == checkboxCounter) {
                toggleCheckAllButton(true, checkAll);
            } else {
                toggleCheckAllButton(false, checkAll);
            }
        }

        if (itemCount !== "null") {
            $(itemCount).html(`(${ checkboxCounter })`);
        }
    }

    function toggleCheckAllButton(currentStatus, checkAllBtnId) {
        if (currentStatus) {
            $(checkAllBtnId).html(`<i class="far fa-square mr-1"></i> Deselect All`)
            $(checkAllBtnId).addClass("checked")
            $(checkAllBtnId).removeClass("unchecked")
        } else {
            $(checkAllBtnId).html(`<i class="far fa-check-square mr-1"></i> Select All`)
            $(checkAllBtnId).addClass("unchecked")
            $(checkAllBtnId).removeClass("checked")
        }
    }

    return {
        renderCheckbox : RenderCheckbox,
        renderCheckAllBtn : RenderCheckAllBtn
    };
})();