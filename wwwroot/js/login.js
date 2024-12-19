$('#btnlogin').on('click', function (e) {
    e.preventDefault();

    var username = $('#username').val();
    var password = $('#password').val();

    if (username && password) {
        $.ajax({
            url: '/api/index/login',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Username: username, Password: password }),
            success: function (response) {
                if (response === "Success") {
                    window.location.href = '/Home';
                } else {
                    alert('Login failed.');
                }
            },
            error: function (jqXHR) {
                if (jqXHR.status === 401) {
                    alert('Invalid username or password.');
                } else {
                    alert('An error occurred: ' + jqXHR.responseText);
                }
            }
        });
    } else {
        alert('Please enter both username and password.');
    }
});
