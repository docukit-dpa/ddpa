var listToDelete = [];

$(document).ready(function ()
{
    $("#bulkDelete_btn").addClass("disabled");
    initFields();
    $('.ui.dropdown').dropdown();
    $('.button').popup();
});

$("#FieldTable").on("draw.dt", function ()
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
$("#FieldTable").on("change", ":checkbox", function ()
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

$("#FieldTable").on("click", "#checkAll", function ()
{
    //if the checkAll is check, check all below checkbox
    if ($(this).prop("checked"))
    {
        $(":checkbox:not([disabled])").not($("#checkAll")).each(function ()
        {
            var tempCheckBox = $("#FieldTable #" + $(this).attr("id"));
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
            $("#FieldTable #" + $(this).attr("id")).prop("checked", false);
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
    var $dataTable = $("#FieldTable");
    var url = $dataTable.attr("data-table-url");
    var updatedUrl = $dataTable.attr('data-table-url-edit');
    var deleteUrl = $dataTable.attr('data-table-url-delete');
    var itemUrl = $dataTable.attr('data-table-url-items');
    var isComboField = false;
    var isDefault = false;
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
                {
                    "data": "id", "searchable": false, "orderable": false, className: "all",
                    "render": function (data, type, full, meta)
                    {
                        return '<div class="ui checkbox"><input type="checkbox" id="' + data + '" disabled="disabled"/><label></label></div>';
                    }
                },
                {
                    "data": "isDefault", className: "desktop, tablet", visible: false,
                    "render": function (data) 
                    {
                        if (data === true)
                        {
                            isDefault = true;
                            return '<p style="font-size: 20px;">&#10004;</p>';
                        }

                        else
                        {
                            isDefault = false;
                            return '<p style="font-size: 20px;">&#10006;</p>';
                        }

                    }
                },
                {
                    "data": "lifeCycle", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "name", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "typeName", className: "desktop, tablet",
                    "render": function (data) 
                    {
                        if (data === "Dropdown")
                        {
                            isComboField = true;
                        }

                        else
                        {
                            isComboField = false;
                        }

                        return '<p>' + data + '</p>';

                    }
                },
                {
                    "data": "isDefault", className: "desktop, tablet",
                    "render": function (data)
                    {
                        if (data === true)
                        {
                            return "Default Field";
                        }
                        else if (data !== true)
                        {
                            return "User Added Field";
                        }
                    }
                },
                {
                    "data": "purpose", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "id", "searchable": false, "orderable": false, className: "all",
                    "render": function (data, type, full, meta)
                    {
                        $("#id").val(data);
                        var returnString = "";

                        //update button
                        returnString = '<a href="' + updatedUrl + '?id=' + data + '" class="ui compact icon tiny teal button" data-tooltip="Edit" data-position="top center"><i class="pencil alternate icon"></i></a> &nbsp';
                        
                        if (!isDefault)
                        {
                            //delete button
                            returnString += '<a class="ui compact icon tiny button red enable-delete" data-tooltip="Delete" data-position="top center" onclick="$(\'#deleteModal\').modal(\'show\'); $(\'#id\').val(' + data + ');" delete-id="' + data + '"><i class="trash icon"></i></a> &nbsp';
                        }
                        
                        if (isComboField)
                        {
                            //add field button
                            returnString += '<a href="' + itemUrl + '?id=' + data + '" class="ui compact icon tiny teal button" data-tooltip="Add Field" data-position="top center"><i class="plus square icon"></i></a>';
                        }
                        return returnString;
                    }
                }
            ],
            "columnDefs": [
                {
                    "targets": [1],
                    "visible": false,
                    "searchable": false
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