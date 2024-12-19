$(document).ready(function () {
    IloadCategories();
});

var baseUrl = "https://localhost:44372/";

function IloadCategories() {
    $.ajax({
        contentType: "application/json; charset=utf-8",
        type: "GET",
        dataType: "json",
        url: "/api/index/cat",
        success: function (data) {
            var txtHTML = "";

            data.forEach((category, index) => {
                txtHTML += `
                    <div class="col-md-6 col-xl-4 p-b-30 m-lr-auto">
                        <div class="block1 wrap-pic-w">
                            <img src="${baseUrl}${category.imageUrl}" alt="IMG-BANNER">

                            <a href="${baseUrl}Home/products?catID=${category.categoryId}" class="block1-txt ab-t-l s-full flex-col-l-sb p-lr-38 p-tb-34 trans-03 respon3">
                                <div class="block1-txt-child1 flex-col-l">
                                    <span class="block1-name ltext-102 trans-04 p-b-8" style="color: transparent;">
                                        ${category.categoryName}
                                    </span>
                                </div>

                                <div class="block1-txt-child2 p-b-4 trans-05">
                                    <div class="block1-link stext-101 cl0 trans-09">
                                        Shop Now
                                    </div>
                                </div>
                            </a>
                        </div>
                    </div>
                `;
            });

            $("#allCategories").append(txtHTML);
        },
        failure: function (response) {
            ShowError(response.d);
        },
        error: function (jqXHR, exception) {
            var msg = '';
            if (jqXHR.status === 0) {
                msg = 'Not connect.\n Verify Network.';
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