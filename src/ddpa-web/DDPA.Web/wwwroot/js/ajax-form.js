function initAjaxForIndexField(formSelector, className)
{
    $(formSelector).off('submit'); // prevent sending twice
    ajaxSubmitForIndexField($(formSelector), className);
}

function ajaxSubmitForIndexField(formObject, className) {
    formObject.one('submit', function (event) {
        event.preventDefault();

        var $form = $(this);
        var isRequired = false;
        $('[required]').each(function () {
            if ($(this).val().length === 0) {
                    isRequired = true;
                    $(this).addClass("invalid");
                }
        });

        if (isRequired) {
            if (isRequired) {
                //error message here  
            }
            ajaxSubmitForIndexField($form, className);
            return false;
        }
        var invalid = $('.invalid').length;
        if (invalid > 0) {
            if (isRequired) {
                //error message here  
            }
            ajaxSubmitForIndexField($form, className);
            return false;
        }

        if (!$form.valid()) {
            ajaxSubmitForIndexField($form, className);
            return false;
        } else {
            var formData = new FormData();

            var collection = [];
            var datasetField = [];
            var files = [];
            $(className).each(function () {
                if ($(this).attr("name") !== undefined) {
                    var id = $(this).attr("id");
                    var name = $(this).attr("name");
                    var fieldType = $(this).attr("fieldType");
                    var value = $(this).val();
                    var submoduleid = $(this).attr("submoduleid");
                    var fields = {};
                    if (fieldType === "Attachment") {
                        if ($("#" + id)[0].files !== null) {
                            var file = $("#" + id)[0].files[0];
                            if (file !== null) {
                                formData.append('file', file);
                                fields = { "fieldid": id, "type": fieldType, "value": value, "submoduleid": submoduleid, file: true };
                            } else {
                                fields = { "fieldid": id, "type": fieldType, "value": value, "submoduleid": submoduleid, file: false };
                            }
                        } else {
                            fields = { "fieldid": id, "type": fieldType, "value": value, "submoduleid": submoduleid, file: false };
                        }
                    } else {
                        fields = { "fieldid": id, "type": fieldType, "value": value, "submoduleid": submoduleid, file: false };
                    }
                } else {
                    var combofieldType = $(this).children("select").attr("fieldType");
                    var combosubmoduleid = $(this).children("select").attr("submoduleid");
                    var comboid = $(this).children("select").attr("id");

                    value = $("#" + comboid).val();
                    fields = { "fieldid": comboid, "type": combofieldType, "value": value.toString(), "submoduleid": combosubmoduleid, file: false };
                }
                collection.push(fields);
            });
            
            $(".dataset").each(function () {
                if ($(this).attr("name") !== undefined) {
                    var id = $(this).attr("id");
                    var name = $(this).attr("name");
                    var fieldType = $(this).attr("fieldType");
                    var value = $(this).val();
                    var datasetid = $(this).attr("datasetid");
                    var datasetFields = {};
                    if (fieldType === "Attachment") {
                        if ($("#" + id)[0].files !== null) {
                           
                            var file = $("#" + id)[0].files[0];
                            if (file !== null) {
                                formData.append('datasetfile', file);
                                datasetFields = { "fieldid": id, "type": fieldType, "value": value, "datasetid": datasetid, file: true };
                            } else {
                                datasetFields = { "fieldid": id, "type": fieldType, "value": value, "datasetid": datasetid, file: false };
                            }
                        } else {
                            datasetFields = { "fieldid": id, "type": fieldType, "value": value, "datasetid": datasetid, file: false };
                        }

                    } else {
                        datasetFields = { "fieldid": id, "type": fieldType, "value": value, "datasetid": datasetid, file: false };
                    }

                    datasetField.push(datasetFields);
                }
            });

            var url = $form.attr("form-submit-url");
            var id = $("#docId").val();

            formData.append('id', id);
            formData.append('submoduleid', $("#docSubModuleId").val());
            formData.append('jsondocumentfield', JSON.stringify(collection));
            formData.append('status', $("#docStatus").val());
            formData.append('datanumber', $("#docDataNumber").val());
            formData.append('jsondatasetfield', JSON.stringify(datasetField));
            formData.append('datasetid', $("#docDatasetId").val());
            formData.append('buttonaction', $("#buttonAction").val());
            formData.append('departmentid', $("#docDepartmentId").val());

            Pace.track(function () {
                $.ajax({
                    type: 'POST',
                    url: url,
                    cache: false,
                    contentType: false,
                    processData: false,
                    data: formData,
                    success: function (data, status) {                       
                        if (data.success) {
                            if (id === "0") {
                                location.href = $form.attr("form-next-url") + "/" + data.id;
                            } else {
                                location.href = $form.attr("form-next-url");
                            }
                        } else {
                            ajaxSubmitForIndexField($form, className);
                        }
                    },
                    error: function (xhr, status, error) {
                        console.log(error);
                    }
                });
            });
        }
    });
}

