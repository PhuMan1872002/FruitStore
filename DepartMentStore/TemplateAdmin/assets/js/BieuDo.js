function drawRevenueChart(labels, data) {
    const ctx = document.getElementById('revenueChart');

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu',
                data: data,
                borderWidth: 1,
                backgroundColor: ['red', 'green', 'blue', 'rgba(198, 111, 90, 0.8)', 'rgba(255, 199, 120, 0.8)']
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}