"use strict";

// Cicle Chart
Circles.create({
	id:           'task-complete',
	radius:       50,
	value:        80,
	maxValue:     100,
	width:        5,
	text:         function(value){return value + '%';},
	colors:       ['#36a3f7', '#fff'],
	duration:     400,
	wrpClass:     'circles-wrp',
	textClass:    'circles-text',
	styleWrapper: true,
	styleText:    true
})

//Current Month sales Chart

async function CurrentloadSalesData() {
	try {
		const response = await fetch('/api/saleapi/getMonthlySales');
		const data = await response.json();

		// Check if totalSales exists and is a valid number
		const totalSalesAmount = data.totalSales;
		if (totalSalesAmount && !isNaN(totalSalesAmount)) {
			document.querySelector('.card-body h1').textContent = `Rs ${totalSalesAmount.toFixed(2)}`;
		} else {
			console.error('Total sales amount is invalid:', totalSalesAmount);
		}

		// Load the sales data for the line chart
		const dailySales = data.dailySales;  // Keep dailySales as numbers
		const labels = data.labels;  // Labels from the API

		var dailySalesChart = document.getElementById('dailySalesChart').getContext('2d');

		var myDailySalesChart = new Chart(dailySalesChart, {
			type: 'line',
			data: {
				labels: labels,  // Use the Labels from the API
				datasets: [{
					label: "Sale Amount",
					fill: true,
					backgroundColor: "rgba(255,255,255,0.2)",
					borderColor: "#fff",
					borderCapStyle: "butt",
					borderDash: [],
					borderDashOffset: 0,
					pointBorderColor: "#fff",
					pointBackgroundColor: "#fff",
					pointBorderWidth: 1,
					pointHoverRadius: 5,
					pointHoverBackgroundColor: "#fff",
					pointHoverBorderColor: "#fff",
					pointHoverBorderWidth: 1,
					pointRadius: 1,
					pointHitRadius: 5,
					data: dailySales  // Use the DailySales as numbers (without "Rs")
				}]
			},
			options: {
				maintainAspectRatio: false,
				legend: {
					display: false
				},
				animation: {
					easing: "easeInOutBack"
				},
				scales: {
					yAxes: [{
						display: false,  // Hide y-axis completely
						ticks: {
							fontColor: "rgba(0,0,0,0.5)",
							fontStyle: "bold",
							beginAtZero: true,
							maxTicksLimit: 10
						},
						gridLines: {
							drawTicks: false,
							display: false
						}
					}],
					xAxes: [{
						display: false,  // Hide x-axis completely
						gridLines: {
							zeroLineColor: "transparent"
						},
						ticks: {
							padding: -20,
							fontColor: "rgba(255,255,255,0.2)",
							fontStyle: "bold"
						}
					}]
				},
				tooltips: {
					callbacks: {
						label: function (tooltipItem) {
							return 'Rs ' + tooltipItem.yLabel.toLocaleString();  // Prepend "Rs" to tooltip labels
						}
					}
				}
			}
		});
	} catch (error) {
		console.error('Error fetching sales data:', error);
	}
}

// Load the sales data when the page is ready
CurrentloadSalesData();

// Function to get the start date of the current month and today's date
function getCurrentMonthDates() {
	const now = new Date();
	const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);

	const monthNames = [
		"January", "February", "March", "April", "May", "June",
		"July", "August", "September", "October", "November", "December"
	];

	const startMonth = monthNames[startOfMonth.getMonth()];
	const endMonth = monthNames[now.getMonth()];  // Today’s month (same as startOfMonth)

	const startDay = startOfMonth.getDate();  // Start day is always 1
	const endDay = now.getDate();  // Ending day is today's date

	return `${startMonth} ${startDay} - ${endMonth} ${endDay}`;
}

// Set the date range in the card-category
document.getElementById('salesPeriod').textContent = getCurrentMonthDates();


