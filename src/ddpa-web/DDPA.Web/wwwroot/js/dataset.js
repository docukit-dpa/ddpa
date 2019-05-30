var listToDelete = [];

$(document).ready(function ()
{
    initAjaxForm("#deleteDataset_form", "validate");
    $("#bulkDelete_btn").addClass("disabled");
    initFields();
    $('.ui.dropdown').dropdown();
    $('.button').popup();
});

$("#DatasetTable").on("draw.dt", function ()
{
    var oneIsUnchecked = $(":checkbox:checked").not($("#checkAll")).length;
    if (oneIsUnchecked <= 0)
    {
        $("#checkAll").prop("checked", false);
    }
    else if (oneIsUnchecked > 0)
    {
        $("#checkAll").prop("checked", true);
    }
});

//on change of the checkbox except the checkAll
$("#DatasetTable").on("change", ":checkbox", function ()
{
    //pass if the checkbox is checkAll
    if ($(this).attr("id") !== "checkAll")
    {
        if ($(this).prop("checked"))
        {
            listToDelete.push($(this).attr("id"));
        }
        else
        {
            listToDelete.splice($.inArray($(this).attr("id"), listToDelete), 1);
        }
    }

    //enable deleteSelected button
    if (listToDelete.length > 0)
    {
        $("#bulkDelete_btn").removeClass("disabled");
    }

    //disable deleteSelected button
    else
    {
        $("#checkAll").prop("checked", false);
        $("#bulkDelete_btn").addClass("disabled");
    }
});

$("#DatasetTable").on("click", "#checkAll", function ()
{
    //if the checkAll is check, check all below checkbox
    if ($(this).prop("checked"))
    {
        $(":checkbox").not($("#checkAll")).each(function ()
        {
            var tempCheckBox = $("#DatasetTable #" + $(this).attr("id"));
            tempCheckBox.prop("checked", true);
            listToDelete.push(tempCheckBox.attr("id"));
        });
    }

    //if the checkAll is uncheck, uncheck all below checkbox
    else
    {
        //uncheck all the fields
        $(":checkbox").not($("#checkAll")).each(function ()
        {
            $("#DatasetTable #" + $(this).attr("id")).prop("checked", false);
            listToDelete.splice($.inArray($(this).attr("id"), listToDelete), 1);
        });
    }
});

function bulkDelete()
{
    $.ajax({
        url: '/Maintenance/BulkDeleteDataset',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(listToDelete),
        dataType: "json",
        success: function (response)
        {
            location.reload();
        },
        error: function (response)
        {

        }
    });
}

function initFields()
{
    var $dataTable = $("#DatasetTable");
    var url = $dataTable.attr("data-table-url");
    var updatedUrl = $dataTable.attr('data-table-url-edit');
    var itemUrl = $dataTable.attr('data-table-url-field');
    var downloadUrl = $dataTable.attr('data-table-url-download');
    var table = $dataTable.DataTable
        ({
            "paging": true,
            "info": true,
            "lengthChange": true,
            "pageLength": 10,
            "searching": true,
            "responsive": true,
            "ajax":
            {
                "url": url,
                "type": "POST",
                "contentType": "application/json; charset=utf-8",
                "dataType": "json",
            },
            "columns": [
                {
                    "data": "id", "searchable": false, "orderable": false, className: "all",
                    "render": function (data, type, full, meta)
                    {
                        return '<div class="ui checkbox"><input type="checkbox" id="' + data + '"/><label></label></div>';
                    }
                },
                { "data": "name", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                {
                    "data": "id", className: "all center aligned",
                    "render": function (data, type, full, meta)
                    {
                        //add field button
                        return '<a href="' + itemUrl + '?id=' + data + '" class="ui compact icon tiny teal button" data-tooltip="Add Field" data-position="top center"><i class="plus icon"></i></a>';
                    }
                },
                {
                    "data": "id", className: "all center aligned",
                    "render": function (data, type, full, meta)
                    {
                        //update button
                        return '<a href="' + updatedUrl + '?id=' + data + '" class="ui compact icon tiny teal button" data-tooltip="Edit" data-position="top center"><i class="pencil alternate icon"></i></a>';
                    }
                },
                {
                    //delete button
                    "data": "id", className: "all center aligned",
                    "render": function (data, type, full, meta)
                    {
                        return '<a class="ui compact icon tiny red button" data-tooltip="Delete" data-position="top center" onclick="$(\'#deleteModal\').modal(\'show\'); $(\'#id\').val(' + data + ');"><i class="trash icon"></i></a>';
                    }
                }
            ]
        });
}
