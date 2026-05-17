// wwwroot/js/charts.js
// Requires Chart.js loaded via CDN in index.html

window.initRevenueChart = function () {
    const ctx = document.getElementById('revenueChart');
    if (!ctx) return;

    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

    const revenue = [120, 145, 132, 160, 175, 155, 190, 210, 185, 230, 215, 240];
    const expenses = [80, 95, 88, 105, 110, 98, 115, 125, 108, 135, 125, 140];
    const profit = revenue.map((r, i) => r - expenses[i]);

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: months,
            datasets: [
                {
                    label: 'Revenue',
                    data: revenue,
                    backgroundColor: 'rgba(99,102,241,0.85)',
                    borderRadius: 4,
                    borderSkipped: false,
                    barPercentage: 0.55,
                    categoryPercentage: 0.7,
                },
                {
                    label: 'Expenses',
                    data: expenses,
                    backgroundColor: 'rgba(239,68,68,0.75)',
                    borderRadius: 4,
                    borderSkipped: false,
                    barPercentage: 0.55,
                    categoryPercentage: 0.7,
                },
                {
                    label: 'Profit',
                    data: profit,
                    backgroundColor: 'rgba(16,185,129,0.8)',
                    borderRadius: 4,
                    borderSkipped: false,
                    barPercentage: 0.55,
                    categoryPercentage: 0.7,
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#1E2130',
                    borderColor: '#2A2F45',
                    borderWidth: 1,
                    titleColor: '#F9FAFB',
                    bodyColor: '#9CA3AF',
                    padding: 10,
                    cornerRadius: 8,
                    callbacks: {
                        label: ctx => ` ${ctx.dataset.label}: $${ctx.parsed.y}K`
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    border: { display: false },
                    ticks: {
                        color: '#6B7280',
                        font: { size: 10, family: 'DM Sans' }
                    }
                },
                y: {
                    grid: {
                        color: '#1E2130',
                        drawBorder: false,
                    },
                    border: { display: false, dash: [4, 4] },
                    ticks: {
                        color: '#6B7280',
                        font: { size: 10, family: 'DM Sans' },
                        callback: v => '$' + v + 'K'
                    }
                }
            }
        }
    });
};