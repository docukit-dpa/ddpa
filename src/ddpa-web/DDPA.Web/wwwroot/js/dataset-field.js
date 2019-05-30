
$(document).ready(function ()
{
    initFields();

    $("#searchvalue").keydown(function () {
        SearchAvailableField();
    });

});



function initFields()
{
    var $availDataTable = $("#AvailFieldTable");
    var addUrl = $availDataTable.attr("data-table-add-url");
    var availUrl = $availDataTable.attr("data-table-available-url");
    var datasetId = $availDataTable.attr("data-table-dataset-id");
    var $currentDataTable = $("#CurrentFieldTable");
    var currentUrl = $currentDataTable.attr("data-table-current-url");
    var deleteUrl = $currentDataTable.attr("data-table-delete-url");

    var availField = $availDataTable.DataTable
        ({
            "paging": true,
            "pagingType": "simple",
            "info": false,
            "lengthChange": false,
            "pageLength": 5,
            "searching": true,
            "ajax":
                {
                    "url": availUrl + '?id=' + datasetId,
                    "type": "POST",
                    "contentType": "application/json; charset=utf-8",
                    "dataType": "json"
                },
            "columns": [
                {
                    "data": "name", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "id", "searchable": false, "orderable": false, width: "18%", className: "all, center aligned",
                    "render": function (data, type, full, meta)
                    {
                        return '<a href="' + addUrl + '?datasetId=' + datasetId + '&fieldId=' + data + '" class="ui compact icon tiny teal button" data-tooltip="Add Field" data-position="top center" data-variation="small">'
                            + '<i class="plus square icon"></i>'
                            + '</a>';
                    }
                }
            ]
        });

    var currentField = $currentDataTable.DataTable
        ({
            "paging": true,
            "info": false,
            "lengthChange": false,
            "pageLength": 5,
            "searching": true,
            "ajax":
                {
                    "url": currentUrl + '?id=' + datasetId,
                    "type": "POST",
                    "contentType": "application/json; charset=utf-8",
                    "dataType": "json"
                },
            "columns": [
                {
                    "data": "field.name", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "id", "searchable": false, "orderable": false, width: "18%", className: "all, center aligned",
                    "render": function (data, type, full, meta)
                    {
                        return '<a href="' + deleteUrl + '?datasetId=' + datasetId + '&id=' + data + '" class="ui compact icon tiny red button" data-tooltip="Delete" data-position="top center" data-variation="small">'
                            + '<i class="trash icon"></i>'
                            + '</a>';
                    }
                }
            ]
        });

}
function SearchAvailableField() {
    var input, filter, table, tr, td, i;

    input = document.getElementById("searchvalue");
    filter = input.value.toUpperCase();
    table = document.getElementById("AvailFieldTable");
    tr = table.getElementsByTagName("tr");
    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[0];
        if (td) {
            if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}