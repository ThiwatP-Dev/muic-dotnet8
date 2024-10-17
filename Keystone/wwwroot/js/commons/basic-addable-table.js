(function () {
    this.BasicAddableTable = function () {
        this.title = null;
        this.targetTable = null;
        this.addRowButtonTitle = null;
        this.hasIndex = null;

        var defaults = {
            title: null,
            targetTable: null,
            addRowButtonTitle: "Add New",
            isEditable: false,
            canDisableRow: false
        };

        // Create options by extending defaults with the passed in arugments
        if (arguments[0] && typeof arguments[0] === "object") {
            this.options = extendDefaults(defaults, arguments[0]);
        }

        initializeElements.call(this);
        inititalizeEvents.call(this);
    };

    // Utility method to extend defaults with user options

    function extendDefaults(source, properties) {
        var property;
        for (property in properties) {
            if (properties.hasOwnProperty(property)) {
                source[property] = properties[property];
            }
        }
        return source;
    }

    // Private Methods

    function initializeElements() {

        this.title = this.options.title;
        this.targetTable = this.options.targetTable;
        this.addRowButtonTitle = this.options.addRowButtonTitle;
        this.tableLength = $(this.targetTable).find('tbody > tr').length;
        this.hasIndex = this.options.hasIndex;

        var addButton = `<button type='button' class="btn btn-mw-120 mb-0 ml-auto btn--primary rounded js-new-row">+ ${ this.addRowButtonTitle }</button>`;

        $(this.targetTable).parent().prepend(`<div class="d-flex section-title justify-content-between mb-3"><h4 class='align-self-center'>${ this.title }</h4> ${ addButton }</div>`);
    }

    function inititalizeEvents() {

        $('.js-new-row').on('click', this.add.bind(this));
        $(this.targetTable).find('.delete').on('click', this.delete.bind(this));
    }

    function ClearInput(selector) {
        $(selector)
            .find("input,textarea,select")
            .not('input[name="__RequestVerificationToken"]')
            .val('')
            .end()
            .find("input[type=checkbox], input[type=radio]")
            .prop("checked", "")
            .end();
    }

    // Public Methods

    BasicAddableTable.prototype.add = function () {
        var _ = this;
        var $targetTable = $(_.targetTable);
        _.tableLength++;
        var currentLastRow = $targetTable.find('tbody > tr').last();

        var newRow = currentLastRow.clone().attr("id", `row-${ _.tableLength - 1 }`).fadeIn('slow');
        newRow.find("select.js-chosen-select").next().remove();

        if(_.hasIndex) {
            newRow.find("td").first().html(_.tableLength);
        }

        ClearInput(newRow);

        newRow.appendTo($targetTable);

        newRow.find(":input").each(function () {
            if ($(this).attr("name")) {
                $(this).attr("name", $(this).attr("name").replace(/\d+/g, (_.tableLength - 1)));
            }

            if ($(this).attr("id")) {
                $(this).attr("id", $(this).attr("id").replace(/\d+/g, (_.tableLength - 1)));
            }

            if ($(this).is('select')) {
                $(this).val('Select').trigger('chosen:updated');
            }

            newRow.find('.delete').on('click', _.delete.bind(_));
        });
    };

    BasicAddableTable.prototype.delete = function (event) {
        var _ = this;
        var $targetTable = $(_.targetTable);
        var tbody = $(this).closest("tbody");
        var rows = $targetTable.find("tbody > tr");

        if (rows.length == 1) {
            return;
        } else {

            event.currentTarget.closest("tr").remove();
            rows = $targetTable.find("tbody > tr");
            _.tableLength = rows.length;

            // Reorder index of each row
            for (var i = 0; i < rows.length; i++) {

                var row = $($(rows)[i]);
                row.attr("id", `row-${ i }`);

                if(_.hasIndex) {
                    row.find("td").first().html(i + 1);
                }

                row.find(":input").each((index, item) => {
                    var id = $(item).attr("id");
                    if (id != undefined) {
                        $(item).attr("name", $(item).attr("name").replace(/\d+/g, i));
                        $(item).attr("id", $(item).attr("id").replace(/\d+/g, i));
                    }
                });
            }
        }

    };

}());