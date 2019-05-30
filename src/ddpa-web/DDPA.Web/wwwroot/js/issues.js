$(document).ready(function ()
{
    initAjaxField("#f_editIssue");
    initFields();
});
function initFields()
{

    var $dataTable = $("#tbl_issueTable");
    var url = $dataTable.attr("data-table-url");

    var table = $dataTable.DataTable
        ({
            "paging": true,
            "lengthChange": true,
            "pageLength": 10,
            "autoWidth": true,
            "ajax":
            {
                "url": url,
                "type": "POST",
                "contentType": "application/json; charset=utf-8",
                "dataType": "json"
            },
            "columns": [
                { "data": "datasetName", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                { "data": "department", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                { "data": "description", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                {
                    "data": "test", "searchable": false, "orderable": false, 
                    "render": function (data, type, full, meta) {
                        var returnDate = "";
                        var d = full.date.slice(0, 10).split('-');
                        returnDate = d[1] + '/' + d[2] + '/' + d[0];
                        return returnDate;
                    }
                },
                { "data": "assignedTo", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                { "data": "action", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                { "data": "status", className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                {
                    "data": "id", "searchable": false, "orderable": false, className: "all",
                    "render": function (data, type, full, meta)
                    {
                        if ($("#userRole").val() === "DPO" || $("#userRole").val() === "ADMINISTRATOR")
                        {
                            var returnString = '';
                            var param = + data + ',\'' + full.description + '\',\'' + full.date + '\',\'' + full.assignedTo + '\',\'' + full.action + '\',\'' + full.status + '\''
                           //update button
                            returnString = '<a href="#editIssue" onclick="updateIssue(' + param + ');" class="ui compact icon tiny teal button modal-trigger" data-position="top center" data-tooltip="Edit"><i class="pencil alternate icon"></i></a >'

                            //delete button
                            return returnString;
                        }
                        else
                        {
                            return '';
                        }
                    }
                }]
        });
}

$("#addIssue").on("submit", function (e)
{
    setTimeout(function ()
    {
        reloadIssues();
    }, 300);
    $('#addIssue').modal('hide');
});

function reloadIssues()
{
    $('#tbl_issueTable').DataTable().ajax.reload();
}

function updateIssue(id, description, date, assignedTo, action, status)
{
    //hide validation display
    $("#eDescField").removeClass("error");
    $("#eAssignedToField").removeClass("error");
    $("#eActionField").removeClass("error");
    $("#editIssueModalMessage").removeClass("error").hide();

    //assign value
    $("#eDocId").val(id);
    $("#eid").val(id);
    $("#eDescription").val(description);
    var now = new Date(date);
    var dt = now.getFullYear() + "-" + ("0" + (now.getMonth() + 1)).slice(-2) + "-" + ("0" + now.getDate()).slice(-2);
    $("#eDate").val(dt);
    $("#eAssignedTo").val(assignedTo);
    $("#eAction").val(action);
    $("#eStatus").val(status).change();
    $('#editIssue').modal('show');
}

$("#editIssue").on("submit", function (e)
{
    setTimeout(function ()
    {
        reloadIssues();
    }, 300);
    $('#editIssue').modal('hide');

});
//Validate EditIssueField
$("#editIssueButton").on("click", function () {
    var ctr = 0;
    if ($("#eDescription").val() === "") {
        $("#eDescField").addClass("error");
        ctr = ctr + 1;
    }
    if ($("#eAssignedTo").val() === "") {
        $("#eAssignedToField").addClass("error");
        ctr = ctr + 1;
    }
    if ($("#eAction").val() === "") {
        $("#eActionField").addClass("error");
        ctr = ctr + 1;
    }

    if (ctr > 0) {
        $("#editIssueModalMessage").children('label').html("Please fill in the required fields.");
        $("#editIssueModalMessage").addClass("error").show();
        return false;
    }
    return true;
});