/*----------------------------------------------------------------------------------------------------*/
// Sales statistics
async function loadSalesData() {
	try {
		const response = await fetch('https://localhost:44372/api/saleapi/GetSalesData');

		if (!response.ok) {
			throw new Error(`Error fetching data: ${response.statusText}`);
		}

		const salesData = await response.json();

		console.log(salesData); // Check the data structure

		const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

		const datasets = [];

		salesData.forEach(data => {
			const monthIndex = data.month - 1; // Adjust to 0-based index
			data.sales.forEach(categoryData => {
				let existingDataset = datasets.find(d => d.label === categoryData.categoryName);

				if (!existingDataset) {
					const color = getRandomColor(); // Generate the color once for both line and background
					existingDataset = {
						label: categoryData.categoryName,
						borderColor: color,
						pointBackgroundColor: color,
						pointRadius: 0,
						backgroundColor: getRandomLightColor(color), // Use the same base color for background
						fill: true,
						borderWidth: 2,
						data: Array(12).fill(0) // Create an array of 12 months initialized to 0
					};
					datasets.push(existingDataset);
				}

				existingDataset.data[monthIndex] += categoryData.totalQty; // Add quantity to the correct month
			});
		});

		var ctx = document.getElementById('statisticsChart').getContext('2d');
		var statisticsChart = new Chart(ctx, {
			type: 'line',
			data: {
				labels: months,
				datasets: datasets
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				legend: {
					display: true
				},
				tooltips: {
					bodySpacing: 4,
					mode: "nearest",
					intersect: 0,
					position: "nearest",
					xPadding: 10,
					yPadding: 10,
					caretPadding: 10
				},
				layout: {
					padding: { left: 5, right: 5, top: 15, bottom: 15 }
				},
				scales: {
					yAxes: [{
						ticks: {
							fontStyle: "500",
							beginAtZero: true,
							maxTicksLimit: 5,
							padding: 10
						},
						gridLines: {
							drawTicks: false,
							display: true
						}
					}],
					xAxes: [{
						gridLines: {
							zeroLineColor: "transparent"
						},
						ticks: {
							padding: 10,
							fontStyle: "500"
						}
					}]
				}
			}
		});

	} catch (error) {
		console.error('Error loading sales data:', error);
	}
}

// Helper function to generate random colors for the chart
function getRandomColor() {
	const letters = '0123456789ABCDEF';
	let color = '#';
	for (let i = 0; i < 6; i++) {
		color += letters[Math.floor(Math.random() * 16)];
	}
	return color;
}

// Helper function to generate a very light color based on a base color for the background
function getRandomLightColor(baseColor) {
	const rgbaColor = hexToRgb(baseColor);
	return `rgba(${rgbaColor.r}, ${rgbaColor.g}, ${rgbaColor.b}, 0.1)`; // Set the alpha to 0.1 for a light background
}

// Helper function to convert hex color to rgb
function hexToRgb(hex) {
	let r = 0, g = 0, b = 0;

	// 3 digits
	if (hex.length === 4) {
		r = parseInt(hex[1] + hex[1], 16);
		g = parseInt(hex[2] + hex[2], 16);
		b = parseInt(hex[3] + hex[3], 16);
	}
	// 6 digits
	else if (hex.length === 7) {
		r = parseInt(hex[1] + hex[2], 16);
		g = parseInt(hex[3] + hex[4], 16);
		b = parseInt(hex[5] + hex[6], 16);
	}

	return { r: r, g: g, b: b };
}

// Call the function to load the sales data and render the chart
loadSalesData();

/*------------------------------------------------------------------------*/

// Format the date range using the function
const formattedDateRange = getCurrentMonthDates();
document.getElementById('dateRange').innerText = formattedDateRange;

// API call to get total items sold for the current month
fetch('/api/saleapi/totalItemsSoldThisMonth')
	.then(response => response.json())
	.then(data => {
		// Check if the data contains totalItemsSold and display it
		if (data && data.totalItemsSold !== undefined) {
			// Only update the number part, not the whole content
				document.getElementById('itemsSoldNumber').innerText = data.totalItemsSold;
		} else {
			console.error("Invalid data:", data);
		}
	})
	.catch(error => {
		console.error('Error fetching total items sold:', error);
	});

/*-------------------------------------------------------------------------------------*/

