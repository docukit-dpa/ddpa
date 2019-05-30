
$(document).ready(function ()
{
    initAjaxForm("form[data-async]");
    initModuleAccess();
    initAjaxForm("#f_register", "validate");
    initAjaxForm("#updateUser_form", "validate");

    $('.ui.dropdown').dropdown();
    $('.button').popup();
});

function initModuleAccess() {
    var $dataTable = $("#ModuleAccess");
    var url = $dataTable.attr("data-table-url");

    var table = $dataTable.DataTable
        ({
            "pagingType": "full_numbers",
            "pageLength": globalPageSize20,
            "responsive": true,
            "searching": false,
            "destroy": true,
            "order": [],
            "ajax":
                {
                    "url": url,
                    "type": "POST",
                    "contentType": "application/json; charset=utf-8",
                    "data": function (data) {      
                        data = JSON.stringify(data);                      
                        return data;
                        
                    }
                },
            "columns": [
                { "data": "moduleName", "searchable": false, "orderable": false, className: "desktop, tablet", render: $.fn.dataTable.render.text() },
                {
                    "data": "view", "searchable": false, "orderable": false, className: "all center aligned",
                    "render": function (data, type, full, meta) {
                        var _id = "view_" + meta.row;
                        var _class = "userRights_" + full.id;
                        var checked = (data === 1) ? "checked" : "";
                        if (data === 2) {
                            return ''
                        }
                        if (full.moduleName === "Dashboard") {
                            return '<div class="ui checkbox"><input id="' + _id + '" name="view" class="' + _class.toUpperCase() + '" style="cursor:not-allowed" type="checkbox" ' + checked + ' disabled /><span style="cursor:not-allowed"></span><label></label></div>';

                        }
                      return '<div class="ui checkbox"><input id="' + _id + '" name="view" onchange="updateValue(\'' + meta.row + '\', \'' + meta.col + '\', \'' + _id + '\', \'' + _class.toUpperCase() + '\')" class="' + _class.toUpperCase() + '" type="checkbox" ' + checked + '/><span></span><label></label></div>';
             
                    }
                },
                {
                    "data": "add", "searchable": false, "orderable": false, className: "all center aligned",
                    "render": function (data, type, full, meta) {
                        var _id = "add_" + meta.row;
                        var _class = "userRights_" + full.id;
                        var checked = (data === 1) ? "checked" : "";
                        if (data === 2) {
                            return ''
                        }
                        return '<div class="ui checkbox"><input id="' + _id + '" name="add" onchange="updateValue(\'' + meta.row + '\', \'' + meta.col + '\', \'' + _id + '\', \'' + _class.toUpperCase() + '\')" class="' + _class.toUpperCase() + '" type="checkbox" ' + checked + '/><span></span><label></label></div>';

                    }
                },
                {
                    "data": "edit", "searchable": false, "orderable": false, className: "all center aligned",
                    "render": function (data, type, full, meta) {
                        var _id = "edit_" + meta.row;
                        var _class = "userRights_" + full.id;
                        var checked = (data === 1) ? "checked" : "";
                        if (data === 2) {
                            return ''
                        }
                        return '<div class="ui checkbox"><input id="' + _id + '" name="edit" onchange="updateValue(\'' + meta.row + '\', \'' + meta.col + '\', \'' + _id + '\', \'' + _class.toUpperCase() + '\')" class="' + _class.toUpperCase() + '" type="checkbox" ' + checked + '/><span></span><label></label></div>';

                    }
                }
            ]
        });
    $(".table-footer").hide();
    $('.dt-buttons').appendTo('.btn-group');
}
function updateValue(row, col, id, chk_class) {
    updated = true;
    var oTable = $('#ModuleAccess').DataTable();

    oTable.row(row).column(col).nodes().each(function (node, index, dt) {
        if (index === parseInt(row, 10)) {   
            console.log('change');
            if ($('#' + id).is(":checked")) {
                oTable.cell(node).data(1);
                console.log(1);
            }
            else
            {
                oTable.cell(node).data(0);
                console.log(0);
            }
        }
    });

    var myTable = $('#ModuleAccess').dataTable();
    var updatedData = JSON.stringify(myTable.fnGetData());
    console.log(updatedData);
    $("#Permissions").val(updatedData);

}

function selectOne(field, id) {
    var input_class = "." + field;
    var input_id = "#" + field;
    $(input_id).prop("checked", false);
    if ($(input_class).length === ($(input_class + ':checked').length)) {
        if ($(input_id).is(':enabled')) {
            $(input_id).prop("checked", true);
        }
    }
    if ($(input_class + ':checked').length < 1) {
        $(".btn_user").attr('disabled', 'disabled');
    } else {
        $(".btn_user").removeAttr('disabled');
    }
}