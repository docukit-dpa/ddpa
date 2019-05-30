
var notNumberAttempt = 0;
$(document).ready(function ()
{
    initAjaxField("#reworkDoc");
    initAjaxForm("#reworkDoc", "validate");

});


function isNumber(evt)
{
    evt = (evt) ? evt : window.event;
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57))
    {
        notNumberAttempt++;
        if (notNumberAttempt >= 3)
        {
            alert("Accepts only numeric.");
            notNumberAttempt = 0;
        }
        return false;
    }
    return true;
}

$("#excelFile").on("change", function ()
{
    $("#excelUpload").submit();
});

$(":checkbox").on("change", function ()
{
    $(this).prop("value", $(this).prop("checked"));
});

$("#save_btn").on("click", function ()
{
    $("#buttonAction").val("Save");
});

$("#submit_btn").on("click", function ()
{
    $("#buttonAction").val("Submit");
});
