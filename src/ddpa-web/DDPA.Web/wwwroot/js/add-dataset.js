
$(document).ready(function ()
{
    initAjaxForm("#addDataset_form", "validate");
    initAjaxForm("#updateDataset_form", "validate");

    $('.ui.dropdown').dropdown();
    $('.button').popup();
});
