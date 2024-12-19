const itemsPerPage = 8;
const itemsContainer = document.getElementById('itemsContainer');
const loadMoreButton = document.getElementById('loadMore');
const indexLoadMoreButton = document.getElementById('IndexloadMore');  // Use a clearer variable name
let currentPage = 1;
let items = []; // Data from API

// Initial load
$(document).ready(function () {
    cloadCategories();
    const catID = new URLSearchParams(window.location.search).get('catID');
    const searchQuery = new URLSearchParams(window.location.search).get('search');
    const storedItems = sessionStorage.getItem('itemsData');

    if (storedItems) {

        const data = JSON.parse(storedItems);

        // Map the retrieved items to match your desired structure
        items = data.map(item => ({
            id: item.jewelryItemId,
            title: item.name,
            description: item.description,
            imageUrl: item.imageUrl,
            price: item.price,
            category: item.category
        }));

        // Clear current items and display the retrieved items
        itemsContainer.innerHTML = ''; // Clear current items
        displayItems(currentPage);
    } else if (searchQuery) {
        // Load and filter items based on search query
        loadItems(null, searchQuery);
    } else {
        // If no search query or items, load items as usual
        loadItems(catID);
    }

});

function displayItems(page) {
    const searchQuery = new URLSearchParams(window.location.search).get('search');

    // If items array is empty, show a message and return early
    if (items.length === 0) {
        const noResultsMessage = document.createElement('div');
        noResultsMessage.className = 'col-12 text-center'; // Styling class for message
        noResultsMessage.innerHTML = `<p>No results found for "${searchQuery}"</p>`;
        itemsContainer.innerHTML = ''; // Clear existing items for no results message
        itemsContainer.appendChild(noResultsMessage);
        return; // Exit function if no items
    }

    // Only clear itemsContainer if we are on the first page
    if (page === 1) {
        itemsContainer.innerHTML = ''; // Clear existing items for the first load
    }

    const startIndex = (page - 1) * itemsPerPage;
    const endIndex = Math.min(startIndex + itemsPerPage, items.length);

    // Append new items to the container instead of replacing
    for (let i = startIndex; i < endIndex; i++) {
        const item = items[i];
        const itemElement = document.createElement('div');
        itemElement.className = `col-sm-6 col-md-4 col-lg-3 p-b-35 isotope-item ${item.category}`;
        itemElement.innerHTML = `
            <div class="block2">
                <div class="block2-pic hov-img0">
                    <a href="https://localhost:44372/Home/item?itmID=${item.id}">
                            <img src="${item.imageUrl.startsWith('http') ? item.imageUrl : `https://localhost:44372/${item.imageUrl}`}" alt="IMG-PRODUCT">
                    </a>
                </div>
                <div class="block2-txt flex-w flex-t p-t-14">
                    <div class="block2-txt-child1 flex-col-l">
                        <a href="https://localhost:44372/Home/item?itmID=${item.id}" class="stext-104 cl4 hov-cl1 trans-04 js-name-b2 p-b-6">
                            ${item.title}
                        </a>
                    </div>
                </div>
            </div>
        `;
        itemsContainer.appendChild(itemElement); // Append each item
    }
}

function loadItems(catID = null, searchQuery = null) {
    const url = catID ? `/api/index/ProByCategory/${catID}` : `/api/index/Pro`;

    $.ajax({
        contentType: "application/json; charset=utf-8",
        type: "GET",
        dataType: "json",
        url: url,
        success: function (data) {
            // Filter the items based on the search query, if provided
            if (searchQuery) {
                data = data.filter(item => item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                    item.description.toLowerCase().includes(searchQuery.toLowerCase()));
            }

            items = data.map((item) => ({
                id: item.jewelryItemId,
                title: item.name,
                description: item.description,
                imageUrl: item.imageUrl,
                price: item.price, // Assuming the API provides a price
                category: item.category // Assuming the API provides a category
            }));

            itemsContainer.innerHTML = '';  // Clear current items
            displayItems(currentPage);
        },
        error: function (jqXHR, exception) {
            var msg = 'Error loading items.';
            ShowError(msg);
        }
    });
}
// Add event listener only if loadMoreButton exists
if (loadMoreButton) {
    loadMoreButton.addEventListener('click', (e) => {
        e.preventDefault();
        currentPage++;
        displayItems(currentPage);
    });
}

// Add event listener only if indexLoadMoreButton exists
if (indexLoadMoreButton) {
    indexLoadMoreButton.addEventListener('click', (e) => {
        e.preventDefault();

        window.location.href = 'Home/products';  // Redirect to the products page
    });
}

function cloadCategories() {
    $.ajax({
        url: '/api/index/cat',  // Your existing API endpoint
        method: 'GET',
        success: function (data) {
            var filterButtons = $('#filter-buttons');
            const catID = new URLSearchParams(window.location.search).get('catID'); // Get the catID from URL

            // Add 'All Products' button with cat-id 'all'
            filterButtons.append('<button class="stext-106 cl6 hov1 bor3 trans-04 m-r-32 m-tb-5 how-active1" data-cat-id="all">All Products</button>');

            // Loop through the categories and create a filter button for each
            $.each(data, function (index, category) {
                var categoryName = category.categoryName;  // Assuming 'categoryName' is the property in your Category model
                var categoryID = category.categoryId; // Category ID
                var buttonHTML = `<button class="stext-106 cl6 hov1 bor3 trans-04 m-r-32 m-tb-5" data-cat-id="${categoryID}">${categoryName}</button>`;
                filterButtons.append(buttonHTML);
            });

            // Add click event to filter buttons
            $('#filter-buttons button').on('click', function () {
                const catID = $(this).data('cat-id');

                currentPage = 1;  // Reset the page number when a filter is applied

                // If the clicked button is 'All Products', pass null to loadItems to load all products
                if (catID === 'all') {
                    loadItems(null);  // Load all products
                } else {
                    loadItems(catID);  // Load items based on the selected category
                }

                // Remove 'how-active1' from all buttons and apply it to the clicked one
                $('#filter-buttons button').removeClass('how-active1');
                $(this).addClass('how-active1');
            });
            if (catID) {
                $('#filter-buttons button').removeClass('how-active1');
                $(`#filter-buttons button[data-cat-id="${catID}"]`).addClass('how-active1');
            }
        },
        error: function () {
            console.error('Failed to load categories');
        }
    });
}
