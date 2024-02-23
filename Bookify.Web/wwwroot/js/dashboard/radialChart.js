function loadRadialBarChart(chartId, data) {
    var options = {
        chart: {
            height: 100,
            width: 100,
            type: "radialBar",
            sparkline: {
                enabled: true
            },
            offsetY: -10,
        },

        series: data.series,

        plotOptions: {
            radialBar: {
                dataLabels: {
                    value: {
                        offsetY: -10,
                    }
                }
            }
        },
        labels: [""],
    };

    var chart = new ApexCharts(document.querySelector("#" + chartId), options);

    chart.render();

}