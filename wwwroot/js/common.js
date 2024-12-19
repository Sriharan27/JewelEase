$(document).ready(function () {
    updateCartCount(); // Update the cart item count on page load
    loadCategories();  // Load categories when the page is ready

    $('.js-show-cart').on('click', function () {
        loadCartItems();
    });

    // Apply perfect scrollbar to specific elements
    $('.js-pscroll').each(function () {
        $(this).css('position', 'relative');
        $(this).css('overflow', 'hidden');
        var ps = new PerfectScrollbar(this, {
            wheelSpeed: 1,
            scrollingThreshold: 1000,
            wheelPropagation: false,
        });

        // Update scrollbar on window resize
        $(window).on('resize', function () {
            ps.update();
        });
    });
});
function loadLoginPage() {
    window.location.href = '/Home/Login';  // Assuming '/Home/Login' is the URL for your login page
}

// Update the cart item count
function updateCartCount() {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    $('.icon-header-noti').attr('data-notify', cart.length);  // Update the data-notify attribute
}

// Logout functionality
function logout() {
    $.ajax({
        url: '/api/index/logout',
        type: 'POST',
        success: function () {
            window.location.href = '/Home/index'; // Redirect to home page after logout
        },
        error: function (jqXHR) {
            alert('An error occurred: ' + jqXHR.responseText);
        }
    });
}

// Bind logout button event to logout function
$('#logoutButton').on('click', function () {
    logout();
});

// Redirect to login page
$('#loginButton').on('click', function () {
    window.location.href = '/Home/login'; // Redirect to login page
});

