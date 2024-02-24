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
            curve: "smooth",
        },
        markers: {
            size: 6,
            strokeWidth: 0,
            hover: {
                size: 7
            }
        },
        xaxis: {
            categories: data.categories,
            labels: {
                style: {
                    colors: "#ddd",
                },
            }
        },
        yaxis: {
            labels: {
                style: {
                    colors: "#fff",
                },
            }
        },
        legend: {
            labels: {
                colors: "#fff",
            },
        },
        tooltip: {
            theme: 'dark'
        }
    };

    var chart = new ApexCharts(document.querySelector("#" + chartId), options);
    chart.render();
}