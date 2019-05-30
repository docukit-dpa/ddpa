var approvalDocuments = [];

$(document).ready(function ()
{
    $("#approveButton").addClass("disabled");
    initFields();
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
});

//on change of the checkbox except the checkAll
$("#approvalTable").on("change", ":checkbox", function ()
{
    //pass if the checkbox is checkAll
    if ($(this).attr("id") !== "checkAll")
    {
        if ($(this).prop("checked"))
        {
            approvalDocuments.push($(this).attr("id"));
        }
        else
        {
            approvalDocuments.splice($.inArray($(this).attr("id"), approvalDocuments), 1);
        }
    }

    //enable deleteSelected button
    if (approvalDocuments.length > 0)
    {
        $("#approveButton").removeClass("disabled");
    }

    //disable deleteSelected button
    else
    {
        $("#checkAll").prop("checked", false);
        $("#approveButton").addClass("disabled");
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
            approvalDocuments.push(tempCheckBox.attr("id"));
        });
    }

    //if the checkAll is uncheck, uncheck all below checkbox
    else
    {
        //uncheck all the fields
        $(":checkbox").not($("#checkAll")).each(function ()
        {
            $("#approvalTable #" + $(this).attr("id")).prop("checked", false);
            approvalDocuments.splice($.inArray($(this).attr("id"), approvalDocuments), 1);
        });
    }
});

function approve()
{
    $.ajax({
        url: '/Approval/ApproveDocuments',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(approvalDocuments),
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
                    "data": "id", "searchable": false, "orderable": false, width: "5%", className: "all",
                    "render": function (data, type, full, meta)
                    {
                        documentId = data;
                        return '<div class="ui checkbox"><input type="checkbox" id="' + data + '"/><label></label></div>';
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
                { "data": "datasetName", className: "desktop, tablet", render: $.fn.dataTable.render.text()},
                {
                    "data": "details", className: "desktop, tablet",
                    "render": function (data, type, full, meta) 
                    {
                        return '<a href="/Dataset/Data/' + full.id + '" class="ui compact icon tiny blue button"><i class="list ul icon"></i></a>';
                    }
                },
                {
                    "data": "logs", className: "desktop, tablet",
                    "render": function (data, full) 
                    {
                        return '<a class="ui compact icon tiny blue button" onclick="$(\'#logs_modal\').modal(\'show\'); documentId=' + documentId + '; initLogs();"><i class="tasks icon"></i></a>';
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