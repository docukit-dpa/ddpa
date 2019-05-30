
$(document).ready(function ()
{
    initAjaxForm("form[data-async]");
    initAjaxForm("#f_updateDepartment", "validate");
    $('.ui.dropdown').dropdown();
    $('.button').popup();
});
