$(document).ready(function () {
    loadCategories();
    loadJewelryItems();

    $('#jewelryItemForm').on('submit', function (e) {
        e.preventDefault();
        saveJewelryItem();
    });

    $('#resetForm').on('click', function () {
        resetForm();
    });

    function loadCategories() {
        $.ajax({
            url: '/api/categoryapi',
            type: 'GET',
            success: function (data) {
                var $categorySelect = $('#categoryId');
                $categorySelect.empty();
                data.forEach(function (item) {
                    $categorySelect.append(new Option(item.categoryName, item.categoryId));
                });
            }
        });
    }

    function loadJewelryItems() {
        $.ajax({
            url: '/api/JewelryItems',
            type: 'GET',
            success: function (data) {
                var table = $('#jewelryItemsTable').DataTable();
                table.clear();
                data.forEach(function (item) {
                    table.row.add([
                        item.jewelryItemId,
                        item.name,
                        item.categoryName,
                        item.description,
                        item.karats,
                        item.price,
                        item.stockLevel,
                        item.imageUrl,
                        '<button class="btn btn-warning btn-sm edit-item" data-id="' + item.jewelryItemId + '">Edit</button> ' +
                        '<button class="btn btn-danger btn-sm delete-item" data-id="' + item.jewelryItemId + '">Delete</button>'
                    ]).draw();
                });
            }
        });
    }

    function saveJewelryItem() {
        var formData = {
            JewelryItemId: $('#jewelryItemId').val(),
            Name: $('#name').val(),
            CategoryId: $('#categoryId').val(),
            Description: $('#description').val(),
            karats: $('#karats').val(),
            Price: $('#price').val(),
            StockLevel: $('#stockLevel').val(),
            ImageUrl: $('#imageUrl').val()
        };

        var url = formData.JewelryItemId == 0 ? '/api/JewelryItems' : '/api/JewelryItems';
        var ajaxType = formData.JewelryItemId == 0 ? 'POST' : 'PUT';

        $.ajax({
            url: url,
            type: ajaxType,
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (data) {
                if (data.success) {
                    loadJewelryItems();
                    resetForm();
                } else {
                    alert('An error occurred.');
                }
            }
        });
    }

    function resetForm() {
        $('#jewelryItemId').val(0);
        $('#name').val('');
        $('#categoryId').val('');
        $('#description').val('');
        $('#karats').val('');
        $('#price').val('');
        $('#stockLevel').val('');
        $('#imageUrl').val('');
    }

    $('#jewelryItemsTable').on('click', '.edit-item', function () {
        var jewelryItemId = $(this).data('id');
        /*var row = $(this).closest('tr');

        $('#jewelryItemId').val(jewelryItemId);
        $('#name').val(row.find('td:eq(1)').text());
        $('#categoryId').val(row.find('td:eq(2)').data('id')); // Set category ID
        $('#description').val(row.find('td:eq(3)').text());
        $('#karats').val(row.find('td:eq(4)').text());
        $('#price').val(row.find('td:eq(5)').text());
        $('#stockLevel').val(row.find('td:eq(6)').text());
        $('#imageUrl').val(row.find('td:eq(7)').text());*/

        window.location.href = '/Admin/EditJewelryItem/' + jewelryItemId;
    });

    $('#jewelryItemsTable').on('click', '.delete-item', function () {
        var jewelryItemId = $(this).data('id');

        if (confirm('Are you sure you want to delete this jewelry item?')) {
            $.ajax({
                url: '/api/jewelryitemsapi/' + jewelryItemId,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        loadJewelryItems();
                    } else {
                        alert('An error occurred.');
                    }
                }
            });
        }
    });

    $('#jewelryItemsTable').DataTable();
});
