
$(document).ready(function ()
{
    initAjaxForm("form[data-async]");
    initAjaxForm("#f_login", "validate");

    $('.ui.dropdown').dropdown();
    $('.button').popup();
});

$('form').submit(function ()
{
    if ($(this).valid())
    {
        $(this).find(':submit').attr('disabled', 'disabled');
    }
});