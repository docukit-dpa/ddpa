
$(document).ready(function ()
{
    $('.ui.dropdown').dropdown();
    $('.button').popup();
});


$("#addresource_form").submit(function (e)
{
    var valid = 0;
    var errorMessage = "Please fill in the required fields.";
    
    if ($("#TypeOfDocument").val().length === 0)
    {
        $("#TypeOfDocument").parent(".input").parent(".field").addClass("error");
    }

    else
    {
        valid++;
        $("#TypeOfDocument").parent(".input").parent(".field").removeClass("error");
    }

    if ($("#NameOfDocument").val().length === 0)
    {
        $("#NameOfDocument").parent(".input").parent(".field").addClass("error");
    }

    else
    {
        valid++;
        $("#NameOfDocument").parent(".input").parent(".field").removeClass("error");
    }

    if ($("#file").val().length === 0)
    {
        doSubmit = false;
        $("#file").parent(".field").addClass("error");
    }

    else
    {
        var fileValue = $("#file").val();
        var extension = fileValue.substr((fileValue.lastIndexOf('.') + 1));
        if (extension !== "pdf")
        {
            $("#file").parent(".field").addClass("error");
            errorMessage = "Invalid file format. Only pdf files are allowed.";
        }

        else
        {
            valid++;
            $("#file").parent(".field").removeClass("error");
        }
    }

    if (valid !== 3)
    {
        $("#pageMessage").children('label').html(errorMessage);
        $("#pageMessage").addClass("error").show();
        e.preventDefault(e);
    }
});

$("#file").on("change", function ()
{
    var fileValue = $("#file").val();
    var extension = fileValue.substr((fileValue.lastIndexOf('.') + 1));
    if (extension !== "pdf")
    {
        $("#file").parent(".field").addClass("error");
        $("#pageMessage").children('label').html("Invalid file format. Only pdf files are allowed.");
        $("#pageMessage").addClass("error").show();
    }

    else
    {
        $("#file").parent(".field").removeClass("error");
        $("#pageMessage").hide();
    }
});
