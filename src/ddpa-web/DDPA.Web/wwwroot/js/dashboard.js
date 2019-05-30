var userRole = $("#userRole").val();

$(document).ready(function ()
{
    $('.ui.dropdown').dropdown();
    $('.button').popup();
    initFields();

    if ($('#LoginState').attr("value") == "1") {
        $('#userGuideInfoModal').modal('show');
    }
});

function initFields()
{
    var $canvas = $("#dataSetPieChart");
    var summaryUrl = "/Dashboard/GetSummary";
    $.ajax({
        url: summaryUrl,
        type: "POST",
        cache: false,
        success: function (data)
        {
            populatePieCharts(data.data);
        }
    });
}

function populatePieCharts(data)
{
    var hasData = false;
    var dataSets;
    var labels;
    var counts;
    var ctx;
    var backgroundColor =
        ['#3366CC', '#DC3912', '#FF9900', '#109618', '#990099', '#3B3EAC', '#0099C6', '#DD4477', '#66AA00', '#B82E2E', '#316395', '#994499', '#22AA99', '#AAAA11', '#6633CC', '#E67300', '#8B0707', '#329262', '#5574A6', '#3B3EAC']

    var options = {
        tooltips: {
            enabled: true
        },
        plugins: {
            datalabels: {
                formatter: (value, ctx) =>
                {

                    let datasets = ctx.chart.data.datasets;
                    var tempValue = Number(value);
                    if (datasets.indexOf(ctx.dataset) === datasets.length - 1)
                    {
                        let sum = datasets[0].data.reduce((a, b) => Number(a) + Number(b), 0);
                        let percentage = Math.round((tempValue / sum) * 100) + '%';
                        return percentage;
                    } else
                    {
                        return percentage;
                    }
                },
                display: function (context)
                {
                        return context.dataset.data[context.dataIndex] !== '0'; 
                },
                color: '#fff'
            }
        }
    };

    //Data sets chart
    ctx = document.getElementById("dataSetPieChart");
    
    //Check if has no data
    if (data.dataSet.label === "no data")
    {
        dataSets =
            [{
                backgroundColor: ['#D3D3D3'],
                data: [100]
            }];

        labels = ["No data found"];
    }
    else if (data.dataSet.label !== "no data")
    {
        labels = data.dataSet.label.split(',');
        counts = data.dataSet.count.split(',');
        dataSets =
            [
                {
                    data: counts,
                    backgroundColor: backgroundColor
                }
            ];
    }

    pieChart = new Chart(ctx, {
        type: 'pie',
        data:
        {
            datasets: dataSets,
            labels: labels
        },
        options: options
    });
    //Data sets chart

    //Storage chart
    hasData = false;
    ctx = document.getElementById("storagePieChart");

    //Check if has no data
    if (data.storage.label === "no data")
    {
        dataSets =
            [{
                backgroundColor: ['#D3D3D3'],
                data: [100]
            }];

        labels = ["No data found"];
    }
    else if (data.storage.label !== "no data")
    {
        labels = data.storage.label.split(',');
        counts = data.storage.count.split(',');

        dataSets =
            [
                {
                    data: counts,
                    backgroundColor: backgroundColor
                }
            ];
    }

    pieChart = new Chart(ctx, {
        type: 'pie',
        data:
        {
            datasets: dataSets,
            labels: labels
        },
        options: options
    });
    //Storage chart

    //External Party chart
    hasData = false;
    ctx = document.getElementById("externalPartyPieChart");

    //Check if has no data
    if (data.externalParty.label === "no data")
    {
        dataSets =
            [{
                backgroundColor: ['#D3D3D3'],
                data: [100]
            }];

        labels = ["No data found"];
    }
    else if (data.externalParty.label !== "no data")
    {
        labels = data.externalParty.label.split(',');
        counts = data.externalParty.count.split(',');

        dataSets =
            [
                {
                    data: counts,
                    backgroundColor: backgroundColor
                }
            ];
    }

    pieChart = new Chart(ctx, {
        type: 'pie',
        data:
        {
            datasets: dataSets,
            labels: labels
        },
        options: options
    });

    hasData = false;
    ctx = document.getElementById("IssueRiskPieChart");
        
    //Check if has no data
    if (data.issue.label === "no data")
    {
        dataSets =
            [{
                backgroundColor: ['#D3D3D3'],
                data: [100]
            }];

        labels = ["No data found"];
    }
    else if (data.issue.label !== "no data")
    {
        labels = data.issue.label.split(',');
        counts = data.issue.count.split(',');

        dataSets =
            [
                {
                    data: counts,
                    backgroundColor: backgroundColor
                }
            ];
    }

    pieChart = new Chart(ctx, {
        type: 'pie',
        data:
        {
            datasets: dataSets,
            labels: labels
        },
        options: options
    });
}

