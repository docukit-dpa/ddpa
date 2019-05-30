
$(document).ready(function () {
    initAjaxForIndexField("#f_DDPA", ".collection");
    initAjaxField("#f_addField");
    initAjaxForm("#f_addFieldItem", "validate");
    initAjaxForm("#f_addIssue", "validate");
    initAjaxField("#f_reworkDoc");

    $('#easyPaginate').easyPaginate({
        paginateElement: '.datasetDiv',
        elementsPerPage: 4,
        effect: 'climb'
    });

    if ($("#docId").val() == 0) {
        $('#btnIssues').remove();
    }

    initDatasetsTable();
    $("#datasetsTable thead").remove();

    $('.ui.dropdown').dropdown();
    $('.button').popup();

    if ($("#userRole").val() === "DEPTHEAD" || $("#userRole").val() === "USER")
    {
        $("#btnAddIssue").hide();
    }

    updateCombofield();
});

function updateFieldId(id, title) {
    //hideValidationDisplay
    $("#fieldItemModalMessage").addClass("error").hide();
    $("#field_name").removeClass("error");
    $("#TempId").val("");
    $("#TempFieldId").val("");
    $(".field").val("");
    $("#FieldItemName").text(title);

    $(".field").removeClass("invalid");
    $(".field").removeClass("valid");
    $("#TempId").val(id);
    $("#TempFieldId").val(id);
    $("#FieldTitle").text(title);
    $('#addfielditem').modal('show');
}

function addIssue(id) {
    //hideValidationDisplay
    $("#aDescription").removeClass("error");
    $("#aDate").removeClass("error");
    $("#aAssignedTo").removeClass("error");
    $("#aAction").removeClass("error");
    $("#aStatus").removeClass("error");
    $("#IssueModalMessage").removeClass("error").hide();

    $("#DocId").val(id);
    $("#Description").val("");
    $("#Date").val("");
    $("#AssignedTo").val("");
    $("#Action").val("");
    $("#Status").val("");
    $('#addIssue').modal('show');
}

function initAutoComplete(id) {
    var result = null;

    $.ajax({
        url: globalAutoComplete + "/" + id,
        type: 'get',
        async: false,
        success: function (data) {
            result = data;
        }
    });
    return result;
}

function autoCompleteField(id) {
 
    var field = initAutoComplete(id);
    var fieldList = $.map(field.data, function (data) {
        return {
            value: data.name,
            key: data.id,
        };
    });
  
    $("#" + id).autocomplete({
        lookup: fieldList,
        showNoSuggestionNotice: true,
        noSuggestionNotice: 'Sorry, no matching results',
        onSelect: function (e, term, item) {
            var isValid = false;

            for (i in fieldList) {
                if (fieldList[i].value.toLowerCase() === this.value.toLowerCase() || this.value === "")
                {
                    isValid = true;
                }
            }

            if (isValid) {
                $(this).removeClass("invalid").addClass("valid");
                $(this).removeAttr("style");
            } else {
                $(this).removeClass("valid").addClass("invalid");
                $(this).css("background-color", "#FFF6F6").css("border-color", "#E0B4B4");
            }
        },
        appendTo: '#' + id+ 'container_item'
    }).focus(function ()
    {
        $(this).autocomplete("search");
    }).keyup(function () {
        var isValid = false;

        for (i in fieldList) {
            if (fieldList[i].value.toLowerCase() === this.value.toLowerCase() || this.value === "")
            {
                isValid = true;
            }
        }
        if (isValid)
        {
            $(this).removeClass("invalid").addClass("valid");
            $(this).removeAttr("style");
        } else
        {
            $(this).removeClass("valid").addClass("invalid");
            $(this).css("background-color", "#FFF6F6").css("border-color", "#E0B4B4");
        }
    });
}

function loadSelectedDataset() {
    var id = $('input[name="datasetId"]:checked').attr('id');
    var divName = id + "_div";
    $(".datasetBorder").removeClass("easyPaginateBorder");
    $("#" + divName).addClass("easyPaginateBorder");
}

function reloaddataset() {
    $("#Page1").trigger("click");
}

