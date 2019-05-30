$(document).ready(function ()
{
    $('#table').DataTable();
    $('#easyPaginate').easyPaginate({
        paginateElement: '.datasetDiv',
        elementsPerPage: 4,
        effect: 'climb'
    });
    initAjaxForm("#deleteData_form", "validate");

    initDatasetsTable();
    $("#datasetsTable thead").remove();    

    initDatasetTable();
    $('.ui.dropdown').dropdown({ showOnFocus: false });
    $('.button').popup();
});

function initDatasetsTable()
{
    var $dataTable = $("#datasetsTable");
    var url = $dataTable.attr("data-table-url");
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
                "type": "POST",
                "contentType": "application/json; charset=utf-8",
                "dataType": "json"
            },
            "columns": [
                {
                    "data": "id", "searchable": false, "orderable": false, className: "one wide column",
                    "render": function (data, type, full, meta)
                    {
                        return '<img class="ui middle aligned mini rounded image" src="/images/doc.png">';
                    }
                },
                {
                    "data": "name", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                }
            ],
            "createdRow": function (row, data, index)
            {
                $(row).attr("onclick", "selectedDataset('" + data.id + "');");
                $(row).attr("id", "datasetRow" + data.id);
                $(row).addClass("datasetRow");
            },
            "initComplete": function (settings, json)
            {
            }
        });
}


var previousDatasetRow = "";
function selectedDataset(id)
{
    if (previousDatasetRow !== "")
    {
        $("#datasetRow" + previousDatasetRow).removeClass("active");
        previousDatasetRow = id;
        $("#datasetRow" + id).addClass("active");
        $("#datasetId").val(id);
    }
    else if (previousDatasetRow === "")
    {
        previousDatasetRow = id;
        $("#datasetRow" + id).addClass("active");
        $("#datasetId").val(id);
    }
}

function initDatasetTable()
{
    var $dataTable = $("#datasetTable");
    var url = $dataTable.attr("data-table-url");
    let userEdit = document.getElementById('userView');
    let editRights = userEdit.getAttribute('value');
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
                    "data": "datasetName", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "purposeOfUse", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "department", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "dataOwner", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "storage", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "outsideSingapore", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "retentionPeriod", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "disposalMethod", className: "desktop, tablet", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "id", className: "desktop, tablet",
                    "render": function (data)
                    {
                        var returnString = '';
                        //edit
                        if (editRights == 1)
                        {
                            //update button
                            returnString += '<a href="/Dataset/Data/' + data + '" class="ui compact icon tiny teal button" data-tooltip="Edit" data-position="top center" data-variation="small"><i class="pencil alternate icon"></i></a> ';
                        }
                        else
                        {
                            returnString += '<a href="#!" class="ui compact icon tiny teal button" data-tooltip="Edit" data-position="top center" data-variation="small" style="cursor:not-allowed"><i class="pencil alternate icon"></i></a> ';
                        }

                        //view
                        returnString += '<a href="#" class="ui compact icon tiny teal view button" data-tooltip="View" data-position="top center" data-variation="small" onclick="initViewTables(\'' + data + '\');$(\'#datasetViewModal\').modal(\'show\');"><i class="eye icon"></i></a> ';

                        //delete
                        returnString += '<a onclick="$(\'#deleteModal\').modal(\'show\');$(\'#docId\').val(' + data + ');" class="ui compact icon tiny red button" data-tooltip="Delete" data-position="top center" data-variation="small"><i class="trash icon"></i></a>';

                        return returnString;
                    }
                }
            ],
            "createdRow": function (row, data, index)
            {
                //todo retention red row
                var currentDate = new Date();
                var createdDate = new Date(data.createdDate.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
                var dateWithRetentionPeriod = new Date(data.createdDate.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
                dateWithRetentionPeriod.setMonth(dateWithRetentionPeriod.getMonth() + parseInt(data.retentionPeriod));
            }
        });
}

function initViewTable(tableName, submoduleId, docId)
{
    var $dataTable = $(tableName);
    var url = $dataTable.attr("data-table-url") + "?docId=" + docId + "&subModuleId=" + submoduleId;
    var table = $dataTable.DataTable
        ({
            "destroy": true,
            "paging": false,
            "info": false,
            "lengthChange": false,
            "pageLength": 10,
            "searching": false,
            "responsive": false,
            "order": [],
            "ajax":
            {
                "url": url,
                "type": "POST",
                "contentType": "application/json; charset=utf-8",
                "dataType": "json"
            },
            "columns": [
                {
                    "data": "field.name", className: "four wide column", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "value", className: "", render: $.fn.dataTable.render.text()
                }
            ]
        });
}

