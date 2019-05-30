var listToDelete = [];

$(document).ready(function ()
{
    $("#bulkDelete").addClass("disabled");
    initFields();
    $('.ui.dropdown').dropdown();
    $('.button').popup();
});

$("#resource_table").on("draw.dt", function ()
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

    enableDelete();
});

//on change of the checkbox except the checkAll
$("#resource_table").on("change", ":checkbox", function ()
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
        $("#bulkDelete").removeClass("disabled");
    }

    //disable deleteSelected button
    else
    {
        $("#checkAll").prop("checked", false);
        $("#bulkDelete").addClass("disabled");
    }
});

$("#resource_table").on("click", "#checkAll", function ()
{
    //if the checkAll is check, check all below checkbox
    if ($(this).prop("checked"))
    {
        //do check the checkbox which is not disabled, and is currently unchecked
        $(":checkbox:not([disabled]):not(:checked)").not($("#checkAll")).each(function ()
        {
            var tempCheckBox = $("#resource_table #" + $(this).attr("id"));
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
            $("#resource_table #" + $(this).attr("id")).prop("checked", false);
            listToDelete.splice($.inArray($(this).attr("id"), listToDelete), 1);
        });
    }
});

function bulkDelete()
{
    $.ajax({
        url: '/Maintenance/BulkDeleteField',
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
    var $dataTable = $("#resource_table");
    var url = $dataTable.attr("data-table-url");
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
                    "dataType": "json"
                },
            "columns": [
                { "data": "nameOfDocument", className: "desktop, tablet", render: $.fn.dataTable.render.text()},
                { "data": "typeOfDocument", className: "desktop, tablet", render: $.fn.dataTable.render.text()},
                {
                    "data": "filePath", "searchable": false, "orderable": false, width: "18%", className: "all",
                    "render": function (data, type, full, meta)
                    {
                        var returnString = "";

                        //html of Update link
                        returnString = '<a href="/' + data + '" target="_blank" class="ui compact icon tiny teal button" data-tooltip="View" data-position="top center"><i class="eye icon"></i></a> &nbsp';
                        returnString += '<a href="/' + data + '" class="ui compact icon tiny teal button" data-tooltip="Download" data-position="top center" download><i class="download icon"></i></a> &nbsp';
                        
                        return returnString;
                    }
                }
            ]
        });
}

$(document).ajaxStop(function ()
{
    
});

$(document).on("click", function ()
{
});

function enableDelete()
{
    //loop thru all row with delete button
    $(".enable-delete").each(function ()
    {
        //get the value of delete-id attribute
        var deleteId = $(this).attr("delete-id");

        //enable checkbox
        $(":checkbox#" + deleteId).removeAttr("disabled");
    });

    //loop thru checkbox in all row that has disabled checkbox
    $(":checkbox[disabled]").each(function ()
    {
        //remove the element
        $(this).closest("td").empty();
    });
}