function openSubModule(evt, subModuleName) {
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByName("tabcontent");
    for (i = 0; i < tabcontent.length; i++) {
        var tab = document.getElementById("btn" + tabcontent[i].id);
        
        if (("btn" + tabcontent[i].id) == subModuleName.id) {
            tabcontent[i].style.display = "block";
            tab.classList.add("active");
            document.getElementById("divTitle").textContent = "Data " + subModuleName.title;
            $("#divTitle").append('<div class="ui grey mini label" data-tooltip="' + $("#localizer_" + tabcontent[i].id.toString().toLowerCase()).val() + '" data-position="top left"><i class= "question icon" style = "margin - right: 0px; "></i></div>');
            document.getElementById("divDetails").textContent = "Data " + subModuleName.title + " Details";

            if (subModuleName.title == "Collection") {
                document.getElementById("btnAddDSet").style.display = "inline-block";
                document.getElementById("btnUpload").style.visibility = "visible";
            } else {
                document.getElementById("btnAddDSet").style.display = "none";
                document.getElementById("btnUpload").style.visibility = "hidden";
            }
        }
        else {
            tab.classList.remove("active");
            tabcontent[i].style.display = "none";
        }
    }
    document.getElementById("lifeCycle").style.display = "block";
    document.getElementById("Issues").style.display = "none";
    document.getElementById("btnAddIssue").style.visibility = "hidden";
    $("#btnIssues").removeClass('active');

    if (subModuleName.title == "Issues") {
        $("#btnIssues").addClass('active');
        document.getElementById("lifeCycle").style.display = "none";
        document.getElementById("Issues").style.display = "block";
        document.getElementById("btnAddIssue").style.visibility = "visible";
        document.getElementById("btnAddDSet").style.display = "none";
        document.getElementById("btnUpload").style.visibility = "hidden";
    }
    
}
// Default Page Collection
document.getElementById("btnCollection").click();

$("#addfielditem").on("submit", function (e) {
    $('#addfielditem').modal('hide');

});

function reworkDSet() {
    $('#reworkDataset').modal('show');
}
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
                        $(this).parent("tr").addClass("haime");
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
    $("#datasetSubmitButton").removeClass("disabled");
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

$("#skipButton, #datasetSubmitButton").on("click", function ()
{
    if ($("#userRole").val() === "DPO" || $("#userRole").val() === "ADMINISTRATOR")
    {
        if ($("#modalDepartmentId").val() === "")
        {
            $("#dataSetModalMessage").children('label').html("Please select a Department.");
            $("#dataSetModalMessage").addClass("error").show();

            $("#modalDepartmentId").parent(".dropdown").addClass("error");
            return false;
        }
    }
});

$("#modalDepartmentId").on("change", function ()
{
    $('#skipButton').attr('href', $('#skipButton').attr('href') + '?modalDepartmentId=' + this.value);
    $("#modalDepartmentId").parent(".dropdown").removeClass("error");
    $("#dataSetModalMessage").hide();
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
        $("#dataSetModalMessage").hide();

        $("#datasetSubmitButton").addClass("disabled");
        $(".datasetRow").each(function ()
        {
            $(this).removeClass("active");
        });
    }
});

//Addfielditem Modal Validation
$("#addFieldItemButton").on("click", function ()
{
    if ($("#FieldItemName").val() === "") 
    {
        $("#fieldItemModalMessage").children('label').html("Please fill in the required fields.");
        $("#fieldItemModalMessage").addClass("error").show();

        $("#field_name").addClass("error");
        return false;
    }
    $("#" + $("#dropdownFieldId").val()).removeClass("invalid").addClass("valid");
    $("#" + $("#dropdownFieldId").val()).removeAttr("style");
    $("#" + $("#dropdownFieldId").val()).val($("#FieldItemName").val());
    $("#" + $("#dropdownFieldId").val()).removeAttr("style");

    var id = $("#dropdownFieldId").val();
    var prevElement = $("#prev_" + id);
    var selectedElement = $("#selected_" + id);
    prevElement.val($("#FieldItemName").val());
    selectedElement.val('');
    $("#prev_" + id).next("div.field").children("div.ui.labeled.input").children("div.ui.labeled.action.input").children("div.ui.fluid.search.selection.dropdown.collection").children("div.menu.transition").children("div.item").each(function ()
    {
        $(this).removeAttr("class");
        $(this).addClass("item");
    });
    
    $("#prev_" + id).next("div.field").children("div.ui.labeled.input").children("div.ui.labeled.action.input").children("div.ui.fluid.search.selection.dropdown.collection").children("a").each(function ()
    {
        $(this).remove();
    });
});
//AddIssue Modal Validation
$("#addIssueButton").on("click", function () {
    var ctr = 0;
    if ($("#Description").val() === "") {
        $("#aDescription").addClass("error");
        ctr = ctr + 1;
    }
    if ($("#Date").val() === "") {
        $("#aDate").addClass("error");
        ctr = ctr + 1;
    }
    if ($("#AssignedTo").val() === "") {
        $("#aAssignedTo").addClass("error");
        ctr = ctr + 1;
    }
    if ($("#Action").val() === "") {
        $("#aAction").addClass("error");
        ctr = ctr + 1;
    }
    if ($("#Status").val() === "") {
        $("#aStatus").addClass("error");
        ctr = ctr + 1;
    }
    if (ctr > 0) {
        $("#IssueModalMessage").children('label').html("Please fill in the required fields.");
        $("#IssueModalMessage").addClass("error").show();
        return false;
    }
    return true;

});

