
$(document).ready(function ()
{
    initFields();
    initAjaxForm("#f_addFieldItem", "validate");
});

function initFields() {
    var $dataTable = $("#FieldTable");
    var url = $dataTable.attr("data-table-url");
    var updateUrl = $dataTable.attr("data-table-url-update");
    var deleteUrl = $dataTable.attr("data-table-url-delete");
    var fieldId = $dataTable.attr("data-table-field-id");
    var isComboField = false;
    var table = $dataTable.DataTable
        ({
            "paging": true,
            "info": true,
            "lengthChange": false,
            "pageLength": 10,
            "searching": false,
            "ajax":
                {
                    "url": url + '?id=' + fieldId,
                    "type": "POST",
                    "contentType": "application/json; charset=utf-8",
                    "dataType": "json"
                },
            "columns": [
                {
                    "data": "name", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "description", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "id", "searchable": false, "orderable": false, className: "all center aligned",
                    "render": function (data, type, full, meta)
                    {
                        var returnString = "";

                        //html of Update link
                        returnString += '<a href="' + deleteUrl + '?id=' + data + '&fieldId=' + fieldId + '" class="ui compact icon tiny red button" data-tooltip="Delete" data-position="top center" data-variation="small"><i class="trash icon"></i></a>';
                        return returnString;
                    }
                }
            ]
        });
}