function initAjaxField(formSelector) {
    $(formSelector).off('submit'); // prevent sending twice
    ajaxSubmitField($(formSelector));
}

function ajaxSubmitField(formObject) {

    formObject.one('submit', function (event) {

        event.preventDefault();

        var $form = $(this);
        var isValid = false;
        var id = $form.attr("id");

        $('#' + id + ' .required').each(function () {
            var fieldType = $(this).attr("type");
           
            if ("text" !== fieldType) {
                var optionValue = $(this).find(":selected").val();
                var dataTarget = $(this).prevAll("input[type=text]").attr("data-target");
                if (optionValue === "Choose your option" || optionValue === undefined) {
                    $('input[data-target=' + dataTarget + ']').addClass("invalid");
                    isRequired = true;
                } else {
                    $('input[data-target=' + dataTarget + ']').removeClass("invalid").addClass("valid");
                }
            } else {
                if ($(this).val().length === 0) {
                    isValid = false;
                    $(this).addClass("invalid");
                } else {
                    isValid = true;
                    $(this).addClass("valid");
                }
            }
        });
      
        if (!isValid) {
            ajaxSubmitField($form);
            return false;
        } else {
            var formData = new FormData();
            Pace.track(function () {
                $.ajax({
                    type: $form.attr('method'),
                    url: $form.attr('action'),
                    data: $form.serialize(),
                    success: function (data, status) {
                        if (data.success) {
                            $('.modal').modal('close');
                            if (data.message === "Field has been successfully added.") {
                                location.reload();
                            } else {
                                ajaxSubmitField($form);
                            }
                        } else {
                            ajaxSubmitField($form);
                        }
                    },
                    error: function (xhr, status, error) {
                        ajaxSubmitField($form);
                    }
                });
            });
        }
    });
}

function initAjaxForm(formSelector)
{
    $(formSelector).off('submit'); // prevent sending twice
    ajaxSubmitForm($(formSelector));
}

function ajaxSubmitForm(formObject)
{
    formObject.one('submit', function (event) {
        event.preventDefault();

        var $form = $(this);
        $form.validate({
            errorPlacement: function (error, element) {

            }
        });
       
        if (!$form.valid())
        {
            var errorMessage = "Please fill in the required fields.";
            var isRequired = false;
            var isFirstIterate = true;
            $("label[class='error']").hide();

            $('#' + formObject.attr('id') + ' [required]').each(function ()
            {
                var fieldType = $(this).attr("type");
                if ("text" !== fieldType && "password" !== fieldType && "email" !== fieldType && "textarea" !== fieldType)
                {
                    var optionValue = $(this).parent(".dropdown").children(".default.text").html();
                    var dataTarget = $(this).prevAll("input[type=text]").attr("data-target");
                    
                    if (optionValue === "Choose your Option")
                    {
                        $(this).parent(".dropdown").parent(".field").addClass("error");
                        isRequired = true;
                    } else
                    {
                        $(this).parent(".dropdown").parent(".field").removeClass("error").addClass("valid");
                    }
                }

                else if (fieldType === "email")
                {
                    if (validateEmail($(this).val()) === false)
                    {
                        isRequired = true;
                        if ($(this).val().length !== 0)
                        {
                            if (isFirstIterate)
                            {
                                errorMessage = "Invalid email format. Valid example is: yourname@gmail.com.";
                                $(this).select();
                            }
                        }
                        $(this).parent(".input").parent(".field").addClass("error");
                    }
                    else
                    {
                        $(this).parent(".input").parent(".field").removeClass("error");
                    }
                }
                else
                {
                    if ($(this).val() !== null)
                    {
                        if ($(this).val().length === 0)
                        {
                            isFirstIterate = false;
                            isRequired = true;
                            $(this).parent(".input").parent(".field").addClass("error");
                        }
                        else
                        {
                            $(this).parent(".input").parent(".field").removeClass("error");
                        }
                    }
                }
            });
            if (isRequired)
            {
                $("#pageMessage").children('label').html(errorMessage);
                $("#pageMessage").addClass("error").show();
            }
            
            ajaxSubmitForm($form);
            return false;
        }
        else
        {
            var formData = new FormData();
            Pace.track(function () {
                $.ajax({
                    type: $form.attr('method'),
                    url: $form.attr('action'),
                    data: $form.serialize(),
                    success: function (data, status)
                    {
                        if (formObject.attr("id") === "f_addFieldItem" && data.success === undefined)
                        {
                            updateCombofieldById($("#f_addFieldItem #TempFieldId").val(), $("#FieldItemName").val());
                        }
                        //if the submit is success
                        if (data.success)
                        {
                            $("#pageMessage").children('label').html(data.message);
                            $("#pageMessage").removeClass("error").addClass("positive").show();

                            //if the page should redirect after submit
                            if ((data.id === "Issue") || (data.id === "EditIssue"))
                            {

                            }
                            else
                            {
                                if (data.isRedirect)
                                {
                                    window.location.href = '/' + data.redirectUrl;
                                }

                                else
                                {
                                    location.reload();
                                }
                            }
                        }
                        //if form is valid, but backend validation is invalid
                        else
                        {
                            //if the submit is error from backend, adjust classes of fields
                            $('[required]').each(function ()
                            {
                                var fieldType = $(this).attr("type");
                                if ("text" !== fieldType && "password" !== fieldType && "email" !== fieldType && "textarea" !== fieldType) {
                                    var optionValue = $(this).parent(".dropdown").children(".default.text").html();
                                    var dataTarget = $(this).prevAll("input[type=text]").attr("data-target");

                                    if (optionValue === "Choose your Option") {
                                        $(this).parent(".dropdown").parent(".field").addClass("error");
                                        isRequired = true;
                                    }
                                    else
                                    {
                                        $(this).parent(".dropdown").parent(".field").removeClass("error").addClass("valid");
                                    }
                                }
                                else
                                {
                                    if ($(this).val() !== null) 
                                    {
                                        if ($(this).val().length === 0) 
                                        {
                                            isRequired = true;
                                            $(this).parent(".input").parent(".field").addClass("error");
                                        }
                                        else
                                        {
                                            $(this).parent(".input").parent(".field").removeClass("error");
                                        }
                                    }
                                }

                            });
                            
                            $("#pageMessage").children('label').html(data.message);
                            $("#pageMessage").addClass("error").show();
                            ajaxSubmitForm($form);
                        }
                    },
                    error: function (xhr, status, error)
                    {
                        ajaxSubmitForm($form);
                    }
                });
            });
        }
    });
}