$("#Status").on("change", function () {
    $("#aStatus").removeClass("error");
});

$("#addIssue").on("submit", function (e) {
    $('#addfielditem').modal('hide');

});

$('#addfielditem').modal({
    onHide: function ()
    {
        
    },
    onShow: function ()
    {
        $("#FieldItemName").val("");
        $("#FieldItemDescription").val("");
    }
});


function initData(id, value, selected) {
    var result = null;

    $.ajax({
        url: globalGetFieldItemsUrl + "/" + id + "?value=" + value + "&selected=" + selected,
        type: 'get',
        async: false,
        success: function (data) {
            result = data;
        }
    });
    return result;
}

function autocompleteitem(id) {
    var dropdown_id = "#" + id; 
    var hiddenText = "#hiddenText_" + id;
    
    var value = $(hiddenText).next("input").next("span").text();
    var selected = $(dropdown_id).dropdown('get value');
   
    var field = initData(id, value, selected);
    var data = "";
    var menu_id = "#menu_" + id;

    var i;
    for (i = 0; i < field.data.length; i++) {
        var item_name = "item_" + i;
        data += '<option value="' + field.data[i].name + '">' + field.data[i].name + '</option>';
    } 
    
    $(dropdown_id).html("");
    $(dropdown_id).append(data);
}

function updateCombofield() {
    $(".search").each(function (index) {
        var id = $(this).children("select").attr("id");
        if (id !== undefined) {
            autocompleteitem(id);
            var str = $("#selected_" + id).val();
            
            var res = str.split(",");
            for (i = 0; i < res.length; i++) {
                $('#' + id + ' option[value="' + res[i] +'"]').attr('selected','selected');
            }
        }
    });
}

function updateCombofieldById(id, value)
{
    autocompleteitem(id);
    var str = $("#selected_" + id).val();
    if (str === "")
    {
        str =  value;
    }
    else if (str !== "")
    {
        str += "," + value;
    }
    $("#selected_" + id).val(str);
    var res = str.split(",");
    for (i = 0; i < res.length; i++)
    {
        $('#' + id + ' option[value="' + res[i] + '"]').attr('selected', 'selected');
    }    
}

function changeItems(id)
{
    var value = $("#" + id).val().toString();
    var selectElement = $("#" + id);
    var prevElement = $("#prev_" + id);
    var selectedElement = $("#selected_" + id);
    var newValue = "";
    if (prevElement.val() === "")
    {
        prevElement.val(value);
        selectedElement.val(newValue);
    }
    else if (prevElement.val() !== "")
    {
        newValue = $("#" + id).val().toString().replace(prevElement.val(), '');
        
        if (value.indexOf(prevElement.val()) + prevElement.val().toString().length < value.toString().length)
        {
            newValue = newValue.substring(1);
        }
        else if (value.indexOf(prevElement.val()) + prevElement.val().toString().length === value.toString().length)
        {
            newValue = newValue.slice(0,-1);
        }
        
        prevElement.val(newValue);
        selectedElement.val(newValue);
        selectElement.val(newValue);     
    }

    console.log($("#prev_" + id).next("div.field").html());
    $("#prev_" + id).next("div.field").children("div.ui.labeled.input").children("div.ui.labeled.action.input").children("div.ui.fluid.search.selection.dropdown.collection").children("div.menu.transition").children("div.item").each(function ()
    {
        $(this).removeAttr("class");
        $(this).addClass("item");
    });

    $("#prev_" + id).next("div.field").children("div.ui.labeled.input").children("div.ui.labeled.action.input").children("div.ui.fluid.search.selection.dropdown.collection").children("a").each(function ()
    {
        $(this).remove();
    });   
}

$("#commentProceedBtn").on("click", function ()
{
    if ($("#Comment").val() === "")
    {
        $("#reworkCommentMessage").children('label').html("Please fill in the comment field.");
        $("#reworkCommentMessage").addClass("error").show();
        return false;
    }
});

$('#Comment').on('input', function ()
{
    if ($('#Comment').val() !== "")
    {
        $("#reworkCommentMessage").addClass("error").hide();
    }
});