// Load and display cart items
function loadCartItems() {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    let cartItemsContainer = $('.cart-items-dropdown');  // Cart items container

    // Clear the cart container before appending new items
    cartItemsContainer.empty();

    // Check if the cart is empty
    if (cart.length === 0) {
        cartItemsContainer.append('<li class="header-cart-item flex-w flex-t m-b-12">Your cart is empty.</li>');
        return;
    }

    // Create a map to count occurrences of each item ID
    let itemCountMap = cart.reduce((acc, itemId) => {
        acc[itemId] = (acc[itemId] || 0) + 1; // Increment count for each occurrence
        return acc;
    }, {});

    // Loop through each unique item ID
    Object.keys(itemCountMap).forEach(itemId => {
        // Validate itemId
        if (!itemId) {
            console.error('Invalid itemId:', itemId);
            return; // Skip if itemId is invalid
        }

        // Make an AJAX call to fetch the item details using the item ID
        $.ajax({
            contentType: "application/json; charset=utf-8",
            type: "GET",
            dataType: "json",
            url: `/api/index/Pro/${itemId}`, // API to get item details
            success: function (data) {
                if (data) {
                    let item = data;
                    let quantity = itemCountMap[itemId]; // Get the quantity of the current item

                    // Dynamically append each item to the cart container
                    cartItemsContainer.append(`
                        <li class="header-cart-item flex-w flex-t m-b-12">
                            <div class="header-cart-item-img">
                                <img src="${item.imageUrl.startsWith('http') ? item.imageUrl : `https://localhost:44372/${item.imageUrl}`}" alt="${item.name}">
                            </div>

                            <div class="header-cart-item-txt p-t-8">
                                <a href="#" class="header-cart-item-name m-b-18 hov-cl1 trans-04">
                                    ${item.name}
                                </a>

                                <span class="header-cart-item-info">
                                    Qty: ${quantity}
                                </span>
                            </div>
                        </li>
                    `);
                } else {
                    cartItemsContainer.append('<li class="header-cart-item flex-w flex-t m-b-12">Item details not found.</li>');
                }
            },
            error: function (jqXHR, exception) {
                let msg = '';
                if (jqXHR.status === 0) {
                    msg = 'Not connected. Verify Network.';
                } else if (jqXHR.status == 404) {
                    msg = 'Requested page not found [404].';
                } else if (jqXHR.status == 500) {
                    msg = 'Internal Server Error [500].';
                } else if (exception === 'parsererror') {
                    msg = 'Requested JSON parse failed.';
                } else if (exception === 'timeout') {
                    msg = 'Time out error.';
                } else if (exception === 'abort') {
                    msg = 'Ajax request aborted.';
                } else {
                    msg = 'Uncaught Error.\n' + jqXHR.responseText;
                }
                alert(msg);
            }
        });
    });
}

function loadCategories() {

    $.ajax({
        contentType: "application/json; charset=utf-8",
        type: "GET",
        dataType: "json",
        url: '/api/index/cat',
        success: function (data) {
            const categoriesList = document.getElementById('categories-list');
            categoriesList.innerHTML = '';  // Clear any existing categories

            data.forEach(function (category) {
                const listItem = document.createElement('li');
                listItem.className = 'p-b-10';

                listItem.innerHTML = `
                    <a href="/Home/products?catID=${category.categoryId}" class="stext-107 cl7 hov-cl1 trans-04">
                        ${category.categoryName}
                    </a>
                `;

                categoriesList.appendChild(listItem);
            });
        },
        failure: function (response) {
            ShowError(response.d);
        },
        error: function (jqXHR, exception) {
            let msg = '';
            if (jqXHR.status === 0) {
                msg = 'Not connected.\n Verify Network.';
            } else if (jqXHR.status == 404) {
                msg = 'Requested page not found. [404]';
            } else if (jqXHR.status == 500) {
                msg = 'Internal Server Error [500].';
            } else if (exception === 'parsererror') {
                msg = 'Requested JSON parse failed.';
            } else if (exception === 'timeout') {
                msg = 'Time out error.';
            } else if (exception === 'abort') {
                msg = 'Ajax request aborted.';
            } else {
                msg = 'Uncaught Error.\n' + jqXHR.responseText;
            }
            ShowError(msg);
        }
    });
}
// When camera icon is clicked, trigger the file input
document.getElementById('camera-icon').addEventListener('click', function () {
    document.getElementById('image-upload').click();
});

// Optional: Handle image upload after selection
let uploadedImage = null; // Store the uploaded image file

document.getElementById('image-upload').addEventListener('change', function (event) {
    const file = event.target.files[0];
    if (file) {
        uploadedImage = file; // Store the uploaded image file for later use
        console.log('Image uploaded:', file.name);
        const searchInput = document.querySelector('.search-input');
        searchInput.placeholder = "Image Uploaded...";
        searchInput.disabled = true;
    }
});
document.querySelector('.search-btn').addEventListener('click', function (event) {
    event.preventDefault(); // Prevent the form from submitting

    sessionStorage.removeItem('itemsData');

    const searchInput = document.querySelector('.search-input').value.trim(); // Get the search input value

    // Start the loading animation
    document.getElementById('loading-overlay').style.display = 'flex';

    // Check if an image is uploaded
    if (uploadedImage) {
        console.log('Performing search with image:', uploadedImage.name);

        // Prepare FormData to send the image file
        const formData = new FormData();
        formData.append('image', uploadedImage);

        // Send image to the server for processing
        fetch('/api/jewelryitems/searchimage', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                if (!response.ok) {
                    // Attempt to parse the JSON response for a message
                    return response.json().then(errorData => {
                        // Check for a specific error message
                        if (errorData.message === "No matching items found.") {
                            return Promise.reject("No matching items found.");
                        } else {
                            return Promise.reject(`Error: ${response.status} ${response.statusText}`);
                        }
                    });
                }
                return response.json();
            })
            .then(data => {
                console.log('Retrieved items:', data);

                sessionStorage.setItem('itemsData', JSON.stringify(data));

                // Redirect to the products page
                window.location.href = '/Home/Products';

            })
            .catch(error => {
                console.error('Error during image search:', error);
                // Display a message to the user based on the specific error
                if (error === "No matching items found.") {
                    swal("Error!", "No matching items found.", "error");
                } else {
                    swal("Error!", "An error occurred while searching for items.", "error");
                }
            })
            .finally(() => {
                document.getElementById('loading-overlay').style.display = 'none';
                uploadedImage = null;
                const searchInput = document.querySelector('.search-input');
                searchInput.placeholder = "Search...";
                searchInput.disabled = false;
            });
    }
    // Otherwise, handle the search text input
    else if (searchInput) {
        console.log('Performing search with text:', searchInput);
        // Redirect to the products page with the search query as a URL parameter
        window.location.href = `/Home/Products?search=${encodeURIComponent(searchInput)}`;
        document.getElementById('loading-overlay').style.display = 'none';

    } else {
        swal("Error!", "No search input or image provided.", "error");
        document.getElementById('loading-overlay').style.display = 'none';
    }
});


function logout() {
    // Make an AJAX request to the logout endpoint
    fetch('api/index/Logout', {
        method: 'POST', // Use POST method for logout
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (response.ok) {
                // If logout is successful, redirect the user or refresh the page
                window.location.href = '/'; // Redirect to the homepage or desired page
            } else {
                console.error('Logout failed');
                // Optionally, show an error message to the user
            }
        })
        .catch(error => {
            console.error('Error during logout:', error);
        });
}
