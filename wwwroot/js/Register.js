$('#btnreg').on('click', function (e) {
   
    e.preventDefault();

    const newUser = {
        FirstName: $('#registerFirstname').val(),
        LastName: $('#registerLastname').val(),
        Username: $('#registerUsername').val(),
        PasswordHash: $('#registerPassword').val(),
        Email: $('#registerEmail').val(),
        Role: 'User' // Default role
    };

    console.log(newUser);

    if (newUser.PasswordHash !== $('#confirmPassword').val()) {
        alert('Passwords do not match!');
        return;
    }

    $.ajax({
        url: '/api/index/register',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(newUser),
        success: function (response) {
            alert('Registration successful!');
            window.location.href = '/Home/login';
        },
        error: function (jqXHR) {
            alert('An error occurred: ' + jqXHR.responseText);
        }
    });
});
