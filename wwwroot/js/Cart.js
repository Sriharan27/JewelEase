$(document).ready(function () {
    loadPageCartItems();
    updateCartCount();

    // Attach click event for updating the cart when the "Update Cart" button is clicked
    $('.flex-c-m.stext-101.cl2.size-119.bg8.bor13.hov-btn3').click(function () {
        updateCartQuantities();
        swal("Cart updated!", "Shopping cart updated", "success");
    });
});

// Function to handle plus and minus buttons
$(document).on('click', '.btn-num-product-up', function () {
    let quantityInput = $(this).siblings('.num-product');
    let currentQty = parseInt(quantityInput.val());
    quantityInput.val(currentQty + 1);
});
$(document).on('click', '.btn-num-product-down', function () {
    let quantityInput = $(this).siblings('.num-product');
    let currentQty = parseInt(quantityInput.val());
    if (currentQty > 1) {
        quantityInput.val(currentQty - 1);
    }
});

function updateCartQuantities() {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    let newCart = [];

    // Loop through each row in the cart and get the updated quantity
    $('.table-shopping-cart tbody tr').each(function () {
        let itemId = $(this).find('.item-id').val(); // Get the item ID from the hidden input
        let updatedQty = parseInt($(this).find('.num-product').val());

        // Add the item ID to the new cart array as many times as its updated quantity
        for (let i = 0; i < updatedQty; i++) {
            newCart.push(itemId);
        }
    });

    // Save the updated cart to local storage
    localStorage.setItem('cart', JSON.stringify(newCart));

    updateCartCount();
}

function updateCartCount() {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    $('.icon-header-noti').attr('data-notify', cart.length);  // Update the data-notify attribute
}

