// Elements related to the form switching
const loginText = document.querySelector(".title-text .login");
const loginForm = document.querySelector("form.login");
const loginBtn = document.querySelector("label.login");
const signupBtn = document.querySelector("label.signup");
const signupLink = document.querySelector("form .signup-link a");

const resetPasswordForm = document.querySelector("form.reset-password");
const signupForm = document.querySelector("form.signup");

const loginTitle = document.querySelector(".title.login");
const signupTitle = document.querySelector(".title.signup");
const resetPasswordTitle = document.querySelector(".title.resetpassword");
const slideControls = document.querySelector(".slide-controls");

function showResetPasswordForm() {
    loginForm.style.display = "none";
    signupForm.style.display = "none";
    resetPasswordForm.style.display = "block"; 

    loginTitle.style.display = "none";
    signupTitle.style.display = "none";
    resetPasswordTitle.style.display = "block";

    slideControls.style.display = "none";
}
function goBackToLogin() {
     window.location.href = '/Home/login';
}

// Switch to signup form
signupBtn.onclick = () => {
    loginForm.style.marginLeft = "-50%";  
    loginText.style.marginLeft = "-50%";  
};

// Switch back to login form
loginBtn.onclick = () => {
    loginForm.style.marginLeft = "0%";  
    loginText.style.marginLeft = "0%";  
};


signupLink.onclick = () => {
    signupBtn.click();
    return false; 
};


$('#btnlogin').on('click', function (e) {

    // Validate the form before AJAX
    var loginForm = document.querySelector('form.login');
    if (!loginForm.checkValidity()) {
        // If form is invalid, trigger the browser's built-in validation
        loginForm.reportValidity();
        return;  // Exit if validation fails
    }

    e.preventDefault();

    var username = $('#username').val();
    var password = $('#password').val();

    if (username && password) {
        // AJAX request for login
        $.ajax({
            url: '/api/index/login',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Username: username, Password: password }),
            success: function (response) {
                if (response.status === "Success") {
                    // Check the user's role for redirection
                    if (response.role === "Customer") {
                        swal("Login Successful!", "You have successfully logged in. Enjoy your shopping!", "success")
                            .then(() => {
                                window.location.href = '/';  // Redirect to homepage for customers after clicking OK
                            });

                    } else if (response.role === "Admin") {
                        swal("Login Successful!", "You have successfully logged in as admin!", "success")
                            .then(() => {
                                window.location.href = '/admin';  // Redirect to admin page after clicking OK
                            });
                    }
                } else {
                    swal("Error!", response.message, "error");
                }
            },
            error: function (jqXHR) {
                if (jqXHR.status === 401) {
                    swal("Error!", "Invalid username or password", "error");
                } else {
                    swal("An Error Occurred!", jqXHR.responseText, "error");
                }
            }
        });
    } else {
        swal("Error!", "Please enter both username and password.", "error");// Input validation
    }
});


$('#btnreg').on('click', function (e) {

    var signupForm = document.querySelector('form.signup');

    if (!signupForm.checkValidity()) {
        signupForm.reportValidity();
        return;  
    }

    e.preventDefault();

    const signupverifyButton = document.getElementById('signupverifyEmail');

    if (signupverifyButton.textContent === "Verify") {
        swal("Error!", "Please verify email to to signup!", "error");
        return;
    }

    const newUser = {
        Name: $('#registerName').val(),
        PasswordHash: $('#registerPassword').val(),
        Email: $('#registerEmail').val(),
        Role: 'Customer' 
    };

    console.log(newUser);

    if (newUser.PasswordHash !== $('#confirmPassword').val()) {
        swal("Error!", "Passwords do not match.", "error");
        return;
    }

    $.ajax({
        url: '/api/index/register',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(newUser),
        success: function (response) {
            swal("Registration Successful!", "You have successfully created your account. Please log in to continue.", "success")
                .then(() => {
                    window.location.href = '/Home/login';
                });
        },
        error: function (jqXHR) {
            swal("Error!", jqXHR.responseText, "error");
        }
    });
});

$('#btnreset').on('click', function (e) {

    var resetPasswordForm = document.querySelector('form.reset-password');

    if (!resetPasswordForm.checkValidity()) {
        // If form is invalid, trigger the browser's built-in validation
        resetPasswordForm.reportValidity();
        return;  // Exit if validation fails
    }

    e.preventDefault();

    const verifyButton = document.getElementById('verifyEmail');

    if (verifyButton.textContent === "Verify") {
        swal("Error!", "Please verify email to reset password!", "error");
        return;
    }

    const newPassword = $('#resetPassword').val();
    const confirmPassword = $('#resetConfirmPassword').val();

    if (newPassword !== confirmPassword) {
        swal("Error!", "Passwords do not match.", "error");
        return;
    }

    const resetData = {
        Email: $('#resetEmail').val(),
        NewPassword: newPassword
    };

    // Send AJAX request to reset password
    $.ajax({
        url: '/api/index/resetpassword',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(resetData),
        success: function (response) {
            swal("Password Reset Successful!", "Your password has been successfully reset.", "success")
                .then(() => {
                    window.location.href = '/Home/login';  // Redirect to login page
                });
        },
        error: function (jqXHR) {
            swal("Error!", jqXHR.responseText, "error");
        }
    });
});

function sendOtp(inputfield,formtype) {
    const resetEmailField = document.getElementById(inputfield);
    const Email = resetEmailField.value.trim(); // Remove any leading or trailing spaces

    if (!Email || !resetEmailField.checkValidity())
    {
        swal("Error!", "Email cannot be empty.", "error");
        return;  // Exit the function if validation fails
    }

    fetch('/api/index/SendOtp', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ Email: Email, formtype: formtype})
    })
        .then(response => response.json())
        .then(data => {
            console.log("API Response:", data);  // Log the response
            if (data.success) {
                const otp = prompt("Enter OTP sent to your email:");
                verifyOtp(otp, Email, formtype);
            } else {

                alert(data.errorMessage);
            }
        });
}
function verifyOtp(otp, Email, formtype) {
    fetch('/api/index/VerifyOtp', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ otp: otp, Email: Email })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                if (formtype === "Signup") {
                    // Update button for Signup form
                    document.getElementById('signupverifyEmail').textContent = 'Verified';
                    document.getElementById('signupverifyEmail').style.backgroundColor = 'green';
                    document.getElementById('signupverifyEmail').disabled = true;
                }
                else if (formtype === "ResetPassword") {
                    // Update button for Reset Password form
                    document.getElementById('verifyEmail').textContent = 'Verified';
                    document.getElementById('verifyEmail').style.backgroundColor = 'green';
                    document.getElementById('verifyEmail').disabled = true;
                }
            } else {
                // Handle invalid OTP
                alert("Invalid OTP. Please try again.");
            }
        })
        .catch(error => {
            console.error("Error during OTP verification:", error);
            alert("An error occurred while verifying OTP. Please try again.");
        });
}


document.getElementById('resetEmail').addEventListener('input', function () {
    const verifyButton = document.getElementById('verifyEmail');

    // Reset the button to its original state if the email changes
    verifyButton.textContent = 'Verify';
    verifyButton.style.backgroundColor = '';  // Reset to default color
    verifyButton.disabled = false;  // Enable the button again
});