fetch('/api/jewelryitems/lowstock')
	.then(response => response.json())
	.then(data => {
		const tableBody = document.getElementById('jewelryItemsTableBody');
		tableBody.innerHTML = ''; // Clear any existing rows

		data.forEach(item => {
			const price = parseFloat(item.price) || 0; // Ensures price is treated as a number
			const row = document.createElement('tr');

			row.innerHTML = `
            <tr>
                <th scope="row" style="display: flex; align-items: center;">
                    <span style="display: inline-flex; justify-content: center; align-items: center; background-color: #ffc107; color: black; padding: 8px; border-radius: 50%; width: 30px; height: 30px; font-size: 14px; text-align: center; margin-right: 10px;">
                        <i class="fa fa-warning"></i>
                    </span>
                    <span style="display: inline-block; white-space: normal; word-wrap: break-word; max-width: 500px;">${item.name}</span>
                </th>
                <td class="text-end">${item.identificationID}</td>
                <td class="text-end">${item.stockLevel}</td>
                <td class="text-end">${item.categoryName}</td>
                <td class="text-end">
                    <a href="Admin/AddEditJewelryItem/${item.jewelryItemId}?redirectPage=Index" class="btn btn-primary btn-sm">
                        Update
                    </a>
                </td>
            </tr>
            `;

			tableBody.appendChild(row);
		});
	})
	.catch(error => console.error('Error fetching data:', error));

/*-----------------------------------------------------------------------------------------------*/
async function loadSalesDataForcast() {
	try {
		const response = await fetch('https://localhost:44372/api/saleapi/GetSalesDataByItem');
		if (!response.ok) throw new Error(`Error fetching data: ${response.statusText}`);
		const salesData = await response.json();

		const forecastData = [];

		const groupedSales = salesData.reduce((acc, data) => {
			if (!acc[data.itemName]) acc[data.itemName] = [];
			acc[data.itemName].push({
				month: data.month,
				totalQty: data.totalQty,
				categoryName: data.categoryName 
			});
			return acc;
		}, {});

		for (const itemName in groupedSales) {
			const itemSales = groupedSales[itemName];

			itemSales.sort((a, b) => a.month - b.month);

			const quantities = itemSales.map(sale => sale.totalQty);

			// Calculating average monthly growth rate (AI/ML logic)
			const growthRates = [];
			for (let i = 1; i < quantities.length; i++) {
				const prevMonthQty = quantities[i - 1];
				if (prevMonthQty > 0) {
					const growthRate = (quantities[i] - prevMonthQty) / prevMonthQty;
					growthRates.push(growthRate);
				}
			}

			// Calculating the average growth rate
			const avgGrowthRate = growthRates.length > 0
				? growthRates.reduce((a, b) => a + b) / growthRates.length
				: 0;

			// Forecast for next month's sales
			const lastMonthSales = quantities[quantities.length - 1] || 0;
			const forecastedSales = Math.round(lastMonthSales * (1 + avgGrowthRate));
			const categoryName = itemSales[0].categoryName;


			const color = getRandomColor();
			forecastData.push({
				label: itemName,
				backgroundColor: getRandomLightColor(color),
				borderColor: color,
				borderWidth: 2,
				data: [forecastedSales],
                category: categoryName 

			});
		}

		const forecastCtx = document.getElementById('forecastChart').getContext('2d');
		new Chart(forecastCtx, {
			type: 'bar',
			data: {
				labels: ["Next Month"],
				datasets: forecastData
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				legend: { display: false },
				tooltips: {
					callbacks: {
						title: function (tooltipItem, data) {
							const dataset = data.datasets[tooltipItem[0].datasetIndex];
							return [dataset.category];
						},
						label: function (tooltipItem, data) {
							const dataset = data.datasets[tooltipItem.datasetIndex];
							const category = dataset.category;
							const itemName = dataset.label;

							return [`${itemName}: ${tooltipItem.yLabel}`];
						}
					}
				},
				scales: {
					yAxes: [{ ticks: { beginAtZero: true } }]
				}
			}
		});

	} catch (error) {
		console.error('Error loading sales data:', error);
	}
}

loadSalesDataForcast();