$("#datasetDownloadPDF").on("click", function ()
{

    var dateFormat = formatDateToString();
    var canvas = document.getElementById("dataSetPieChart");
    //creates image
    var canvasImg = canvas.toDataURL("image/png", 1.0);
    var doc = new jsPDF('p', "mm", "letter");

    var userDept = $("#userDept").val();
    //creates PDF from img
    if (userDept === "")
    {
        doc.text("Data Set Owners", 20, 20);
    }
    else if (userDept !== "")
    {
        doc.text("Data Set Owners", 20, 20);
    }

    doc.addImage(canvasImg, 'PNG', 50, 50, 120, 120);
    doc.save("DATASET SUMMARY " + dateFormat);
});

$("#storageDownloadPDF").on("click", function ()
{

    var dateFormat = formatDateToString();
    var canvas = document.getElementById("storagePieChart");
    //creates image
    var canvasImg = canvas.toDataURL("image/png", 1.0);
    var doc = new jsPDF('p', "mm", "letter");

    var userDept = $("#userDept").val();
    //creates PDF from img
    if (userDept === "")
    {
        doc.text("Storage", 20, 20);
    }
    else if (userDept !== "")
    {
        doc.text("Storage", 20, 20);
    }

    doc.addImage(canvasImg, 'PNG', 50, 50, 120, 120);
    doc.save("STORAGE SUMMARY " + dateFormat);
});

$("#externalPartyDownloadPDF").on("click", function ()
{
    var dateFormat = formatDateToString();
    var canvas = document.getElementById("externalPartyPieChart");
    //creates image
    var canvasImg = canvas.toDataURL("image/png", 1.0);
    var doc = new jsPDF('p', "mm", "letter");

    var userDept = $("#userDept").val();
    //creates PDF from img
    if (userDept === "")
    {
        doc.text("External Parties", 20, 20);
    }
    else if (userDept !== "")
    {
        doc.text("External Parties", 20, 20);
    }

    doc.addImage(canvasImg, 'PNG', 50, 50, 120, 120);
    doc.save("EXTERNAL PARTY SUMMARY " + dateFormat);
});

$("#issueRiskDownloadPDF").on("click", function ()
{
    var dateFormat = formatDateToString();
    var canvas = document.getElementById("IssueRiskPieChart");
    //creates image
    var canvasImg = canvas.toDataURL("image/png", 1.0);
    var doc = new jsPDF('p', "mm", "letter");

    var userDept = $("#userDept").val();
    //creates PDF from img
    if (userDept === "")
    {
        doc.text("Notes", 20, 20);
    }
    else if (userDept !== "")
    {
        doc.text("Notes", 20, 20);
    }

    doc.addImage(canvasImg, 'PNG', 50, 50, 120, 120);
    doc.save("NOTES SUMMARY " + dateFormat);
});

$("#issueRiskBarDownloadPDF").on("click", function ()
{
    var dateFormat = formatDateToString();
    var canvas = document.getElementById("issueRiskBarChart");
    //creates image
    var canvasImg = canvas.toDataURL("image/png", 1.0);
    var doc = new jsPDF('l', "mm", "letter");

    var userDept = $("#userDept").val();
    //creates PDF from img
    if (userDept === "")
    {
        doc.text("Issue Risk Level", 20, 20);
    }
    else if (userDept !== "")
    {
        doc.text("Issue Risk Level", 20, 20);
    }

    doc.addImage(canvasImg, 'PNG', 10, 50, 250, 100);
    doc.save("ISSUE RISK SUMMARY " + dateFormat);
});

function formatDateToString()
{
    const monthNames = ["January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    const monthNamesInitial = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];
    var date = new Date($.now());
    var dd = (date.getDate() < 10 ? '0' : '') + date.getDate();
    var MMM = monthNamesInitial[date.getMonth()];
    var yyyy = date.getFullYear();
    return (dd.toString() + " " + MMM.toString() + " " + yyyy.toString());
}
