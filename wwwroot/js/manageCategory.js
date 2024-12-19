$(document).ready(function () {
    loadCategories();

    $('#categoryForm').on('submit', function (e) {
        e.preventDefault();
        saveCategory();
    });

    $('#resetForm').on('click', function () {
        resetForm();
    });

    function loadCategories() {
        $.ajax({
            url: '/api/categoryapi',
            type: 'GET',
            success: function (data) {
                var table = $('#categoriesTable').DataTable();
                table.clear();
                data.forEach(function (item) {
                    table.row.add([
                        item.categoryId,
                        item.categoryName,
                        item.imageUrl,
                        '<button class="btn btn-warning btn-sm edit-category" data-id="' + item.categoryId + '">Edit</button> ' +
                        '<button class="btn btn-danger btn-sm delete-category" data-id="' + item.categoryId + '">Delete</button>'
                    ]).draw();
                });
            }
        });
    }

    function saveCategory() {
        var formData = {
            CategoryId: $('#categoryId').val(),
            CategoryName: $('#categoryName').val(),
            ImageUrl: $('#imageUrl').val()
        };

        var url = formData.CategoryId == 0 ? '/api/categoryapi' : '/api/categoryapi';
        var ajaxType = formData.CategoryId == 0 ? 'POST' : 'PUT';

        $.ajax({
            url: url,
            type: ajaxType,
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (data) {
                if (data.success) {
                    loadCategories();
                    resetForm();
                } else {
                    alert('An error occurred.');
                }
            }
        });
    }

    function resetForm() {
        $('#categoryId').val(0);
        $('#categoryName').val('');
        $('#imageUrl').val('');
    }

    $('#categoriesTable').on('click', '.edit-category', function () {
        var categoryId = $(this).data('id');
        var row = $(this).closest('tr');

        $('#categoryId').val(categoryId);
        $('#categoryName').val(row.find('td:eq(1)').text());
        $('#imageUrl').val(row.find('td:eq(2)').text());
    });

    $('#categoriesTable').on('click', '.delete-category', function () {
        var categoryId = $(this).data('id');

        if (confirm('Are you sure you want to delete this category?')) {
            $.ajax({
                url: '/api/categoryapi/' + categoryId,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        loadCategories();
                    } else {
                        alert('An error occurred.');
                    }
                }
            });
        }
    });

    $('#categoriesTable').DataTable();
});
