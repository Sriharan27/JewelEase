let header = document.querySelector('header');

(function ($) {
    "use strict";

    
    /*==================================================================
    [ Validate ]*/
    var date = $('.validate-input input[name="date"]');
    var time = $('.validate-input input[name="time"]');


    $('.validate-form').on('submit', function () {
        event.preventDefault();

        var check = true;

        if ($(date).val().trim() == '') {
            showValidate(date);
            check = false;
        }

        if ($(time).val().trim() == '') {
            showValidate(time);
            check = false;
        }

        if (check) {
            saveAppointment();
        }

        return check;
    });


    $('.validate-form .input1').each(function(){
        $(this).focus(function(){
           hideValidate(this);
       });
    });

    function showValidate(input) {
        var thisAlert = $(input).parent();

        $(thisAlert).addClass('alert-validate');
    }

    function hideValidate(input) {
        var thisAlert = $(input).parent();

        $(thisAlert).removeClass('alert-validate');
    }

})(jQuery);
function isIndexPage() {
    return window.location.pathname === '/';
}

function openAppointmentForm() {

    const userId = parseInt(document.getElementById("openVirtualAppointment").getAttribute("data-user-id"));

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
                window.location.href = '/Home/Login';
            }
        });

        return;  // Exit if no valid user ID
    }
    var modal = document.getElementById('appointmentModal');
    if (modal) {
        modal.style.display = 'block';
        if (header) {
            header.style.display = 'none';
        }
    } else {
        console.error('Modal or overlay not found');
    }
}

function closeAppointmentForm() {
    var modal = document.getElementById('appointmentModal');
    if (modal) {
        modal.style.display = 'none';

        if (header) {
            header.style.display = 'block';
        }
    } else {
        console.error('Modal or overlay not found');
    }
}

document.addEventListener('DOMContentLoaded', function () {
    window.onscroll = function () {
        var button = document.getElementById('virtualAppointmentBtn');
        if (isIndexPage() && window.scrollY > 400) {
            button.style.display = 'block'; // Show button when scrolled
        } else {
            button.style.display = 'none'; // Hide button when not scrolled
        }
    };
});

function saveAppointment() {
    var date = $('.validate-input input[name="date"]').val(); 
    var time = $('.validate-input input[name="time"]').val() + ":00"; 
    var message = $('.validate-input textarea[name="message"]').val(); 
    const userId = parseInt(document.getElementById("scheduleAppointmentBtn").getAttribute("data-user-id"));

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
                window.location.href = '/Home/Login';
            }
        });

        return;  
    }

    var formData = {
        UserId: userId,
        AppointmentDate: date,
        AppointmentTime: time,
        Message: message, 
        Status: "Pending"
    };

    $.ajax({
        type: 'POST',
        url: '/api/AppointmentApi', 
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (response) {
            swal("Success", "Appointment scheduled successfully!", "success")
            closeAppointmentForm();
        },
        error: function (xhr, status, error) {
            console.error('Error scheduling appointment:', error);
            swal("An Error Occured!", jqXHR.responseText, "error");
        }
    });
}