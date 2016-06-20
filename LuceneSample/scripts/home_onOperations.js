/// <reference path="jquery-1.9.1.js" />
$(document).ready(function () {
   
    $('#txtSearch').autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/Home/ScoredTerms",
                data: { Prefix: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.label,
                            value: item.value
                        }
                    }));
                }
            })
        },
        select: function (event, ui) {
            $('#txtSearch').val(ui.item.label);
            //$('#Id').val(ui.item.value);
            return false;
        },
        minLength: 1
    });
})