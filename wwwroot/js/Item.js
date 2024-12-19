let jewelryItemId;
let jewelryName;
$(document).ready(function () {
    const itmID = new URLSearchParams(window.location.search).get('itmID'); // Retrieve item ID from URL
    loadProduct(itmID);

    const decrementBtn = document.querySelector(".btn-num-product-down");
    const incrementBtn = document.querySelector(".btn-num-product-up");
    const quantityInput = document.querySelector("input[name='num-product']");

    decrementBtn.addEventListener("click", function () {
        let quantity = parseInt(quantityInput.value);
        if (quantity > 1) {
            quantityInput.value = quantity - 1;
        }
    });

    incrementBtn.addEventListener("click", function () {
        let quantity = parseInt(quantityInput.value);
        quantityInput.value = quantity + 1;
    });

    // Handle adding item to cart
    $(document).on('click', '.add-to-cart', function () {
        const quantity = parseInt($(this).closest('.size-250').find('.num-product').val(), 10) || 1; // Get quantity or default to 1
        addToCart(jewelryItemId,quantity);
        swal(jewelryName, "is added to cart !", "success");
    });

    // Update cart count when the page loads
    updateCartCount();
});

function loadProduct(itmID) {
    $.ajax({
        contentType: "application/json; charset=utf-8",
        type: "GET",
        dataType: "json",
        url: `/api/index/Pro/${itmID}`,
        success: function (data) {
            if (data) {
                let item = data;
                console.log(item);

                // Fetch category name and render product details
                fetchCategoryName(item.categoryId, function (categoryName) {
                    updateBreadcrumb(item.categoryId, categoryName, item.name);
                    renderProductDetails(item, categoryName);
                    renderProductImage(item.imageUrl);
                    renderProductDescription(item.description);
                    loadRelatedProducts(item.categoryId,itmID);
                    jewelryItemId = item.jewelryItemId
                    jewelryName = item.name
                });
            } else {
                $('#itemDetails').html('<p>No item details found.</p>');
            }
        },
        error: function (jqXHR, exception) {
            handleAjaxError(jqXHR, exception);
        }
    });
}

function fetchCategoryName(catID, callback) {
    $.ajax({
        contentType: "application/json; charset=utf-8",
        type: "GET",
        dataType: "json",
        url: `/api/index/Cat/${catID}`,
        success: function (data) {
            if (data) {
                callback(data.categoryName); // Pass category name to the callback
            } else {
                callback('Unknown');
            }
        },
        error: function (jqXHR, exception) {
            handleAjaxError(jqXHR, exception);
            callback('Unknown');
        }
    });
}
function updateBreadcrumb(catId, categoryName, itemName) {
    // Update the category name and link
    $('.category-link').attr('href', `/Home/products?catID=${catId}`);
    $('.category-name').text(categoryName);

    // Update the item name
    $('.item-name').text(itemName);
}

function renderProductDetails(item, categoryName) {
    $('.js-name-detail').text(item.name);
    $('.mtext-106').text(`${item.karats} Karats`);
    $('.stext-102').text(item.description);
    $('.js-category').text(`Category : ${categoryName}`);
    $('.SKU').text(`SKU : ${item.identificationID}`);
    $('.itemName').text(`SKU : ${item.name}`);

    if (item.tryOnUrl != "")
    {
        $('.js-tryon-detail').attr('data-tryon-url', item.tryOnUrl).show();
    }

    $('.js-addcart-detail').attr('data-id', item.jewelryItemId).attr('data-name', item.name);

    $('.js-addcart-detail').on('click', function () {
        addToCart(item.jewelryItemId, item.name);
    });
}

$(document).on('click', '.js-tryon-detail', function () {
    const tryOnUrl = $(this).attr('data-tryon-url'); // Get the URL from the data attribute
    if (tryOnUrl) {
        window.open(tryOnUrl, '_blank'); // Open the URL in a new tab
    } else {
        alert('Try On URL not available.'); // Optional: Handle the case where the URL is not set
    }
});


function renderProductImage(imageUrl) {
    const imagePath = imageUrl.startsWith('http') ? imageUrl : `https://localhost:44372/${imageUrl}`;
    $('#itemImageContainer').html(`
        <img src="${imagePath}" class="img-fluid" alt="Item Image">
    `);
}

function renderProductDescription(description) {
    $('#description .stext-102').html(description);
}

function addToCart(itemId, quantity) {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];

    for (let i = 0; i < quantity; i++) {
        cart.push(itemId);
    }

    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartCount();
}

function updateCartCount() {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    $('.icon-header-noti').attr('data-notify', cart.length);  // Update the data-notify attribute
}

function handleAjaxError(jqXHR, exception) {
    let msg = '';
    if (jqXHR.status === 0) {
        msg = 'Not connected. Verify network.';
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
        msg = `Uncaught Error: ${jqXHR.responseText}`;
    }
    alert(msg);
}
function loadRelatedProducts(categoryId, itmID) {
    $.ajax({
        contentType: "application/json; charset=utf-8",
        type: "GET",
        dataType: "json",
        url: `/api/index/ProByCategory/${categoryId}`,
        success: function (products) {
            if (products && products.length > 0) {
                // Filter out the product with the given itmID
                const filteredProducts = products.filter(product => product.jewelryItemId !== parseInt(itmID));

                if (filteredProducts.length > 0) {
                    renderRelatedProducts(filteredProducts);
                } else {
                    $('.slick2').html('<p>No related products found.</p>');
                }
            } else {
                $('.slick2').html('<p>No related products found.</p>');
            }
        },
        error: function (jqXHR, exception) {
            handleAjaxError(jqXHR, exception);
        }
    });
}

function renderRelatedProducts(products) {
    let htmlContent = '';
    products.forEach(product => {
        const imagePath = `https://localhost:44372/${product.imageUrl}`;

        htmlContent += `
            <div class="item-slick2 p-l-15 p-r-15 p-t-15 p-b-15">
                <div class="block2">
                    <div class="block2-pic hov-img0">
                        <a href="https://localhost:44372/Home/item?itmID=${product.jewelryItemId}">
                            <img src="${imagePath}" alt="IMG-PRODUCT">
                        </a>
                    </div>
                    <div class="block2-txt flex-w flex-t p-t-14">
                        <div class="block2-txt-child1 flex-col-l ">
                            <a href="https://localhost:44372/Home/item?itmID=${product.jewelryItemId}" 
                               class="stext-104 cl4 hov-cl1 trans-04 js-name-b2 p-b-6">
                                ${product.name}
                            </a>
                        </div>
                    </div>
                </div>
            </div>`;
    });

    $('.slick2').html(htmlContent);

    initializeSlick();
}

function initializeSlick() {
    $('.slick2').slick({
        slidesToShow: 4,
        slidesToScroll: 4,
        infinite: false,
        autoplay: false,
        autoplaySpeed: 6000,
        arrows: true, // Ensure arrows are enabled
        prevArrow: $('.prev-slick2'), // Custom previous arrow selector
        nextArrow: $('.next-slick2'), // Custom next arrow selector
        dots: false, // Optional: you can enable dots if needed
        responsive: [
            { breakpoint: 1200, settings: { slidesToShow: 4, slidesToScroll: 4 } },
            { breakpoint: 992, settings: { slidesToShow: 3, slidesToScroll: 3 } },
            { breakpoint: 768, settings: { slidesToShow: 2, slidesToScroll: 2 } },
            { breakpoint: 576, settings: { slidesToShow: 1, slidesToScroll: 1 } }
        ]
    });

}