function loadPageCartItems() {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    let cartItemsContainer = $('.table-shopping-cart tbody'); // Target tbody inside the table-shopping-cart

    // Clear the cart container
    cartItemsContainer.empty();

    // Check if the cart is empty
    if (cart.length === 0) {
        cartItemsContainer.append('<tr><td colspan="3" class="text-center">Your cart is empty.</td></tr>');
        return;
    }

    // Create an object to count occurrences of each item ID
    let itemCountMap = cart.reduce((acc, itemId) => {
        acc[itemId] = (acc[itemId] || 0) + 1; // Increment count for each occurrence
        return acc;
    }, {});

    // Loop through each unique item ID
    Object.keys(itemCountMap).forEach(itemId => {
        // Make an AJAX call to get the item details using the item ID
        if (typeof itemId === 'undefined' || itemId === null) {
            console.error('Invalid itemId:', itemId);
            return; // Skip this item
        }

        $.ajax({
            contentType: "application/json; charset=utf-8",
            type: "GET",
            dataType: "json",
            url: `/api/index/Pro/${itemId}`,
            success: function (data) {
                if (data) {
                    let item = data;
                    let quantity = itemCountMap[itemId]; // Get the quantity of the current item

                    // Dynamically append each item as a table row to the cart container
                    cartItemsContainer.append(`
                        <tr class="table_row">
                            <td class="column-1">
                                <div class="how-itemcart1">
                                    <img src="${item.imageUrl.startsWith('http') ? item.imageUrl : `https://localhost:44372/${item.imageUrl}`}" alt="${item.name}">
                                </div>
                            </td>
                            <td class="column-2" style="padding-right:5px">${item.name}</td>
                            <td class="column-3">
                                <div class="wrap-num-product flex-w m-l-auto m-r-0">
                                    <div class="btn-num-product-down cl8 hov-btn3 trans-04 flex-c-m">
                                        <i class="fs-16 zmdi zmdi-minus"></i>
                                    </div>

                                    <input class="mtext-104 cl3 txt-center num-product" type="number" value="${quantity}">

                                    <div class="btn-num-product-up cl8 hov-btn3 trans-04 flex-c-m">
                                        <i class="fs-16 zmdi zmdi-plus"></i>
                                    </div>
                                </div>
                                <input type="hidden" class="item-id" value="${itemId}" />
                            </td>
                            <td class="column-4" style="padding-right:10px;">
                                <button class="remove-item-btn btn btn-danger" data-item-id="${itemId}">Remove</button>
                            </td>
                        </tr>
                    `);
                } else {
                    cartItemsContainer.append('<tr><td colspan="3" class="text-center">Item details not found.</td></tr>');
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
// Event listener for the remove button
$(document).on('click', '.remove-item-btn', function () {
    let itemId = $(this).data('item-id'); // Get the item ID from the button's data attribute
    console.log('Remove item clicked:', itemId);  // Debugging line
    removeFromCart(itemId);  // Remove the item from the cart
});

function removeFromCart(itemId) {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    console.log('Cart before removal:', cart);

    itemId = String(itemId);  // Make sure itemId is a string

    // Filter out the item from the cart (both itemId and cart items as strings)
    cart = cart.filter(id => String(id) !== itemId);  // Explicit conversion to string for both

    console.log('Cart after removal:', cart);

    // Save the updated cart back to localStorage
    localStorage.setItem('cart', JSON.stringify(cart));

    // Clear the current cart items and reload them
    $('#cart-items').empty();
    loadPageCartItems();
    updateCartCount();
    swal("Item Removed!", "Item removed from cart!", "success");
}

document.getElementById("requestQuotationBtn").addEventListener("click", function () {
    // Get the current date in ISO format
    const RequestedDate = new Date().toISOString();

    // Get the logged-in user's ID from the data attribute and ensure it's an integer
    const userId = parseInt(document.getElementById("requestQuotationBtn").getAttribute("data-user-id"));

    if (!userId || userId === 0) {
        swal({
            title: "Login Required!",
            text: "Please Login or signup to continue!",
            icon: "warning",
            buttons: {
                cancel: "Cancel",
                login: "Login"
            }
        }).then((value) => {
            if (value === "login") {
                // Redirect to the login page if "Login" is pressed
                window.location.href = '/Home/Login';
            }
        });

        return;  // Exit if no valid user ID
    }

    // Prepare the ItemQuotation object
    const itemQuotation = {
        userId: userId,
        discount: 0, // Default discount 0
        quotationPrice: 0, // Default QuotationPrice 0
        status: "Pending", // Default status 'pending'
        requestedDate: RequestedDate
    };

    // Send ItemQuotation data to the server
    fetch('/api/QuotationApi/CreateItemQuotation', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(itemQuotation)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const quotationId = data.quotationId; // Get the created QuotationId
                const cartItems = JSON.parse(localStorage.getItem("cart")) || [];

                const lineItems = [];
                // Count quantities of each JewelryItemId in cart
                const itemCounts = {};
                cartItems.forEach(itemId => {
                    itemCounts[itemId] = (itemCounts[itemId] || 0) + 1;
                });

                // Prepare the LineItems
                for (const [jewelryItemId, qty] of Object.entries(itemCounts)) {
                    lineItems.push({
                        quotationId: quotationId,
                        jewelryItemId: parseInt(jewelryItemId),
                        qty: qty,
                        totalPrice: 0 // Default total price
                    });
                }

                // Send line items to the server
                fetch('/api/QuotationApi/CreateLineItems', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(lineItems)
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            // Optionally, clear the cart after request
                            localStorage.removeItem("cart");
                            updateCartCount();
                            swal("Quotation requested!", "Quotation requested successfully!", "success")
                                .then(() => {
                                    // Redirect to the login page in HomeController after OK is clicked
                                    window.location.href = '/';
                                });
                        } else {
                            alert('Failed to create line items.');
                        }
                    });
            } else {
                alert('Failed to create quotation. Check the server for more details.');
                console.error(data); // Log the detailed error for debugging
            }
        });
});
