$(document).ready(function (e) {
    function refresh() {
        $('#drop-text-mail').text("Click to select mail or just drop it here.");
        $('#drop-text-attachements').text("Click to select attachements or just drop them here.");
        $('form').each(function () { this.reset() });
        $("#main-alert").removeClass();
        $("#main-alert").html('');
        $('#select-sender').selectpicker('refresh');
        $('#select-receiver').selectpicker('refresh');
        $('#select-type').selectpicker('refresh');
    }

    $('#btn-change-password').on('click', function () {
        function clearAlert() {
            $("#alert-change-password").removeClass();
            $("#alert-change-password").addClass("alert");
        }
        function clearAll() {
            clearAlert();
            $('#inputs-change-password').find("input").val("");
        }
        if ($("#input-old-password").val() == "" || $("#input-new-password").val() == "" || $("#input-confirm-password").val() == "") {

            clearAlert();

            $("#alert-change-password").addClass("alert-danger");
            $("#alert-change-password").html("Please complete all fields!");
            $("#modal-change-password").effect("shake");
        }
        else if ($("#input-new-password").val() != $("#input-confirm-password").val()) {

            clearAll();

            $("#alert-change-password").addClass("alert-danger");
            $("#alert-change-password").html("Password confirmation doesn't match");
            $("#modal-change-password").effect("shake");

        }
        else {
            $.post(
                '/Home/ChangePassword',
                { oldPassword: $("#input-old-password").val(), newPassword: $("#input-new-password").val(), confirmPassword: $("#input-confirm-password").val() },
                function (response) {
                    if (!response.signedIn)
                        window.location.reload(true);
                    else {
                        clearAll();

                        $("#alert-change-password").addClass("alert-" + response.alertClass);
                        $("#alert-change-password").html(response.alertMessage);

                        if (response.alertClass == "danger")
                            $("#modal-change-password").effect("shake");
                    }
                }
            );
        }
    });

    $('#input-mail').change(function () {

        if (this.files.length == 1) {

            var file = $(this).prop("files");
            var filename = $.map(file, function (val) { return val.name; });
            var string2 = filename.join(', ');

            $('#drop-text-mail').text("Selected file : " + string2);

            $('#btn-ocr').removeClass('d-none');
        }
        else if (this.files.length == 0) {
            $('#btn-ocr').addClass('d-none');
            $('#drop-text-mail').text("Click to select mail or just drop it here.");
        }
        else {
            $('#btn-ocr').addClass('d-none');
            $('#drop-text-mail').text("You can not select more than one mail file! Click to retry");
        }
    });

    $('#input-attachements').change(function () {

        if (this.files.length > 0) {
            var string1;
            if (this.files.length > 1)
                string1 = "Selected files : ";
            else
                string1 = "Selected file : ";

            var files = $(this).prop("files");
            var filenames = $.map(files, function (val) { return val.name; });
            var string2 = filenames.join(' ; ');

            $('#drop-text-attachements').text(string1 + string2);
        }
        else {
            $(this).next().text("Click to select attachements or just drop them here.");
        }
    });

    $("#form-new-mail").submit(function (e) {

        e.preventDefault();
        var formData = new FormData(this);
        refresh();

        $.ajax({
            url: 'AddMail',
            type: 'POST',
            data: formData,
            cache: false,
            success: function (response) {
                
                if (!response.signedIn) {
                    window.location.reload(true);
                }
                else {
                    $("#main-alert").addClass("alert");
                    $('#form-new-mail').find("input").val("");
                    $("#main-alert").addClass("alert-" + response.alertClass);
                    if (response.alertClass == "danger") {
                        $("#main-alert").html('<i class="fas fa-times"></i> ' + response.alertMessage);
                        $("#main-alert").effect("shake");
                    }
                    else if (response.alertClass == "warning") {
                        $("#main-alert").html('<i class="fas fa-exclamation"></i> ' + response.alertMessage);
                        $("#main-alert").effect("shake");
                    }
                    else
                        $("#main-alert").html('<i class="fas fa-check"></i> ' + response.alertMessage);
                }
            },
            contentType: false,
            processData: false
        });
    });

    $('#select-type').on('change', function () {

        $('#select-sender').selectpicker('destroy');
        $('#select-receiver').selectpicker('destroy');
        $('#select-sender').find('option').remove().end();
        $('#select-receiver').find('option').remove().end();
        $('#btn-add-new-contact').remove();

        $.post(
            'GetParties',
            function (response) {
                var Parties = JSON.parse(response);

                var btnAddNewContact = document.createElement("button");
                btnAddNewContact.innerText = "Add new contact";
                btnAddNewContact.setAttribute("type", "button");
                btnAddNewContact.setAttribute("id", "btn-add-new-contact");
                btnAddNewContact.setAttribute("class", "btn btn-block btn-light rounded-0 border-0");

                $("#select-sender").append('<option selected value="" >Choose...</option>');
                $("#select-receiver").append('<option selected value="" >Choose...</option>');


                if ($("#select-type").val() == "incoming") {

                    for (var i = 0; Parties.Employees[i]; i++) {
                        $("#select-receiver").append(new Option(Parties.Employees[i].Position + "    " + Parties.Employees[i].Firstname + "    " + Parties.Employees[i].Lastname, Parties.Employees[i].Id));
                    }

                    for (var i = 0; Parties.Contacts[i]; i++) {
                        $("#select-sender").append(new Option(Parties.Contacts[i].Firstname + "    " + Parties.Contacts[i].Lastname + "    " + Parties.Contacts[i].SocialReason, Parties.Contacts[i].Id));
                    }

                    $('#select-sender').selectpicker('refresh');
                    $('#select-receiver').selectpicker('refresh');
                    $('.dropdown-menu')[2].append(btnAddNewContact)

                }
                else if ($("#select-type").val() == "outgoing") {
                    for (var i = 0; Parties.Employees[i]; i++) {
                        $("#select-sender").append(new Option(Parties.Employees[i].Position + "    " + Parties.Employees[i].Firstname + "    " + Parties.Employees[i].Lastname, Parties.Employees[i].Id));
                    }

                    for (var i = 0; Parties.Contacts[i]; i++) {
                        $("#select-receiver").append(new Option(Parties.Contacts[i].Firstname + "    " + Parties.Contacts[i].Lastname + "    " + Parties.Contacts[i].SocialReason, Parties.Contacts[i].Id));
                    }
                    $('#select-sender').selectpicker('refresh');
                    $('#select-receiver').selectpicker('refresh');
                    $('.dropdown-menu')[4].append(btnAddNewContact)
                }
                $('.dropdown-menu').addClass("pb-0");
            }


        );


    });

    $('.drop-area').bind('dragover', function () {
        $(this).addClass('drag-over');
    });

    $('.drop-area').bind('dragleave', function () {
        $(this).removeClass('drag-over');
    });

    $('.drop-area').on('drop', function () {
        $(this).removeClass('drag-over');

    });

    $('#btn-ocr').on('click', function () {
        alert("Working on it");        
    });



});