$("select").on('change', function ()
{
    if ($(this).val() !== "Choose your option") {
        var optionValue = $(this).find(":selected").val();
        var dataTarget = $(this).prevAll("input[type=text]").attr("data-target");
        $('input[data-target=' + dataTarget + ']').removeClass("invalid").addClass("valid");
    }
    else {
    }
});

$(document).ready(function ()
{
    activeSubModules();
});

//new code for adding "active" class in active sub modules
function activeSubModules()
{
    var hasMatch = false;

    //get url ex: "/Maintenance/Field"
    var url = window.location.pathname.split("/");

    //change url to only /Controller/Action
    url = "/" + url[1] + "/" + url[2];

    //add "active" class to a(link) with href of the url
    $('a[href$="' + String(url) + '"]').addClass("active");

    //if a(link) exist
    if ($('a[href$="' + String(url) + '"]').length > 0)
    {
        //if url is from button and not from tabs
        hasMatch = true;

        //set a local value to be passed on the next page, this value will be use if the url is not from tabs
        localStorage.setItem("previousSubModule", String(url));
    }
    
    //if url is not from tabs(module, subModule)
    if (url != "/Maintenance/ChangePasswordUser" && url != "/Maintenance/ChangeCompanyName")
    {
        if (hasMatch == false)
        {
            //add a active class to previous sub module that was active
            $('a[href$="' + String(localStorage.getItem("previousSubModule")) + '"]').addClass("active");
        }
    }
}

$("input.validate, textarea.validate").change(function ()
{
    //Email field on change
    if ($(this).attr("Type") === "email")
    {
        if (validateEmail($(this).val()) === false)
        {
            $(this).parent(".input").parent(".field").addClass("error");
            $("#pageMessage").children('label').html("Invalid email format. Valid example is: yourname@gmail.com.");
            $("#pageMessage").addClass("error").show();
        }
        else
        {
            $(this).parent(".input").parent(".field").removeClass("error");
            $("#pageMessage").hide();
        }
    }
    else
    {
        if ($(this).val().length === 0)
        {
            $(this).parent(".input").parent(".field").addClass("error");
        }
        else
        {
            $(this).parent(".input").parent(".field").removeClass("error");
        }
    }
});

$(".dropdown").children("input[type=hidden]").change(function ()
{
    if ($(this).val() !== "0")
    {
        $(this).parent(".dropdown").parent(".field").removeClass("error").addClass("valid");

    }
    else
    {
        $(this).parent(".dropdown").parent(".field").addClass("error");
    }
});

$("input:file").change(function ()
{
    if ($(this).val() !== "0")
    {
        $(this).parent(".field").removeClass("error").addClass("valid");

    }
    else
    {
        $(this).parent(".field").addClass("error");
    }
});

function validateEmail(email)
{
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(String(email).toLowerCase());
}
