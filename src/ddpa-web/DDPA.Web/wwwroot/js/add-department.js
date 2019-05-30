
$(document).ready(function ()
{
    initAjaxForm("form[data-async]");
    initAjaxForm("#f_addDepartment", "validate");
    $('.ui.dropdown').dropdown();
    $('.button').popup();
});
