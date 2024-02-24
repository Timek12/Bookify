$(document).ready(function () {
    loadCustomerAndBookingLineChart();
})

function loadCustomerAndBookingLineChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetMemberAndBookingLineChartData",
        type: "GET",
        data: 'json',
        success: function (data) {

            loadLineChart("newMembersAndBookingsLineChart", data);

            $(".chart-spinner").hide();
        }
    })
}

function loadLineChart(chartId, data) {
    var chartColors = getChartColorsArray(chartId);

    var options = {
        series: data.series,
        colors: chartColors,
        chart: {
            height: 350,
            type: 'line',
        },
        stroke: {
            show: false
        },
        markers: {
            size: 0,
            hover: {
                sizeOffset: 6
            }
        },
        xaxis: {
            categories: data.categories,
        },
    };

    var chart = new ApexCharts(document.querySelector("#" + chartId), options);
    chart.render();
}