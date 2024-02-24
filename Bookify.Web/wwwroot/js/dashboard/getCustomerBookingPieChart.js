$(document).ready(function () {
    loadCustomerBookingPieChart();
})

function loadCustomerBookingPieChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetBookigPieChartData",
        type: "GET",
        data: 'json',
        success: function (data) {

            loadPieChart("customerBookingsPieChart", data);

            $(".chart-spinner").hide();
        }
    })
}

function loadPieChart(chartId, data) {
    var chartColors = getChartColorsArray(chartId);

    var options = {
        series: data.series,
        labels: data.labels,
        colors: chartColors,
        chart: {
            type: 'pie',
            width: 380
        },
        stroke: {
            show: false
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            labels: {
                colors: "#fff",
                userSeriesColors: true
            },
        },
    };

    var chart = new ApexCharts(document.querySelector("#" + chartId), options);
    chart.render();
}