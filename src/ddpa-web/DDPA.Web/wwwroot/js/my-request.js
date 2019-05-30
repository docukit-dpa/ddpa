var listToDelete = [];

$(document).ready(function ()
{
    initFields();
    $("#btn_approve").addClass("disabled");
    $("#bulkDelete_btn").addClass("disabled");
    $('.ui.dropdown').dropdown();
    $('.button').popup();
});

$("#approvalTable").on("draw.dt", function ()
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

    showCheckBox();
});

//on change of the checkbox except the checkAll
$("#approvalTable").on("change", ":checkbox", function ()
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

$("#approvalTable").on("click", "#checkAll", function ()
{
    //if the checkAll is check, check all below checkbox
    if ($(this).prop("checked"))
    {
        $(":checkbox:not(:checked)").not($("#checkAll")).each(function ()
        {
            var tempCheckBox = $("#approvalTable #" + $(this).attr("id"));
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
            $("#approvalTable #" + $(this).attr("id")).prop("checked", false);
            listToDelete.splice($.inArray($(this).attr("id"), listToDelete), 1);
        });
    }
});

function approve()
{
    $.ajax({
        url: '/Approval/ApproveDocuments',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(listToApprove),
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

function bulkDelete()
{
    $.ajax({
        url: '/Approval/BulkDeleteDrafts',
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

var documentId = "";
function initFields()
{
    var $dataTable = $("#approvalTable");
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
                {
                    "data": "id", "searchable": false, "orderable": false, className: "all",
                    "render": function (data, type, full, meta)
                    {
                        documentId = data;
                        return '<div class="ui checkbox"><input type="checkbox" id="' + data + '"disabled="disabled"/><label></label></div>';
                    }
                },
                {
                    "data": "departmentId", className: "desktop, tablet",
                    "render": function (data)
                    {
                        if (data === "" || data === null)
                        {
                            return '<p>DPO/ADMIN</p>';
                        }

                        else
                        {
                            return '<p>' + data + '</p>';
                        }
                    }
                },
                { "data": "datasetName", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                {
                    "data": "details", className: "desktop, tablet",
                    "render": function (data, type, full, meta)
                    {
                        return '<a href="/Dataset/Data/' + documentId + '" class="ui compact icon tiny blue button"><i class="list ul icon"></i></a>' + '</p>';
                    }
                },
                {
                    "data": "logs", className: "desktop, tablet",
                    "render": function (data, full)
                    {
                        return '<a class="ui compact icon tiny blue button" onclick="$(\'#logs_modal\').modal(\'show\'); documentId=' + documentId + '; initLogs();"><i class="tasks icon"></i></a>';
                    }
                },
                {
                    "data": "state", className: "desktop, tablet",
                    "render": function (data, full)
                    {
                        if (data === "Draft")
                        {
                            return '<p>' + data + '&nbsp&nbsp<a href="/Dataset/Data/' + documentId + '" class="ui compact icon tiny red button enable-checkbox" check-id=' + documentId + '><i class="list ul icon"></i></a>' + '</p>';
                        }

                        else
                        {
                            return '<p>' + data + '</p>';
                        }
                    }
                }
            ]
        });
}

function initLogs()
{
    var $dataTable = $("#logsTable");
    var destroyTable = $("#logsTable").DataTable();
    destroyTable.destroy();
    var url = $dataTable.attr("data-table-url");
    url = url + "?docId=" + documentId;
    var table = $dataTable.DataTable
        ({
            "paging": true,
            "info": true,
            "lengthChange": false,
            "pageLength": 5,
            "searching": true,
            "responsive": true,
            "ajax":
            {
                "url": url,
                "type": "GET",
                "contentType": "application/json; charset=utf-8",
                "dataType": "json"
            },
            "columns": [
                { "data": "datasetName", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                { "data": "action", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                { "data": "description", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                { "data": "comment", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                { "data": "actionDate", className: "desktop, tablet", render: $.fn.dataTable.render.text() }

            ]
        });
}

function showCheckBox()
{
    //loop thru all row with draft status
    $(".enable-checkbox").each(function ()
    {
        //get the value of check-id attribute
        var checkId = $(this).attr("check-id");

        //enable checkbox
        $(":checkbox#" + checkId).removeAttr("disabled");
    });

    //loop thru checkbox in all row that has disabled checkbox
    $(":checkbox[disabled]").each(function ()
    {
        //remove the element
        $(this).closest("td").empty();
    });
}