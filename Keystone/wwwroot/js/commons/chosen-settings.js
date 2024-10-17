var config = {
    '.chosen-select': {
        search_contains: true
    },
    '.chosen-select-deselect': {
        allow_single_deselect: true
    },
    '.chosen-select-no-single': {
        disable_search_threshold: 10
    },
    '.chosen-select-no-results': {
        no_results_text: 'Oops, nothing found!'
    },
    '.chosen-select-width': {
        width: "95%"
    },
    '.chosen-select-no-search': {
        disable_search: "true"
    }
}

for (var selector in config) {
    $(selector).chosen(config[selector]);
}