function initViewTables(docId)
{
    $("#dataSetViewModalLoader").addClass("active");
    initViewTable("#collectionViewTable", "2", docId);
    initViewTable("#storageViewTable", "3", docId);
    initViewTable("#useViewTable", "4", docId);
    initViewTable("#disclosureViewTable", "5", docId);
    initViewTable("#retentionViewTable", "6", docId);
    initViewTable("#disposalViewTable", "7", docId);
    initDataSetTemplateTble(docId);

    $.ajax({
        url: '/Maintenance/GetDataSetName?docId=' + docId,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: docId,
        dataType: "json",
        success: function (data)
        {
            $("#dataSetFieldsHeader").html(data);
        },
        error: function (response)
        {
            $("#dataSetFieldsHeader").html("Data Set Fields");
        },
        complete: function ()
        {
            $("#dataSetViewModalLoader").removeClass("active");
        }
    });

    $(".viewTable thead").each(function ()
    {
        $(this).remove();
    });
}

function initDataSetTemplateTble(docId)
{
    var $dataTable = $("#datasetViewTable");
    var url = $dataTable.attr("data-table-url") + "?docId=" + docId;
    var table = $dataTable.DataTable
        ({
            "destroy": true,
            "paging": false,
            "info": false,
            "lengthChange": false,
            "pageLength": 10,
            "searching": false,
            "responsive": false,
            "order": [],
            "ajax":
            {
                "url": url,
                "type": "POST",
                "contentType": "application/json; charset=utf-8",
                "dataType": "json"
            },
            "columns": [
                {
                    "data": "field.name", className: "four wide column", render: $.fn.dataTable.render.text()
                },
                {
                    "data": "field.classification", className: "",
                    "render": function (data)
                    {
                        if (data !== 0) {
                            if (data === 1) {
                                return 'Non-sensitive';
                            }
                            else if (data === 2) {
                                return 'Sensitive';
                            }
                        }

                        return '';
                    }
                }
            ]
        });
}

$("#skipButton").on("click", function ()
{
    if ($("#userRole").val() === "DPO" || $("#userRole").val() === "ADMINISTRATOR")
    {
        if ($("#modalDepartmentId").val() === "")
        {
            $("#pageMessage").children('label').html("Please select a Department.");
            $("#pageMessage").addClass("error").show();

            $("#modalDepartmentId").parent(".dropdown").addClass("error");
            return false;
        }
    }
});

$("#datasetSubmitButton").on("click", function ()
{
    if ($("#userRole").val() === "DPO" || $("#userRole").val() === "ADMINISTRATOR")
    {
        if ($("#modalDepartmentId").val() === "")
        {
            $("#pageMessage").children('label').html("Please select a Department.");
            $("#pageMessage").addClass("error").show();

            $("#modalDepartmentId").parent(".dropdown").addClass("error");
            return false;
        }
        else if ($("#datasetId").val() === "")
        {
            $("#pageMessage").children('label').html("Please select a Data Set.");
            $("#pageMessage").addClass("error").show();
            return false;
        }
    }
    else if ($("#userRole").val() === "USER" || $("#userRole").val() === "DEPTHEAD")
    {
        if ($("#datasetId").val() === "")
        {
            $("#pageMessage").children('label').html("Please select a Data Set.");
            $("#pageMessage").addClass("error").show();

            $("#modalDepartmentId").parent(".dropdown").addClass("error");
            return false;
        }
    }
});

$("#bulkUploadButton").on("click", function ()
{
    if ($("#userRole").val() === "DPO" || $("#userRole").val() === "ADMINISTRATOR")
    {
        if ($("#modalDepartmentId").val() === "")
        {
            $("#pageMessage").children('label').html("Please select a Department.");
            $("#pageMessage").addClass("error").show();

            $("#modalDepartmentId").parent(".dropdown").addClass("error");
            return false;
        }
    }
    $('#bulkUploadExcelFile').click();
});

$("#modalDepartmentId").on("change", function ()
{
    $('#skipButton').attr('href', $('#skipButton').attr('href') + '?modalDepartmentId=' + this.value);
    $("#modalDepartmentId").parent(".dropdown").removeClass("error");
    $("#pageMessage").hide();
});

$('#addDataset').modal({
    onHide: function ()
    {
        
    },
    onShow: function ()
    {
        $("#modalDepartmentDefaultText").addClass("default");
        $("#modalDepartmentDefaultText").html("Choose your Option");
        $("#modalDepartmentId").attr("value", "");
        $("#modalDepartmentId").parent(".dropdown").removeClass("error");
        $("#pageMessage").hide();
        
        $(".datasetRow").each(function ()
        {
            $(this).removeClass("active");
        });
    }
});

$("#dataSetViewModalLoader").ajaxStop(function ()
{
});

$(document).ajaxComplete(function ()
{
});

$("#bulkUploadExcelFile").on("change", function ()
{
    $("#bulkDeptId").val($("#modalDepartmentId").val());
    $("#bulkDataSetId").val($("#datasetId").val());
    $("#f_bulkUploadExcelFile").submit();
    $("#bulkUploadLoader, #bulkUploadLoader2").addClass("active");
});

$("#bulkUploadModalDeptId, #bulkUploadModalDatSetId").on("change", function ()
{
    if ($("#bulkUploadModalDeptId").val() !== "" && $("#bulkUploadModalDatSetId").val() !== "")
    {
        $("#deptId").val($("#bulkUploadModalDeptId").val());
        $("#dataSetId").val($("#bulkUploadModalDatSetId").val());
        $("#bulkUploadButton").removeClass("disabled");
    }
});